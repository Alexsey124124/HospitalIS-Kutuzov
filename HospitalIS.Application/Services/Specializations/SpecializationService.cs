using HospitalIS.Application.DTOs.Specializations;
using HospitalIS.Domain.Entities;
using HospitalIS.Infrastructure.Repositories;

namespace HospitalIS.Application.Services.Specializations;

public interface ISpecializationService { Task<IEnumerable<SpecializationDto>> GetAllAsync(); }

public class SpecializationService : ISpecializationService
{
    private readonly IRepository<Specialization> _repository;
    public SpecializationService(IRepository<Specialization> repository) => _repository = repository;

    public async Task<IEnumerable<SpecializationDto>> GetAllAsync()
    {
        var list = await _repository.GetAllAsync();
        return list.Select(s => new SpecializationDto { Id = s.Id, Name = s.Name });
    }
}