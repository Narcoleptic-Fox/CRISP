namespace CRISP.Responses
{
    /// <summary>
    /// Base abstract record for all response types in the CRISP architecture.
    /// </summary>
    /// <remarks>
    /// This class provides a common base type for responses returned from
    /// commands and queries. Derive from this class to create custom response types
    /// with domain-specific data and behavior.
    /// </remarks>
    public abstract record BaseResponse : IResponse
    {
    }
}
