claude.rules.md
1. General Behavior Rules
1.1 Autonomy Level
Claude may act autonomously within the boundaries of a single sub-phase (1A–1G).
Claude must:

Follow the workflows in this file

Ask only when something is ambiguous

Never assume requirements not stated in Phases.md

Never modify files outside the current sub-phase

Never create modules before Phase 2

Never generate business logic before Phase 2

1.2 Communication Style
Before making changes, Claude must:

Restate the plan

List the files/directories to be created or modified

Wait for confirmation

After confirmation, Claude may proceed without further interruptions unless:

A build error occurs

A test fails

A requirement is unclear

1.3 Scope Boundaries
Claude must not:

Invent new architecture

Add dependencies not listed in Phases.md

Modify global CLAUDE context files

Change .gitignore unless explicitly asked

Create placeholder modules before Phase 2

Generate UI code before Phase 1E

2. Directory & File Creation Workflow
When creating directories or files:

Claude lists the exact structure it will create

Claude waits for approval

Claude creates all directories in one batch

Claude creates all files in one batch

Claude stops

Claude must never create directories incrementally unless requested.

3. Multi‑File Change Workflow
When a task requires modifying multiple files:

Claude lists all files to be changed

Claude summarizes the changes per file

Claude waits for approval

Claude applies all changes in one atomic batch

Claude stops

If a change touches more than 10 files, Claude must ask for confirmation again.

4. Testing Workflow
4.1 Test‑First Rule
For all domain logic, infrastructure plumbing, and architecture rules:

Claude writes tests first unless the user explicitly says otherwise.

4.2 Test Placement
Unit tests go next to the project they test

Integration tests go in Canvas.IntegrationTests

Architecture tests go in Canvas.ArchitectureTests

4.3 Test Requirements
Claude must ensure:

≥ 80% coverage for Domain and Application

≥ 60% overall

Unit tests run < 10 seconds

Integration tests run < 2 minutes

5. Clean Architecture Enforcement
Claude must enforce the following rules at all times:

Domain depends on nothing

Application depends only on Domain

Infrastructure depends on Application + Domain

API depends on Application

Modules never reference each other

Shared primitives live in Canvas.Common

Shared infrastructure lives in Canvas.Infrastructure.Shared

Composition root is Canvas.Host

Claude must use NetArchTest to enforce these rules.

6. CQRS & MediatR Workflow
When implementing commands/queries:

Use MediatR

Use pipeline behaviors for validation, logging, exception handling

Use FluentValidation for request validation

Use Mapster for mapping

Use ErrorOr for result handling

Use Vogen for strongly‑typed IDs

Claude must not invent new patterns.

7. EF Core Workflow
When working with EF Core:

Claude must define entities and configurations in the module’s Infrastructure folder

Claude must register configurations in Canvas.Infrastructure.Shared

Claude must generate migrations only after confirmation

Claude must not create multiple DbContexts in Phase 1

Claude must use Testcontainers for integration tests

8. Module Registration Workflow
Even though modules are not created until Phase 2, Claude must:

Implement the Module.cs pattern in Phase 1C

Prove it with a stub module

Ensure Canvas.Host discovers modules automatically

9. Phase‑Specific Rules
Phase 1A
Create solution + Canvas.Common

Implement base types

Write unit tests

Write architecture tests enforcing zero outbound references

Phase 1B
Create Canvas.Infrastructure.Shared

Implement CQRS plumbing

Implement domain event collector + dispatcher

Scaffold outbox table

Write unit + integration tests

Phase 1C
Create Canvas.Host

Implement module discovery pattern

Add /health endpoint

Add OpenAPI via Scalar

Write integration test hitting /health

Phase 1D
Set up SQL Server 2025 connectivity

Apply first migration

Write integration test verifying DB round‑trip

Phase 1E
Scaffold Next.js front-end

Implement /health call

Add Playwright smoke test

Phase 1F
Create docker-compose for full stack

Write end‑to‑end smoke test

Phase 1G
Implement GitHub Actions CI

Run unit + integration + architecture tests

Upload coverage report

10. Safety Boundaries
Claude must:

Never delete files unless explicitly asked

Never refactor outside the current sub-phase

Never modify code unrelated to the task

Never generate placeholder business logic

Never create modules before Phase 2

11. Error Handling Rules
If a build error occurs:

Claude must show the error

Claude must propose a fix

Claude must wait for approval

Claude must apply the fix

If tests fail:

Claude must show the failing tests

Claude must propose a fix

Claude must wait for approval

12. Session Restart Rules
When a new Claude Code session starts:

Claude must load this file

Claude must ask:

“Which Phase and sub-phase are we working on?”

Claude must not assume context from previous sessions