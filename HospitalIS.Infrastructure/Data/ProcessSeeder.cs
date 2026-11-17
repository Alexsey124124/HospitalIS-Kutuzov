using HospitalIS.Domain.Entities;
using HospitalIS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Infrastructure.Data;

public static class ProcessSeeder
{
    public static async Task SeedAsync(HospitalDbContext context)
    {
        
        if (!await context.AppointmentRequests.AnyAsync())
        {
            var patients = await context.Patients.OrderBy(p => p.Id).Take(3).ToListAsync();
            var doctors = await context.Doctors.OrderBy(d => d.Id).Take(2).ToListAsync();
            if (patients.Count > 0 && doctors.Count > 0)
            {
                var statuses = Enum.GetValues<AppointmentStatus>();
                for (var i = 0; i < patients.Count; i++)
                {
                    context.AppointmentRequests.Add(new AppointmentRequest
                    {
                        PatientId = patients[i].Id,
                        DoctorId = doctors[i % doctors.Count].Id,
                        AppointmentDateTime = DateTime.Now.Date.AddDays(1 + i).AddHours(9 + i * 2),
                        Status = statuses[i % statuses.Length],
                        CreatedAt = DateTime.Now
                    });
                }
                await context.SaveChangesAsync();
            }
        }

        
        if (!await context.Consultations.AnyAsync())
        {
            var appointments = await context.AppointmentRequests.OrderBy(a => a.Id).Take(2).ToListAsync();
            var diagnoses = await context.Diagnoses.OrderBy(d => d.Id).Take(2).ToListAsync();
            if (appointments.Count > 0 && diagnoses.Count > 0)
            {
                var statuses = Enum.GetValues<ConsultationStatus>();
                var treatments = new[]
                {
                    "Назначена медикаментозная терапия, контрольный осмотр через 7 дней.",
                    "Выданы рекомендации, назначено повторное обследование через 30 дней."
                };
                for (var i = 0; i < appointments.Count; i++)
                {
                    var a = appointments[i];
                    var consultation = new Consultation
                    {
                        AppointmentRequestId = a.Id,
                        PatientId = a.PatientId,
                        DoctorId = a.DoctorId,
                        DiagnosisId = diagnoses[i % diagnoses.Count].Id,
                        StartDateTime = a.AppointmentDateTime,
                        Status = statuses[i % statuses.Length]
                    };
                    context.Consultations.Add(consultation);
                    await context.SaveChangesAsync();
                    context.MedicalRecords.Add(new MedicalRecord
                    {
                        ConsultationId = consultation.Id,
                        TreatmentText = treatments[i % treatments.Length],
                        CreatedAt = a.AppointmentDateTime
                    });
                }
                await context.SaveChangesAsync();
                
                if (!await context.DispensaryRecords.AnyAsync())
                {
                    var first = await context.Consultations
                        .Where(c => c.DiagnosisId != null)
                        .OrderBy(c => c.Id)
                        .FirstOrDefaultAsync();
                    if (first != null)
                    {
                        context.DispensaryRecords.Add(new DispensaryRecord
                        {
                            PatientId = first.PatientId,
                            DoctorId = first.DoctorId,
                            DiagnosisId = first.DiagnosisId!.Value,
                            StartDate = first.StartDateTime.Date,
                            NextVisitDate = first.StartDateTime.Date.AddMonths(3),
                            Status = "Наблюдение"
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}