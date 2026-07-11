namespace Application.DTOs;

public class PageFilterDto
{
    public string? ViewOrder { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}