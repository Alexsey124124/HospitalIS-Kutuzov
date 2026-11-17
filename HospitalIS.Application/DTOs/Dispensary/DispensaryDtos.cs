using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Application.DTOs.Dispensary;

public static class DispensaryStatuses
{
    public const string Observation = "Наблюдение";
    public const string Completed = "Завершён";
    public const string Removed = "Снят с учёта";
    public static readonly string[] All = { Observation, Completed, Removed };
}

public record IdNameOption(long Id, string Label);

public class DispensaryRecordDto
{
    public long Id { get; set; }
    public long PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public long DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public long DiagnosisId { get; set; }
    public string DiagnosisName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime NextVisitDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateDispensaryRecordDto
{
    [Range(1, long.MaxValue, ErrorMessage = "Выберите пациента")]
    public long PatientId { get; set; }
    [Range(1, long.MaxValue, ErrorMessage = "Выберите врача")]
    public long DoctorId { get; set; }
    [Range(1, long.MaxValue, ErrorMessage = "Выберите диагноз")]
    public long DiagnosisId { get; set; }
    [Required(ErrorMessage = "Укажите дату постановки на учёт")]
    public DateTime StartDate { get; set; } = DateTime.Now.Date;
    [Required(ErrorMessage = "Укажите дату следующего визита")]
    public DateTime NextVisitDate { get; set; } = DateTime.Now.Date.AddMonths(3);
    public string Status { get; set; } = DispensaryStatuses.Observation;
}

public class UpdateDispensaryRecordDto
{
    [Range(1, long.MaxValue, ErrorMessage = "Выберите диагноз")]
    public long DiagnosisId { get; set; }
    [Required(ErrorMessage = "Укажите дату следующего визита")]
    public DateTime NextVisitDate { get; set; }
    [Required(ErrorMessage = "Выберите статус")]
    public string Status { get; set; } = string.Empty;
}