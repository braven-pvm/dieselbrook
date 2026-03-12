# Domain Analysis Template

## Purpose

Use this template for each canonical domain pack in the synthesis phase.

The aim is to keep every domain analysis structured the same way so macro and micro documentation stay joinable.

## Template

### 1. Domain Summary

- business purpose
- why the domain matters to the migration
- phase-1 importance

### 2. Current Systems And Actors

- systems involved
- human roles involved
- source-of-truth boundaries

### 3. Current Source Components

- FoxPro files
- Nop plugin components
- SQL procedures/tables/views/functions
- external integrations

### 4. Current Workflows

For each major workflow:

- trigger
- inputs
- major logic steps
- outputs
- side effects
- error/failure behavior

### 5. Operational Dependencies

- other domains that depend on this one
- downstream side effects
- upstream assumptions
- timing or sequencing dependencies

### 6. Replacement Boundary

- what must be replaced in phase 1
- what can remain ERP-side or legacy-side temporarily
- what is candidate for later phase modernization

### 7. Risks And Fragility Points

- hidden side effects
- parity-critical rules
- data quality concerns
- operational failure modes

### 8. Open Decisions

- decision IDs from `02_open_decisions_register.md`
- assumptions/dependencies from `04_assumptions_and_dependencies_register.md`

### 9. Recommended Phase-1 Capabilities

- named middleware/app capabilities required
- recommended service ownership boundaries

### 10. Evidence Base

- source files
- staging objects
- existing discovery docs
- any stakeholder-supplied artifacts

## Rule For Authors

Do not turn a domain pack into a solution spec.

It should remain an analysis artifact until Dieselbrook target-state intent is firm enough to justify explicit requirements/specification documents.