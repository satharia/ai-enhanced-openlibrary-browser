namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging
{
    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest reqeust, CancellationToken cancellationToken);
    }
}
