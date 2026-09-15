# Data Model: Document Upload and Management

## Overview

The feature adds document and document-sharing data to the existing ContosoDashboard domain so users can upload files, link them to projects and tasks, share them with peers, and review activity history.

## Core Entities

### Document

Represents a stored file and its associated metadata.

**Fields**
- DocumentId: integer primary key
- Title: required string, user-facing document name
- Description: optional string
- Category: required string label; one of the supported categories
- FileName: generated file name, not user-controlled
- FilePath: stored path or relative file reference used by the file storage abstraction
- FileType: MIME type text, up to 255 characters
- FileSizeBytes: integer size in bytes
- UploadedByUserId: integer user reference
- ProjectId: optional integer project reference
- TaskId: optional integer task reference
- UploadedAt: timestamp
- UpdatedAt: timestamp
- ScanStatus: enum or string state such as Pending, Scanning, Clean, Infected, Failed
- ScannedAt: optional timestamp when the background scan finishes
- IsDeleted: boolean for soft-delete or retention handling when relevant

**Relationships**
- Many-to-one with User (`UploadedByUserId`)
- Many-to-one with Project (`ProjectId`)
- Many-to-one with TaskItem (`TaskId`, when attached to a task)
- One-to-many with DocumentShare
- One-to-many with AuditEvent

**Validation Rules**
- Title is required.
- Category is required and must be one of the supported values.
- File type must be on the allowed list.
- File size must be <= 25 MB per file.
- FilePath must be generated before database persistence; it must not be user-controlled.

### DocumentShare

Tracks explicit sharing access that goes beyond a user’s normal project membership.

**Fields**
- DocumentShareId: integer primary key
- DocumentId: integer document reference
- SharedWithUserId: integer recipient reference
- SharedByUserId: integer owner or manager reference
- SharedAt: timestamp
- AccessLevel: role/permission label such as View or Download
- IsActive: boolean

**Relationships**
- Many-to-one with Document
- Many-to-one with User (recipient)
- Many-to-one with User (owner)

### AuditEvent

Captures document-related activity for audit and reporting.

**Fields**
- AuditEventId: integer primary key
- EventType: upload, download, delete, replace, share, preview
- DocumentId: integer document reference
- UserId: integer actor reference
- ProjectId: optional integer project reference
- EventTime: timestamp
- Details: optional text or metadata for the action

**Relationships**
- Many-to-one with Document
- Many-to-one with User
- Many-to-one with Project

## Existing Entity Integration

### User

The feature extends the existing `User` model with the expectation that a user’s role and project membership determine which documents are available to them. No separate document-role registry is required for the initial feature.

### Project

Projects can own document collections, but the project itself does not own the document file; it only provides a context and permission boundary.

### TaskItem

Tasks can be linked to documents for explicit work-context association, with `TaskId` applied when a document is attached from a task detail view.

## State and Access Model

The system should treat document access as a derived permission model:

- Document owner can manage the file metadata and replacement flow.
- Project manager can manage documents associated with their project.
- Project member can view/download project documents.
- Explicit share recipients can access shared documents.
- Administrators can review all document activity.

This model ensures the feature is consistent with the repository’s existing authorization patterns.
