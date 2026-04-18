# AZ-ANNIQUE-WEB — Source Code Map & Development Patterns
## Dieselbrook — Internal Reference

**Date:** 2026-04-18
**Access used:** Subscription Owner → Azure Run-Command (SYSTEM) + direct RDP
**Status:** First source-code-focused forensic pass

> ⚠️ CONFIDENTIAL — Dieselbrook internal.

---

## 1. The question asked

Do we have visibility of all/any currently-in-development source code on AZ-ANNIQUE-WEB? Is there source control? What folder, what source, what component?

## 2. The short answer

**There is essentially no formal source control on this server.** What exists:

- **One Git repository** — `C:\Backoffice\web\Reports\.git` — points to Rod's personal GitHub account (`github.com/rodneyea/Reports`). It has **a single "first commit" from August 2023** and has never been updated since. Effectively abandoned.
- **Rod's "source control" pattern:** timestamped ZIP files in `C:\Backups of sites\annique_live_plugin_backup\` — `Annique.Customization_<date>.zip` — one per deployment. This is the de-facto commit history.

Active development on this box follows one of two patterns depending on what's being changed:

1. **Compiled .NET code (Annique.Customization plugin, AnqIntegrationAPI)** — **source is NOT on this box.** Only compiled `.dll` + `.pdb` binaries are present. Rod builds on a **personal/laptop environment** (possibly containing a Visual Studio project) and drops the compiled DLL into the deployed plugin folder.

2. **Interpreted / markup code (Razor `.cshtml`, CSS, JS, VFP `.prg`/`.ann`, `.json` config files)** — **edited LIVE in the production deploy folders.** VS Code Local History confirms Rod opens production files directly, edits, saves. No staging, no review, no build step.

A fresh NopCommerce 4.60 database (`RodWork`, `.mdf` file named `Annique_D.mdf`) is being **actively written to RIGHT NOW** (log file 102 GB and growing, last modified 11:19 AM today). This is likely a mid-flight migration or major schema change that Rod is working on.

## 3. Evidence base

### 3.1 The one Git repo

```
Location:   C:\Backoffice\web\Reports\.git
Remote:     https://github.com/rodneyea/Reports.git
Branch:     main
Last commit:
  bd06c8313d4ca4f1af90563401a7895dbd349a75
  Rodney Eales <rodney.eales@outlook.com>
  2023-08-31 12:45
  "first commit"
```

Working tree (`C:\Backoffice\web\Reports\`) contains only `js/`, `html/`, `css/` folders, last touched 2025-02-13. A copy also exists at `C:\Temp\Backoffice\Backoffice\web\Reports\.git` from the same era.

**Key takeaway:** Rod knows git exists, created an account, made one commit in 2023, then stopped using it. Everything since has been manual.

### 3.2 VS Code Local History (Rod's actual edit log)

`C:\Users\AmAdmin\AppData\Roaming\Code\User\History\` is VS Code's automatic per-file edit history. Each opened file gets a timestamped version kept. This acts as a complete record of what Rod has been editing.

Recent edits (last 6 months, trimmed):

| Last opened | File | Category |
|---|---|---|
| 2026-03-30 | `C:\Software\LandingPages\RebexTinySftpServer.exe.config` | SFTP server config |
| 2026-03-27 | `C:\inetpub\wwwroot\Staging\Themes\Avenue\Content\css\theme.custom-1.css` | **Live CSS — staging** |
| 2026-03-27 | `C:\inetpub\wwwroot\Annique\Themes\Avenue\Content\css\theme.custom-1.css` | **Live CSS — production** |
| 2026-03-25 | `C:\inetpub\wwwroot\AnqIntegrationAPI\appsettings.json` | Production API config |
| 2026-03-24 | `C:\Apps\ImageSync\appsettings.json` | ImageSync config |
| 2026-03-19 | `C:\inetpub\wwwroot\Annique\wwwroot\Reports\js\ReportCommon.js` | Production JS |
| 2026-02-25 | `C:\Backoffice\web\customerlist.ann` | VFP form file |
| 2026-02-17 | `C:\Apps\BrevoContactSync\appsettings.json` | Brevo config |
| 2026-01-13 | `C:\inetpub\wwwroot\AnqIntegrationAPI\logs\brevo-20260113.log` | Log file viewed |
| 2026-01-13 | `C:\inetpub\wwwroot\AnqIntegrationAPI\web.config` | IIS web.config |
| 2025-12-15 | `C:\inetpub\wwwroot\Annique\wwwroot\Reports\js\review.js` | Production JS |
| 2025-12-15 | `C:\Backoffice\web\campaign.ann` | VFP form |
| 2025-10-29 | `C:\inetpub\wwwroot\Annique\wwwroot\assets\html\BLOCK-1.html` | Production HTML |
| 2025-10-08 | Various CSS/HTML/fonts in Staging + NopIntegration Landing Pages | Mix |

**Every single edit target is a deployed-production file.** There is no separate workspace. Saving the file in VS Code is the deployment.

### 3.3 VS Code recent workspaces

`C:\Users\AmAdmin\AppData\Roaming\Code\User\workspaceStorage\` records which folders have been opened as workspaces. The recent list (newest first):

- `c:\NopIntegration\deploy` — VFP Web Connection app source
- `c:\NopIntegration\web\LandingPages\Registration` — VFP landing pages
- `c:\inetpub\wwwroot\Staging\wwwroot\assets\css` — Staging CSS
- `c:\Backoffice\web` — VFP BackOffice web
- `c:\inetpub\wwwroot\Registrations\web` — Registrations site
- `c:\inetpub\wwwroot\Staging\wwwroot\assets` — Staging assets
- `c:\inetpub\wwwroot\Staging\wwwroot\assets\html` — Staging HTML

All production-deployed folders.

### 3.4 Recently modified `.cshtml` (Razor views — the UI source for NopCommerce plugins)

Most recent, in `C:\inetpub\wwwroot\Staging\Plugins\Annique.Customization\Themes\Avenue\Views\`:

| Date | File |
|---|---|
| 2026-04-09 | `AnniqueEvents\BookingDetails.cshtml` |
| 2026-04-09 | `CustomCheckout\_GiftProductPopUp.cshtml` |
| 2026-04-09 | `Shared\_Header.cshtml` |
| 2026-03-31 | `Shared\Components\OrderSummary\Default.cshtml` |
| 2026-03-14 | `Customer\Info.cshtml` |
| 2026-03-14 | `Shared\Components\UserProfileAdditionalInfo\Default.cshtml` |
| 2026-03-14 | `Shared\Components\WhatsAppOptInPopup\Default.cshtml` |
| 2026-03-14 | `Shared\Components\FlyoutShoppingCart\Default.cshtml` |

The same files exist in `C:\inetpub\wwwroot\Annique\Plugins\Annique.Customization\...` but typically with an older timestamp — Rod edits staging first, then copies to production.

### 3.5 `.csproj` / `.sln` files on the entire box

Full search excluding Windows / Program Files / AppData / node_modules:

**4 `.csproj` files total — all from late 2022 / early 2023:**

| Date | Path |
|---|---|
| 2023-01-14 | `C:\Software\WebApi\WebApi.Frontend\WebApi.Frontend\nopCommerce 4.60\Nop.Plugin.Misc.WebApi.Frontend\Nop.Plugin.Misc.WebApi.Frontend.csproj` |
| 2023-01-14 | `C:\Software\WebApi\WebApi.Backend\WebApi.Backend\nopCommerce 4.60\Nop.Plugin.Misc.WebApi.Backend\Nop.Plugin.Misc.WebApi.Backend.csproj` |
| 2022-12-30 | `C:\Software\WebApi\Nop.Plugin.Misc.WebApi.Framework.csproj` |
| 2022-12-22 | `C:\inetpub\wwwroot\Staging\Plugins\Misc.NopMobileApp\nopCommerce 4.60\Nop.Plugin.Misc.NopMobileApp\Nop.Plugin.Misc.NopMobileApp.csproj` |

**Zero `.sln` files** (excluding VS Code internals and one SSMS template).

**Implication:** no `.NET` Solution files = nothing is being built on this box. The `Annique.Customization` plugin (1.4 MB DLL, rebuilt 2026-04-10) and `AnqIntegrationApi` (.NET 9 IIS app) must both be built on Rod's laptop — the only `.csproj` files ever present are old WebApi / NopMobileApp plugins from NopCommerce's template days.

### 3.6 `.cs` source files on the box

Negligible count — mostly inside compiled packages (Reference source), test tooling, or Windows' own components once we exclude system paths. No real Annique C# source code is present.

### 3.7 The "ZIP as version control" pattern

`C:\Backups of sites\annique_live_plugin_backup\` is Rod's manual VCS:

| Date | Size | File |
|---|---|---|
| 2025-11-12 | 311 KB | `XcellenceIT.ProductRibbons_12_11_2025.zip` |
| 2025-11-04 | 3.5 MB | `Annique.Customization_11_04_2025.zip` |
| 2025-10-16 | 3.5 MB | `Annique.Customization_16_10_2025.zip` |
| 2025-10-09 | 3.5 MB | `Annique.Customization_09_10_2025.zip` |
| 2025-09-04 | 3.5 MB | `Annique.Customization_04_09_2025.zip` |
| 2025-08-20 | 3.4 MB | `Annique.Customization_20_8_2025.zip` |
| 2025-06-06 | 3.4 MB | `Annique.Customization_06_06_2025.zip` |
| 2025-05-12 | 3.3 MB | `Annique.Customization_12-05-2025.zip` |
| 2025-03-24 | 3.3 MB | `Annique.Customization_24_03_2025.zip` |
| 2025-03-12 | 3.3 MB | `Annique.Customization_12_03_2025.zip` |
| 2025-02-12 | 3.8 MB | `Annique.Customization_12_02_2025.zip` |
| 2023-04-05 | 203 KB | `Annique.Customization.zip` |

The 2023-04-05 zip at 203 KB vs the 2025+ zips at 3.3-3.8 MB tells you the plugin has grown ~15× in 2 years. These zips contain the compiled `.dll` + the `Views/Themes/Content/Localization/SqlScript` resource folders — i.e. the plugin deployment bundle, not the C# source.

### 3.8 `C:\Backups of sites\api\` — compiled AnqIntegrationAPI drop

Last modified 2026-02-02 17:38, contains:
- `AnqIntegrationApi.exe`, `.dll`, `.pdb`, `.deps.json`, `.runtimeconfig.json`, `.xml`, `.staticwebassets.endpoints.json`
- `BrevoApiHelpers.exe`, `.dll`, `.pdb`, matching metadata files
- `appsettings.json` (2025-10-21)

This is the **compiled `.NET 9` AnqIntegrationApi build** (confirming the project ships `BrevoApiHelpers` as a separate DLL). No source — just the publish output. A different dated backup of the API.

### 3.9 `RodWork` database — active work happening RIGHT NOW

```
Database:       RodWork
Logical name:   nopCommerce_4.60.0
MDF file:       C:\Software\Annique_D.mdf    (16.3 GB, modified 2026-04-18 01:47)
LDF file:       C:\Software\AnniqueD_log.ldf (102 GB, modified 2026-04-18 11:19 — right now)
Recovery model: FULL
State:          ONLINE
```

A related file `C:\Software\RESTORED-Annique1.mdf` (16.3 GB, 2026-02-03) suggests there was an earlier `RESTORED-Annique1` attempt.

**Interpretation:** Rod has a **second full NopCommerce 4.60 database** restored from production (by file name, and matching the production `Annique` DB size range). The 102 GB transaction log is enormous — meaning a massive set of uncommitted (or recently-committed-but-not-checkpointed) transactions. This is typical of:

- A large data migration / schema upgrade in progress
- Bulk data load
- Large DELETE or UPDATE transactions yet to finish
- A NopCommerce version upgrade being rehearsed

We should ask Rod (via proper channel) what `RodWork` is for, because it's a 16 GB copy of production data being actively manipulated on the live production host. No formal change-control, no backup of the transaction-log-pending state.

## 4. Component-by-component source map

### 4.1 NopCommerce production store (`annique.com`)

| Component | Deployed to | Source? |
|---|---|---|
| NopCommerce 4.60 binaries | `C:\inetpub\wwwroot\Annique\` | Standard NopCommerce — not custom source. Last binary update 2023-11-09. |
| `Annique.Customization` plugin `.dll` | `C:\inetpub\wwwroot\Annique\Plugins\Annique.Customization\` | **NOT on this box.** Built externally. |
| `Annique.Customization` plugin `.cshtml` views | `C:\inetpub\wwwroot\Annique\Plugins\Annique.Customization\Themes\Avenue\Views\` | **This IS the source.** Edited directly. |
| `Annique.Customization` plugin CSS/JS/HTML | Same folder tree | Same — edited live |
| `Annique.Customization` plugin `.sql` scripts | `C:\inetpub\wwwroot\Annique\Plugins\Annique.Customization\SqlScript\` | Edited live |
| `XcellenceIT.ProductRibbons` plugin | `C:\inetpub\wwwroot\Annique\Plugins\XcellenceIT.ProductRibbons\` | 3rd party — source not present |
| Themes / front-end assets | `C:\inetpub\wwwroot\Annique\Themes\Avenue\Content\*` + `wwwroot\assets\*` | Edited live |

### 4.2 NopCommerce staging (`stage.annique.com`)

Same mirror of the above under `C:\inetpub\wwwroot\Staging\`. Plugin DLLs in staging are often newer than production (Rod deploys to staging → tests → promotes to production).

Also includes an extra plugin:
- `Annique.AdumoOnline` (payment plugin) — binary from 2024-01-09, last touched 2024-01-10

### 4.3 `AnqIntegrationAPI` (the `.NET 9` web API)

| Component | Deployed to | Source? |
|---|---|---|
| API binaries (`.dll`, `.exe`, `.deps.json`, `.pdb`) | `C:\inetpub\wwwroot\AnqIntegrationAPI\` | **Binary only.** Last deploy 2026-03-25. |
| `BrevoApiHelpers.dll` | Same folder | Binary only |
| `appsettings.json` | Same folder | **Edited live on server** (VS Code History confirms edits 2026-03-25, 2026-01-13) |
| `web.config` | Same folder | Edited live (2026-01-13) |

Backup copy: `C:\Backups of sites\api\` with 2026-02-02 build.

### 4.4 VFP Web Connection apps (`nopintegration.annique.com`, `backoffice.annique.com`)

| Component | Deployed to | Source? |
|---|---|---|
| `nopintegration.exe` | `C:\NopIntegration\deploy\` | Binary (1.1 MB, built 2026-04-02) |
| `backoffice.exe` | `C:\Backoffice\deploy\` | Binary |
| `.ini` config files | Each `deploy\` folder | Edited live |
| `.api`, `.wc`, `.html`, `.css`, `.js` view files | Each `web\` folder | **THIS IS the source** — edited live in VS Code |
| `.prg` / `.fxp` VFP source files | Not visible in `deploy\` or `web\` on production | **NOT on this box.** VFP source lives elsewhere. |

The VFP `.prg` source code that compiles into `nopintegration.exe` is NOT on the Azure VM. Rod has to edit and compile it somewhere else (presumably in VFP 9 IDE on his laptop, or on AMSERVER-TEST where we saw compile artefacts in an earlier discovery). On the Azure VM we see only `.api` endpoint shells (`helloscript.api`) and compiled `.fxp` intermediate files.

The **`.ann` files** (e.g. `customerlist.ann`, `campaign.ann`) that Rod edits in `C:\Backoffice\web\` ARE source — these are VFP visual form/report definitions. Editable without recompilation.

### 4.5 Scheduled integration tasks

| Component | Deployed to | Source? |
|---|---|---|
| `AnqImageSync.Console.exe` (.NET 10) | `C:\Apps\ImageSync\` | **Binary only.** `appsettings.json` edited live. |
| `BrevoContactsSync.exe` (.NET 9) | `C:\Apps\BrevoContactSync\` | **Binary only.** `appsettings.json` edited live. |

### 4.6 Supporting sites

| Site | Folder | Notes |
|---|---|---|
| `quiz.annique.com` | `C:\NopIntegration\web\LandingPages\Registration\` | HTML/CSS/JS edited live |
| `newregistration.annique.com` | `C:\Registrations\web\` (and `C:\inetpub\wwwroot\Registrations\web\`) | Edited live; currently stopped |
| `backoffice.annique.com` | `C:\Backoffice\web\` | VFP + HTML/JS, edited live |
| `NopBlockPublisher` | `C:\inetpub\wwwroot\NopBlockPublisher\` | Last modified 2025-09-11 |

### 4.7 Historical backup folders

| Path | Content |
|---|---|
| `C:\__BACKUP__\Live\<date>\` | 17 dated folders from 2023-10 through 2024-06 (last updated June 2024). Presumably full plugin-tree backups. |
| `C:\__BACKUP__\Staging\` | 2024-02-28 staging snapshot |
| `C:\Backups of sites\<date-or-name>\` | Dated + named backups going back to 2023-04-05 |
| `C:\Backups of sites\annique_live_plugin_backup\*.zip` | The de-facto version history for the `Annique.Customization` plugin |

## 5. What this means for the project

### 5.1 The missing source code

**The authoritative source for three critical components is NOT on this server:**

1. `Annique.Plugins.Nop.Customization.dll` — the NopCommerce customisation plugin (1.4 MB, continuously rebuilt). The C# source lives on Rod's laptop or in his private workspace.
2. `AnqIntegrationApi` — the `.NET 9` web API + `BrevoApiHelpers` sidecar DLL.
3. `AnqImageSync.Console.exe` — the .NET 10 image sync job.
4. `BrevoContactsSync.exe` — the daily Brevo sync job.
5. VFP `.prg` source for `nopintegration.exe` and `backoffice.exe`.

If Rod's laptop disappears, goes through ransomware, or he stops working with Annique:
- The compiled DLLs keep running but **can never be updated, patched, or recompiled for a newer .NET/NopCommerce**.
- Any attempt to debug or modify customisation behaviour requires decompiling production DLLs — possible but lossy and legally murky.
- The SQL migration work currently in `RodWork` (the 102 GB transaction log) is similarly locked to whatever scripts Rod has locally.

**This is a critical engagement risk** and should be flagged explicitly in the project record. Dieselbrook should not assume source-code continuity; we should require it.

### 5.2 The "edit-in-production" pattern

Rod (logged in as `AmAdmin`) uses VS Code to open production files, edit them, save. The save is the deployment.

Files affected by this pattern on `annique.com`:
- `Themes\Avenue\Content\css\*.css`
- `wwwroot\assets\*.html`, `*.css`, `*.js`
- `Plugins\Annique.Customization\Themes\Avenue\Views\**\*.cshtml` — Razor views served to every live request
- `Plugins\Annique.Customization\Views\**\*.cshtml`

**Every edit is live in the same second it's saved.** There is no testing, no review, no atomic rollback. If Rod typos a Razor syntax error, every request to that view returns a 500 until he fixes it.

### 5.3 The `RodWork` database is a live production-class risk

16 GB of real NopCommerce data, 102 GB of uncommitted transaction log, on the same SQL instance as the production `Annique` database, on the same VM as everything else. If that transaction log fills the C: drive (currently 351 GB free, 102 GB log actively growing), production SQL tempdb, production `Annique` DB, and the entire VM start having write failures.

**Flag to raise:** `RodWork` should not live on the production VM.

### 5.4 Source control approach for DBM

DBM's own code (the middleware we build) should:
- Be in a proper Git repo from day one (Dieselbrook-owned, like the existing repo)
- Deploy via CI/CD with artefacts in a real registry / storage account — never hand-copied DLLs
- Have an explicit build pipeline — no "builds on Rod's laptop" single points of failure
- Staging and production should be different subscriptions / VNets — not different folders on the same VM

We can't force Rod to put existing Annique code in git, but we MUST require that the source for anything DBM consumes (or wraps) is in our repo, in a sanctioned form.

## 6. What we didn't look at (deferred)

- `RodWork` database actual contents — is it a schema migration, a data-load, or something else? Can be answered via a few SQL queries next session.
- Full recursive enumeration of `C:\NopIntegration\web\` — may contain more Razor / HTML / JS we haven't catalogued.
- `C:\Software\LandingPages\` and `C:\Software\WebApi\` — some source-like content per VS Code history.
- Whether Rod's laptop sync is happening via OneDrive (`C:\Users\AmAdmin\OneDrive\`) — could reveal the laptop source location.
- Recent file modification sweep by date across whole drive — would catch any edits we haven't specifically enumerated.
- SSMS recent-file list — might reveal scripts Rod is using (they appear as `.sql` recent files in SSMS registry).

## 7. Evidence capture summary

All via `az vm run-command invoke`:

| Evidence | Source |
|---|---|
| VS Code workspaces | `C:\Users\AmAdmin\AppData\Roaming\Code\User\workspaceStorage\<guid>\workspace.json` |
| VS Code file history | `C:\Users\AmAdmin\AppData\Roaming\Code\User\History\<guid>\entries.json` |
| Git repo config | `C:\Backoffice\web\Reports\.git\config`, `HEAD`, `logs\HEAD` |
| Plugin DLL timestamps | `Get-Item` on `C:\inetpub\wwwroot\*\Plugins\*\*.dll` |
| `.csproj` / `.sln` search | Recursive `Get-ChildItem` on `C:\` excluding system paths |
| `RodWork` DB metadata | `sys.databases` + `sys.master_files` in SQL |
| Backup ZIP history | `Get-ChildItem C:\Backups of sites\annique_live_plugin_backup\` |
