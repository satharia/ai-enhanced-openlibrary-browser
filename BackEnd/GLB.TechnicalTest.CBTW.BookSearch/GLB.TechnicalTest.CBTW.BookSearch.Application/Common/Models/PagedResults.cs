namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Models
{
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
    {
        public static PagedResult<T> NewEmpty(int page, int pageSize)
        {
            return new([], 0, page, pageSize);
        }
    }
}
