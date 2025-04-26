using CRISP.Requests;
using CRISP.Responses;

namespace CRISP.Samples
{
    public class SampleRequest : IRequest<SampleResponse>
    {
        public string Query { get; set; } = string.Empty;
    }

    public class SampleResponse : IResponse
    {
        public string Result { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}