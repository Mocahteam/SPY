<?php
// La page est servie par PHP pour envoyer les en-têtes de sécurité (send_security_headers).
declare(strict_types=1);
require __DIR__ . '/../../../lib/lrs.php';
send_security_headers();
?>
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>SPY xAPI Explorer</title>
  <link rel="icon" type="image/png" href="../TemplateData/favicon.png">
  <link rel="preconnect" href="https://fonts.googleapis.com">
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
  <link href="https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@400;500&family=IBM+Plex+Sans:wght@400;500;600&display=swap" rel="stylesheet">
  <link rel="stylesheet" href="style.css">
</head>
<body>
  <div class="layout">
    <aside class="filters">
      <a href="../openTraces.html"><img src="../TemplateData/LogoSPY.png" alt="SPY logo" class="app-logo"></a>
      <h1>xAPI Explorer</h1>

      <form id="filter-form" autocomplete="off">
        <fieldset>
          <legend>Who</legend>
          <label>Player ID
            <input type="text" name="agent" placeholder="e.g. 86D3E3AC">
          </label>
        </fieldset>

        <fieldset>
          <legend>What</legend>
          <label>Verb (IRI)
            <input type="url" name="verb" list="common-verbs" placeholder="http://adlnet.gov/expapi/verbs/launched">
          </label>
          <datalist id="common-verbs">
            <option value="https://spy.lip6.fr/xapi/verbs/bugged">bugged</option>
            <option value="https://spy.lip6.fr/xapi/verbs/cleaned">cleaned</option>
            <option value="https://spy.lip6.fr/xapi/verbs/closed">closed</option>
            <option value="https://spy.lip6.fr/xapi/verbs/created">created</option>
            <option value="http://adlnet.gov/expapi/verbs/completed">completed</option>
            <option value="https://spy.lip6.fr/xapi/verbs/deleted">deleted</option>
            <option value="http://id.tincanapi.com/verb/disabled">disabled</option>
            <option value="http://id.tincanapi.com/verb/enabled">enabled</option>
            <option value="https://spy.lip6.fr/xapi/verbs/executed">executed</option>
            <option value="http://adlnet.gov/expapi/verbs/exited">exited</option>
            <option value="https://spy.lip6.fr/xapi/verbs/filtered">filtered</option>
            <option value="https://spy.lip6.fr/xapi/verbs/inserted">inserted</option>
            <option value="http://adlnet.gov/expapi/verbs/interacted">interacted</option>
            <option value="http://adlnet.gov/expapi/verbs/launched">launched</option>
            <option value="https://spy.lip6.fr/xapi/verbs/modified">modified</option>
            <option value="https://spy.lip6.fr/xapi/verbs/opened">opened</option>
            <option value="https://spy.lip6.fr/xapi/verbs/paused">paused</option>
            <option value="https://spy.lip6.fr/xapi/verbs/redone">redone</option>
            <option value="https://spy.lip6.fr/xapi/verbs/renamed">renamed</option>
            <option value="https://spy.lip6.fr/xapi/verbs/resumed">resumed</option>
            <option value="https://spy.lip6.fr/xapi/verbs/stepped">stepped</option>
            <option value="https://spy.lip6.fr/xapi/verbs/stopped">stopped</option>
            <option value="https://spy.lip6.fr/xapi/verbs/traced">traced</option>
            <option value="https://spy.lip6.fr/xapi/verbs/tried">tried</option>
            <option value="https://spy.lip6.fr/xapi/verbs/undone">undone</option>
          </datalist>
          <label>Activity (IRI)
            <input type="url" name="activity" list="common-activities" placeholder="https://w3id.org/xapi/seriousgames/activity-types/level">
          </label>
          <datalist id="common-activities">
            <option value="https://spy.lip6.fr/xapi/activity-types/block">block</option>
            <option value="https://spy.lip6.fr/xapi/activity-types/briefing">briefing</option>
            <option value="https://spy.lip6.fr/xapi/activity-types/camera">camera</option>
            <option value="https://spy.lip6.fr/xapi/activity-types/competencies">competencies</option>
            <option value="https://w3id.org/xapi/seriousgames/activity-types/level">level</option>
            <option value="https://spy.lip6.fr/xapi/activity-types/levelEditor">levelEditor</option>
            <option value="https://spy.lip6.fr/xapi/activity-types/program">program</option>
            <option value="https://spy.lip6.fr/xapi/activity-types/script">script</option>
            <option value="https://spy.lip6.fr/xapi/activity-types/scenarioEditor">scenarioEditor</option>
            <option value="https://spy.lip6.fr/xapi/activity-types/tts">tts</option>
          </datalist>
        </fieldset>

        <fieldset>
          <legend>Extension</legend>
          <label>Key contains
            <input type="text" name="extKey" list="common-extKeys" placeholder="e.g. error">
          </label>
          <datalist id="common-extKeys">
            <option value="content">content</option>
            <option value="context">context</option>
            <option value="error">error</option>
            <option value="oldvalue">oldvalue</option>
            <option value="progress">progress</option>
            <option value="score">score</option>
            <option value="value">value</option>
          </datalist>
          <label>Value contains
            <input type="text" name="extValue" placeholder="e.g. Detected">
          </label>
        </fieldset>

        <fieldset>
          <legend>When (UTC)</legend>
          <label>Since
            <input type="datetime-local" name="since">
          </label>
          <label>Until
            <input type="datetime-local" name="until">
          </label>
        </fieldset>

        <fieldset>
          <legend>Experiment</legend>
          <label>ID
            <input type="text" name="experimentationid">
          </label>
        </fieldset>

        <fieldset class="inline">
          <label>Per page
            <select name="limit">
              <option>25</option>
              <option selected>50</option>
              <option>100</option>
              <option>250</option>
            </select>
          </label>
          <label>Order
            <select name="ascending">
              <option value="false" selected>Newest first</option>
              <option value="true">Oldest first</option>
            </select>
          </label>
        </fieldset>

        <div class="form-actions">
          <button type="submit" class="primary">Search</button>
          <button type="reset">Clear</button>
        </div>
        <p class="hint">Experiment and extension filters are applied after the data is received, so a page may contain fewer rows than requested.</p>
      </form>
    </aside>

    <main>
      <header class="toolbar">
        <p id="status" role="status">Run a search to display statements.</p>
        <button id="download-btn" type="button" disabled>Download all (JSON)</button>
      </header>

      <div id="export-progress" class="export-progress" hidden>
        <div class="progress" role="progressbar" aria-label="Export in progress"><div class="progress-bar"></div></div>
        <span id="export-text" role="status">Starting export…</span>
        <button id="export-cancel" type="button">Cancel</button>
      </div>

      <div id="error" class="error" role="alert" hidden></div>

      <div class="table-wrap">
        <table id="statements">
          <thead>
            <tr>
              <th scope="col">Date</th>
              <th scope="col">Actor</th>
              <th scope="col">Verb</th>
              <th scope="col">Object</th>
              <th scope="col">Result</th>
            </tr>
          </thead>
          <tbody></tbody>
        </table>
      </div>

      <div class="pager">
        <button id="more-btn" type="button" hidden>Show more</button>
      </div>
    </main>
  </div>

  <script src="app.js"></script>
</body>
</html>
