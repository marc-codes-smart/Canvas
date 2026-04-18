# Canvas — Claude Context Index

This file is Claude's entry point for every new session or task in this project.
It is an **index**, not a document. Keep it short. When new context is worth
persisting, create a file for it and add a link (or `@` import) below.

---

## What Canvas Is

Canvas is a personal software **dev center** — a meta-tool for incubating ideas,
turning them into projects, guiding architecture and design, exposing APIs that
other tools (VS Code, Claude Code, scripts) can call, and building a RAG-style
long-term memory graph. Single-user for v1, designed to extend to multi-user.

Full concept: [docs/design/Canvas.md](docs/design/Canvas.md)

---

## Current State

- **Roadmap (authoritative):** [docs/architecture/Phases.md](docs/architecture/Phases.md) — 8 Phases, lettered sub-phases, Definition of Done per sub-phase.
- **Active Phase:** Phase 1 — Clean Architecture Foundation (Jira Epic **SCRUM-5**).
- **Active sub-phase:** SCRUM-6 (1A) — Set up Domain layer. Domain scope and folder structure under discussion; no code written yet.
- **Solution code:** not yet created. Only docs and configs exist in the repo so far.

When you start a session, confirm current state by:
1. Reading the bottom of [docs/architecture/Phases.md](docs/architecture/Phases.md) for any updates.
2. Querying Jira for the status of SCRUM-5 and its children via the Atlassian MCP server.

---

## How I Should Work Here

These rules apply to every session. Ignore none of them without explicit direction.

1. **Ask clarifying questions before acting.** Especially on domain, requirements, or architectural decisions.
2. **Never generate code until explicitly told** (e.g., "generate the code now"). Scaffolding, folder structures, and config are fine *with prior approval*.
3. **Show proposed folder structures before creating files.** Wait for approval.
4. **One Story at a time.** After a Story (sub-phase) is complete, wait for direction on the next.
5. **Provide architectural reasoning** before implementation — why, not just what.
6. **Respect the Definition of Done** from [Phases.md](docs/architecture/Phases.md): code + unit tests + integration tests + green CI + passing architecture tests. No sub-phase is "done" without all five.
7. **Never put secrets in committed files.** Use env vars and `.gitignore`. This repo targets **public GitHub**.
8. **Concise responses.** No filler, no trailing summaries unless asked.

---

## Locked Architectural Decisions

*(Full rationale and the complete tech stack live in [Phases.md](docs/architecture/Phases.md) → "Global Conventions".)*

- **Clean Architecture**: `Domain ← Application ← Infrastructure`, `Application ← API`. Domain depends on nothing.
- **Entity-centric folder grouping** inside each layer (VOs, services, commands, queries, events live under their owning entity's folder).
- **Strongly-typed, immutable IDs** (`readonly record struct XxxId(Guid Value)`).
- **Immutable, self-validating value objects.**
- **Memory is a separate bounded context**, integrated only via events.
- **.NET 10**, **EF Core 10**, **SQL Server 2025** (native `VECTOR` for Memory).
- **Testing baked in from day 1**: xUnit + FluentAssertions + NSubstitute + Testcontainers + NetArchTest.
- **CI**: GitHub Actions on every push/PR.

---

## Key References

**Always-loaded (via local, gitignored import chain):**

@CLAUDE.local.md

*(`CLAUDE.local.md` is gitignored and imports Marc's personal global context from `CLAUDE/` — a symlink to `~/OneDrive/dev/CLAUDE/`. Neither the symlink nor the local file is committed. Public cloners simply skip these imports.)*

**On-demand (read when the topic is relevant):**

- Roadmap: [docs/architecture/Phases.md](docs/architecture/Phases.md)
- Project concept: [docs/design/Canvas.md](docs/design/Canvas.md)
- Initial observations / Q&A log: [docs/design/Claude_Initial_Observations_Questions.md](docs/design/Claude_Initial_Observations_Questions.md)
- Jira config (gitignored, local only): `jira-config.md`

---

## Jira Integration

- **Site:** `marccodessmart.atlassian.net` (Jira Cloud, free tier).
- **MCP server:** `atlassian` (configured in [.mcp.json](.mcp.json) via env vars `JIRA_URL`, `JIRA_USERNAME`, `JIRA_API_TOKEN`).
- **Issue key format:** `SCRUM-N`.
- **Hierarchy in Jira:** Epic → Story → Sub-task.
- **Hierarchy in Canvas's own domain model:** Idea → Project → Epic → Story → Activity (different purpose; don't conflate).

---

## How to Extend This File

When you want Claude to know something new:

1. **If it's always relevant** (persistent rules, tech stack updates, critical constraints): add a short note here **or** create a file under `docs/context/` and reference it as `@docs/context/<file>.md` under "Always-loaded".
2. **If it's topical** (a design doc, an ADR, a runbook): create the file and add a link under "On-demand".
3. **Keep this file small.** If a section grows beyond ~10 lines of substantive content, move it to its own file and leave a link.

The pattern: **this file is the table of contents; the details live elsewhere.**
