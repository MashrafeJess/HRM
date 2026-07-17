using MediatR;

namespace Application.DTOs;

public class RoleDto : IRequest
{
    public long? RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }
}