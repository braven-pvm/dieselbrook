# Source Code Gap Analysis — Complete Inventory
## Dieselbrook — Internal Reference

**Date:** 2026-04-18
**Premise:** If our local copy (in the dieselbrook repo) of any Annique-delivered source disappears, what can we recover? And for anything compiled, where is the authoritative source?
**Scope:** Every application, plugin, script, config file, and piece of runnable code observed on AZ-ANNIQUE-WEB (doc 32/33/34) and AMSERVER-TEST (doc 31).

> ⚠️ CONFIDENTIAL — Dieselbrook internal.

---

## 1. Classification framework

Every piece of executable or configurable content falls into one of three categories:

| Category | Description | Source-of-truth location |
|---|---|---|
| **A. Interpreted / served** | Razor views, CSS, JS, HTML, JSON config, `.ini`, `.ann` VFP forms, SQL scripts. The server reads the file at request time. | **The deployed file IS the source.** Copy the folder = have the source. |
| **B. Compiled** | `.dll`, `.exe`. The server executes a binary; the source (`.cs`, `.prg`) lives elsewhere. | **Source must come from somewhere else** (repo, laptop, vendor, open-source project). |
| **C. Data / state** | SQL stored procs / functions / triggers, SQL Agent jobs, IIS config, NSG rules, scheduled tasks, Solr index data. | The SQL instance / IIS / Azure config IS the source — but can be scripted out. |

**If Rod vanishes with his laptop**, Category A survives entirely (copy Azure VM folders). Category C survives entirely (dump SQL / export configs). **Only Category B is at risk** — and only to the extent we don't already have the source.

---

## 2. Complete component matrix

### 2.1 NopCommerce core + stock plugins

| Component | Path | Category | Source | Risk |
|---|---|---|---|---|
| NopCommerce 4.60 binaries | `C:\inetpub\wwwroot\Annique\*.dll` | B | **nopcommerce.com / github.com/nopSolutions/nopCommerce** — version 4.60 tag | ✅ LOW |
| `Payments.Manual`, `Payments.CheckMoneyOrder`, `Payments.CashOnDelivery`, `Payments.CyberSource`, `Payments.PayPalCommerce` | `Plugins\Payments.*\` | B | NopCommerce open source | ✅ LOW |
| `DiscountRules.CustomerRoles`, `ExchangeRate.EcbExchange` | `Plugins\*` | B | NopCommerce open source | ✅ LOW |
| `ExternalAuth.Facebook`, `MultiFactorAuth.GoogleAuthenticator` | `Plugins\*` | B | NopCommerce open source | ✅ LOW |
| `Feed.GoogleShopping`, `Misc.Zettle` | `Plugins\*` | B | NopCommerce open source | ✅ LOW |
| `Pickup.PickupInStore` | `Plugins\*` | B | NopCommerce open source | ✅ LOW |
| `Shipping.FixedByWeightByTotal`, `Shipping.UPS` | `Plugins\*` | B | NopCommerce open source | ✅ LOW |
| `Tax.Avalara`, `Tax.FixedOrByCountryStateZip` | `Plugins\*` | B | NopCommerce open source | ✅ LOW |
| `Widgets.AccessiBe`, `Widgets.FacebookPixel`, `Widgets.GoogleAnalytics`, `Widgets.NivoSlider`, `Widgets.What3words` | `Plugins\Widgets.*\` | B | NopCommerce open source | ✅ LOW |
| `Misc.Sendinblue` | `Plugins\Misc.Sendinblue\` | B | Brevo/Sendinblue on GitHub (official NopCommerce plugin) | ✅ LOW |

**Recovery for the whole stock-NopCommerce set:** `git clone https://github.com/nopSolutions/nopCommerce --branch release-4.60` and rebuild.

### 2.2 Third-party commercial plugins (licensed vendors)

| Component | Path | Category | Source | Risk |
|---|---|---|---|---|
| `SevenSpikes.Core` + 15 SevenSpikes plugins (AjaxCart, AjaxFilters, AnywhereSliders, CloudZoom, InstantSearch, JCarousel, MegaMenu, NewsletterPopup, NopQuickTabs, PrevNextProduct, ProductRibbons, QuickView, RichBlog, SaleOfTheDay, SmartProductCollections, SocialFeed) | `Plugins\SevenSpikes.*\` | B | SevenSpikes — commercial, **re-downloadable via Annique's licence portal** | 🟡 MEDIUM — requires active licence |
| **`SevenSpikes.Theme.Avenue`** | `Plugins\SevenSpikes.Theme.Avenue\` | B | SevenSpikes — commercial theme | 🟡 MEDIUM — **without this the site UI breaks** |
| `FoxNetSoft.BuyXGetY`, `FoxNetSoft.CMSManager`, `FoxNetSoft.IPFilter`, `FoxNetSoft.TrackingCodeManager` | `Plugins\FoxNetSoft.*\` | B | foxnetsoft.com — commercial | 🟡 MEDIUM — requires licence |
| `NopStation.Core`, `NopStation.Plugin.Misc.WidgetManager`, `NopStation.Plugin.Widgets.Announcement`, `NopStation.Plugin.Widgets.Usermaven` | `Plugins\NopStation.*\` | B | nopstation.com — commercial | 🟡 MEDIUM |
| `NopTech.Core`, `NopTech.GoogleAnalytics` | `Plugins\NopTech.*\` | B | NopTech — commercial | 🟡 MEDIUM |
| `XcellenceIT.ProductRibbons` | `Plugins\XcellenceIT.ProductRibbons\` | B | XcellenceIT — commercial | 🟡 MEDIUM |
| `Widgets.Chatbot`, `Widgets.UserNotify` | `Plugins\Widgets.*\` | B | 3rd party — specific vendor TBC | 🟡 MEDIUM |
| `Annique.Plugins.Payments.AdumoOnline` | `Staging\Plugins\Annique.AdumoOnline\` | B | Custom-built by Rod OR Adumo SDK-based — **not confirmed** | 🟡 MEDIUM — needs investigation |
| `nopAccelerate-Plus-Pro-0.13.0.zip` | `C:\Software\` | B | nopAccelerate.com — commercial (zip kept) | ✅ LOW — ZIP is on disk |
| `NopHelpDesk_4.6.323.35604.zip` | `C:\Software\` | B | 3rd party — ZIP on disk | ✅ LOW |
| `foxnetsoft.specialoffers.zip`, `foxnetsoft.buyxgety.zip` | `C:\Software\` | B | FoxNetSoft — ZIPs on disk | ✅ LOW |

**Recovery:** require Annique to re-share vendor licences + re-download binaries. Not strictly "source" — most commercial plugins don't ship source, only DLLs. Vendor relationship stays intact.

### 2.3 Annique custom .NET components (the critical set)

| Component | Path on AZ-ANNIQUE-WEB | Category | Source — where | Risk |
|---|---|---|---|---|
| **`Annique.Plugins.Nop.Customization.dll`** (1.44 MB, deployed 2026-02-25 to prod, 2026-04-10 to staging) | `Plugins\Annique.Customization\` | B | **Only Rod's laptop.** No source on either server. ZIPs of compiled output in `C:\Backups of sites\annique_live_plugin_backup\` are NOT source. | 🔴 **HIGH** |
| **`AnqIntegrationApi.dll`** (.NET 9, deployed to `/API`) | `C:\inetpub\wwwroot\AnqIntegrationAPI\` | B | **In our repo at `AnqIntegrationApiSource/`** (119 `.cs`, `.csproj`, `.sln`). Origin: Rod → Marius, committed to dieselbrook 2026-03-05. | 🟡 MEDIUM — local copy is the only one we know of |
| **`BrevoApiHelpers.dll`** (side-car DLL to AnqIntegrationApi) | Same folder | B | **In our repo at `AnqIntegrationApiSource/BrevoApiHelpers/`** (own `.sln` + `.csproj`) | 🟡 MEDIUM — as above |
| **`AnqImageSync.Console.exe`** (.NET 10, runs every 10 min) | `C:\Apps\ImageSync\` | B | **Only Rod's laptop.** Not in our repo, not on server. | 🔴 **HIGH** |
| **`BrevoContactsSync.exe`** (.NET 9, daily) | `C:\Apps\BrevoContactSync\` | B | **Only Rod's laptop.** Not in our repo, not on server. | 🔴 **HIGH** |
| **`Misc.WebApi.Backend/Frontend/Framework`** plugins (older NopCommerce mobile-app API) | `Plugins\Misc.WebApi.*\`, also sources in `C:\Software\WebApi\` | B | `.csproj` files are on AZ-ANNIQUE-WEB (`C:\Software\WebApi\`), dated 2022-12 → 2023-01 | ✅ LOW — partial source on server (old version) |
| `Annique.Plugins.Payments.AdumoOnline.dll` (staging only, 56 KB) | `Staging\Plugins\Annique.AdumoOnline\` | B | Unknown — likely Rod's laptop | 🔴 HIGH — but only on staging, payment not currently live |

**Critical observations:**
- **Three custom .NET components have their ONLY source on Rod's laptop**: `Annique.Customization`, `AnqImageSync`, `BrevoContactsSync`. If Rod's laptop is lost or he becomes unavailable, those three cannot be recompiled.
- **`AnqIntegrationApi` + `BrevoApiHelpers`** are already in our repo — this is the ONE Annique custom component we could rebuild today.
- `AnqIntegrationApi` was delivered to Marius by Rod before 2026-03-05 (initial commit date). Method of delivery not documented.

### 2.4 VFP Web Connection applications

| Component | Path on AZ-ANNIQUE-WEB | Category | Source — where | Risk |
|---|---|---|---|---|
| **`nopintegration.exe`** (1.1 MB, built 2026-04-02, serves `nopintegration.annique.com`) | `C:\NopIntegration\deploy\` | B | **VFP `.prg` source NOT on Azure VM.** Possibly on AMSERVER-TEST at `C:\wconnect7\` or `C:\webconnectionprojects\webstore\` (doc 31 saw these) — **need to verify specifically for nopintegration sources**. Otherwise: Rod's laptop (VFP 9 IDE was present on AMSERVER-TEST). | 🔴 HIGH |
| **`backoffice.exe`** (serves `backoffice.annique.com`) | `C:\Backoffice\deploy\` | B | Same story as nopintegration — `.prg` source off-box | 🔴 HIGH |
| `runasyncrequest.exe` | `C:\NopIntegration\deploy\` | B | Likely part of the same VFP project bundle | 🔴 HIGH |
| `compruns.exe` (MLM compensation calculator, 2.3 MB) | `C:\CompPLan\` on **AMSERVER-TEST** (not on Azure VM) | B | Source off-box — Rod's laptop | 🔴 HIGH — this is the MLM engine |
| `email2sms.exe` (email-to-SMS gateway) | `C:\Email2SMS\` on **AMSERVER-TEST** | B | **`.prg` source IS on AMSERVER-TEST** (we saw `pop.prg`, `smsclass.prg`, `email2sms.PJT/.pjx` in doc 31) | 🟡 MEDIUM — source on AMSERVER-TEST |
| `.ann` VFP form/report files (`customerlist.ann`, `campaign.ann`, ~20 others) | `C:\Backoffice\web\` | A | **Server IS source** | ✅ LOW |
| `.api`, `.wc`, `.wcs`, `.html` view files | `C:\NopIntegration\web\`, `C:\Backoffice\web\` | A | **Server IS source** | ✅ LOW |

### 2.5 Plugin markup and assets (Category A — server IS source)

| Component | Location | Risk |
|---|---|---|
| `Annique.Customization\Themes\Avenue\Views\**\*.cshtml` (Razor views — UI templates for the custom plugin) | Both `Annique\Plugins\` (prod) and `Staging\Plugins\` — edited live by Rod | ✅ LOW — server IS source |
| `Annique.Customization\Views\**\*.cshtml` (plugin admin views) | Same | ✅ LOW |
| `Annique.Customization\Content\css\*.css` (plugin CSS) | Same | ✅ LOW |
| `Annique.Customization\Localization\*.xml` (translation files) | Same | ✅ LOW |
| **`Annique.Customization\SqlScript\*.sql`** (plugin migration scripts) | Same | ✅ LOW — **critical for install/upgrade, but on server** |
| `plugin.json` manifests | Each plugin folder | ✅ LOW |
| NopCommerce themes (`Themes\Avenue\Content\css\theme.custom-1.css`, etc.) | `wwwroot\Annique\Themes\`, `wwwroot\Staging\Themes\` — edited live | ✅ LOW |
| Static assets (`wwwroot\assets\*.html`, `*.css`, `*.js`, `wwwroot\Reports\js\*`) | Server | ✅ LOW |

### 2.6 App config files (Category A — server IS source)

| File | Purpose | Risk |
|---|---|---|
| `C:\inetpub\wwwroot\AnqIntegrationAPI\appsettings.json` | AnqIntegrationApi runtime config (DB connstrings, API keys, JWT secret, Brevo config) | ✅ LOW |
| `C:\inetpub\wwwroot\AnqIntegrationAPI\web.config` | IIS config | ✅ LOW |
| `C:\Apps\ImageSync\appsettings.json` | ImageSync config (plaintext `sa` creds for AM + NopCommerce) | ✅ LOW |
| `C:\Apps\BrevoContactSync\appsettings.json` | Brevo sync config | ✅ LOW |
| `C:\NopIntegration\deploy\Nopintegration.ini` | VFP NopIntegration config (4 SQL connection strings) | ✅ LOW |
| `C:\Backoffice\deploy\Backoffice.ini` | VFP BackOffice config | ✅ LOW |
| NopCommerce `App_Data\appsettings.json` | NopCommerce production config | ✅ LOW |
| `C:\Users\AmAdmin\Desktop\hosts` (staging override file Rod edits) | Deployment utility | ✅ LOW |

### 2.7 Infrastructure & data (Category C)

| Item | Location | Recovery |
|---|---|---|
| **SQL stored procs** (all 72 in `Annique` DB + 62 ANQ_* procs + equivalent in other DBs) | `sys.sql_modules.definition` in each database | Script out any time via `GENERATE SCRIPTS` or `OBJECT_DEFINITION()` — have already extracted the key ones (pricing oracle, HTTP helper, sync procs) into docs. |
| **SQL Agent jobs** (21 jobs — the integration engine) | `msdb.dbo.sysjobs` + `sysjobsteps` + `sysjobschedules` | Script out via SSMS or T-SQL |
| **SQL triggers** (5 on Annique DB + others) | Per-DB `sys.triggers` | Scriptable |
| **Scheduled Tasks** (Brevo Contact Sync, Sync Images to Nop, win-acme) | Windows Task Scheduler | Export via `schtasks /query /xml` |
| **IIS configuration** (sites, bindings, app pools) | `C:\Windows\System32\inetsrv\config\applicationHost.config` | Copy the XML file |
| **Azure NSG rules** | Azure control plane | Export via `az network nsg rule list` — already done in doc 32 |
| **Apache Solr configsets + schemas** | `C:\solr-8.11.0\server\solr\*` | Copy folder — index data is rebuildable from NopCommerce SQL |
| **Rebex SFTP Server config** | `C:\Software\LandingPages\RebexTinySftpServer.exe.config` | Copy file |
| **Windows Firewall rules** | Registry + NSG | `netsh advfirewall export` |
| **Veeam backup policy** | Veeam config | External tool; vendor-managed |
| **Let's Encrypt cert + win-acme state** | `C:\Software\win-acme.*\` | Copy folder; certs re-issuable anyway |
| **hosts file** (`127.0.0.1 annique.com` overrides) | `C:\Windows\System32\drivers\etc\hosts` | Copy file |

### 2.8 Tools and runtimes (supporting infrastructure)

| Item | Location | Source |
|---|---|---|
| Visual Studio Code | `C:\Users\<user>\AppData\Local\Programs\Microsoft VS Code\` | Re-downloadable from microsoft.com |
| .NET runtimes (9.0.7, 9.0.11) | Installed | Re-downloadable from dotnet.microsoft.com |
| Java 8 (Solr dependency) | Installed | Re-downloadable |
| Apache Solr 8.11.0 | `C:\solr-8.11.0\` | Re-downloadable from solr.apache.org |
| SQL Server 2019 Standard | Installed | `SW_DVD9_NTRL_SQL_Svr_Standard_Edtn_2019Dec2019_64Bit_English_OEM_VL_X22-22109.ISO` on disk |
| Rebex TinySftpServer | `C:\Software\RebexTinySftpServer-Binaries-Latest\` | Re-downloadable |
| New Relic, Datto RMM, Veeam, ESET | Installed | Vendor-managed |

---

## 3. Provenance trace for what we have locally

### 3.1 `AnqIntegrationApiSource/` in dieselbrook repo

- **Committed:** 2026-03-05 by Marius Bloemhof, commit `7ba14cf` ("Initial commit")
- **Delivery method:** Rod → Marius, method not documented (likely email or RDP copy). Almost certainly a zipped copy of the solution folder.
- **Completeness check passed:**
  - 119 `.cs` files
  - `AnqIntegrationApi.csproj` + `AnqIntegrationApi.sln`
  - `BrevoApiHelpers/BrevoApiHelpers.csproj` + `BrevoApiHelpers.sln`
  - Controllers: Auth, Brevo, BrevoTest, Health, JwtTest, Me, OutboxDebug, ProductReviews, RoleTest, Sync, Upload, WhatsappOptIn — 12 controllers
  - Services/, Models/, Middleware/, DbContexts/ all populated
- **Currency check:** unknown whether this matches the currently-deployed `AnqIntegrationApi.dll` (last deployed 2026-03-25 — 20 days after our commit date). Rod likely shipped more work to production since.

**Action:** compare our `AnqIntegrationApiSource/` build output hash with the currently-deployed `AnqIntegrationApi.dll`. If they don't match, we need an updated copy.

### 3.2 Nothing else we've been given

Every other custom Annique component (the three high-risk ones in section 2.3) has never been delivered to Dieselbrook. We have zero source for them.

---

## 4. The gap picture summarised

**Categorised risks:**

- 🔴 **HIGH risk (Rod's laptop is the only source):**
  - `Annique.Plugins.Nop.Customization.dll` — the main NopCommerce customisation, actively developed
  - `AnqImageSync.Console.exe` — .NET 10, critical 10-minute sync
  - `BrevoContactsSync.exe` — daily Brevo sync
  - `nopintegration.exe` VFP source
  - `backoffice.exe` VFP source
  - `runasyncrequest.exe`
  - `compruns.exe` (MLM compensation)
  - `Annique.Plugins.Payments.AdumoOnline.dll` (staging — low operational impact for now)

- 🟡 **MEDIUM risk:**
  - `AnqIntegrationApi` + `BrevoApiHelpers` — we have ONE copy in our repo, may be out of date
  - All commercial 3rd party plugins — recoverable via vendor licences if Annique keeps them current
  - `email2sms.exe` — source on AMSERVER-TEST (accessible but single location)

- ✅ **LOW risk:**
  - NopCommerce core and all stock plugins (open source)
  - Everything in Category A (markup, assets, config) — server is source
  - Everything in Category C (SQL, IIS, infra config) — dumpable
  - 3rd party ZIPs already on disk in `C:\Software\`

**Aggregate picture:** ~80-90% of the code that makes Annique run is Category A or Category C — recoverable from the servers themselves. The risk is concentrated in **5-6 specific .NET/VFP binaries whose source only exists on Rod's laptop.**

---

## 5. Action items

### 5.1 For Dieselbrook (immediate)

1. **Compare `AnqIntegrationApiSource/` build output to production `AnqIntegrationApi.dll`** to determine if Rod has shipped updates since March 5. If so, request an updated delivery.
2. **Mirror Category A + Category C content to a Dieselbrook-controlled location** for backup:
   - `rsync` / `robocopy` the contents of `C:\inetpub\wwwroot\Annique\`, `Staging\`, `Registrations\`, `NopIntegration\`, `Backoffice\`, and `C:\Apps\` to our Azure storage
   - Dump every SQL stored proc, Agent job, trigger to scripted SQL files in our repo
3. **Stop assuming AMSERVER-TEST has VFP source** — verify specifically which `.prg` files are on AMSERVER-TEST for which VFP apps. We saw `pop.prg` / `smsclass.prg` for email2sms; we need to find the rest.

### 5.2 For Annique (via Marcel / Rod — to include in next access conversation)

**Critical source hand-over request:**

> To protect against single-point-of-failure loss of the Annique custom software, please provide Dieselbrook with the current source code for:
>
> 1. `Annique.Plugins.Nop.Customization` — the NopCommerce customisation plugin — the full Visual Studio solution
> 2. `AnqImageSync.Console` — the .NET 10 image-sync app
> 3. `BrevoContactsSync` — the daily Brevo sync
> 4. The VFP source (`.prg`, `.scx`, `.vcx`, `.pjx`) for `nopintegration.exe` and `backoffice.exe`
> 5. The VFP source for `compruns.exe` (the MLM compensation calculator)
> 6. `Annique.Plugins.Payments.AdumoOnline` if it is custom-built
>
> Please also confirm whether the `AnqIntegrationApi` source delivered to Marius on 2026-03-05 is still current, or provide an updated copy.
>
> We'll commit all received source to Dieselbrook's controlled Git repo; this protects Annique from the single-laptop-loss risk and gives Dieselbrook the artefacts needed to build the migration plan.

### 5.3 For the DBM project (build-time)

- All DBM-owned code lives in a proper Git repo (dieselbrook) from day one.
- DBM's CI/CD builds artefacts in Azure Container Registry / storage account — never hand-copied DLLs.
- Everything DBM consumes from Annique is wrapped in a typed interface so that when Rod's source eventually gets handed over, we can swap implementations without changing callers.

---

## 6. Open questions (to close with more investigation)

1. **Is the `AnqIntegrationApiSource/` in our repo in sync with production?** Build the solution, compare DLL hash with `C:\inetpub\wwwroot\AnqIntegrationAPI\AnqIntegrationApi.dll` on the server. If diff, request update.
2. **Where specifically is the VFP source for `nopintegration.exe` / `backoffice.exe`?** Likely on AMSERVER-TEST under `C:\wconnect7\` or `C:\webconnectionprojects\webstore\` (per doc 31), OR on Rod's laptop. Needs targeted re-enumeration of AMSERVER-TEST once SSH access is re-established.
3. **What's inside `C:\Software\WebApi\`?** That has 3 `.csproj` files from 2022-2023. It's old but may be a starter template we can learn from. Worth enumerating.
4. **What is `Annique.Plugins.Payments.AdumoOnline`?** Custom Rod code, or based on an Adumo SDK? Only 56 KB — tiny.
5. **The `RodWork` database** (16 GB data, 102 GB log being actively written) — is this a NopCommerce 4.x → 4.y upgrade in progress? A migration to DBM-compatible schema? Rod is the only person who knows. We can inspect the schema and compare to `Annique` DB to infer the diff.
