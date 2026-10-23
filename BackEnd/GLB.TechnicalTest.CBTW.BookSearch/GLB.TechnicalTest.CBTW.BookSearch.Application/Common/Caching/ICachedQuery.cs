namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Caching
{
    public interface ICachedQuery
    {
        string CacheKey { get; }
        TimeSpan TTL { get; }
    }
}
