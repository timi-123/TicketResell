using System.ComponentModel.DataAnnotations;

namespace TicketResell.Models;

public class TicketFile
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [StringLength(260)] public string FileName { get; set; } = string.Empty;
    [StringLength(100)] public string StoredName { get; set; } = string.Empty;
    [StringLength(100)] public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}
