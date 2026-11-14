namespace HospitalIS.Application.DTOs.Doctor;

public class CreateDoctorDto
{
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public long SpecializationId { get; set; }
    public string OfficeNumber { get; set; } = string.Empty;
}