using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Application.DTOs.Consultations;

public class ConsultationDto
{
    public long Id { get; set; }
    public long AppointmentId { get; set; }
    public long PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public long DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public long? DiagnosisId { get; set; }
    public string DiagnosisName { get; set; } = "—";
    public DateTime StartDateTime { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string TreatmentText { get; set; } = string.Empty;
    public bool HasRecord { get; set; }
}

public class CreateConsultationDto
{
    [Range(1, long.MaxValue, ErrorMessage = "Выберите запись на приём")]
    public long AppointmentId { get; set; }
    public long? DiagnosisId { get; set; }
    [Required(ErrorMessage = "Укажите дату и время начала")]
    public DateTime StartDateTime { get; set; } = DateTime.Now;
}

public class UpdateConsultationDto
{
    public long? DiagnosisId { get; set; }
    public int Status { get; set; }
    public string TreatmentText { get; set; } = string.Empty;
}
public record DiagnosisOption(long Id, string Label);