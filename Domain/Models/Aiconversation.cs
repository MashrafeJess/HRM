using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class Aiconversation
{
    public long AiconversationId { get; set; }

    public long? CompanyId { get; set; }

    public long? EmployeeId { get; set; }

    public string? UserPrompt { get; set; }

    public string? Airesponse { get; set; }

    public int? TokensUsed { get; set; }

    public int? ResponseTimeMs { get; set; }

    public DateTime? CreatedAt { get; set; }
}
