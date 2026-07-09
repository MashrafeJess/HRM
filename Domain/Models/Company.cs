using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class Company
{
    public long CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string CompanyEmail { get; set; } = null!;

    public string CompanyPhone { get; set; } = null!;

    public string? CompanyAddress { get; set; }

    public string? LogoUrl { get; set; }

    public string? SubscriptionPlan { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
