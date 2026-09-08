namespace NapsterAI.Api.Exceptions;

/// <summary>
/// Thrown when a requested Real Estate resource (project/unit type/inventory/amenity/...)
/// does not exist. Additive alongside NapsterResourceNotFoundException - the Real Estate
/// module has its own local persistence, so it gets its own exception rather than reusing
/// the Napster-integration-specific one.
/// </summary>
public class RealEstateResourceNotFoundException : Exception
{
    public RealEstateResourceNotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// Thrown when a Real Estate write would violate a uniqueness/state constraint
/// (e.g. a duplicate ProjectCode, or a duplicate UnitNumber within a project).
/// </summary>
public class RealEstateConflictException : Exception
{
    public RealEstateConflictException(string message) : base(message)
    {
    }
}
