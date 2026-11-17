namespace HospitalIS.Application.DTOs.Appointments;

public class AppointmentDto
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public long DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime AppointmentDateTime { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}