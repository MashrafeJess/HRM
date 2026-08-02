namespace Domain.Models;

public partial class AiauditLog
{
    public long AiauditLogId { get; set; }

    public long? CompanyId { get; set; }

    public long? EmployeeId { get; set; }

    public string? FeatureName { get; set; }

    public int? PromptLength { get; set; }

    public int? TokensUsed { get; set; }

    public bool? Success { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CreatedAt { get; set; }
}
