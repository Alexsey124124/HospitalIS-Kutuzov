using HospitalIS.Application.DTOs.Appointments;
using HospitalIS.Domain.Enums;
using HospitalIS.Infrastructure.Repositories;
using AppointmentEntity = HospitalIS.Domain.Entities.AppointmentRequest;
using PatientEntity = HospitalIS.Domain.Entities.Patient;
using DoctorEntity = HospitalIS.Domain.Entities.Doctor;

namespace HospitalIS.Application.Services.Appointments;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync();
    Task<AppointmentDto?> GetByIdAsync(long id);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    Task UpdateStatusAsync(long id, int status);
    Task DeleteAsync(long id);
    Task<IEnumerable<AppointmentDto>> GetByPatientAsync(long patientId);
    Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(long doctorId);
}

public class AppointmentService : IAppointmentService
{
    private readonly IRepository<AppointmentEntity> _appointments;
    private readonly IRepository<PatientEntity> _patients;
    private readonly IRepository<DoctorEntity> _doctors;

    public AppointmentService(
        IRepository<AppointmentEntity> appointments,
        IRepository<PatientEntity> patients,
        IRepository<DoctorEntity> doctors)
    {
        _appointments = appointments;
        _patients = patients;
        _doctors = doctors;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        
        var patients = (await _patients.GetAllAsync()).ToDictionary(p => p.Id);
        var doctors = (await _doctors.GetAllAsync()).ToDictionary(d => d.Id);
        var list = await _appointments.GetAllAsync();

        return list.OrderBy(a => a.AppointmentDateTime).Select(a => Map(a, patients, doctors));
    }

    public async Task<AppointmentDto?> GetByIdAsync(long id)
    {
        var a = await _appointments.GetByIdAsync(id);
        if (a == null) return null;
        var patients = (await _patients.GetAllAsync()).ToDictionary(p => p.Id);
        var doctors = (await _doctors.GetAllAsync()).ToDictionary(d => d.Id);
        return Map(a, patients, doctors);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        var patient = await _patients.GetByIdAsync(dto.PatientId)
            ?? throw new Exception("Пациент не найден");
        var doctor = await _doctors.GetByIdAsync(dto.DoctorId)
            ?? throw new Exception("Врач не найден");
        if (dto.AppointmentDateTime <= DateTime.Now)
            throw new Exception("Дата приёма должна быть в будущем");

        var firstStatus = Enum.GetValues<AppointmentStatus>().First();
        var entity = new AppointmentEntity
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            AppointmentDateTime = dto.AppointmentDateTime,
            Status = firstStatus,
            CreatedAt = DateTime.Now
        };
        await _appointments.AddAsync(entity);
        await _appointments.SaveChangesAsync();

        return new AppointmentDto
        {
            Id = entity.Id,
            PatientId = patient.Id,
            PatientName = $"{patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim(),
            DoctorId = doctor.Id,
            DoctorName = $"{doctor.LastName} {doctor.FirstName} {doctor.MiddleName}".Trim(),
            AppointmentDateTime = entity.AppointmentDateTime,
            Status = (int)entity.Status,
            StatusName = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task UpdateStatusAsync(long id, int status)
    {
        if (!Enum.IsDefined(typeof(AppointmentStatus), status))
            throw new Exception("Недопустимый статус");
        var a = await _appointments.GetByIdAsync(id)
            ?? throw new Exception("Запись не найдена");
        a.Status = (AppointmentStatus)status;
        await _appointments.UpdateAsync(a);
        await _appointments.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var a = await _appointments.GetByIdAsync(id);
        if (a == null) return;
        await _appointments.DeleteAsync(a);
        await _appointments.SaveChangesAsync();
    }
    public async Task<IEnumerable<AppointmentDto>> GetByPatientAsync(long patientId)
    {
        var patients = (await _patients.GetAllAsync()).ToDictionary(p => p.Id);
        var doctors = (await _doctors.GetAllAsync()).ToDictionary(d => d.Id);
        var list = await _appointments.FindAsync(a => a.PatientId == patientId);
        return list.OrderBy(a => a.AppointmentDateTime).Select(a => Map(a, patients, doctors));
    }

    public async Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(long doctorId)
    {
        var patients = (await _patients.GetAllAsync()).ToDictionary(p => p.Id);
        var doctors = (await _doctors.GetAllAsync()).ToDictionary(d => d.Id);
        var list = await _appointments.FindAsync(a => a.DoctorId == doctorId);
        return list.OrderBy(a => a.AppointmentDateTime).Select(a => Map(a, patients, doctors));
    }
    private static AppointmentDto Map(
        AppointmentEntity a,
        Dictionary<long, PatientEntity> patients,
        Dictionary<long, DoctorEntity> doctors)
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
    }
}