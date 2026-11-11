namespace HospitalIS.Domain.Entities;

public class MedicalRecord
{
    public long Id { get; set; }
    public long ConsultationId { get; set; }
    public string TreatmentText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Consultation Consultation { get; set; } = null!;
}