using HospitalIS.Application.DTOs.Specializations;
using HospitalIS.Domain.Entities;
using HospitalIS.Infrastructure.Repositories;

namespace HospitalIS.Application.Services.Specializations;

public interface ISpecializationService
{
    Task<IEnumerable<SpecializationDto>> GetAllAsync();
    Task<SpecializationDto?> GetByIdAsync(long id);
    Task<SpecializationDto> CreateAsync(CreateSpecializationDto dto);
    Task<SpecializationDto> UpdateAsync(long id, CreateSpecializationDto dto);
    Task DeleteAsync(long id);
}

public class SpecializationService : ISpecializationService
{
    private readonly IRepository<Specialization> _repository;
    public SpecializationService(IRepository<Specialization> repository) => _repository = repository;

    private static SpecializationDto Map(Specialization s) => new() { Id = s.Id, Name = s.Name, Description = s.Description };

    public async Task<IEnumerable<SpecializationDto>> GetAllAsync()
        => (await _repository.GetAllAsync()).Select(Map);

    public async Task<SpecializationDto?> GetByIdAsync(long id)
    {
        var s = await _repository.GetByIdAsync(id);
        return s == null ? null : Map(s);
    }

    public async Task<SpecializationDto> CreateAsync(CreateSpecializationDto dto)
    {
        var s = new Specialization { Name = dto.Name, Description = dto.Description ?? "" };
        await _repository.AddAsync(s);
        await _repository.SaveChangesAsync();
        return Map(s);
    }

    public async Task<SpecializationDto> UpdateAsync(long id, CreateSpecializationDto dto)
    {
        var s = await _repository.GetByIdAsync(id) ?? throw new Exception("Специальность не найдена");
        s.Name = dto.Name; s.Description = dto.Description ?? "";
        await _repository.UpdateAsync(s);
        await _repository.SaveChangesAsync();
        return Map(s);
    }

    public async Task DeleteAsync(long id)
    {
        var s = await _repository.GetByIdAsync(id);
        if (s == null) return;
        await _repository.DeleteAsync(s);
        await _repository.SaveChangesAsync();
    }
}