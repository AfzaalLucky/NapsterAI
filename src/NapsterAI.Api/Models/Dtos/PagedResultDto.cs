namespace NapsterAI.Api.Models.Dtos;

/// <summary>
/// Common paging envelope used by the Napster "list" endpoints (companions, sessions).
/// </summary>
public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int FilteredCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}
