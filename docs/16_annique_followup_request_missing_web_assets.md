# Follow-Up Request To Annique For Missing Web-App Assets

## Purpose

This draft is meant to close the gap exposed by the newly supplied campaign module.

It is written to be specific and actionable so Annique can send the missing source in one pass instead of another partial drop.

## Suggested Email / Message Draft

Subject: Follow-up request for remaining nopintegration campaign/web module source files

Hi team,

Thank you for sending the campaign source files.

We reviewed the zip and can see that it contains a campaign administration module made up of:

- `Campaign.html`
- `campaign.prg`
- `campaigns.js`
- `Campaign_class.prg`

This is useful and clearly relevant, but it also appears to depend on additional shared files and runtime pieces that were not included in the zip.

To make sure we understand the legacy system correctly and do not miss any important customizations, could you please send the remaining files or confirm where they live?

## What we still need

### 1. Shared page layouts / views

The campaign module references a shared layout:

- `~/views/_layoutpageVue.wcs`

Please send:

- the full `views/` folder
- any `.wcs` files used by nopintegration or related web modules
- any shared layout, partial, or template files used by campaign or other admin pages

### 2. Shared JavaScript / static assets

The campaign module references shared script assets and helper functions that were not included.

Please send:

- the full `scripts/` folder used by this application
- any shared JavaScript helpers used by pages such as `ajaxCallMethod`, debounce helpers, number-formatting helpers, etc.
- any static CSS/JS assets loaded globally for the Vue pages

### 3. Missing server-side campaign dependencies

The campaign class appears to rely on additional FoxPro classes/data definitions that were not included.

Please send:

- the source file that defines `CampData`
- the source files that define `Campaign`, `CampCat`, `CampSku`, `CampDetail`, `CampBrand`, and `CampSponSum` if they are not all inside one shared file
- any callback/router source that exposes methods such as:
  - `Camp_GetSettings`
  - `Camp_GetCamps`
  - `Camp_GetItemLookups`
  - `camp_getspontypes`
  - `camp_getskubymonthvert`
  - `camp_getsummary`
  - `camp_update`
  - `camp_updatesku`
  - `camp_synctostage`

### 4. Other browser-facing nopintegration modules

The campaign module looks like part of a broader web application rather than a standalone page.

Please confirm whether there are other similar browser-facing modules besides campaign management, and if so please send them as well.

Examples of what we mean:

- other `.prg` page modules
- other `.html` page templates
- any Vue/JavaScript admin modules
- any back-office or internal management screens related to pricing, products, consultants, offers, or reports

### 5. Clarification on canonical source structure

Please confirm which of the following are considered the authoritative source files in your system:

- `Campaign.html`
- `campaign.prg`
- `campaigns.js`

In particular, we would like to know:

- whether `Campaign.html` and `campaign.prg` are two different runtime files or two representations of the same page
- whether `campaigns.js` is copied/renamed during deployment to `campaign.js`
- whether there is a build/deploy step or if files are served directly

### 6. Deployment/runtime context

Please share any short notes on how this web-facing part of nopintegration was structured in production, for example:

- where the shared views/scripts were stored
- whether these pages were served by the same nopintegration application or a companion admin app
- whether the campaign module was actively used in production or only in staging/back-office contexts

## Why we are asking

At the moment, the supplied campaign zip looks like a valid feature module, but not a complete runnable application slice.

We want to avoid incorrectly assuming:

- that this is the full campaign customization when it is not, or
- that there were no other web-facing custom modules when in fact they were just not included

Sending the missing shared files now will help us complete the legacy mapping accurately and reduce repeated follow-up questions later.

Thanks,

[Your Name]

## Shorter Checklist Version

If a shorter request is easier for them to action, send this checklist:

1. Full `views/` folder and any `.wcs` layout files.
2. Full `scripts/` folder and shared JS helpers.
3. `CampData` source and related campaign data-object files.
4. Callback/router source for `jsonCallbacks.ann` / `jsonCallbackst.ann` methods used by campaign.
5. Any other browser-facing nopintegration modules similar to campaign.
6. A short note explaining whether these pages were part of nopintegration proper or a separate admin/back-office app.