using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Application.DTOs.Patients;

public class CreatePatientDto
{
    [Required(ErrorMessage = "Укажите фамилию")]
    public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Укажите имя")]
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Укажите дату рождения")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; } = new DateTime(1990, 1, 1);
    [Required(ErrorMessage = "Укажите номер полиса")]
    public string PolicyNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; } = string.Empty;
}