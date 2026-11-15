// Côté navigateur : aucun identifiant LRS ici. On n'appelle que le proxy api.php.

const form = document.getElementById('filter-form');
const tbody = document.querySelector('#statements tbody');
const statusEl = document.getElementById('status');
const errorEl = document.getElementById('error');
const moreBtn = document.getElementById('more-btn');
const downloadBtn = document.getElementById('download-btn');
const exportBox = document.getElementById('export-progress');
const exportText = document.getElementById('export-text');
const exportCancel = document.getElementById('export-cancel');
let exportController = null; // export en cours (permet de l'annuler)
let exportHideTimer = null;

const state = {
  params: null,   // filtres de la recherche en cours
  more: null,     // lien "more" renvoyé par le LRS
  scanStopped: false, // le serveur a arrêté de parcourir avant de remplir la page
  shown: 0,       // lignes affichées
  fetched: 0,     // statements reçus du LRS (avant filtres locaux)
  loading: false,
};

// ---------------------------------------------------------------------------
// Utilitaires
// ---------------------------------------------------------------------------

// Lit les filtres du formulaire ; les dates locales sont converties en ISO UTC.
function readFilters() {
  const data = new FormData(form);
  const params = new URLSearchParams();
  for (const [key, raw] of data.entries()) {
    const value = String(raw).trim();
    if (!value) continue;
    if (key === 'since' || key === 'until') {
      params.set(key, new Date(value).toISOString());
    } else {
      params.set(key, value);
    }
  }
  return params;
}

// Choisit la meilleure traduction dans une "language map" xAPI
function lang(map) {
  if (!map || typeof map !== 'object') return '';
  const prefs = ['en-US', 'en-GB', 'en', navigator.language, 'und'];
  for (const p of prefs) if (map[p]) return map[p];
  return Object.values(map)[0] || '';
}

function lastSegment(iri = '') {
  return iri.split(/[/#]/).filter(Boolean).pop() || iri;
}

function hueFrom(text) {
  let h = 0;
  for (const c of text) h = (h * 31 + c.charCodeAt(0)) % 360;
  return h;
}

const dateFmt = new Intl.DateTimeFormat(undefined, { dateStyle: 'short', timeStyle: 'medium' });

function el(tag, props = {}, ...children) {
  const node = document.createElement(tag);
  Object.assign(node, props);
  for (const child of children) {
    if (child == null || child === '') continue;
    node.append(child instanceof Node ? child : document.createTextNode(String(child)));
  }
  return node;
}

// ---------------------------------------------------------------------------
// Extraction des champs d'un statement
// ---------------------------------------------------------------------------

function describeActor(actor = {}) {
  const id =
    actor.mbox?.replace(/^mailto:/, '') ||
    (actor.account ? `${actor.account.name} @ ${actor.account.homePage}` : '') ||
    actor.openid ||
    (actor.mbox_sha1sum ? `sha1:${actor.mbox_sha1sum.slice(0, 10)}…` : '');
  const name = actor.name || (actor.objectType === 'Group' ? `Group (${actor.member?.length || 0})` : '');
  return { name: name || id || 'Unknown', id: name ? id : '' };
}

function describeObject(obj = {}) {
  switch (obj.objectType) {
    case 'Agent':
    case 'Group':
      return { name: describeActor(obj).name, id: 'Agent' };
    case 'StatementRef':
      return { name: 'Referenced statement', id: obj.id };
    case 'SubStatement':
      return { name: `Sub-statement: ${lang(obj.verb?.display) || lastSegment(obj.verb?.id)}`, id: '' };
    default:
      return {
        name: lang(obj.definition?.name) || lastSegment(obj.id),
        id: obj.id || '',
      };
  }
}

function describeResult(result) {
  if (!result) return null;
  const parts = [];
  if (result.success === true) parts.push(el('span', { className: 'success' }, 'Success'));
  if (result.success === false) parts.push(el('span', { className: 'failure' }, 'Failure'));
  if (result.completion === true) parts.push('Completed');

  const s = result.score;
  if (s) {
    if (typeof s.scaled === 'number') parts.push(`${Math.round(s.scaled * 100)}%`);
    else if (typeof s.raw === 'number') parts.push(s.max != null ? `${s.raw}/${s.max}` : `${s.raw} pts`);
  }
  if (result.duration) parts.push(result.duration);
  if (result.response) parts.push(`“${String(result.response).slice(0, 60)}”`);

  if (!parts.length) return null;
  const frag = document.createDocumentFragment();
  parts.forEach((p, i) => {
    if (i) frag.append(', ');
    frag.append(p);
  });
  return frag;
}

// ---------------------------------------------------------------------------
// Rendu
// ---------------------------------------------------------------------------

function renderRow(st) {
  const actor = describeActor(st.actor);
  const obj = describeObject(st.object);
  const verbLabel = lang(st.verb?.display) || lastSegment(st.verb?.id);
  const when = st.timestamp || st.stored;

  const verb = el('span', { className: 'verb', title: st.verb?.id || '' }, verbLabel);
  verb.style.setProperty('--h', hueFrom(st.verb?.id || verbLabel));

  const row = el('tr', { className: 'statement', tabIndex: 0 },
    el('td', { className: 'date' }, when ? dateFmt.format(new Date(when)) : '—'),
    el('td', {}, actor.name),
    el('td', {}, verb),
    el('td', {}, obj.name, obj.id && obj.id !== obj.name && el('span', { className: 'sub' }, obj.id)),
    el('td', {}, describeResult(st.result) || '—'),
  );

  let detail = null;
  const toggle = () => {
    if (detail) {
      detail.remove();
      detail = null;
      row.classList.remove('open');
      return;
    }
    detail = el('tr', { className: 'details' },
      el('td', { colSpan: 5 }, el('pre', {}, JSON.stringify(st, null, 2))));
    row.after(detail);
    row.classList.add('open');
  };
  row.addEventListener('click', toggle);
  row.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); toggle(); }
  });

  return row;
}

function renderEmpty(message) {
  tbody.replaceChildren(el('tr', { className: 'empty' }, el('td', { colSpan: 5 }, message)));
}

function updateStatus() {
  if (state.loading) {
    statusEl.textContent = 'Loading…';
  } else if (state.params) {
    const extra = state.fetched !== state.shown ? ` out of ${state.fetched} scanned in the LRS` : '';
    let text = `${state.shown} statement${state.shown === 1 ? '' : 's'} shown${extra}`;
    if (state.scanStopped) {
      text += '. The search stopped before filling the page: click “Show more” to continue.';
    } else {
      text += state.more ? '; more are available.' : '.';
    }
    statusEl.textContent = text;
  }
  moreBtn.hidden = !state.more;
  moreBtn.disabled = state.loading;
  downloadBtn.disabled = state.loading || !state.params || exportController !== null;
}

function showError(message) {
  errorEl.textContent = message;
  errorEl.hidden = !message;
}

// ---------------------------------------------------------------------------
// Chargement
// ---------------------------------------------------------------------------

async function fetchPage(params) {
  const res = await fetch(`api.php?action=statements&${params}`, { headers: { Accept: 'application/json' } });
  const body = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(body.error || `Error ${res.status}`);
  return body;
}

async function load({ reset }) {
  if (state.loading) return;
  showError('');

  let params;
  if (reset) {
    state.params = readFilters();
    state.more = null;
    state.shown = 0;
    state.fetched = 0;
    params = new URLSearchParams(state.params);
    tbody.replaceChildren();
  } else {
    // Page suivante : on renvoie les filtres appliqués après réception
    // (extension, expérimentation) avec le lien more
    params = new URLSearchParams(state.params);
    params.set('more', state.more);
  }

  state.loading = true;
  updateStatus();
  try {
    const data = await fetchPage(params);
    const frag = document.createDocumentFragment();
    data.statements.forEach((st) => frag.append(renderRow(st)));
    tbody.append(frag);

    state.shown += data.statements.length;
    state.fetched += data.fetched;
    state.more = data.more;
    state.scanStopped = Boolean(data.scanStopped);

    if (state.shown === 0) {
      renderEmpty(state.more
        ? 'No matching statements in the part scanned so far. Click “Show more” to search further.'
        : 'No statements match these filters.');
    } else {
      tbody.querySelector('tr.empty')?.remove();
    }
  } catch (err) {
    showError(`Unable to load statements: ${err.message}`);
  } finally {
    state.loading = false;
    updateStatus();
  }
}

// ---------------------------------------------------------------------------
// Export avec barre de progression
// ---------------------------------------------------------------------------

const formatNumber = (n) => Number(n).toLocaleString('en');

function showExportBox(text, { finished = false, hideAfter = 0 } = {}) {
  clearTimeout(exportHideTimer);
  exportBox.hidden = false;
  exportBox.classList.toggle('finished', finished);
  exportCancel.hidden = finished;
  exportText.textContent = text;
  if (hideAfter) exportHideTimer = setTimeout(() => { exportBox.hidden = true; }, hideAfter);
}

// Le serveur prépare le fichier en suivant tous les liens "more" et envoie
// sa progression ligne par ligne ; le fichier n'est téléchargé qu'une fois complet.
async function downloadAll() {
  if (!state.params || exportController) return;
  exportController = new AbortController();
  showError('');
  showExportBox('Starting export…');
  updateStatus();

  try {
    const res = await fetch(`api.php?action=export_prepare&${state.params}`, {
      signal: exportController.signal,
    });
    if (!res.ok || !res.body) {
      const body = await res.json().catch(() => ({}));
      throw new Error(body.error || `Error ${res.status}`);
    }

    const reader = res.body.getReader();
    const decoder = new TextDecoder();
    let buffer = '';
    let result = null;

    for (;;) {
      const { value, done } = await reader.read();
      if (done) break;
      buffer += decoder.decode(value, { stream: true });

      let newline;
      while ((newline = buffer.indexOf('\n')) >= 0) {
        const line = buffer.slice(0, newline).trim();
        buffer = buffer.slice(newline + 1);
        if (!line) continue;

        const msg = JSON.parse(line);
        if (msg.error) throw new Error(msg.error);
        if (msg.done) {
          result = msg;
        } else {
          const scanned = msg.scanned !== msg.exported ? ` (${formatNumber(msg.scanned)} scanned)` : '';
          showExportBox(`Exporting… ${formatNumber(msg.exported)} statements${scanned}`);
        }
      }
    }
    if (!result) throw new Error('the export stopped before completion');

    const a = el('a', {
      href: `api.php?action=export_download&token=${encodeURIComponent(result.token)}`,
      download: '',
    });
    document.body.append(a);
    a.click();
    a.remove();

    const limit = result.limitReached ? ' (export limit reached)' : '';
    showExportBox(`Download started: ${formatNumber(result.exported)} statements${limit}.`, {
      finished: true,
      hideAfter: result.limitReached ? 10000 : 5000,
    });
  } catch (err) {
    if (err.name === 'AbortError') {
      showExportBox('Export cancelled.', { finished: true, hideAfter: 3000 });
    } else {
      exportBox.hidden = true;
      showError(`Export failed: ${err.message}`);
    }
  } finally {
    exportController = null;
    updateStatus();
  }
}

// ---------------------------------------------------------------------------
// Événements
// ---------------------------------------------------------------------------

form.addEventListener('submit', (e) => {
  e.preventDefault();
  load({ reset: true });
});

form.addEventListener('reset', () => {
  state.params = null;
  state.more = null;
  state.scanStopped = false;
  state.shown = state.fetched = 0;
  renderEmpty('Run a search to display statements.');
  statusEl.textContent = 'Filters cleared.';
  showError('');
  updateStatus();
});

moreBtn.addEventListener('click', () => load({ reset: false }));
downloadBtn.addEventListener('click', downloadAll);
exportCancel.addEventListener('click', () => exportController?.abort());

// Valeur par défaut de "Since" : il y a un mois, à 00:00, heure locale du navigateur
function setDefaultSince() {
  const input = form.elements.since;
  if (!input) return;

  const d = new Date();
  const day = d.getDate();
  d.setDate(1);                     // évite les débordements (ex. 31 mars → « 31 février »)
  d.setMonth(d.getMonth() - 1);
  const lastDay = new Date(d.getFullYear(), d.getMonth() + 1, 0).getDate();
  d.setDate(Math.min(day, lastDay)); // 31 mars → 28 ou 29 février

  const pad = (n) => String(n).padStart(2, '0');
  // Format attendu par <input type="datetime-local"> : AAAA-MM-JJTHH:MM
  input.defaultValue = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T00:00`;
}
setDefaultSince();

renderEmpty('Run a search to display statements.');
