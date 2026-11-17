using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Application.DTOs.Diagnoses;

public class DiagnosisDto
{
    public long Id { get; set; }
    public string MKBCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}

public class CreateDiagnosisDto
{
    [Required(ErrorMessage = "Укажите код МКБ-10")]
    [StringLength(10, ErrorMessage = "Код МКБ не длиннее 10 символов")]
    public string MKBCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите название")]
    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}