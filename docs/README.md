# Documentation

Reference material for the **Blast Management API** technical exercise.

## Exercise specification

| Document | Description |
|----------|-------------|
| [Senior-Developer-Technical-Exercise.pdf](./Senior-Developer-Technical-Exercise.pdf) | Official exercise brief — domain model, CQRS commands/queries, HTTP endpoints, constraints, and evaluation criteria |

## Related project guidance

Project rules live in `.cursor/rules/` (local, not committed — see `.git/info/exclude`).

## Solution layout

```
src/
  BlastManagement.Domain/          Aggregates, events, invariants
  BlastManagement.Application/     Commands, queries, handlers
  BlastManagement.Infrastructure/  In-memory event store
  BlastManagement.Api/             Minimal API endpoints
```
