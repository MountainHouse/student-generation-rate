# Deployment

The app can run as a hosted ASP.NET application or as a static Blazor WebAssembly application.

## Hosted ASP.NET

`src/SchoolGrowth.Web` serves the Blazor app and exposes API endpoints:

```text
GET  /api/summary
POST /api/simulate
GET  /api/simulation/status
POST /api/simulation/validate
POST /api/simulation/search
```

The hosted path is useful during development because Household Simulation validation/search can run on the server, where multithreading is much faster and simpler.

For normal local hosted development, prefer `dotnet watch`:

```powershell
dotnet watch --project src\SchoolGrowth.Web run --urls http://localhost:5003
```

`dotnet watch` owns the rebuild/run loop, which avoids the common problem where a separately running web process locks build output. Some changes still require restart or browser refresh. If watch mode causes confusing behavior, stop it and use explicit `dotnet build` / `dotnet run` checks, then document what went wrong so the team can adjust this guidance.

`SchoolGrowth.Web.csproj` explicitly includes the Blazor project tree and `data/` as watch inputs. That is intentional: the hosted app serves generated Blazor/static output, so client or data edits should trigger the hosted build loop. If a Blazor edit is still not reflected, stop watch mode and run a clean explicit build before debugging the app behavior.

The ASP.NET host sends cross-origin isolation headers by default:

```text
Cross-Origin-Opener-Policy: same-origin
Cross-Origin-Embedder-Policy: require-corp
Cross-Origin-Resource-Policy: same-origin
```

These headers are required for WebAssembly threads. They can be disabled with configuration:

```text
SchoolGrowth:EnableCrossOriginIsolationHeaders=false
```

## Static Blazor WebAssembly

The Blazor app can run without a backend. It loads CSVs from:

```text
wwwroot/data/
```

Those files are linked from the repository `data/` folder by `SchoolGrowth.Blazor.csproj` into build/publish output. The hosted ASP.NET app serves `/data` from its generated output `wwwroot/data/` folder, not from source `src/SchoolGrowth.Blazor/wwwroot/data/`.

In static deployments, Household Simulation must run in browser mode. The UI `Auto` execution mode chooses browser mode for non-local/static hosts.

## AOT and Threads

AOT greatly improves browser simulation performance.

Threaded WebAssembly can improve larger simulation runs, but it requires the page to be cross-origin isolated.

During normal development, use quick non-AOT builds. Reserve AOT/threaded publishes for browser performance testing, static deployment validation, or AOT/threading-specific fixes.

One-command static threaded AOT publish:

```powershell
.\scripts\publish-static-aot-threads.ps1
```

If packages and workloads are already restored locally, skip restore:

```powershell
.\scripts\publish-static-aot-threads.ps1 -NoRestore
```

The script publishes the Blazor WebAssembly app, copies the deployable static files to:

```text
artifacts/static-aot-threads/
```

It also writes:

- `_headers` for hosts that support COOP/COEP/CORP header files, such as Cloudflare Pages and Netlify.
- `.nojekyll` for GitHub Pages.

For GitHub Pages project sites, pass the project path as the base href:

```powershell
.\scripts\publish-static-aot-threads.ps1 -BaseHref /repository-name/
```

Publish a threaded AOT client:

```powershell
dotnet publish src\SchoolGrowth.Blazor\SchoolGrowth.Blazor.csproj -c Release -p:RunAOTCompilation=true -p:WasmEnableThreads=true -o artifacts\blazor-aot-threads
```

Publish a hosted threaded AOT app:

```powershell
dotnet publish src\SchoolGrowth.Web\SchoolGrowth.Web.csproj -c Release -p:RunAOTCompilation=true -p:WasmEnableThreads=true -o artifacts\web-aot-threads
```

If the hosted publish serves an unresolved Blazor fingerprint placeholder, copy the processed Blazor `wwwroot` into the hosted artifact:

```powershell
Copy-Item -Path artifacts\blazor-aot-threads\wwwroot\* -Destination artifacts\web-aot-threads\wwwroot -Recurse -Force
```

This is a known follow-up task; see `tasks.md`.

## Static Host Options

Recommended for threaded static deployment:

- Cloudflare Pages
- Netlify
- Vercel
- Firebase Hosting
- Azure Static Web Apps

These hosts can be configured to send COOP/COEP/CORP headers.

Example `_headers` file for Cloudflare Pages or Netlify:

```text
/*
  Cross-Origin-Opener-Policy: same-origin
  Cross-Origin-Embedder-Policy: require-corp
  Cross-Origin-Resource-Policy: same-origin
```

## GitHub Pages

Plain GitHub Pages cannot directly configure the required cross-origin isolation headers.

The project includes `wwwroot/coi-serviceworker.js` as a fallback. It registers a service worker, reloads the page through that service worker, and adds the required headers to same-origin responses.

Important behavior:

- The script must load before Blazor.
- Blazor startup is guarded until cross-origin isolation is ready.
- Normal refresh and Ctrl+refresh were tested locally.
- Real host headers are still preferred when available.

If GitHub Pages uses a project subpath, make sure `base href` is correct for the published path.

## GitHub Actions Deployment

The repository publishes the static threaded AOT app to GitHub Pages from `.github/workflows/deploy-pages.yml`.

Current behavior:

- runs on every `master` commit
- can also be run manually with `workflow_dispatch`
- restores the .NET SDK from `global.json`
- restores WebAssembly workloads and NuGet packages
- runs `scripts/publish-static-aot-threads.ps1 -BaseHref /student-generation-rate/ -NoRestore`
- uploads `artifacts/static-aot-threads/` as the GitHub Pages artifact

The expected project-site URL is:

```text
https://mountainhouse.github.io/student-generation-rate/
```

GitHub Pages must be configured to use **GitHub Actions** as its build and deployment source.
