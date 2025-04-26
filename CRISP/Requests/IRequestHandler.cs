using CRISP.Responses;

namespace CRISP.Requests
{
    public interface IRequestHandler<in TRequest> where TRequest : IRequest
    {
        ValueTask Handle(TRequest request);
    }
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResponse
    {
        ValueTask<TResponse> Handle(TRequest request);
    }
}
