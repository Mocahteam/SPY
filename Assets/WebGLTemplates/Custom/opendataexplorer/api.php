<?php
// Proxy entre le navigateur et le LRS.
// Les identifiants du LRS ne quittent jamais le serveur : le navigateur n'appelle
// que api.php, et c'est ce script qui s'authentifie auprès du LRS.
//
//   api.php?action=statements&...   une page (ou la suivante avec &more=...)
//   api.php?action=export_prepare&...     prépare l'export complet et envoie la progression
//   api.php?action=export_download&token=  télécharge l'export préparé
//   api.php?action=stats                   statistiques publiques (mises à jour de façon incrémentale)

declare(strict_types=1); # Ne pas chercher à convertir automatiquement les types, affecter un float à un int échouera

require __DIR__ . '/../../../lib/lrs.php'; # Chargement du fichier lrs.php
require __DIR__ . '/../../../lib/stats.php'; # Statistiques de la page openTraces.html

send_security_headers();

function send_json(array $data, int $status = 200): void
{
    http_response_code($status);
    header('Content-Type: application/json; charset=utf-8');
    header('Cache-Control: no-store');
    echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
}

// Réponses envoyées au fil de l'eau (export, statistiques) : une ligne JSON par
// étape (format NDJSON), sans tampon ni compression pour que chaque ligne parte
// immédiatement.
function start_ndjson(): void
{
    @ini_set('zlib.output_compression', '0');
    if (function_exists('apache_setenv')) {
        @apache_setenv('no-gzip', '1');
    }
    while (ob_get_level() > 0) {
        ob_end_flush();
    }
    header('Content-Type: application/x-ndjson; charset=utf-8');
    header('Cache-Control: no-store');
    header('X-Accel-Buffering: no'); // nginx : ne pas mettre la réponse en tampon
}

function emit_line(array $message): void
{
    echo json_encode($message, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES), "\n";
    flush();
}

// Renvoie une page de résultats. Quand des filtres sont appliqués après réception,
// enchaîne plusieurs pages du LRS jusqu'à atteindre le nombre de lignes demandé,
// dans la limite de MAX_SCAN_PAGES pages et de SCAN_TIME_BUDGET secondes.
function action_statements(array $q): void
{
    $filter = build_post_filter($q);
    $filtering = post_filters_active($q);
    $wanted = max(1, min(500, (int) (param($q, 'limit') ?: 50)));
    $maxPages = $filtering ? max(1, (int) cfg('MAX_SCAN_PAGES', 20)) : 1;
    $deadline = microtime(true) + (float) cfg('SCAN_TIME_BUDGET', 15);

    $more = param($q, 'more');
    $url = $more !== '' ? resolve_more($more) : build_query_url($q);

    $statements = [];
    $fetched = 0;
    $pages = 0;

    while (true) {
        $data = lrs_get($url);
        $pages++;
        $page = is_array($data['statements'] ?? null) ? $data['statements'] : [];
        $fetched += count($page);
        foreach ($page as $st) {
            if ($filter($st)) $statements[] = $st;
        }
        $next = !empty($data['more']) ? $data['more'] : null;

        // On s'arrête en fin de page : le lien "more" pointe sur la page suivante,
        // donc couper une page en deux ferait perdre des statements.
        if ($next === null || count($statements) >= $wanted
            || $pages >= $maxPages || microtime(true) >= $deadline) {
            break;
        }
        $url = resolve_more($next);
    }

    send_json([
        'statements'  => $statements,
        'fetched'     => $fetched, // parcourus dans le LRS avant filtres locaux
        'more'        => $next,
        // Arrêt sur une limite de parcours alors que la page n'est pas remplie
        'scanStopped' => $next !== null && count($statements) < $wanted,
    ]);
}

// ---------------------------------------------------------------------------
// Export en deux étapes, pour pouvoir afficher la progression :
//   1. export_prepare  : parcourt le LRS, écrit le fichier sur le serveur et
//                        envoie la progression au fil de l'eau (une ligne JSON
//                        par page du LRS, format NDJSON) ;
//   2. export_download : envoie le fichier terminé, puis le supprime.
// Le fichier n'est proposé au téléchargement que s'il est complet.
// ---------------------------------------------------------------------------

// Chemin du fichier d'export associé à un jeton (le format du jeton est vérifié
// pour qu'il ne puisse pas désigner un autre fichier)
function export_path(string $token): string
{
    if (!preg_match('/^[a-f0-9]{32}$/', $token)) {
        throw new HttpError(400, 'Invalid export token');
    }
    return sys_get_temp_dir() . "/xapi-export-$token.json";
}

// Supprime les exports abandonnés (annulés ou jamais téléchargés) depuis plus d'une heure
function purge_old_exports(): void
{
    foreach (glob(sys_get_temp_dir() . '/xapi-export-*.json') ?: [] as $file) {
        if (@filemtime($file) < time() - 3600) {
            @unlink($file);
        }
    }
}

function action_export_prepare(array $q): void
{
    set_time_limit(0);
    ignore_user_abort(false);
    purge_old_exports();

    $filter = build_post_filter($q);
    $max = (int) cfg('MAX_EXPORT', 200000);
    // Construite avant tout envoi : une erreur de paramètre reste une réponse JSON classique
    $url = build_query_url($q, 500);

    $token = bin2hex(random_bytes(16));
    $path = export_path($token);
    $oldUmask = umask(0077);
    $fh = fopen($path, 'wb');
    umask($oldUmask);
    if ($fh === false) {
        throw new HttpError(500, 'Unable to create the temporary export file');
    }

    start_ndjson();

    $exported = 0;
    $scanned = 0;
    $limitReached = false;

    try {
        fwrite($fh, "[\n");
        while ($url !== null) {
            $data = lrs_get($url);
            foreach ($data['statements'] ?? [] as $st) {
                $scanned++;
                if (!$filter($st)) continue;
                fwrite($fh, ($exported ? ",\n" : '') . json_encode($st, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES));
                if (++$exported >= $max) {
                    $limitReached = true;
                    break;
                }
            }
            emit_line(['exported' => $exported, 'scanned' => $scanned]);
            if (connection_aborted()) {
                throw new RuntimeException('aborted'); // l'utilisateur a annulé
            }
            $url = (!$limitReached && !empty($data['more'])) ? resolve_more($data['more']) : null;
        }
        fwrite($fh, "\n]\n");
        fclose($fh);

        emit_line([
            'done'         => true,
            'token'        => $token,
            'exported'     => $exported,
            'scanned'      => $scanned,
            'limitReached' => $limitReached,
        ]);
    } catch (Throwable $e) {
        if (is_resource($fh)) fclose($fh);
        @unlink($path);
        if ($e->getMessage() === 'aborted') {
            return;
        }
        if (!$e instanceof HttpError || $e->status >= 500) {
            error_log('[xapi-viewer] Export : ' . $e->getMessage());
        }
        // Les en-têtes sont déjà partis : l'erreur est envoyée comme une ligne de progression
        emit_line(['error' => $e instanceof HttpError ? $e->getMessage() : 'Internal error']);
    }
}

function action_export_download(array $q): void
{
    $path = export_path(param($q, 'token'));
    if (!is_file($path)) {
        throw new HttpError(404, 'Export not found or expired');
    }

    $stamp = gmdate('Y-m-d\TH-i-s\Z');
    header('Content-Type: application/json; charset=utf-8');
    header("Content-Disposition: attachment; filename=\"statements-$stamp.json\"");
    header('Content-Length: ' . filesize($path));
    header('Cache-Control: no-store');

    while (ob_get_level() > 0) ob_end_clean();
    readfile($path);
    @unlink($path);
}

// ---------------------------------------------------------------------------
// Statistiques publiques (page openTraces.html), envoyées au fil de l'eau
// pour que la page construise la courbe et fasse monter les compteurs
// pendant le calcul.
//   {"type":"progress","stats":{…}}   résultat partiel
//   {"type":"done","stats":{…}}       résultat final
//   {"type":"error","message":"…"}
// ---------------------------------------------------------------------------

// Un calcul est déjà lancé par un autre visiteur : on relaie sa progression
// (lue dans le fichier qu'il met à jour) jusqu'à ce que le résultat soit prêt.
function follow_running_computation(): void
{
    $progressFile = stats_progress_file();
    $lastSeen = '';
    $lastEmit = time();

    while (!connection_aborted()) {
        $cached = read_stats_cache(stats_cache_file());
        if (stats_are_fresh($cached)) {
            emit_line(['type' => 'done', 'stats' => $cached]);
            return;
        }

        $raw = (string) @file_get_contents($progressFile);
        if ($raw !== '' && $raw !== $lastSeen && is_array($partial = json_decode($raw, true))) {
            $lastSeen = $raw;
            $lastEmit = time();
            emit_line(['type' => 'progress', 'stats' => $partial]);
        } elseif (stats_computation_idle()) {
            // Le calcul s'est arrêté : soit il vient de finir, soit il a échoué
            $cached = read_stats_cache(stats_cache_file());
            if (stats_are_fresh($cached)) {
                emit_line(['type' => 'done', 'stats' => $cached]);
                return;
            }
            throw new HttpError(502, 'The statistics computation stopped before the end. Reload the page to start it again.');
        } elseif (time() - $lastEmit >= 5) {
            // Ligne de maintien : garde la connexion ouverte à travers les proxys
            $lastEmit = time();
            emit_line(['type' => 'wait']);
        }
        usleep(500000);
    }
}

function action_stats(): void
{
    set_time_limit(0);
    $cached = read_stats_cache(stats_cache_file());
    start_ndjson();

    try {
        // Mise à jour récente : on renvoie le résumé tel quel
        if (stats_are_fresh($cached)) {
            emit_line(['type' => 'done', 'stats' => $cached]);
            return;
        }

        // Sinon on complète les statistiques (ou on les construit la première fois)
        // en envoyant la progression, ou on suit le calcul lancé par un autre visiteur
        $stats = run_stats_computation(static function (array $partial): void {
            emit_line(['type' => 'progress', 'stats' => $partial]);
        });
        if ($stats !== null) {
            emit_line(['type' => 'done', 'stats' => $stats]);
        } else {
            follow_running_computation();
        }
    } catch (Throwable $e) {
        error_log('[xapi-viewer] Stats : ' . $e->getMessage());
        emit_line([
            'type'    => 'error',
            'message' => $e instanceof HttpError ? $e->getMessage() : 'Internal error',
        ]);
    }
}

try {
    if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
        throw new HttpError(405, 'Method not allowed');
    }
    match ($_GET['action'] ?? '') {
        'statements' => action_statements($_GET),
        'export_prepare'  => action_export_prepare($_GET),
        'export_download' => action_export_download($_GET),
        'stats'           => action_stats(),
        default      => throw new HttpError(404, 'Unknown action'),
    };
} catch (HttpError $e) {
    if ($e->status >= 500) error_log('[xapi-viewer] ' . $e->getMessage());
    send_json(['error' => $e->getMessage()], $e->status);
} catch (Throwable $e) {
    error_log('[xapi-viewer] ' . $e);
    send_json(['error' => 'Internal error'], 500);
}
