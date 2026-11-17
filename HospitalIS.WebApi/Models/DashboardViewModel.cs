namespace HospitalIS.WebApi.Models;

public class DashboardViewModel
{
    public int DoctorsCount { get; set; }
    public int PatientsCount { get; set; }
    public int SpecializationsCount { get; set; }
    public int DiagnosesCount { get; set; }
    public int TotalRecords { get; set; }
    public List<SpecLoad> Load { get; set; } = new();
    public List<RecentPatient> Recent { get; set; } = new();
    public List<DiagnosisRow> Diagnoses { get; set; } = new();

    public record SpecLoad(string Name, int Count, string Hue, int Percent);
    public record RecentPatient(long Id, string FullName, string Birth, string Policy);
    public record DiagnosisRow(string Code, string Name, string Category);
}