namespace HospitalIS.Domain.Entities;

public class Diagnosis
{
    public long Id { get; set; }
    public string MKBCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<DispensaryRecord> DispensaryRecords { get; set; } = new List<DispensaryRecord>();
}