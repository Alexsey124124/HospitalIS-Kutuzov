using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Application.DTOs.Doctor;

public class CreateDoctorDto
{
    [Required(ErrorMessage = "Укажите фамилию")]
    public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Укажите имя")]
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    [Range(1, long.MaxValue, ErrorMessage = "Выберите специальность")]
    public long SpecializationId { get; set; }
    [Required(ErrorMessage = "Укажите кабинет")]
    public string OfficeNumber { get; set; } = string.Empty;
}