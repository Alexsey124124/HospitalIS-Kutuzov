using HospitalIS.Application.DTOs.Diagnoses;
using HospitalIS.Infrastructure.Repositories;
using ConsultationEntity = HospitalIS.Domain.Entities.Consultation;
using DiagnosisEntity = HospitalIS.Domain.Entities.Diagnosis;
using DispensaryEntity = HospitalIS.Domain.Entities.DispensaryRecord;

namespace HospitalIS.Application.Services.Diagnoses;

public interface IDiagnosisService
{
    Task<IEnumerable<DiagnosisDto>> GetAllAsync();
    Task<DiagnosisDto?> GetByIdAsync(long id);
    Task<DiagnosisDto> CreateAsync(CreateDiagnosisDto dto);
    Task<DiagnosisDto> UpdateAsync(long id, CreateDiagnosisDto dto);
    Task DeleteAsync(long id);
}

public class DiagnosisService : IDiagnosisService
{
    private readonly IRepository<DiagnosisEntity> _diagnoses;
    private readonly IRepository<ConsultationEntity> _consultations;
    private readonly IRepository<DispensaryEntity> _dispensary;

    public DiagnosisService(
        IRepository<DiagnosisEntity> diagnoses,
        IRepository<ConsultationEntity> consultations,
        IRepository<DispensaryEntity> dispensary)
    {
        _diagnoses = diagnoses;
        _consultations = consultations;
        _dispensary = dispensary;
    }

    public async Task<IEnumerable<DiagnosisDto>> GetAllAsync()
    {
        var usage = await UsageMapAsync();
        var list = await _diagnoses.GetAllAsync();
        return list.OrderBy(d => d.MKBCode).Select(d => Map(d, usage));
    }

    public async Task<DiagnosisDto?> GetByIdAsync(long id)
    {
        var d = await _diagnoses.GetByIdAsync(id);
        if (d == null) return null;
        return Map(d, await UsageMapAsync());
    }

    public async Task<DiagnosisDto> CreateAsync(CreateDiagnosisDto dto)
    {
        var code = dto.MKBCode.Trim().ToUpperInvariant();
        if ((await _diagnoses.FindAsync(d => d.MKBCode == code)).Any())
            throw new Exception("Диагноз с таким кодом МКБ уже существует");

        var entity = new DiagnosisEntity
        {
            MKBCode = code,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? "",
            Category = dto.Category?.Trim() ?? ""
        };
        await _diagnoses.AddAsync(entity);
        await _diagnoses.SaveChangesAsync();
        return Map(entity, new Dictionary<long, int>());
    }

    public async Task<DiagnosisDto> UpdateAsync(long id, CreateDiagnosisDto dto)
    {
        var entity = await _diagnoses.GetByIdAsync(id) ?? throw new Exception("Диагноз не найден");
        var code = dto.MKBCode.Trim().ToUpperInvariant();
        if ((await _diagnoses.FindAsync(d => d.MKBCode == code && d.Id != id)).Any())
            throw new Exception("Диагноз с таким кодом МКБ уже существует");

        entity.MKBCode = code;
        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim() ?? "";
        entity.Category = dto.Category?.Trim() ?? "";
        await _diagnoses.UpdateAsync(entity);
        await _diagnoses.SaveChangesAsync();
        return Map(entity, await UsageMapAsync());
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _diagnoses.GetByIdAsync(id) ?? throw new Exception("Диагноз не найден");
        var usage = await UsageMapAsync();
        if (usage.TryGetValue(id, out var count) && count > 0)
            throw new Exception($"Диагноз используется в {count} записях (консультации/диспансер) — удаление невозможно");
        await _diagnoses.DeleteAsync(entity);
        await _diagnoses.SaveChangesAsync();
    }

    private async Task<Dictionary<long, int>> UsageMapAsync()
    {
        var map = new Dictionary<long, int>();
        foreach (var c in await _consultations.GetAllAsync())
            if (c.DiagnosisId != null)
                map[c.DiagnosisId.Value] = map.GetValueOrDefault(c.DiagnosisId.Value) + 1;
        foreach (var r in await _dispensary.GetAllAsync())
            map[r.DiagnosisId] = map.GetValueOrDefault(r.DiagnosisId) + 1;
        return map;
    }

    private static DiagnosisDto Map(DiagnosisEntity d, Dictionary<long, int> usage) => new()
    {
        Id = d.Id,
        MKBCode = d.MKBCode,
        Name = d.Name,
        Description = d.Description,
        Category = d.Category,
        UsageCount = usage.GetValueOrDefault(d.Id)
    };
}