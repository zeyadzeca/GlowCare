namespace GlowCare.API.DTOs.Common;

/// <summary>
/// Standard paged list returned by list endpoints.
/// TotalCount lets the frontend render page controls (total pages = ceil(TotalCount / PageSize)).
/// </summary>
public class PagedResultDto<T>
{
    public IList<T> Items { get; set; } = new List<T>();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
