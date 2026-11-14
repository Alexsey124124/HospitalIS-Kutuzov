namespace HospitalIS.Application.DTOs.Doctor;

public class DoctorDto
{
    public long Id { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string SpecializationName { get; set; } = string.Empty; 
    public string OfficeNumber { get; set; } = string.Empty;
}