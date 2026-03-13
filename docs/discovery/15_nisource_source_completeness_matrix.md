# NISource Source Completeness Matrix

## Purpose

This document answers a broader question than the campaign module alone:

How complete does the current `NISource` snapshot appear to be?

The goal is to avoid two bad assumptions:

1. assuming the whole legacy application is present in source when it is not
2. assuming the whole source tree is missing when we actually do have many of the important business processes

## Executive Conclusion

The current `NISource` snapshot appears to contain a substantial amount of the custom middleware and sync logic that matters to Dieselbrook.

However, it does not look like a complete, self-sufficient source checkout of the whole nopintegration web application.

The strongest gaps are in:

- shared web-facing UI shell and views
- shared client-side assets and helpers
- some callback/routing glue
- some data-object definitions used by newly received feature modules

## Confidence Scale

| Rating | Meaning |
|---|---|
| High confidence present | enough source is present to understand behavior materially well |
| Medium confidence present | meaningful source is present, but some supporting glue or definitions are missing |
| Low confidence present | only fragments or references are present |
| Clearly missing | dependencies are directly referenced but absent from the current snapshot |

## Completeness Matrix

| Area | What we have | Confidence | Why |
|---|---|---|---|
| FoxPro middleware entry points | `nopintegrationmain.prg`, `apiprocess.prg`, `appprocess.prg`, `brevoprocess.prg` | High confidence present | core process/host files are available and readable |
| Order sync logic | `syncorders.prg`, `syncorderstatus.prg` | High confidence present | core order import and reverse status flows are present in source |
| Consultant sync logic | `syncconsultant.prg`, `nopnewregistrations.prg`, `syncstaff.prg` | High confidence present | major consultant lifecycle and provisioning flows are present |
| Product/inventory sync logic | `syncproducts.prg` | High confidence present | product/inventory sync behavior is present in source |
| Reporting logic | `reports.prg` plus staging report metadata evidence | Medium to high confidence present | source is present, but some surrounding runtime/report hosting concerns remain external |
| Communication adapters | `communicationsapi.prg`, Brevo files | High confidence present | major messaging adapters are present in source |
| Shared sync/runtime abstractions | `syncclass.prg`, `basedata.prg`, `amdata.prg`, `nopdata.prg`, `nopapi.prg` | Medium confidence present | key abstractions are present, but runtime context and some external dependencies remain implicit |
| SSO support | `ssoclass.prg` | Medium confidence present | source exists, but external identity/runtime dependencies likely extend beyond current snapshot |
| Campaign admin module | newly received `NISource/campaign/` files | Medium confidence present for business intent; low confidence present for runnable implementation | feature code exists, but important dependencies are missing |
| Shared web layouts and `.wcs` views | no `views/` tree, no `.wcs` files | Clearly missing | campaign module references `_layoutpageVue.wcs` that is absent |
| Shared front-end scripts | no `scripts/` tree under `NISource` | Clearly missing | campaign module references shared/static scripts not present |
| Shared client-side helper functions | `ajaxCallMethod`, debounce, library globals not defined locally | Clearly missing | page script depends on helpers not included in snapshot |
| Data-object classes for campaign module | `CampData` classes not present | Clearly missing | campaign server class instantiates them directly but no definitions are visible |
| Third-party/runtime FoxPro libraries | many `.fxp` compiled files present | Present only in compiled form | runtime exists, but source for those libraries is not included |
| Full deployable web application shell | not enough files to reconstruct complete web host + assets + views | Low confidence present | source snapshot looks application-partial rather than deployment-complete |

## What This Means By Layer

## 1. Core business logic layer

This is the healthiest part of the snapshot.

We have direct source for the main business processes that matter most to replacement work:

- orders
- order status
- consultants
- staff
- products
- communications
- Brevo
- reporting

That means Dieselbrook can still perform meaningful archaeology and replacement planning without having the full UI/application shell.

## 2. Runtime/framework layer

This layer is mixed.

We have many compiled FoxPro/Web Connection runtime components in `.fxp` form, such as:

- `wwserver.fxp`
- `wwprocess.fxp`
- `wwrequest.fxp`
- `wwresponse.fxp`
- `wwsql.fxp`
- `wconnect.fxp`

That tells us the application depended on a real runtime framework and those binaries may have been enough to run it historically.

But for code archaeology and maintainability, compiled `.fxp` files are not the same as source availability.

## 3. Web-facing UI layer

This is where the current snapshot looks most incomplete.

The campaign drop exposed the gap clearly:

- there are effectively no HTML/view assets elsewhere in `NISource`
- there are no `.wcs` layout/view files in the workspace
- there is no visible shared JavaScript asset tree

So if the legacy application had more browser-driven pages, we likely do not yet have their full source footprint.

## 4. Feature-module completeness

The campaign module is useful as an example of how a UI module was built.

It also strongly suggests there may have been more modules like it, because it references shared page layouts, shared JS helpers, shared authentication state, and shared callbacks that usually serve more than one page.

That does not prove many more Vue pages existed.

But it does prove this one was not designed in isolation.

## Strongest Evidence That The Snapshot Is Partial

1. The campaign zip itself contains only one module, not a full web app.
2. The module references `_layoutpageVue.wcs`, but no `.wcs` files exist locally.
3. The module references `scripts/campaign.js` and `scripts/lookupsvue.js`, but no `scripts/` folder exists locally.
4. The module depends on client-side globals and helpers not present in the current source tree.
5. The module expects `CampData` classes that are not present in the current source tree.
6. The newly received files are untracked additions, which means they were not part of the earlier repository snapshot.

## Strongest Evidence That The Snapshot Is Still Valuable

1. The main sync engines are present in source.
2. The major domain behaviors can be traced through FoxPro files already in the repo.
3. Staging SQL access lets us validate database-side dependencies directly, even when application shell source is missing.
4. The custom Nop plugin source is also present, which covers a large part of storefront behavior from the other side.

## Practical Answer To The User's Two-Part Concern

### Is the entire `NISource` therefore missing vast portions of source code?

Not in the sense of the core business middleware.

We appear to have a lot of the custom sync and integration source that matters most for migration replacement.

### Is it incomplete as a total application/source snapshot?

Yes.

Especially for:

- browser-facing pages
- shared web layouts
- shared static assets
- some supporting glue and data-object definitions

## Recommended Working Assumption

Use the following working assumption going forward:

- `NISource` in the workspace is materially useful for reverse-engineering business behavior
- `NISource` in the workspace is not yet a complete representation of the entire deployable nopintegration application
- if Annique has more web-facing modules or shared view/script assets, they have not yet supplied them

## What To Ask For Next

1. All shared `views/` and `.wcs` layout files for the nopintegration web app.
2. All shared `scripts/` and static assets used by the web-facing modules.
3. Any additional page modules comparable to the newly provided campaign feature.
4. Any source file containing `CampData` class definitions.
5. Any callback/router source that serves `jsonCallbacks.ann` and `jsonCallbackst.ann`.
6. A brief explanation of whether the browser-facing admin/UI modules were part of nopintegration proper or a related companion app.