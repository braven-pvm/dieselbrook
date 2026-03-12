# Campaign Module Missing Dependencies Analysis

## Purpose

This document answers a narrow but important question:

If the newly received `NISource/campaign/` folder is treated as a client-facing Vue/Web Connection module, what else would need to exist around it for it to function?

It is written to separate what is present from what is clearly missing.

## Direct Conclusion

The campaign module is not standalone in its current delivered form.

It is a partial feature-module drop that depends on a larger FoxPro Web Connection web application shell and shared client-side assets that are not present in the current workspace snapshot.

## What Was Actually Supplied

The supplied zip contains only these four source files:

- `campaign/Campaign.html`
- `campaign/campaign.prg`
- `campaign/campaigns.js`
- `campaign/Campaign_class.prg`

That means Annique did not supply a full web application or a complete front-end asset tree.

They supplied one campaign-oriented feature module.

## Dependency Categories

## 1. Missing layout and view shell

### Evidence

- `Campaign.html` declares `Layout = "~/views/_layoutpageVue.wcs"`
- `campaign.prg` calls `wwScript.RenderAspScript("~/views/_layoutpageVue.wcs")`

### What this implies

This module expects a shared layout template that likely provides:

- outer page shell
- common CSS/JS includes
- Vue/bootstrap mounting environment
- authenticated web application framing
- common nav/footer/header structure

### Current state

- no `.wcs` files are present in the workspace
- no `views/` folder is present under `NISource`

### Conclusion

The client-facing page cannot be rendered in its intended runtime form from the supplied files alone.

## 2. Missing shared JavaScript asset pipeline

### Evidence

- `campaign.prg` references `scripts/campaign.js?v=1`
- `campaign.prg` comments out `scripts/lookupsvue.js`
- supplied file name is `campaigns.js`, not `campaign.js`

### What this implies

There was likely a broader static asset structure such as:

- shared `scripts/` directory
- naming/build/copy convention that may have renamed `campaigns.js` to `campaign.js`
- common helper scripts reused across multiple pages

### Current state

- no `NISource/scripts/` folder is present
- no additional JavaScript files exist under `NISource`
- no bundler/build config is visible in the supplied snapshot

### Conclusion

The provided JavaScript appears to be one page-specific script extracted out of a larger asset environment.

## 3. Missing shared client-side libraries

### Evidence from `campaigns.js`

The module references the following globals/libraries:

- `Vue`
- `VeeValidate`
- `VeeValidateRules`
- `VueTypeaheadBootstrap`
- `ajaxCallMethod`
- `debounce`
- jQuery-style helpers such as `$.number`
- BootstrapVue components/events/modals

### What this implies

The page expects a shared browser environment to already include:

- Vue runtime
- BootstrapVue
- validation libraries
- typeahead component library
- common AJAX helper library
- utility helpers such as debounce and number formatting

### Current state

- none of those supporting libraries are present in `NISource`
- no shared HTML shell is present that would include those libraries globally

### Conclusion

The campaign page script is not independently runnable as shipped.

It assumes a parent application shell already injects those dependencies.

## 4. Missing server-side callback routing

### Evidence

The page calls callback endpoints such as:

- `jsonCallbacks.ann`
- `jsonCallbackst.ann`

and methods such as:

- `Camp_GetSettings`
- `Camp_GetCamps`
- `Camp_GetItemLookups`
- `camp_getspontypes`
- `camp_getskubymonthvert`
- `camp_getsummary`
- `camp_update`
- `camp_updatesku`
- `camp_synctostage`

### Current state

- these callback methods are referenced from the JavaScript but are not implemented anywhere else in the current repo snapshot outside the campaign folder's own class definitions
- no visible routing file or callback registration for these methods is present in the current source snapshot

### Conclusion

The client-side code knows the names of the callback methods, but the surrounding callback-dispatch infrastructure is not visible in the supplied files.

That makes this another sign of an extracted feature slice rather than a whole runnable application.

## 5. Missing `CampData` object model definitions

### Evidence

`Campaign_class.prg` creates objects from `CampData`, including:

- `Campaign`
- `CampCat`
- `CampSponSum`
- `CampBrand`
- `CampSku`
- `CampDetail`

### Current state

- no `CampData.prg` or equivalent file is present in the workspace
- no visible definitions for those data classes exist elsewhere in the current repo snapshot

### What this likely means

Either:

1. the data classes exist in source but were not included in the zip, or
2. the data classes exist only in compiled form in another runtime location not included here

### Conclusion

The server-side business class layer is incomplete as delivered.

## 6. Missing or externalized authentication/session shell

### Evidence

The campaign files rely on:

- `Process.cAuthenticatedName`
- `process.cauthenticateduser`
- `LoadUserSettings(..., "camp")`
- home/logout routes like `default.ann` and `logout.ann`

### What this implies

This module expects:

- an authenticated web app session model
- shared `Process` state
- user-specific authorization settings for campaign access

### Current state

- the local snapshot does not include the view/auth/session shell that would expose all of those pieces together for this page module

### Conclusion

The campaign page is not a free-standing admin micro-app. It expects to live inside the existing web application session and permission model.

## 7. Missing deployment/static-file conventions

### Evidence

- `Campaign.html` and `campaign.prg` appear to be overlapping representations of the same page
- `campaigns.js` versus `campaign.js` suggests naming/copy/build conventions outside the delivered files

### Interpretation

There was likely a deployment/runtime convention such as:

- authoring template source in one format
- compiled/generated `.prg` page script in another
- copied/renamed client asset in a shared scripts folder

### Conclusion

We likely have the feature code, but not the complete authoring-to-runtime pipeline.

## What Seems Present And Useful Anyway

Even though it is incomplete as a runnable page, the supplied module is still useful for discovery.

It gives us direct evidence of:

- the campaign administration use cases the legacy system supported
- the shape of campaign summary, category split, sponsor split, reward budget, and SKU exposure workflows
- the stored procedures and data families the admin UI relied on
- stage publishing actions that matter to migration scope (Namibia copy action is present in legacy source but Namibia is out of scope)

That means it is highly relevant to replacement analysis even if it is not deployable as-is.

## Practical Answer To The User's Question

### Is the campaign source standalone in its client-facing Vue app status?

No.

It is a feature module that depends on a wider web shell, shared JavaScript libraries, shared callbacks, shared settings/auth infrastructure, and additional server-side data definitions.

### Is it also incomplete?

Yes.

It is almost certainly incomplete as a runnable module in the current snapshot.

## Exact Missing Items To Ask For

1. The `views/` folder, especially `_layoutpageVue.wcs` and any shared layout/view files.
2. The shared `scripts/` folder or the exact static assets deployed with this module.
3. The source or compiled definitions for the `CampData` object model classes.
4. The callback routing/registration source for `jsonCallbacks.ann` and `jsonCallbackst.ann`.
5. Any shared JavaScript helpers providing `ajaxCallMethod`, debounce, and number formatting.
6. The exact deployment/runtime instructions showing whether `Campaign.html` or `campaign.prg` is the canonical source.