using HospitalIS.Application.DTOs.Doctor;
using HospitalIS.Infrastructure.Repositories;
using DoctorEntity = HospitalIS.Domain.Entities.Doctor;
using SpecializationEntity = HospitalIS.Domain.Entities.Specialization;

namespace HospitalIS.Application.Services.Doctor;

public class DoctorService : IDoctorService
{
    private readonly IRepository<DoctorEntity> _doctorRepository;
    private readonly IRepository<SpecializationEntity> _specializationRepository;

    public DoctorService(
        IRepository<DoctorEntity> doctorRepository,
        IRepository<SpecializationEntity> specializationRepository)
    {
        _doctorRepository = doctorRepository;
        _specializationRepository = specializationRepository;
    }

    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
    {
        var doctors = await _doctorRepository.GetAllAsync();
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
                SpecializationId = doc.SpecializationId,
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
            SpecializationId = doc.SpecializationId,
            SpecializationName = spec?.Name ?? "Не указана"
        };
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto)
    {
        var spec = await _specializationRepository.GetByIdAsync(dto.SpecializationId);
        if (spec == null) throw new Exception("Специальность не найдена");

        var newDoctor = new DoctorEntity
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            SpecializationId = dto.SpecializationId,
            OfficeNumber = dto.OfficeNumber,
            Login = dto.LastName + Guid.NewGuid().ToString("N").Substring(0, 6),
            PasswordHash = "changeme",
            Role = "Doctor",
            IsActive = true,
            RegistrationDate = DateTime.Now,
            Phone = "+70000000000"
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
            SpecializationId = newDoctor.SpecializationId,
            SpecializationName = spec.Name
        };
    }

    public async Task<DoctorDto> UpdateAsync(long id, CreateDoctorDto dto)
    {
        var doc = await _doctorRepository.GetByIdAsync(id);
        if (doc == null) throw new Exception("Врач не найден");

        var spec = await _specializationRepository.GetByIdAsync(dto.SpecializationId);
        if (spec == null) throw new Exception("Специальность не найдена");

        doc.FirstName = dto.FirstName;
        doc.LastName = dto.LastName;
        doc.MiddleName = dto.MiddleName;
        doc.SpecializationId = dto.SpecializationId;
        doc.OfficeNumber = dto.OfficeNumber;

        await _doctorRepository.UpdateAsync(doc);
        await _doctorRepository.SaveChangesAsync();

        return new DoctorDto
        {
            Id = doc.Id,
            FirstName = doc.FirstName,
            LastName = doc.LastName,
            MiddleName = doc.MiddleName,
            OfficeNumber = doc.OfficeNumber,
            SpecializationId = doc.SpecializationId,
            SpecializationName = spec.Name
        };
    }

    public async Task DeleteAsync(long id)
    {
        var doc = await _doctorRepository.GetByIdAsync(id);
        if (doc == null) return;
        await _doctorRepository.DeleteAsync(doc);
        await _doctorRepository.SaveChangesAsync();
    }
}