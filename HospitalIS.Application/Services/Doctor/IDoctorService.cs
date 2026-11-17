using HospitalIS.Application.DTOs.Doctor;

namespace HospitalIS.Application.Services.Doctor;

public interface IDoctorService
{
    Task<IEnumerable<DoctorDto>> GetAllAsync();
    Task<DoctorDto?> GetByIdAsync(long id);
    Task<DoctorDto> CreateAsync(CreateDoctorDto dto);
    Task<DoctorDto> UpdateAsync(long id, CreateDoctorDto dto);
    Task DeleteAsync(long id);
}