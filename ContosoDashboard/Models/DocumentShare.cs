using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentShare
{
    [Key]
    public int DocumentShareId { get; set; }

    public int DocumentId { get; set; }

    public int SharedWithUserId { get; set; }

    public int SharedByUserId { get; set; }

    public DateTime SharedAt { get; set; } = DateTime.UtcNow;

    public string Permission { get; set; } = "View";

    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;

    [ForeignKey(nameof(SharedWithUserId))]
    public virtual User SharedWithUser { get; set; } = null!;

    [ForeignKey(nameof(SharedByUserId))]
    public virtual User SharedByUser { get; set; } = null!;
}
