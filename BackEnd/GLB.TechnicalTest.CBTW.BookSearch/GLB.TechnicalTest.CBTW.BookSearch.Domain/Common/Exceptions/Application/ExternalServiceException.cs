namespace GLB.TechnicalTest.CBTW.BookSearch.Domain.Common.Exceptions.Application
{
    public sealed class ExternalServiceException(string service, string message, Exception? innerException = null)
        : Exception($"{service} => {message}", innerException)
    {
        public string Service { get; } = service;
    }
}
