namespace CRISP.ServiceDefaults.Data.Entities;
public interface IAchievable
{
    bool IsArchived { get; set; }
    string? ArchivingReason { get; set; }
}
