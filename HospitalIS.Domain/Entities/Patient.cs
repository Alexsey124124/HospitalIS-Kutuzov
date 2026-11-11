namespace HospitalIS.Domain.Entities;

public class Patient : User
{
    public DateTime BirthDate { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<AppointmentRequest> AppointmentRequests { get; set; } = new List<AppointmentRequest>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<DispensaryRecord> DispensaryRecords { get; set; } = new List<DispensaryRecord>();
}