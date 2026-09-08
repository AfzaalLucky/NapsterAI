namespace NapsterAI.Api.Models.Entities.RealEstate;

/// <summary>
/// Standard audit columns shared by every Real Estate table, matching the
/// CreatedBy/CreatedDate/ModifiedBy/ModifiedDate convention in database\RealEstateDB.sql.
/// </summary>
public abstract class AuditableEntity
{
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
