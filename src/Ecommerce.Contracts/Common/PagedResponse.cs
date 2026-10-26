namespace Ecommerce.Contracts.Common;

public sealed record PagedRequest(int Page = 1, int PageSize = 20)
{
    public const int MaxPageSize = 100;

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => PageSize is < 1 or > MaxPageSize ? 20 : PageSize;

    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
}

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
