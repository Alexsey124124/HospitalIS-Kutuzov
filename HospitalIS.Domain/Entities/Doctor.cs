namespace HospitalIS.Domain.Entities;

public class Doctor : User
{
    public long SpecializationId { get; set; }
    public string OfficeNumber { get; set; } = string.Empty;

    public Specialization Specialization { get; set; } = null!;
    public ICollection<AppointmentRequest> AppointmentRequests { get; set; } = new List<AppointmentRequest>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<DispensaryRecord> DispensaryRecords { get; set; } = new List<DispensaryRecord>();
}