using HospitalIS.Domain.Enums;

namespace HospitalIS.Domain.Entities;

public class Consultation
{
    public long Id { get; set; }
    public long AppointmentRequestId { get; set; }
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    public long? DiagnosisId { get; set; }
    public DateTime StartDateTime { get; set; }
    public ConsultationStatus Status { get; set; }

    public AppointmentRequest AppointmentRequest { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Diagnosis? Diagnosis { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }
}