# Document Storage Contract

## Purpose

This contract defines the storage and file-handling interface expected by the document feature. The implementation is intentionally abstracted so the training app can use local filesystem storage while future cloud migration remains straightforward.

## Interface Contract

```csharp
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteAsync(string filePath);
    Task<Stream> DownloadAsync(string filePath);
    Task<string> GetUrlAsync(string filePath, TimeSpan expiration);
}
```

## Behavior Rules

- File names must be generated server-side and never taken directly from user input.
- A unique GUID-based path must be created before database persistence to avoid duplicate-key and orphan-record issues.
- Paths must be stored relative to the chosen storage root so the application is portable.
- Delete operations must remove both the file and the referenced metadata when the workflow is confirmed.
- Download operations must enforce authorization checks before serving file content.

## Local Implementation Expectations

- Use a dedicated directory outside `wwwroot` such as `AppData/uploads`.
- Store files by a pattern similar to `{userId}/{projectId or "personal"}/{guid}.{ext}`.
- Keep metadata in the database and use the storage service only for filesystem operations.

## Security Guarantees

- Validate extension against a whitelist before upload.
- Enforce the 25 MB file-size limit.
- Reject path traversal attempts and malicious file names.
- Ensure download endpoints are scoped to authorized users only.

## Compatibility Requirement

The service contract must be swappable without changing the document service business flow. This keeps the local training version and future Azure version aligned to a single interface.
