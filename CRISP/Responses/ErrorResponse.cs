namespace CRISP.Responses
{
    public sealed record ErrorResponse : BaseResponse
    {
        public int Status { get; set; }
        public string ErrorMessage { get; set; } = "";
    }
}
