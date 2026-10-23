namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging
{
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
