using HospitalIS.Application.DTOs.Patients;
using HospitalIS.Infrastructure.Repositories;
using PatientEntity = HospitalIS.Domain.Entities.Patient;

namespace HospitalIS.Application.Services.Patients;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllAsync();
    Task<PatientDto?> GetByIdAsync(long id);
    Task<PatientDto> CreateAsync(CreatePatientDto dto);
    Task<PatientDto> UpdateAsync(long id, CreatePatientDto dto);
    Task DeleteAsync(long id);
}

public class PatientService : IPatientService
{
    private readonly IRepository<PatientEntity> _repository;
    public PatientService(IRepository<PatientEntity> repository) => _repository = repository;

    private static PatientDto Map(PatientEntity p) => new()
    {
        Id = p.Id,
        LastName = p.LastName,
        FirstName = p.FirstName,
        MiddleName = p.MiddleName,
        BirthDate = p.BirthDate,
        PolicyNumber = p.PolicyNumber,
        Phone = p.Phone,
        Email = p.Email
    };

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
        => (await _repository.GetAllAsync()).Select(Map);

    public async Task<PatientDto?> GetByIdAsync(long id)
    {
        var p = await _repository.GetByIdAsync(id);
        return p == null ? null : Map(p);
    }

    public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
    {
        var p = new PatientEntity
        {
            LastName = dto.LastName,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            BirthDate = dto.BirthDate,
            PolicyNumber = dto.PolicyNumber,
            Phone = dto.Phone ?? "",
            Email = dto.Email ?? "",
            Login = dto.LastName + Guid.NewGuid().ToString("N")[..6],
            PasswordHash = "changeme",
            Role = "Patient",
            IsActive = true,
            RegistrationDate = DateTime.Now
        };
        await _repository.AddAsync(p);
        await _repository.SaveChangesAsync();
        return Map(p);
    }

    public async Task<PatientDto> UpdateAsync(long id, CreatePatientDto dto)
    {
        var p = await _repository.GetByIdAsync(id) ?? throw new Exception("Пациент не найден");
        p.LastName = dto.LastName; p.FirstName = dto.FirstName; p.MiddleName = dto.MiddleName;
        p.BirthDate = dto.BirthDate; p.PolicyNumber = dto.PolicyNumber;
        p.Phone = dto.Phone ?? ""; p.Email = dto.Email ?? "";
        await _repository.UpdateAsync(p);
        await _repository.SaveChangesAsync();
        return Map(p);
    }

    public async Task DeleteAsync(long id)
    {
        var p = await _repository.GetByIdAsync(id);
        if (p == null) return;
        await _repository.DeleteAsync(p);
        await _repository.SaveChangesAsync();
    }
}