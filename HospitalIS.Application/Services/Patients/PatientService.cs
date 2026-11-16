using HospitalIS.Application.DTOs.Patients;
using HospitalIS.Domain.Entities;
using HospitalIS.Infrastructure.Repositories;

namespace HospitalIS.Application.Services.Patients;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllAsync();
    Task<PatientDto?> GetByIdAsync(long id);
}

public class PatientService : IPatientService
{
    private readonly IRepository<Patient> _repository;
    public PatientService(IRepository<Patient> repository) => _repository = repository;

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        return list.Select(p => new PatientDto { Id = p.Id, LastName = p.LastName, FirstName = p.FirstName, MiddleName = p.MiddleName, BirthDate = p.BirthDate, PolicyNumber = p.PolicyNumber, Phone = p.Phone });
    }

    public async Task<PatientDto?> GetByIdAsync(long id)
    {
        var p = await _repository.GetByIdAsync(id);
        if (p == null) return null;
        return new PatientDto { Id = p.Id, LastName = p.LastName, FirstName = p.FirstName, MiddleName = p.MiddleName, BirthDate = p.BirthDate, PolicyNumber = p.PolicyNumber, Phone = p.Phone };
    }
}