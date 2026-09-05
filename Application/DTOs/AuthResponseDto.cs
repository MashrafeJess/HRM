namespace Application.DTOs
{
    public record AuthResponseDto(string AccessToken, string RefreshToken,long? EmployeeId, long? CompanyId, long? DepartmentId);
}
