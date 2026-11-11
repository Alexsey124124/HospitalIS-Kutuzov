using HospitalIS.Domain.Enums;

namespace HospitalIS.Domain.Entities;

public class AppointmentRequest
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}