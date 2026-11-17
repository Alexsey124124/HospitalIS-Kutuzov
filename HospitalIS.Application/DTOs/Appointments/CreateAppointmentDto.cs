using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Application.DTOs.Appointments;

public class CreateAppointmentDto
{
    [Range(1, long.MaxValue, ErrorMessage = "Выберите пациента")]
    public long PatientId { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "Выберите врача")]
    public long DoctorId { get; set; }

    [Required(ErrorMessage = "Укажите дату и время приёма")]
    public DateTime AppointmentDateTime { get; set; } = DateTime.Now.AddDays(1);
}