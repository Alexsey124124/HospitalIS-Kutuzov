using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Application.DTOs.Specializations;

public class CreateSpecializationDto
{
    [Required(ErrorMessage = "Укажите название")]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}