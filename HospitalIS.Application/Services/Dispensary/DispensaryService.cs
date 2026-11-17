using HospitalIS.Application.DTOs.Dispensary;
using HospitalIS.Infrastructure.Repositories;
using DiagnosisEntity = HospitalIS.Domain.Entities.Diagnosis;
using DispensaryEntity = HospitalIS.Domain.Entities.DispensaryRecord;
using DoctorEntity = HospitalIS.Domain.Entities.Doctor;
using PatientEntity = HospitalIS.Domain.Entities.Patient;
using ConsultationEntity = HospitalIS.Domain.Entities.Consultation;

namespace HospitalIS.Application.Services.Dispensary;

public interface IDispensaryService
{
    Task<IEnumerable<DispensaryRecordDto>> GetAllAsync();
    Task<IEnumerable<DispensaryRecordDto>> GetByPatientAsync(long patientId);
    Task<IEnumerable<DispensaryRecordDto>> GetByDoctorAsync(long doctorId);
    Task<DispensaryRecordDto?> GetByIdAsync(long id);
    Task<IEnumerable<IdNameOption>> GetPatientOptionsAsync();
    Task<IEnumerable<IdNameOption>> GetDoctorOptionsAsync();
    Task<IEnumerable<IdNameOption>> GetDiagnosisOptionsAsync();
    Task<DispensaryRecordDto> CreateAsync(CreateDispensaryRecordDto dto, long? actorDoctorId);
    Task<DispensaryRecordDto> UpdateAsync(long id, UpdateDispensaryRecordDto dto);
    Task DeleteAsync(long id);
}

public class DispensaryService : IDispensaryService
{
    private readonly IRepository<DispensaryEntity> _records;
    private readonly IRepository<PatientEntity> _patients;
    private readonly IRepository<DoctorEntity> _doctors;
    private readonly IRepository<DiagnosisEntity> _diagnoses;
    private readonly IRepository<ConsultationEntity> _consultations;

    public DispensaryService(
        IRepository<DispensaryEntity> records,
        IRepository<PatientEntity> patients,
        IRepository<DoctorEntity> doctors,
        IRepository<DiagnosisEntity> diagnoses,
        IRepository<ConsultationEntity> consultations)
    {
        _records = records;
        _patients = patients;
        _doctors = doctors;
        _diagnoses = diagnoses;
        _consultations = consultations;
    }

    public async Task<IEnumerable<DispensaryRecordDto>> GetAllAsync() => await Load(null, null);
    public async Task<IEnumerable<DispensaryRecordDto>> GetByPatientAsync(long patientId) => await Load(patientId, null);
    public async Task<IEnumerable<DispensaryRecordDto>> GetByDoctorAsync(long doctorId) => await Load(null, doctorId);

    public async Task<DispensaryRecordDto?> GetByIdAsync(long id)
    {
        var r = await _records.GetByIdAsync(id);
        if (r == null) return null;
        return (await Load(null, null)).First(x => x.Id == id);
    }

    public async Task<IEnumerable<IdNameOption>> GetPatientOptionsAsync()
        => (await _patients.GetAllAsync()).OrderBy(p => p.LastName)
            .Select(p => new IdNameOption(p.Id, $"{p.LastName} {p.FirstName} {p.MiddleName}".Trim()));

    public async Task<IEnumerable<IdNameOption>> GetDoctorOptionsAsync()
        => (await _doctors.GetAllAsync()).OrderBy(d => d.LastName)
            .Select(d => new IdNameOption(d.Id, $"{d.LastName} {d.FirstName} {d.MiddleName}".Trim()));

    public async Task<IEnumerable<IdNameOption>> GetDiagnosisOptionsAsync()
        => (await _diagnoses.GetAllAsync()).OrderBy(d => d.MKBCode)
            .Select(d => new IdNameOption(d.Id, $"{d.MKBCode} — {d.Name}"));

    public async Task<DispensaryRecordDto> CreateAsync(CreateDispensaryRecordDto dto, long? actorDoctorId)
    {
        var patient = await _patients.GetByIdAsync(dto.PatientId) ?? throw new Exception("Пациент не найден");
        var doctor = await _doctors.GetByIdAsync(dto.DoctorId) ?? throw new Exception("Врач не найден");
        var diagnosis = await _diagnoses.GetByIdAsync(dto.DiagnosisId) ?? throw new Exception("Диагноз не найден");

        if (actorDoctorId != null)
        {
            if (dto.DoctorId != actorDoctorId) throw new Exception("Можно поставить на учёт только от своего имени");
            var hadConsultation = (await _consultations.FindAsync(c =>
                c.PatientId == dto.PatientId && c.DoctorId == dto.DoctorId)).Any();
            if (!hadConsultation)
                throw new Exception("Постановка на учёт возможна только после проведённой консультации с этим пациентом");
        }

        if (dto.NextVisitDate.Date < dto.StartDate.Date)
            throw new Exception("Дата следующего визита не может быть раньше даты постановки");
        if (!DispensaryStatuses.All.Contains(dto.Status))
            throw new Exception("Недопустимый статус");

        var entity = new DispensaryEntity
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            DiagnosisId = dto.DiagnosisId,
            StartDate = dto.StartDate.Date,
            NextVisitDate = dto.NextVisitDate.Date,
            Status = dto.Status
        };
        await _records.AddAsync(entity);
        await _records.SaveChangesAsync();

        return new DispensaryRecordDto
        {
            Id = entity.Id,
            PatientId = patient.Id,
            PatientName = $"{patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim(),
            DoctorId = doctor.Id,
            DoctorName = $"{doctor.LastName} {doctor.FirstName} {doctor.MiddleName}".Trim(),
            DiagnosisId = diagnosis.Id,
            DiagnosisName = $"{diagnosis.MKBCode} {diagnosis.Name}",
            StartDate = entity.StartDate,
            NextVisitDate = entity.NextVisitDate,
            Status = entity.Status
        };
    }

    public async Task<DispensaryRecordDto> UpdateAsync(long id, UpdateDispensaryRecordDto dto)
    {
        var r = await _records.GetByIdAsync(id) ?? throw new Exception("Запись учёта не найдена");
        var diagnosis = await _diagnoses.GetByIdAsync(dto.DiagnosisId) ?? throw new Exception("Диагноз не найден");
        if (!DispensaryStatuses.All.Contains(dto.Status)) throw new Exception("Недопустимый статус");
        if (dto.NextVisitDate.Date < r.StartDate.Date)
            throw new Exception("Дата следующего визита не может быть раньше даты постановки");

        r.DiagnosisId = dto.DiagnosisId;
        r.NextVisitDate = dto.NextVisitDate.Date;
        r.Status = dto.Status;
        await _records.UpdateAsync(r);
        await _records.SaveChangesAsync();
        return (await Load(null, null)).First(x => x.Id == id);
    }

    public async Task DeleteAsync(long id)
    {
        var r = await _records.GetByIdAsync(id);
        if (r == null) return;
        await _records.DeleteAsync(r);
        await _records.SaveChangesAsync();
    }

    private async Task<List<DispensaryRecordDto>> Load(long? patientId, long? doctorId)
    {
        var patients = (await _patients.GetAllAsync()).ToDictionary(p => p.Id);
        var doctors = (await _doctors.GetAllAsync()).ToDictionary(d => d.Id);
        var diagnoses = (await _diagnoses.GetAllAsync()).ToDictionary(d => d.Id);

        var list = await _records.GetAllAsync();
        return list
            .Where(r => patientId == null || r.PatientId == patientId)
            .Where(r => doctorId == null || r.DoctorId == doctorId)
            .OrderByDescending(r => r.StartDate)
            .Select(r =>
            {
                patients.TryGetValue(r.PatientId, out var p);
                doctors.TryGetValue(r.DoctorId, out var d);
                diagnoses.TryGetValue(r.DiagnosisId, out var diag);
                return new DispensaryRecordDto
                {
                    Id = r.Id,
                    PatientId = r.PatientId,
                    PatientName = p == null ? $"#{r.PatientId}" : $"{p.LastName} {p.FirstName} {p.MiddleName}".Trim(),
                    DoctorId = r.DoctorId,
                    DoctorName = d == null ? $"#{r.DoctorId}" : $"{d.LastName} {d.FirstName} {d.MiddleName}".Trim(),
                    DiagnosisId = r.DiagnosisId,
                    DiagnosisName = diag == null ? "—" : $"{diag.MKBCode} {diag.Name}",
                    StartDate = r.StartDate,
                    NextVisitDate = r.NextVisitDate,
                    Status = r.Status
                };
            })
            .ToList();
    }
}