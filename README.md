<table align="center">
  <tr>
    <td valign="middle">
      <picture>
       <img src="public/favicon.svg" alt="Smart CV" width="72" />
      </picture>
    </td>
    <td valign="middle" style="padding-left: 12px;">
      <h1>Smart CV</h1>
    </td>
  </tr>
</table>

<p align="center">Dynamic Resume Creator</p>

<p align="center">
  <img alt="CI" src="https://github.com/michaelshirley1/door-manager-prod/actions/workflows/ci.yml/badge.svg">
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8-512BD4">
  <img alt="React 19" src="https://img.shields.io/badge/React-19-61DAFB">
  <img alt="TypeScript" src="https://img.shields.io/badge/TypeScript-4.9-3178C6">
</p>


<img width="1420" height="889" alt="image" src="https://github.com/user-attachments/assets/37a165da-947b-4e78-a057-6a2bd85835f2" />

A desktop CV editor that tailors your CV to each job.

Keep every line you could put on a CV in one place, tag the lines, then paste a job advert (or pick tags) and
Smart CV chooses the lines that fit the job best and lays them out on a page you can export as a real PDF.

It runs entirely on your machine: one executable, no account, no server, your data in a local SQLite file.

## Features

- **Line bank per profile**: sections, entries (role, organisation, dates) and lines, with `**bold**` inline
  highlights. Several profiles, switchable from the top bar; create, rename, duplicate, delete.
- **Tags**: tag lines and sections (`Frontend`, `React`, `C#` …), each with the keywords job adverts use.
  The built-in **Always on** tag pins a line or section into every CV.
- **Tailoring**
  - Paste a job description: keyword matching picks the relevant lines, scored by how often their tags appear.
  - Or choose tags in the top bar and drag them into order of importance.
  - Optional local AI scoring through [Ollama](https://ollama.com), with automatic fallback to keywords.
  - Priorities (Lowest … Highest) on sections and lines, max lines per entry and per section.
- **Length**: aim for 1–5 pages, or no limit. Lines are trimmed lowest priority and weakest match first until
  the CV fits.
- **Templates**: three built-ins plus your own. Any installed font, colours, spacing, margins, bullets,
  page size (US Letter / A4). Templates are plain CSS files you can also edit by hand.
- **Edit in place** on the CV preview, drag to reorder, undo / redo for every content change.
- **Import** an existing CV from Word (`.docx`), PDF, `.txt` or `.md` into a new profile.
- **PDF export** that matches the preview exactly, with working links.

## Download

Get the latest build from [Releases](../../releases/latest):

| Platform | File |
|---|---|
| Windows | `SmartCv-<version>-win-x64.exe` |
| macOS (Apple Silicon) | `SmartCv-<version>-osx-arm64.zip` |
| macOS (Intel) | `SmartCv-<version>-osx-x64.zip` |
| Linux | `SmartCv-<version>-linux-x64.tar.gz` |

No .NET install is needed.

- **Windows**: run the exe. SmartScreen may warn about an unknown publisher; choose *More info → Run anyway*.
- **macOS**: unzip and move `SmartCv.app` to Applications. The app isn't signed, so the first time
  right-click it and choose *Open*.
- **Linux**: needs WebKitGTK (installed on most desktops). Extract, then `./SmartCv`.

PDF export uses an installed Edge, Chrome, Chromium or Brave. If none is found, one is downloaded into the app
data folder on the first export.

## Using it

1. On first run a profile called **My CV** is created. Add sections in the right-hand panel, or use
   *Profile menu → Import CV…* to start from an existing CV.
2. Create tags in the tag manager and give them keywords (`frontend` → `react, typescript, css`). Tag your
   lines and sections.
3. Paste a job description at the top of the right panel and press **Tailor** (Ctrl+Enter), or choose tags
   from the **Tags** selector in the top bar.
4. Switch between **All lines** and **Tailored** in the top bar. Each line in the content panel says why it was
   kept or left out; pin a line to force it in.
5. Style the CV from the left panel. Changing a built-in template saves your own copy automatically.
6. **Export PDF** (Ctrl+P).

Hover a side panel to widen it to half the window; use its expand button to fill the window, `Esc` to go back.

### Shortcuts

| Keys | Action |
|---|---|
| Ctrl+Z / Ctrl+Shift+Z or Ctrl+Y | Undo / redo |
| Ctrl+P | Export PDF |
| Ctrl+Enter | Tailor to the job description |
| Enter / Shift+Enter | Save an edit / new line inside a line |
| Esc | Cancel an edit, close a dialog, collapse a full panel |
| Alt+↑ / Alt+↓ | Move the focused item when on its drag handle |

### Ollama

Install Ollama, pull a model (for example `ollama pull gemma3:4b`), then open **Settings** (the gear in the top
bar), choose **Ollama**, press **Check** and pick the model. Each line is scored 0–10 against the job; 5 and
above counts as relevant. Scores are cached, so re-tailoring only sends lines you changed. If Ollama is not
reachable the app falls back to keyword matching and tells you.

### Where your data lives

Everything is in the app data folder:

| OS | Folder |
|---|---|
| Windows | `%LOCALAPPDATA%\SmartCv` |
| macOS | `~/Library/Application Support/SmartCv` |
| Linux | `~/.local/share/SmartCv` |

It holds `smartcv.db` (profiles, lines, tags, settings), `templates/` (CSS template files) and `exports/`.
Back it up by copying the folder. Run with `--data <folder>` to use a different folder, for example a portable
copy on a USB stick.

## Development

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download) and Node.js 22.

```bash
npm ci
dotnet run --project server -- --dev
npm start
```

`--dev` opens the app window against the Vite dev server on port 5173, with the API on 5080. Use
`dotnet run --project server -- --headless` instead to run only the API, and open `http://localhost:5173` in a
browser while `npm start` is running.

Tests (xUnit v3 against temporary SQLite databases):

```bash
dotnet test --solution SmartCv.slnx
```

### Building an executable

```powershell
./build.ps1 -Runtime win-x64
```

```bash
./build.sh osx-arm64
```

Runtimes: `win-x64`, `win-arm64`, `osx-x64`, `osx-arm64`, `linux-x64`, `linux-arm64`. The result is a single
self-contained file in `publish/<runtime>/` (a `.app` bundle on macOS). Pass a version with
`./build.ps1 -Version 1.2.3` or `./build.sh <runtime> 1.2.3`.

## Releases and versions

Versions are `major.minor.patch`. `VERSION` holds `major.minor`; the patch number is counted by the pipeline.

- Every push to `main` runs the tests, builds Windows, Linux and both macOS targets, and publishes a GitHub
  release tagged with the next patch number (`v1.0.0`, `v1.0.1`, …) with notes generated from the commits.
  Pushes that only change Markdown files don't release.
- To start a new minor or major series, change `VERSION` (for example to `1.1`); the next release is `v1.1.0`.
- A release can also be started by hand from the *Release* workflow in the Actions tab.
- Pull requests and other branches run the tests only.

The version appears in the window title and the executable's file properties.

## How it's built

| Part | Technology |
|---|---|
| Window | [Photino.NET](https://www.tryphotino.io) (WebView2 / WKWebView / WebKitGTK) |
| API | ASP.NET Core on .NET 10, in-process on `127.0.0.1` |
| Storage | SQLite with Microsoft.Data.Sqlite and Dapper |
| UI | React 19, TypeScript, Vite, SCSS |
| PDF | PuppeteerSharp printing the CV's print page |
| Import | Word XML read directly, PDF through PdfPig |
| Fonts | SkiaSharp font enumeration |

```
server/
  Controllers/    thin async endpoints, one factory call each
  Factories/      business rules: tailoring, duplication, import, templates
  Repositories/   SQL only, async Dapper
  Services/       Ollama, PDF, fonts, template files, document readers
  Models/
  Host/           window, server setup, app paths
  Templates/      built-in template CSS
src/
  api/  components/  hooks/  styles/
  pages/editor/   top-bar, style-panel, cv-wrapper, content-panel
tests/SmartCv.Tests/
```

The back end follows Controller → Factory → Repository. Controllers await one factory call inside a
try / catch and map its result to an HTTP response. Factories combine repository calls and own the rules.
Repositories hold only SQL and its execution.

All front-end styling is in SCSS; templates are CSS custom properties on `.cv-wrapper`, so React never sets a
style.
