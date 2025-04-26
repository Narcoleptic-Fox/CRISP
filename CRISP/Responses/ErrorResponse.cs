namespace CRISP.Responses
{
    /// <summary>
    /// Represents an error response returned when an operation fails.
    /// </summary>
    /// <remarks>
    /// This response type provides a standardized way to communicate errors
    /// back to clients. It includes both an HTTP-style status code and a 
    /// human-readable error message describing what went wrong.
    /// </remarks>
    public sealed record ErrorResponse : BaseResponse
    {
        /// <summary>
        /// Gets or sets the status code representing the error type.
        /// </summary>
        /// <remarks>
        /// Typically uses HTTP status codes (e.g., 400 for bad request, 
        /// 404 for not found, 500 for server error).
        /// </remarks>
        public int Status { get; set; }
        
        /// <summary>
        /// Gets or sets a human-readable message describing the error.
        /// </summary>
        public string ErrorMessage { get; set; } = "";
    }
}
