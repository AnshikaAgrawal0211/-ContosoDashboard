# Quickstart Validation Guide

## Prerequisites

- .NET 8 SDK installed
- SQL Server LocalDB available
- Project checked out locally
- Existing ContosoDashboard authentication users available

## Setup

1. Open a terminal in the repository root.
2. Restore the solution dependencies:
   ```powershell
   dotnet restore
   ```
3. Build the project:
   ```powershell
   dotnet build
   ```
4. Run the app:
   ```powershell
   dotnet run --project ContosoDashboard/ContosoDashboard.csproj
   ```
5. Browse to the local app URL and sign in as a seeded user such as `camille.nicole@contoso.com` or `ni.kang@contoso.com`.

## Validation Scenarios

### Scenario 1: Upload a valid document

1. Open the dashboard or a project document view.
2. Choose a valid PDF or Office document under the 25 MB limit.
3. Add a title, category, optional description and tags, and submit the upload.
4. Expected outcome: the upload succeeds, metadata appears in the document list, and the file is stored in the local uploads area without exposing the file to the web root.

### Scenario 2: Reject invalid uploads

1. Try to upload a file type outside the allowed list or a file larger than 25 MB.
2. Expected outcome: the form blocks the upload or returns a clear validation error before saving the document.

### Scenario 3: Access control for project documents

1. Sign in as a project member and view a project document list.
2. Attempt to navigate to a document URL or file endpoint without project access.
3. Expected outcome: access is denied and the app prevents unauthorized retrieval.

### Scenario 4: Share and notification flow

1. Upload a file owned by one user.
2. Share it with another employee.
3. Log in as the recipient.
4. Expected outcome: the shared document appears in the recipient’s shared area and an in-app notification is created.

### Scenario 5: Search and dashboard discovery

1. Search by title, description, or tag.
2. Open the dashboard recent documents area.
3. Expected outcome: only authorized documents are returned and the recent items reflect the user’s own or accessible document history.

## Expected Result

The feature is considered ready when the upload workflow, access enforcement, project visibility, and share behavior all operate as described in the feature spec and maintain the repository’s offline training constraints.
