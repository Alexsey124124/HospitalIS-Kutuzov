namespace HospitalIS.Domain.Entities;

public class DispensaryRecord
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    public long DiagnosisId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime NextVisitDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Diagnosis Diagnosis { get; set; } = null!;
}