# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`  
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Shared infrastructure and storage scaffolding for document upload processing

- [X] T001 Create the document storage and scan-status folder structure under ContosoDashboard/Services and ContosoDashboard/Pages with the planned local upload directory contract
- [X] T002 [P] Add storage abstraction contract in ContosoDashboard/Services/IFileStorageService.cs for UploadAsync, DeleteAsync, DownloadAsync, and GetUrlAsync
- [X] T003 [P] Add upload and scan configuration placeholders in ContosoDashboard/appsettings.json and ContosoDashboard/appsettings.Development.json for storage path, queue, and local/offline settings

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core data and service infrastructure that MUST be complete before any user story work starts

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create document-domain models in ContosoDashboard/Models/Document.cs, DocumentShare.cs, and AuditEvent.cs with integer keys, metadata fields, and scan-state values
- [X] T005 [P] Extend the EF Core model setup in ContosoDashboard/Data/ApplicationDbContext.cs to include Document, DocumentShare, and AuditEvent and add indexes for user, project, and scan-state lookups
- [X] T006 [P] Implement the local filesystem storage provider in ContosoDashboard/Services/LocalFileStorageService.cs using GUID-based paths under AppData/uploads and secure path generation
- [X] T007 Create the primary upload/service layer in ContosoDashboard/Services/DocumentService.cs with validation, file-type checks, file-size enforcement, and permission-aware query methods
- [X] T008 Add queue-based background scan contract and payload model in ContosoDashboard/Services/DocumentScanQueueMessage.cs and ContosoDashboard/Services/DocumentScanStatus.cs for Azure Queue Storage processing
- [X] T009 Register the storage, queue, and document services in ContosoDashboard/Program.cs and wire the existing authorization policies to the new document workflow

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Upload and organize work documents (Priority: P1) 🎯 MVP

**Goal**: Employees can upload valid documents with metadata, see them in their own list, and keep configurable categories

**Independent Test**: A signed-in employee can upload a PDF or Office document under 25 MB, set the required title and category, and see it appear in the correct document list without unauthorized leakage.

### Implementation for User Story 1

- [ ] T010 [P] [US1] Implement upload validation and metadata capture in ContosoDashboard/Services/DocumentService.cs for title, category, tags, project assignment, file size, MIME type, and upload timestamp handling
- [ ] T011 [US1] Add the document upload page and form flow in ContosoDashboard/Pages/Documents.razor with client-side validation, progress handling, and success/error messaging
- [ ] T012 [US1] Add the user document list view in ContosoDashboard/Pages/Documents.razor to render title, category, date, size, status, and associated project details
- [ ] T013 [US1] Update the dashboard navigation and routing in ContosoDashboard/Shared/NavMenu.razor and ContosoDashboard/Pages/Index.razor to expose the document feature and recent uploads entry points
- [ ] T014 [US1] Connect the upload and list flow to the database and local storage pipeline in ContosoDashboard/Services/DocumentService.cs and ContosoDashboard/Services/LocalFileStorageService.cs so each upload writes metadata and a secure file path before saving

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Access and share project documents with the right team members (Priority: P1)

**Goal**: Users can view project documents, share them with permitted teammates, and download only authorized files

**Independent Test**: A project member can view project documents in context, a document owner can share a file with a specific recipient, and a non-member cannot access the file by direct request.

### Implementation for User Story 2

- [ ] T015 [P] [US2] Add the explicit share model and relationship logic in ContosoDashboard/Models/DocumentShare.cs and ContosoDashboard/Data/ApplicationDbContext.cs
- [ ] T016 [US2] Implement document access checks in ContosoDashboard/Services/DocumentService.cs so ownership, project membership, manager access, and explicit share access are all enforced before view or download
- [ ] T017 [US2] Add the project document view and shared-with-me list in ContosoDashboard/Pages/ProjectDetails.razor and ContosoDashboard/Pages/Documents.razor, including filter and search actions constrained to the user’s permissions
- [ ] T018 [US2] Add notification generation for share events in ContosoDashboard/Services/NotificationService.cs and ContosoDashboard/Pages/Documents.razor so recipients receive in-app alerts for shared documents
- [ ] T019 [US2] Add download and preview authorization handling in ContosoDashboard/Services/DocumentService.cs and the document page/controller pipeline to deny unauthorized attempts without exposing file content

**Checkpoint**: At this point, User Stories 1 and 2 should both work independently and securely

---

## Phase 5: User Story 3 - Review and maintain document records over time (Priority: P2)

**Goal**: Owners and managers can maintain document metadata, replace files, delete records, and review audit activity

**Independent Test**: A document owner or manager can edit metadata, replace a file, and delete a document after confirmation while audit records capture the action.

### Implementation for User Story 3

- [ ] T020 [P] [US3] Implement audit logging and scan-state tracking in ContosoDashboard/Models/AuditEvent.cs, ContosoDashboard/Data/ApplicationDbContext.cs, and ContosoDashboard/Services/DocumentService.cs
- [ ] T021 [US3] Implement metadata edit, replace-file, and delete flows in ContosoDashboard/Services/DocumentService.cs with confirmation safeguards and secure file cleanup
- [ ] T022 [US3] Add the admin/audit reporting view in ContosoDashboard/Pages/Index.razor or ContosoDashboard/Pages/Documents.razor to list upload, download, delete, share, and scan events for administrators
- [ ] T023 [US3] Add document replacement and quarantine handling to the local storage flow in ContosoDashboard/Services/LocalFileStorageService.cs so stale paths are removed and failed scans do not leave unsafe files accessible

**Checkpoint**: At this point, User Story 3 should be independently functional for records management and audit review

---

## Phase 6: User Story 4 - Discover documents quickly from the dashboard (Priority: P2)

**Goal**: Users can locate document history, recent files, and task-linked content from the dashboard and project workflows

**Independent Test**: A user can search by title, description, tag, uploader, or project and see only documents they are authorized to access from dashboard, task, and project views.

### Implementation for User Story 4

- [ ] T024 [P] [US4] Add recent document widgets and summary counts in ContosoDashboard/Pages/Index.razor and ContosoDashboard/Services/DashboardService.cs to show the last five available items and document totals
- [ ] T025 [US4] Implement search and filtering in ContosoDashboard/Services/DocumentService.cs for title, description, tags, uploader, and associated project while enforcing access rules
- [ ] T026 [US4] Add task-to-document association in ContosoDashboard/Pages/Tasks.razor and ContosoDashboard/Services/TaskService.cs so users can attach or view documents related to a project task
- [ ] T027 [US4] Add dashboard or project-level document sorting and category filtering in ContosoDashboard/Pages/Documents.razor and ContosoDashboard/Services/DocumentService.cs for title, upload date, category, and size

**Checkpoint**: At this point, User Stories 1 through 4 should all be independently functional and user-visible

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, security hardening, and feature polish across all user stories

- [ ] T028 [P] Add validation and regression checks in ContosoDashboard/Services/DocumentService.cs for path traversal attacks, blank metadata, unsupported extensions, and duplicate uploads
- [ ] T029 [P] Update the project documentation and quickstart steps in README.md and specs/001-document-upload-management/quickstart.md to describe the upload process, permissions, and local storage requirements
- [ ] T030 Finalize Azure migration readiness by documenting the queue-backed scan flow and interface contract in specs/001-document-upload-management/contracts/document-storage-contract.md and specs/001-document-upload-management/contracts/document-authorization-contract.md
- [ ] T031 Run the end-to-end validation pass against the document feature using the quickstart scenarios and ensure all required access checks keep the offline training environment safe and functional

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User Story 1 (P1) delivers the MVP
  - User Story 2 (P1) adds controlled sharing and access enforcement
  - User Story 3 (P2) adds lifecycle and audit management
  - User Story 4 (P2) adds dashboard discovery and search
- **Polish (Final Phase)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - no dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational and may consume the upload/ownership model from US1, but should remain independently testable
- **User Story 3 (P3)**: Can start after Foundational and should depend only on the underlying document service and audit model
- **User Story 4 (P2)**: Can start after Foundational and may use the uploads, search, and project metadata built in earlier stories

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel within Phase 2
- Once Phase 2 completes, User Story 1 and the service foundation can proceed in parallel with other story setup tasks if staffing allows
- Models and service-level tasks for a user story can be parallelized when different files are involved

---

## Parallel Example: User Story 1

```bash
# Launch model/service setup work in parallel:
Task: "Add the document metadata model in ContosoDashboard/Models/Document.cs"
Task: "Add the upload/storage abstraction in ContosoDashboard/Services/IFileStorageService.cs"

# Launch view work in parallel after service foundations:
Task: "Add the upload form in ContosoDashboard/Pages/Documents.razor"
Task: "Update dashboard navigation in ContosoDashboard/Shared/NavMenu.razor"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Upload, category assignment, and user document listing should all work independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → upload and organize documents
3. Add User Story 2 → controlled sharing and secure access
4. Add User Story 3 → lifecycle and audit management
5. Add User Story 4 → discovery and dashboard integration
6. Final polish and validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once foundational work is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3 / US4 integration
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps each task to the relevant user story for traceability
- Each user story remains independently testable and deployable
- Keep file ownership clear to avoid multi-developer conflicts in the same files
- The Azure Functions + Queue Storage scan job should be treated as a future cloud-migration pattern and a local stub/no-op in offline training mode
