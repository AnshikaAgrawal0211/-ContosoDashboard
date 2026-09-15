namespace ContosoDashboard.Services;

public class DocumentScanQueueMessage
{
    public int DocumentId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime EnqueuedAtUtc { get; set; } = DateTime.UtcNow;
}
