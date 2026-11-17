using HospitalIS.Application.DTOs.Appointments;
using HospitalIS.Application.DTOs.Consultations;
using HospitalIS.Domain.Enums;
using HospitalIS.Infrastructure.Repositories;
using AppointmentEntity = HospitalIS.Domain.Entities.AppointmentRequest;
using ConsultationEntity = HospitalIS.Domain.Entities.Consultation;
using DiagnosisEntity = HospitalIS.Domain.Entities.Diagnosis;
using DoctorEntity = HospitalIS.Domain.Entities.Doctor;
using MedicalRecordEntity = HospitalIS.Domain.Entities.MedicalRecord;
using PatientEntity = HospitalIS.Domain.Entities.Patient;

namespace HospitalIS.Application.Services.Consultations;



public interface IConsultationService
{
    Task<IEnumerable<ConsultationDto>> GetAllAsync();
    Task<IEnumerable<ConsultationDto>> GetByPatientAsync(long patientId);
    Task<IEnumerable<ConsultationDto>> GetByDoctorAsync(long doctorId);
    Task<ConsultationDto?> GetByIdAsync(long id);
    Task<IEnumerable<AppointmentDto>> GetFreeAppointmentsAsync(long? doctorId);
    Task<IEnumerable<DiagnosisOption>> GetDiagnosisOptionsAsync();
    Task<ConsultationDto> CreateAsync(CreateConsultationDto dto, long? actorDoctorId);
    Task<ConsultationDto> UpdateAsync(long id, UpdateConsultationDto dto);
    Task DeleteAsync(long id);
}

public class ConsultationService : IConsultationService
{
    private readonly IRepository<ConsultationEntity> _consultations;
    private readonly IRepository<MedicalRecordEntity> _records;
    private readonly IRepository<AppointmentEntity> _appointments;
    private readonly IRepository<PatientEntity> _patients;
    private readonly IRepository<DoctorEntity> _doctors;
    private readonly IRepository<DiagnosisEntity> _diagnoses;

    public ConsultationService(
        IRepository<ConsultationEntity> consultations,
        IRepository<MedicalRecordEntity> records,
        IRepository<AppointmentEntity> appointments,
        IRepository<PatientEntity> patients,
        IRepository<DoctorEntity> doctors,
        IRepository<DiagnosisEntity> diagnoses)
    {
        _consultations = consultations;
        _records = records;
        _appointments = appointments;
        _patients = patients;
        _doctors = doctors;
        _diagnoses = diagnoses;
    }

    public async Task<IEnumerable<ConsultationDto>> GetAllAsync()
        => await Load(null, null);

    public async Task<IEnumerable<ConsultationDto>> GetByPatientAsync(long patientId)
        => await Load(patientId, null);

    public async Task<IEnumerable<ConsultationDto>> GetByDoctorAsync(long doctorId)
        => await Load(null, doctorId);

    public async Task<ConsultationDto?> GetByIdAsync(long id)
    {
        var c = await _consultations.GetByIdAsync(id);
        if (c == null) return null;
        return (await Load(null, null)).First(x => x.Id == id);
    }

    public async Task<IEnumerable<AppointmentDto>> GetFreeAppointmentsAsync(long? doctorId)
    {
        var used = (await _consultations.GetAllAsync()).Select(c => c.AppointmentRequestId).ToHashSet();
        var patients = (await _patients.GetAllAsync()).ToDictionary(p => p.Id);
        var doctors = (await _doctors.GetAllAsync()).ToDictionary(d => d.Id);
        var list = await _appointments.GetAllAsync();

        return list
            .Where(a => !used.Contains(a.Id))
            .Where(a => doctorId == null || a.DoctorId == doctorId)
            .OrderBy(a => a.AppointmentDateTime)
            .Select(a =>
            {
                patients.TryGetValue(a.PatientId, out var p);
                doctors.TryGetValue(a.DoctorId, out var d);
                return new AppointmentDto
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    PatientName = p == null ? $"#{a.PatientId}" : $"{p.LastName} {p.FirstName} {p.MiddleName}".Trim(),
                    DoctorId = a.DoctorId,
                    DoctorName = d == null ? $"#{a.DoctorId}" : $"{d.LastName} {d.FirstName} {d.MiddleName}".Trim(),
                    AppointmentDateTime = a.AppointmentDateTime,
                    Status = (int)a.Status,
                    StatusName = a.Status.ToString(),
                    CreatedAt = a.CreatedAt
                };
            });
    }

    public async Task<IEnumerable<DiagnosisOption>> GetDiagnosisOptionsAsync()
        => (await _diagnoses.GetAllAsync())
            .OrderBy(d => d.MKBCode)
            .Select(d => new DiagnosisOption(d.Id, $"{d.MKBCode} — {d.Name}"));

    public async Task<ConsultationDto> CreateAsync(CreateConsultationDto dto, long? actorDoctorId)
    {
        var appointment = await _appointments.GetByIdAsync(dto.AppointmentId)
            ?? throw new Exception("Запись на приём не найдена");
        if (actorDoctorId != null && appointment.DoctorId != actorDoctorId)
            throw new Exception("Консультацию можно создать только по своему приёму");
        if ((await _consultations.FindAsync(c => c.AppointmentRequestId == dto.AppointmentId)).Any())
            throw new Exception("По этой записи приём уже проведён");

        if (dto.DiagnosisId != null)
        {
            var diag = await _diagnoses.GetByIdAsync(dto.DiagnosisId.Value);
            if (diag == null) throw new Exception("Диагноз не найден");
        }

        var entity = new ConsultationEntity
        {
            AppointmentRequestId = dto.AppointmentId,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            DiagnosisId = dto.DiagnosisId,
            StartDateTime = dto.StartDateTime,
            Status = Enum.GetValues<ConsultationStatus>()[0]
        };
        await _consultations.AddAsync(entity);
        await _consultations.SaveChangesAsync();
        return (await Load(null, null)).First(x => x.Id == entity.Id);
    }

    public async Task<ConsultationDto> UpdateAsync(long id, UpdateConsultationDto dto)
    {
        var c = await _consultations.GetByIdAsync(id) ?? throw new Exception("Консультация не найдена");
        if (!Enum.IsDefined(typeof(ConsultationStatus), dto.Status))
            throw new Exception("Недопустимый статус");

        if (dto.DiagnosisId != null && await _diagnoses.GetByIdAsync(dto.DiagnosisId.Value) == null)
            throw new Exception("Диагноз не найден");

        c.DiagnosisId = dto.DiagnosisId;
        c.Status = (ConsultationStatus)dto.Status;
        await _consultations.UpdateAsync(c);

        // Медзапись 1-к-1: обновляем / создаём / удаляем
        var record = (await _records.FindAsync(r => r.ConsultationId == id)).FirstOrDefault();
        var text = dto.TreatmentText?.Trim() ?? "";
        if (text.Length == 0 && record != null)
        {
            await _records.DeleteAsync(record);
        }
        else if (text.Length > 0 && record != null)
        {
            record.TreatmentText = text;
            await _records.UpdateAsync(record);
        }
        else if (text.Length > 0)
        {
            await _records.AddAsync(new MedicalRecordEntity
            {
                ConsultationId = id,
                TreatmentText = text,
                CreatedAt = DateTime.Now
            });
        }
        await _consultations.SaveChangesAsync();
        return (await Load(null, null)).First(x => x.Id == id);
    }

    public async Task DeleteAsync(long id)
    {
        var record = (await _records.FindAsync(r => r.ConsultationId == id)).FirstOrDefault();
        if (record != null) await _records.DeleteAsync(record);
        var c = await _consultations.GetByIdAsync(id);
        if (c == null) return;
        await _consultations.DeleteAsync(c);
        await _consultations.SaveChangesAsync();
    }

    private async Task<List<ConsultationDto>> Load(long? patientId, long? doctorId)
    {
        var patients = (await _patients.GetAllAsync()).ToDictionary(p => p.Id);
        var doctors = (await _doctors.GetAllAsync()).ToDictionary(d => d.Id);
        var diagnoses = (await _diagnoses.GetAllAsync()).ToDictionary(d => d.Id);
        var records = (await _records.GetAllAsync()).ToDictionary(r => r.ConsultationId);

        var list = await _consultations.GetAllAsync();
        return list
            .Where(c => patientId == null || c.PatientId == patientId)
            .Where(c => doctorId == null || c.DoctorId == doctorId)
            .OrderByDescending(c => c.StartDateTime)
            .Select(c =>
            {
                patients.TryGetValue(c.PatientId, out var p);
                doctors.TryGetValue(c.DoctorId, out var d);
                diagnoses.TryGetValue(c.DiagnosisId ?? 0, out var diag);
                records.TryGetValue(c.Id, out var rec);
                return new ConsultationDto
                {
                    Id = c.Id,
                    AppointmentId = c.AppointmentRequestId,
                    PatientId = c.PatientId,
                    PatientName = p == null ? $"#{c.PatientId}" : $"{p.LastName} {p.FirstName} {p.MiddleName}".Trim(),
                    DoctorId = c.DoctorId,
                    DoctorName = d == null ? $"#{c.DoctorId}" : $"{d.LastName} {d.FirstName} {d.MiddleName}".Trim(),
                    DiagnosisId = c.DiagnosisId,
                    DiagnosisName = diag == null ? "—" : $"{diag.MKBCode} {diag.Name}",
                    StartDateTime = c.StartDateTime,
                    Status = (int)c.Status,
                    StatusName = c.Status.ToString(),
                    TreatmentText = rec?.TreatmentText ?? "",
                    HasRecord = rec != null
                };
            })
            .ToList();
    }
}