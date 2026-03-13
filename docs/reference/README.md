# Reference — Client-Provided Artifacts

**What this folder is:** Raw artifacts provided by Annique / the client. These are source inputs, not our analysis outputs.

> For the project-wide navigation index, see [`../README.md`](../README.md).

---

## Contents

| File | Description |
|---|---|
| `User Requirements - Annotated - 19 Feb.docx` | Annotated user requirements document (19 Feb 2026) — primary client requirement input. Annotations mark key scope and integration implications. |
| `WS1 - Campaigns.docx` | Workshop 1 materials: Campaigns module — workshop session notes and requirements. |
| `WS1 - Item Master.docx` | Workshop 1 materials: Item Master module — workshop session notes and requirements. |
| `Accountmate -SQL Jobs for Dieselbrook.xlsx` | SQL Agent jobs from the AccountMate instance, exported as client artifact. |
| `NOP-SQL Jobs for Dieselbrook.xlsx` | SQL Agent jobs from the NopCommerce instance, exported as client artifact. |
| `Dieselbrook_NDA_Annique_Project.pdf` | NDA for the Annique project. |
| `campaign.zip` | Zipped NISource campaign module source (Vue/Web Connection files) — fed into gap analysis in `discovery/14_campaign_module_missing_dependencies.md`. |

---

## Usage

- The `.docx` requirements documents inform scope decisions captured in `analysis/02_open_decisions_register.md`
- `campaign.zip` is the source artifact analysed in `discovery/14_campaign_module_missing_dependencies.md` and `discovery/15_nisource_source_completeness_matrix.md`
- SQL job exports provide evidence for the hosted procedure inventory in the discovery layer
