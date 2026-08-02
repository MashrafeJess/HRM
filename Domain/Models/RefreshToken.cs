namespace Domain.Models;

public partial class RefreshToken
{
    public long RefreshTokenId { get; set; }

    public string RefreshToken1 { get; set; } = null!;

    public long EmployeeId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsRevoked { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
