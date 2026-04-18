# Canvas — Development Phases

## Purpose

This document describes the complete, phased roadmap for building Canvas. It is the single source of truth for **what we are building**, **in what order**, and **what "done" means** at every step. Each Phase corresponds to a Jira Epic; each sub-phase corresponds to a Jira Story; each testable unit of work inside a sub-phase corresponds to a Jira Sub-task.

Phases are numbered (1–8). Sub-phases are lettered (1A, 1B, …). The ordering within a Phase is intentional and reflects dependencies.

---

## Global Conventions

### Definition of Done (applies to every sub-phase)

A sub-phase is not "done" until **all** of the following are true:

1. Production code is written and reviewed.
2. **Unit tests** cover all domain logic, business rules, and non-trivial branches.
3. **Integration tests** cover any cross-layer behavior (DB access, HTTP endpoints, messaging, etc.).
4. The full test suite runs **green** locally and in CI.
5. Code is formatted and passes analyzer rules (no warnings).
6. Public-facing APIs, entities, and value objects are documented with XML doc comments where the intent is non-obvious.
7. The corresponding Jira Sub-task is transitioned to **Done**.

### Testing Strategy

- **Unit tests** live next to the project they test (e.g., `Canvas.Domain.Tests` for `Canvas.Domain`). Framework: **xUnit**; assertions: **FluentAssertions** (or `Shouldly`); mocks: **NSubstitute**.
- **Integration tests** live in a dedicated project (`Canvas.IntegrationTests`) and use **Testcontainers for SQL Server** so tests hit a real database without polluting the local dev DB.
- **Architecture tests** (`Canvas.ArchitectureTests`) use **NetArchTest** to enforce Clean Architecture rules (e.g., "Domain depends on nothing", "Infrastructure does not reference API").
- Target: **≥ 80% line coverage** on Domain and Application layers; ≥ 60% overall.
- Test must run fast: full unit suite < 10 seconds, full integration suite < 2 minutes.

### Tech Stack (locked)

- **.NET 10** (LTS, Nov 2025)
- **C# 14**
- **EF Core 10** (SQL Server provider)
- **MediatR** for CQRS dispatch
- **FluentValidation** for input validation
- **Mapster** for object mapping
- **Serilog** for structured logging
- **Scalar** (replacement for Swashbuckle) for OpenAPI/Swagger
- **SQL Server 2025** with native `VECTOR` type for the Memory bounded context
- **GitHub Actions** for CI/CD
- **Testcontainers** for database integration tests
- Front-end tech: TBD in Phase 1E, but leaning **Next.js 15 + React 19 + TypeScript + shadcn/ui**

### Architectural Rules

- Clean Architecture layering: `Domain` ← `Application` ← `Infrastructure`, `Application` ← `API`. Domain depends on nothing.
- **Entity-centric folder grouping** inside each layer. Value objects, services, commands, queries, and events live under their owning entity's folder.
- **Strongly-typed, immutable IDs** (`readonly record struct XxxId(Guid Value)`).
- **Value objects are immutable** and self-validating.
- **Aggregates enforce their own invariants**; cross-aggregate consistency is eventual, via domain/integration events.
- **Memory is a separate bounded context**, integrated only via events.

---

## Phase 1 — Clean Architecture Foundation *(Jira Epic: SCRUM-5)*

**Goal:** Stand up the empty but fully wired solution. Every layer exists, references are correct, DI works end-to-end, CI runs green on first commit.

| Sub-phase | Story | Testable "done" bar |
|---|---|---|
| **1A** | Solution + Domain project scaffold | Domain project compiles; architecture tests assert it has zero outbound references; base types (`Entity<TId>`, `ValueObject`, `DomainEvent`) unit-tested. |
| **1B** | Application layer (CQRS scaffolding) | MediatR wired; generic behaviors (validation, logging) in place; sample no-op command + query pass unit tests. |
| **1C** | Infrastructure layer (EF Core + SQL Server) | DbContext compiles; a migration can be created and applied; integration test using Testcontainers confirms DB connectivity and migration. |
| **1D** | API layer (Minimal APIs + controllers optional) | API boots; `/health` endpoint responds 200; OpenAPI doc generates; integration test hits `/health`. |
| **1E** | Front-end scaffold | Next.js app builds; calls `/health`; displays status; e2e smoke test (Playwright) passes. |
| **1F** | End-to-end DI + smoke test | Full stack (API + DB + front-end) boots via `docker-compose`; smoke test confirms front-end → API → DB round-trip. |
| **1G** | GitHub Actions CI pipeline | Push to `main` or PR triggers build + unit tests + integration tests + architecture tests; coverage report uploaded; red CI blocks merge. |

**Exit criteria:** Solution builds, CI is green, every layer is reachable from every other valid layer, testing infrastructure proven. *No business features yet.*

---

## Phase 2 — Ideation & Project Core

**Goal:** Users can capture Ideas, refine them, and promote them to Projects. This is the first business-value phase.

| Sub-phase | Story | Testable "done" bar |
|---|---|---|
| **2A** | `Idea` entity + value objects | Domain-level create/edit/discard flows fully tested; invariants (e.g., non-empty title) enforced at construction. |
| **2B** | `Project` entity + value objects | Project creation, rename, archive flows with tests; Project owns all child artifacts. |
| **2C** | Idea → Project promotion workflow | Domain service `IdeaPromotionService`; rules tested (e.g., cannot promote an archived Idea); emits `IdeaPromotedToProject` domain event. |
| **2D** | API endpoints for Ideas and Projects | CRUD + promote endpoints; request/response DTOs; FluentValidation rules; integration tests on each endpoint. |
| **2E** | Front-end: Idea capture + Project list | Form to capture an Idea; list view for Ideas and Projects; promote action; Playwright tests for the happy path. |

**Exit criteria:** A user can capture an idea, refine it, promote it to a project, and see both in the UI.

---

## Phase 3 — Planning Hierarchy

**Goal:** Under each Project, users can plan work using Epic → Story → Activity.

| Sub-phase | Story | Testable "done" bar |
|---|---|---|
| **3A** | `Epic` under Project | Epic is a child of Project; cannot exist without a parent Project; full CRUD + tests. |
| **3B** | `Story` under Epic | Story is a child of Epic; ordering within Epic supported (e.g., `SortIndex` VO); full CRUD + tests. |
| **3C** | `Activity` under Story | Activity is the testable unit of work; states: Todo, In Progress, Done; transitions tested. |
| **3D** | Cross-cutting: status rollup | Project/Epic/Story status derived from child completion; rollup calculations unit-tested. |
| **3E** | Front-end: planning tree | Tree/board view for Project → Epic → Story → Activity; drag-to-reorder; Playwright tests. |

**Exit criteria:** A user can fully plan a Project with nested Epics, Stories, and Activities, and see status roll up.

---

## Phase 4 — Architecture Artifacts

**Goal:** Canvas becomes useful for architectural decision capture and design documentation.

| Sub-phase | Story | Testable "done" bar |
|---|---|---|
| **4A** | `ArchitectureDecision` (ADR) entity | ADR lifecycle: Proposed → Accepted → Superseded; full CRUD + state transitions tested. |
| **4B** | Design document attachment | Markdown-based design docs tied to Project; versioned; retrieval tested. |
| **4C** | API + front-end for ADRs and design docs | Endpoints, UI for creating/viewing ADRs; markdown rendering with syntax highlighting; Playwright tests. |

**Exit criteria:** A user can capture ADRs and design documents against any Project, and browse their history.

---

## Phase 5 — Memory (RAG) *(separate bounded context)*

**Goal:** Introduce long-term memory as a knowledge graph with fast vector retrieval. Integrates with other bounded contexts via events — never direct references.

| Sub-phase | Story | Testable "done" bar |
|---|---|---|
| **5A** | `KnowledgeNode` aggregate | Node has content, embedding, tags, source metadata; created via command; unit-tested invariants. |
| **5B** | `KnowledgeEdge` — relations between nodes | Typed edges (e.g., `Relates`, `Derives`, `Contradicts`); cycle detection where appropriate; unit-tested. |
| **5C** | Vector storage + indexing on SQL Server 2025 | `VECTOR` column + HNSW index; `IKnowledgeRetrievalService` interface with SQL-backed adapter; integration test measures p95 < 100ms at target dataset size. |
| **5D** | Retrieval API (`/memory/search`) | Semantic search endpoint returning ranked nodes; hybrid search (vector + BM25/full-text); integration tests on retrieval quality and latency. |
| **5E** | Integration event subscribers | Memory subscribes to domain events from Ideation/Planning/Architecture contexts and auto-indexes their content; tested end-to-end with in-memory broker. |
| **5F** | Front-end: memory explorer | Graph visualization (e.g., Cytoscape.js) + search UI; Playwright tests. |

**Exit criteria:** All content from other contexts is automatically indexed into Memory; semantic search is fast and accurate; the graph can be explored in the UI.

**Deferred decisions (captured here so we don't lose them):**
- Embedding model: choose at start of Phase 5. Candidates: local `bge-small-en`, OpenAI `text-embedding-3-small`, Anthropic embeddings if/when available.
- Re-indexing strategy on model change.

---

## Phase 6 — Integration APIs

**Goal:** Expose Canvas as a service other tools (VS Code, Claude Code, scripts) can call at the start and end of tasks.

| Sub-phase | Story | Testable "done" bar |
|---|---|---|
| **6A** | Task-start hook API | External tool posts "I'm starting work on Activity X"; Canvas returns relevant memory + ADRs + prior work. Contract and integration tests. |
| **6B** | Task-end hook API | External tool posts "I finished Activity X with output Y"; Canvas records the result and triggers re-indexing. |
| **6C** | Webhook subscriptions | Canvas can push events to external subscribers (e.g., "Project archived"). HMAC-signed payloads; delivery retry; integration tests. |
| **6D** | API authentication (API keys) | Per-client API keys; scoped permissions; tested for access control. |

**Exit criteria:** A shell script or VS Code extension can call Canvas before and after a task, receive context, and submit results.

---

## Phase 7 — Multi-user Extension

**Goal:** Introduce `User` and `UserGroup` so Canvas can be shared. Architecturally prepared-for since Phase 1; this phase turns the feature on.

| Sub-phase | Story | Testable "done" bar |
|---|---|---|
| **7A** | `User` entity + authentication | User as a first-class entity with its own services, commands, queries; auth via OIDC (e.g., GitHub as IdP). |
| **7B** | `UserGroup` value object | Group as an immutable VO attached to users and/or projects; equality semantics tested. |
| **7C** | Permission model | Project-level roles (Owner, Editor, Viewer); enforced in Application handlers; tested per endpoint. |
| **7D** | Front-end: auth + multi-user UI | Login flow, user menu, project sharing dialog; Playwright tests covering role-based access. |

**Exit criteria:** Canvas can be used by multiple users with distinct permissions per Project.

---

## Phase 8 — Hardening & Operations

**Goal:** Production-readiness. Canvas should be observable, performant, deployable, and secure.

| Sub-phase | Story | Testable "done" bar |
|---|---|---|
| **8A** | Observability | Structured logging (Serilog → Seq/Grafana Loki); OpenTelemetry traces; Prometheus metrics; dashboards committed as code. |
| **8B** | Performance pass | Identify p95 hotspots; add caching where measured; load test baseline captured and documented. |
| **8C** | Deployment | Containerized; GitHub Actions deploys to a target environment (Azure/Fly.io/self-hosted — decided in this sub-phase); rollback tested. |
| **8D** | Security review | Dependency audit; OWASP Top 10 review; API key rotation flow; secrets scanning in CI; penetration smoke test. |

**Exit criteria:** Canvas is deployable, observable, and passes a baseline security review.

---

## Out of Scope (for now)

Explicitly *not* part of this roadmap, captured so we don't rathole on them:

- Real-time collaboration (multiple users editing the same Idea concurrently).
- Mobile apps.
- AI-generated content creation (Canvas is a tool *used by* AI, not necessarily generating for users).
- Marketplace / plugin ecosystem.

These can become future Phases (9+) if and when demand is real.

---

## How We'll Keep This Doc Honest

- When a Phase is created as a Jira Epic, add the Epic key next to the Phase heading.
- When a sub-phase's acceptance criteria shift, update this doc in the same PR that makes the code change.
- When we decide a deferred decision (e.g., embedding model in Phase 5), update the "Deferred decisions" list with the choice and rationale.
- This doc is the spec; if the code and the doc disagree, the doc wins until proven otherwise.
