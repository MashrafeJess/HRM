namespace Application.Common;

public static class PaginationDefaults
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 100;

    public static (int PageNumber, int PageSize) Normalize(int? pageNumber, int? pageSize)
    {
        var normalizedPageNumber = pageNumber is null or <= 0 ? DefaultPageNumber : pageNumber.Value;
        var normalizedPageSize = pageSize is null or <= 0 ? DefaultPageSize : Math.Min(pageSize.Value, MaxPageSize);
        return (normalizedPageNumber, normalizedPageSize);
    }
}
