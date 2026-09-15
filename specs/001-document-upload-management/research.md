# Research: Document Upload and Management

## Decision: Local file storage with repository-level abstraction

The feature will store uploaded files on the local filesystem outside the web root, while persisting metadata in the existing EF Core database. The storage layer will expose an `IFileStorageService` contract with `UploadAsync`, `DeleteAsync`, `DownloadAsync`, and `GetUrlAsync` methods, and the current training implementation will use a local service backed by `System.IO` operations. This matches the requirement for offline development and future Azure migration without changing the UI or business logic.

### Rationale

- The project explicitly requires offline operation and local storage for training.
- Files must not be placed under `wwwroot` because the app should not expose raw upload paths directly.
- The design enables future cloud migration without changing document service code.
- The repository already follows explicit layering and service abstractions, so this fits the architecture style.

### Alternatives considered

- Storing files directly under `wwwroot`: rejected because it exposes uploads to the web and weakens security.
- Using database-only storage: rejected because the feature requires secure file handling and large file management patterns that are clearer with filesystem-backed storage.
- External cloud dependency in the training app: rejected because the project must remain offline and not require an Azure subscription.

## Decision: Document metadata is stored in the existing relational model

Document metadata will be stored in a dedicated `Document` entity with integer `DocumentId` and string category values, matching the existing application conventions and the technical constraints in the stakeholder document.

### Rationale

- Constraint states that `DocumentId` must be integer for consistency with existing `User` and `Project` keys.
- Category is text-based for simplicity and lower friction in training.
- This is consistent with the currently implemented `User`, `Project`, and `TaskItem` integer-key pattern.

### Alternatives considered

- GUID-based primary keys and enum category storage: rejected because it would not align with repository conventions and would add unnecessary complexity.
- File metadata embedded directly in the `Project` or `TaskItem` table: rejected because it would couple unrelated concerns and reduce maintainability.

## Decision: Authorization follows project membership, ownership, and explicit share records

Document access will be governed by a layered access model:

1. owners can manage their own uploads,
2. project managers can manage project documents,
3. project members can view and download project documents,
4. shared users can access documents explicitly granted to them,
5. administrators can view and audit all documents.

### Rationale

- The feature requires both user-owned and project-scoped access patterns.
- The repository already enforces role and project membership checks in `ProjectService`.
- The requirement explicitly calls out IDOR prevention and activity tracking.

### Alternatives considered

- Open access to all documents in a project: rejected because it would violate least-privilege principles.
- Single global read access model: rejected because it fails the business model that distinguishes private, shared, and project-level documents.

## Decision: Document-related activity is recorded as audit events

Every upload, download, replacement, delete, and share action should produce an audit record or event log entry. The audit model is designed to support admin reporting without exposing implementation details to end users.

### Rationale

- The stakeholder requirement explicitly demands reporting and audit capability.
- The feature is a training example for compliance-friendly business workflows.
- It aligns with the repository’s emphasis on security and traceability.

### Alternatives considered

- Logging only to application text files: rejected because it is harder to query and less aligned with a relational application model.
- No audit trail: rejected because reporting and compliance are core feature requirements.

## Decision: Search and list views should be permission-aware

List and search results must be filtered through the current user and access policy before data is rendered.

### Rationale

- The repository already uses service-layer authorization patterns.
- Search must not leak documents outside the user’s permitted scope.
- The user requirement explicitly states that results must only include accessible documents.

### Alternatives considered

- Filtering only in the UI: rejected because it is not a secure boundary.
- Returning all documents and relying on page-level checks: rejected because it fails the security model and is not appropriate for training or production patterns.

## Decision: Async malware scanning should use Azure Functions + Queue Storage

File uploads will enqueue a message for a background virus-scanning job. When the application is running in a cloud-ready environment, an Azure Function will subscribe to a Queue Storage trigger, process the file, and update the document’s scan state asynchronously.

### Rationale

- The business requirement states the system must scan uploaded files for viruses and malware before storage.
- Async queue processing prevents a slow antivirus scan from blocking the user upload workflow.
- Azure Functions with Queue Storage is a clear migration path consistent with the existing abstraction-first design.
- The queue-based workflow keeps the app responsive while preserving auditability and security enforcement.

### Alternatives considered

- Synchronous scan on the web request thread: rejected because it would delay uploads and degrade user experience.
- Polling from the UI: rejected because it is inefficient and not aligned with a cloud-ready background-processing pattern.
- Local-only scan after upload without queue abstraction: rejected because it would not support the future Azure migration story described by the feature.

## Open implementation notes

- The current repository has no existing test project. The feature should add or extend a test project for service-level authorization and file validation tests.
- The app still uses the current mock authentication claims model, so all document authorization checks must use the existing `NameIdentifier`, `Role`, and project membership data.
- The project should not add cloud SDK dependencies during the training implementation.
