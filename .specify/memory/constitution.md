<!--
Sync Impact Report
- Version change: 0.0.0 -> 1.0.0
- Modified principles: N/A (new constitution)
- Added sections: Training & Security Constraints, Development Workflow
- Removed sections: N/A
- Deferred items: none
-->

# ContosoDashboard Constitution

## Core Principles

### I. Security-By-Design
Every feature and route in ContosoDashboard MUST enforce the least-privilege security model expected for the training scenario. Authentication, authorization, and data isolation are mandatory, not optional; protected pages, service methods, and data access paths MUST validate the caller’s identity and role before exposing information or changing state. The rationale is that the repository is intentionally a training environment for secure engineering patterns, and weak access checks create misleading examples.

### II. Offline-First & Abstraction
The application MUST remain runnable without external cloud infrastructure. Data persistence, authentication, and file handling MUST be implemented behind abstractions that allow local development and future cloud substitution without changing business logic. The rationale is to support offline training while demonstrating good architecture.

### III. Test-First Validation
All new behavior MUST be validated with a failing test or demonstrable repro before implementation, and tests MUST verify the user-visible outcome or security boundary they protect. The repository’s training value depends on proving that changes work and that access controls and business rules remain correct across iterations.

### IV. User-Scoped Data Boundaries
Each user, project, task, and notification MUST be treated as belonging to a defined authorization boundary. Services MUST filter and validate access by current user identity and role, and feature work must not bypass these rules by direct object access or shared state. The rationale is that shared dashboards and multi-user roles are core to the training scenario.

### V. Clarity Over Cleverness
The codebase MUST favor readable, maintainable patterns over clever abstractions or hidden dependencies. New modules, methods, and pages should be easy to follow for learners, with names and structure that align with the training domain and business workflow. The rationale is that this project exists to teach disciplined development practices, not optimize for a production deployment.

## Training & Security Constraints

ContosoDashboard is explicitly a training application. It MUST be suitable for offline local use, intentionally avoid production-grade external dependencies, and clearly document any mock or simplified implementation. Features may use demo credentials or local-only services only when they make the learning objective explicit.

No feature may introduce production secrets, external cloud services, or irreversible operational assumptions without clearly labeling them as non-production examples. Authentication and authorization examples MUST remain educational and must not imply that mock login, local database usage, or in-memory data access are a production recommendation. Security controls are mandatory: input validation, authorization checks, isolation boundaries, and explicit user-role logic must be preserved even when the application uses simplified training data.

## Development Workflow

All changes MUST follow the same evidence-based workflow: understand the requirement, identify the impacted security and data boundaries, validate the expected behavior, implement the minimal change, and verify the result. Pull requests and direct changes to the repository MUST preserve the teaching intent of the project, and changes that weaken security, remove access validation, or blur the offline-only training scope are not acceptable.

New features MUST be kept aligned with the existing architecture: Models, Services, Data, and Pages responsibilities should remain clear; the user experience should stay consistent with the dashboard training scenarios; and documentation must be updated when behavior, security assumptions, or setup requirements change. The project should prefer incremental, reviewable updates over broad refactors.

## Governance

This Constitution supersedes informal practices and guides all repository decisions. Amendments require a documented rationale, a clear impact assessment on security, data access, and training scope, and approval by the repository maintainer or designated project owner before adoption. Any change that broadens scope, adds production assumptions, or weakens security controls MUST include a migration note or explicit justification.

Versioning follows Semantic Versioning: MAJOR changes remove or redefine non-negotiable principles; MINOR changes add or materially expand rules; PATCH changes make clarifications, wording fixes, or non-semantic improvements. Compliance review is expected for every material change: reviewers MUST verify that the update preserves the repository’s training-only intent, access-control expectations, and offline-first constraints. The most recent version of this constitution governs all work until a later amendment is approved.

**Version**: 1.0.0 | **Ratified**: 2026-09-15 | **Last Amended**: 2026-09-15
