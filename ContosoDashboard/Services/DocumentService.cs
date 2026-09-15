using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public class DocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".ppt",
        ".pptx",
        ".txt",
        ".png",
        ".jpg",
        ".jpeg"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain",
        "image/png",
        "image/jpeg"
    };

    public DocumentService(ApplicationDbContext context, IFileStorageService fileStorageService, INotificationService notificationService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
    }

    public async Task<List<Document>> GetAccessibleDocumentsAsync(int projectId, int userId)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);

        if (project == null)
        {
            return new List<Document>();
        }

        var isManager = project.ProjectManagerId == userId;
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == userId);

        if (!isManager && !isMember)
        {
            return new List<Document>();
        }

        return await _context.Documents
            .Where(d => d.ProjectId == projectId && !d.IsDeleted)
            .Include(d => d.Project)
            .Include(d => d.UploadedByUser)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<Document>> GetUserDocumentsAsync(int userId)
    {
        return await _context.Documents
            .Where(d => d.UploadedByUserId == userId && !d.IsDeleted)
            .Include(d => d.Project)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<UploadValidationResult> ValidateUploadAsync(byte[] fileBytes, string fileName, string contentType, int projectId, int userId)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new UploadValidationResult(false, "A file name is required.");
        }

        if (fileBytes.Length == 0)
        {
            return new UploadValidationResult(false, "The uploaded file is empty.");
        }

        if (fileBytes.Length > 25 * 1024 * 1024)
        {
            return new UploadValidationResult(false, "The file exceeds the 25 MB maximum size.");
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return new UploadValidationResult(false, "Unsupported file type. Please upload a supported document or image.");
        }

        if (!AllowedContentTypes.Contains(contentType.Trim()))
        {
            return new UploadValidationResult(false, "Unsupported content type.");
        }

        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);

        if (project == null)
        {
            return new UploadValidationResult(false, "Project not found.");
        }

        var isManager = project.ProjectManagerId == userId;
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == userId);
        if (!isManager && !isMember)
        {
            return new UploadValidationResult(false, "You do not have access to upload files to this project.");
        }

        return new UploadValidationResult(true, string.Empty);
    }

    public async Task<Document?> UploadDocumentAsync(
        int uploadedByUserId,
        int projectId,
        string title,
        string category,
        string? description,
        string? tags,
        Stream fileStream,
        string fileName,
        string contentType)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var trimmedTitle = title.Trim();
        if (string.IsNullOrWhiteSpace(trimmedTitle))
        {
            return null;
        }

        var normalizedCategory = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();

        var bytes = await ReadAllBytesAsync(fileStream);
        var validation = await ValidateUploadAsync(bytes, fileName, contentType, projectId, uploadedByUserId);
        if (!validation.IsValid)
        {
            return null;
        }

        var safeFileName = Path.GetFileName(fileName);
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        var relativePath = await _fileStorageService.UploadAsync(fileStream, safeFileName, contentType);

        var document = new Document
        {
            Title = trimmedTitle,
            Category = normalizedCategory,
            Description = description,
            Tags = tags,
            UploadedByUserId = uploadedByUserId,
            ProjectId = projectId,
            FileName = safeFileName,
            FilePath = relativePath,
            FileType = contentType,
            FileSizeBytes = bytes.Length,
            UploadedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ScanStatus = Models.DocumentScanStatus.Pending,
            IsDeleted = false,
            IsShared = false
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        var message = new DocumentScanQueueMessage
        {
            DocumentId = document.DocumentId,
            FilePath = document.FilePath,
            FileName = document.FileName,
            EnqueuedAtUtc = DateTime.UtcNow
        };

        await _notificationService.CreateNotificationAsync(new Notification
        {
            UserId = uploadedByUserId,
            Title = "Document uploaded",
            Message = $"{title} is queued for security scanning.",
            Type = NotificationType.SystemAnnouncement,
            Priority = NotificationPriority.Informational
        });

        return document;
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        return buffer.ToArray();
    }
}

public class UploadValidationResult
{
    public UploadValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public bool IsValid { get; }
    public string ErrorMessage { get; }
}
