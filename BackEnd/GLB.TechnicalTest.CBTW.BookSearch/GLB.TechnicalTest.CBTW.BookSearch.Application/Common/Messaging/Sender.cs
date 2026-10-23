using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging
{
    public sealed class Sender(IServiceProvider provider) : ISender
    {
        private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var wrapper = (RequestWrapper<TResponse>)Wrappers.GetOrAdd(request.GetType(), static (requestType, responseType) => {
                var wrapperType = typeof(RequestWrapperImpl<,>).MakeGenericType(requestType, responseType);
                return Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Failed to instantiate RequestWrapper for {typeof(TResponse).Name}");
            }, typeof(TResponse));

            return wrapper.Handle(request, provider, cancellationToken);
        }

        private abstract class RequestWrapper<TResponse>
        {
            public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider provider, CancellationToken cancellationToken);
        }

        private sealed class RequestWrapperImpl<TRequest, TResponse> : RequestWrapper<TResponse> where TRequest : IRequest<TResponse>
        {
            public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider provider, CancellationToken cancellationToken)
            {
                var handler = provider.GetService<IRequestHandler<TRequest, TResponse>>()
                    ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}");

                var castRequest = (TRequest)request;

                // Run main handler
                RequestHandlerDelegate<TResponse> next = () =>
                    handler.Handle(castRequest, cancellationToken);

                // Run behaviors
                foreach (var behavior in provider.GetServices<IPipelineBehavior<TRequest, TResponse>>())
                {
                    var inner = next;
                    next = () => behavior.Handle(castRequest, inner, cancellationToken);
                }

                return next();
            }
        }
    }
}
