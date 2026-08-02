namespace Domain.Models;

public partial class Aireport
{
    public long AireportId { get; set; }

    public long? CompanyId { get; set; }

    public string? ReportType { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public string? ReportContent { get; set; }

    public long? GeneratedBy { get; set; }

    public DateTime? GeneratedAt { get; set; }
}
