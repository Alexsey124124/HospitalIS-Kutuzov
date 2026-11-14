using HospitalIS.Application.DTOs.Doctor;
using HospitalIS.Domain.Entities;
using HospitalIS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Application.Services.Doctor;

public class DoctorService : IDoctorService
{
    private readonly IRepository<Domain.Entities.Doctor> _doctorRepository;
    private readonly IRepository<Specialization> _specializationRepository;

    public DoctorService(
        IRepository<Domain.Entities.Doctor> doctorRepository,
        IRepository<Specialization> specializationRepository)
    {
        _doctorRepository = doctorRepository;
        _specializationRepository = specializationRepository;
    }

    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
    {
        
        var doctors = await _doctorRepository.FindAsync(d => true);
        

        var result = new List<DoctorDto>();
        foreach (var doc in doctors)
        {
            var spec = await _specializationRepository.GetByIdAsync(doc.SpecializationId);
            result.Add(new DoctorDto
            {
                Id = doc.Id,
                FirstName = doc.FirstName,
                LastName = doc.LastName,
                MiddleName = doc.MiddleName,
                OfficeNumber = doc.OfficeNumber,
                SpecializationName = spec?.Name ?? "Не указана"
            });
        }
        return result;
    }

    public async Task<DoctorDto?> GetByIdAsync(long id)
    {
        var doc = await _doctorRepository.GetByIdAsync(id);
        if (doc == null) return null;

        var spec = await _specializationRepository.GetByIdAsync(doc.SpecializationId);
        return new DoctorDto
        {
            Id = doc.Id,
            FirstName = doc.FirstName,
            LastName = doc.LastName,
            MiddleName = doc.MiddleName,
            OfficeNumber = doc.OfficeNumber,
            SpecializationName = spec?.Name ?? "Не указана"
        };
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto)
    {
        
        var spec = await _specializationRepository.GetByIdAsync(dto.SpecializationId);
        if (spec == null)
            throw new Exception("Специальность не найдена");

        var newDoctor = new Domain.Entities.Doctor
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            SpecializationId = dto.SpecializationId,
            OfficeNumber = dto.OfficeNumber
        };

        await _doctorRepository.AddAsync(newDoctor);
        await _doctorRepository.SaveChangesAsync();

        return new DoctorDto
        {
            Id = newDoctor.Id,
            FirstName = newDoctor.FirstName,
            LastName = newDoctor.LastName,
            MiddleName = newDoctor.MiddleName,
            OfficeNumber = newDoctor.OfficeNumber,
            SpecializationName = spec.Name
        };
    }
}