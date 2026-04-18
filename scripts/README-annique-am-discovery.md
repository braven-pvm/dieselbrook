# Annique AM Server Discovery Script — Instructions for Annique IT

## Purpose

This PowerShell script captures a complete read-only inventory of the AM server
(hardware, OS, SQL Server, databases, installed software, network configuration)
into a single text file. Dieselbrook needs this information to plan the AM Azure
migration and the DBM integration, without needing RDP or admin access to the
production server.

## Safety statement — this script is READ-ONLY

The script:

- **Does NOT modify any files**, databases, registry keys, or services
- **Does NOT install anything**
- **Does NOT change any configuration**
- **Does NOT send any data over the network** — it only writes a text file to
  `C:\Temp\` which you can review and forward to Dieselbrook at your discretion
- **Does NOT capture live/production data** — only metadata (schema names, row
  counts, table definitions, config file contents that are already in plaintext
  on disk)

The script only issues `SELECT` statements against SQL Server (no `INSERT`,
`UPDATE`, `DELETE`, `DROP`, `CREATE`, `ALTER`). You can review the entire
`.ps1` file in any text editor before running it.

## What it captures

**Inventory sections (1-11)**

1. Operating system, hardware, disk layout, domain membership
2. Network configuration (IP, gateway, DNS, listening/established ports)
3. Local admins, RDP users, current login sessions
4. Running services and scheduled tasks
5. Installed software (from the Windows uninstall registry)
6. AccountMate install location, version, licence files, config files
7. Top-level folder structure on every drive
8. IIS sites, applications, and bindings (if IIS is present)
9. SQL Server: version, databases, file sizes, linked servers, logins, roles,
   Agent jobs, backup history, schema counts, row counts of key AM tables,
   pricing procedure source (`sp_camp_getSPprice`, `vsp_ic_getcontractprice`),
   HTTP helper source (`sp_ws_HTTP`)

   Also (for staging/migration planning):
   - SQL Agent job **steps** (the actual commands that run, not just job names)
   - SQL Agent alerts + operators + Database Mail profiles/accounts (SMTP relays)
   - SQL CLR assemblies (custom .NET inside SQL)
   - All DDL and DML **triggers** in `amanniquelive`
   - Every proc/function/trigger that makes **external calls** (HTTP, OLE
     automation, xp_cmdshell, OPENROWSET, OPENQUERY, linked servers,
     sp_send_dbmail, BULK INSERT)
   - Service Broker state per database + queues/services/routes
   - Currently connected SQL sessions (who is talking to AM right now)
   - Distinct login/host/program combinations (what apps connect to AM)
   - Plan-cache queries referencing linked servers
10. Common application config files that contain connection string metadata
11. ODBC DSN inventory

**Integration-graph sections (12-20)**

12. **Hosts file** (DNS overrides — what does `AMSERVER-v9` actually resolve to?)
    Plus `nslookup` on every known Annique hostname
13. **SMB shares** (file-drop integration surface)
14. **Windows Firewall rules** — every inbound rule (not just profile state)
15. **COM+ registered applications** (NISource pattern — what else is registered?)
16. **WMI event subscriptions** + legacy at-jobs (silent scheduled integrations)
17. **Full process list with command lines** (reveals background services,
    Python/Node/curl invocations, file watchers)
18. **Netstat snapshots sampled 3 times over 30 seconds** (catches
    periodic outbound connections — backups, syncs, pings)
19. **IIS log volume summary** (is IIS actually serving traffic on prod, or
    dormant?)
20. **Integration file-drop folders** (EFT, imports, exports, inbox/outbox)

## A note on sensitive data

The application config files in section 10 (e.g. `C:\CompPLan\Compplan.ini`)
may contain plaintext SQL passwords. These passwords are already stored on disk
in cleartext — this script does not expose anything that is not already
accessible to anyone with local access to the server. However, before sending
the output file to Dieselbrook, please **open it in a text editor and review
section 10**. If you want to redact specific passwords, do so before sending.

We (Dieselbrook) already know the historic `sa` password `AnniQu3S@` from the
VFP application source code, so the contents of these files are unlikely to
reveal anything new — but the review step is still important for your own
audit trail.

## How to run

1. **Copy the file** `annique-am-discovery.ps1` to the AM server. Any method
   works — RDP copy-paste, email attachment, USB drive.

2. **Open PowerShell as Administrator**: click Start, type `powershell`,
   right-click "Windows PowerShell", choose **Run as administrator**, click
   Yes on the UAC prompt.

3. **Navigate to the folder** containing the file:
   ```powershell
   cd C:\Path\To\Folder
   ```

4. **Run the script** (the `-ExecutionPolicy Bypass` flag avoids the
   "scripts are disabled" error):
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\annique-am-discovery.ps1
   ```

5. **Wait for completion** (typically 2-5 minutes). The script will print
   progress to the PowerShell window as it runs each section.

6. **Locate the output file**. When the script finishes, it prints the full
   path, which will be something like:
   ```
   C:\Temp\Annique_AM_Discovery_20260420_143022.txt
   ```

7. **Review the output file** in Notepad. Check section 10 for any config
   files with passwords you'd prefer to redact. **Note:** if you edit the
   file, the integrity check below will flag the edit. This is by design —
   Dieselbrook is not trying to stop you from redacting, we just want to know
   that something was changed so we can ask what was redacted and why.

8. **Send the files to Dieselbrook**. The script produces two files:
   - `Annique_AM_Discovery_<timestamp>.txt` — the main report
   - `Annique_AM_Discovery_<timestamp>.txt.sha256` — a small text file with
     the tamper-evidence hash

   Please send **both** files:
   - Email to: `developer@pvm.co.za`
   - Or: attach to the Linear issue ANN-24, or to the shared Notion page
   - Or: any other channel that works for Annique

   **Also helpful:** when the script finishes, it prints a SHA-256 hash to the
   PowerShell window in a highlighted block. Copy that hash into the body of
   your email (even though it's also inside the files). This gives us a
   third, independent channel to confirm the file is the one you actually
   generated.

## Tamper-evidence (the SHA-256 hash)

The script computes a SHA-256 hash over the report content and records it in
three places:

1. As a **signature block at the end of the `.txt` file itself**, following a
   clearly marked line (`===== DIESELBROOK TAMPER-EVIDENCE SIGNATURE ... =====`).
   The block includes the exact rule for recomputing the hash.
2. In a **sidecar file** named `Annique_AM_Discovery_<timestamp>.txt.sha256`.
3. **Printed to the PowerShell console** when the script finishes.

Dieselbrook will verify the file on receipt. If the hash in the signature
block matches the hash recomputed from the file content, the report is
guaranteed to be exactly what your server produced. If they disagree, the
file was modified after generation — in which case we'll come back and ask
what changed and why, and we may ask you to re-run the script.

**This is transparency, not a restriction.** You are always free to redact
information you cannot share; the script does not try to prevent that. We
just want an audit trail.

## Expected output

- **Size:** 200–500 KB text file
- **Format:** plain UTF-8 text, readable in Notepad
- **Run time:** 2–5 minutes on a production-class server

## Troubleshooting

### "Scripts are disabled on this system"

Use the `-ExecutionPolicy Bypass` flag as shown in step 4 above. This bypasses
the policy for this one execution only and does not persist.

### "sqlcmd: command not found" or SQL section is empty

This can happen if:
- The account running the script is not a local admin
- The account running the script does not have SQL sysadmin rights
- `sqlcmd` is installed in a non-standard location

If the SQL section is empty, please email Dieselbrook and we'll send a tweaked
version that uses the specific SQL instance name on your server.

### "Access denied" on scheduled tasks or registry

Make sure PowerShell was opened **as Administrator** (right-click → Run as
administrator). Standard user sessions cannot read all scheduled tasks.

### The script seems to hang

Some sections (installed software, scheduled tasks) can take 30–60 seconds on
older servers. Leave it running for at least 5 minutes before killing it. If
it genuinely hangs, press Ctrl+C and let Dieselbrook know which section was
running.

## Questions?

Contact Dieselbrook at `developer@pvm.co.za`.
