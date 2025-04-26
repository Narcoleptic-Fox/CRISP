namespace CRISP.Models
{
    /// <summary>
    /// Base abstract record for all domain models in the CRISP architecture.
    /// </summary>
    /// <remarks>
    /// This class serves as the foundation for all model classes in the system,
    /// providing a common type that can be used for generic constraints.
    /// Derive from this class to create domain-specific model types.
    /// </remarks>
    public abstract record BaseModel;
}
