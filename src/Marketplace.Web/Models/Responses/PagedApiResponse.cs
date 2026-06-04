using Marketplace.Application.DTOs.Common;

namespace Marketplace.Web.Models.Responses;

public class PagedApiResponse<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }

    public PagedApiResponse(PagedResult<T> result)
    {
        Items = result.Items;
        TotalCount = result.TotalCount;
        Page = result.Page;
        PageSize = result.PageSize;
        TotalPages = result.TotalPages;
        HasPreviousPage = result.HasPreviousPage;
        HasNextPage = result.HasNextPage;
    }
}
