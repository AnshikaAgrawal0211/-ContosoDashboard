using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContosoDashboard.Tests;

public class DocumentServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Users.AddRange(
            new User
            {
                UserId = 1,
                Email = "owner@contoso.com",
                DisplayName = "Owner User",
                Department = "Engineering",
                JobTitle = "Engineer",
                Role = UserRole.Employee,
                AvailabilityStatus = AvailabilityStatus.Available
            },
            new User
            {
                UserId = 2,
                Email = "manager@contoso.com",
                DisplayName = "Manager User",
                Department = "Engineering",
                JobTitle = "Project Manager",
                Role = UserRole.ProjectManager,
                AvailabilityStatus = AvailabilityStatus.Available
            },
            new User
            {
                UserId = 3,
                Email = "other@contoso.com",
                DisplayName = "Other User",
                Department = "Engineering",
                JobTitle = "Engineer",
                Role = UserRole.Employee,
                AvailabilityStatus = AvailabilityStatus.Available
            }
        );

        context.Projects.Add(new Project
        {
            ProjectId = 1,
            Name = "Demo Project",
            Description = "Demo",
            ProjectManagerId = 2,
            Status = ProjectStatus.Active
        });

        context.ProjectMembers.Add(new ProjectMember
        {
            ProjectMemberId = 1,
            ProjectId = 1,
            UserId = 1,
            Role = "Developer"
        });

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetProjectDocumentsAsync_ReturnsOnlyAuthorizedDocuments()
    {
        using var context = CreateContext();
        var service = new DocumentService(context, new FakeFileStorageService(), new NotificationService(context));

        context.Documents.Add(new Document
        {
            DocumentId = 1,
            Title = "Project File",
            Category = "Project Documents",
            UploadedByUserId = 1,
            ProjectId = 1,
            FilePath = "1/1/file.pdf",
            FileName = "file.pdf",
            FileType = "application/pdf",
            FileSizeBytes = 1024,
            UploadedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ScanStatus = ContosoDashboard.Models.DocumentScanStatus.Clean,
            IsDeleted = false
        });

        context.Documents.Add(new Document
        {
            DocumentId = 2,
            Title = "Private File",
            Category = "Personal Files",
            UploadedByUserId = 3,
            ProjectId = null,
            FilePath = "3/private/other.pdf",
            FileName = "other.pdf",
            FileType = "application/pdf",
            FileSizeBytes = 2048,
            UploadedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ScanStatus = ContosoDashboard.Models.DocumentScanStatus.Clean,
            IsDeleted = false
        });

        context.SaveChanges();

        var results = await service.GetAccessibleDocumentsAsync(1, 1);

        Assert.Single(results);
        Assert.Equal("Project File", results[0].Title);
    }

    [Fact]
    public async Task ValidateUploadAsync_RejectsUnsupportedType()
    {
        using var context = CreateContext();
        var service = new DocumentService(context, new FakeFileStorageService(), new NotificationService(context));
        var fileBytes = new byte[] { 1, 2, 3 };

        var result = await service.ValidateUploadAsync(fileBytes, "bad.exe", "application/octet-stream", 1, 1);

        Assert.False(result.IsValid);
        Assert.Contains("supported", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
            => Task.FromResult($"uploads/{Guid.NewGuid()}.pdf");

        public Task DeleteAsync(string filePath)
            => Task.CompletedTask;

        public Task<Stream> DownloadAsync(string filePath)
            => Task.FromResult<Stream>(new MemoryStream());

        public Task<string> GetUrlAsync(string filePath, TimeSpan expiration)
            => Task.FromResult($"/files/{filePath}");
    }
}
