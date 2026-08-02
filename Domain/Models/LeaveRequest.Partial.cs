using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public partial class LeaveRequest
{
    [NotMapped]
    public string? ApprovedByName { get; set; }
}
