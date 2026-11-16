using HospitalIS.Domain.Entities;
using HospitalIS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(HospitalDbContext context)
    {
        
        await context.Database.EnsureCreatedAsync();

        
        if (await context.Specializations.AnyAsync())
            return;

        // Специальности
        var specializations = new List<Specialization>
        {
            new() { Name = "Терапевт" },
            new() { Name = "Кардиолог" },
            new() { Name = "Невролог" },
            new() { Name = "Хирург" },
            new() { Name = "Офтальмолог" }
        };
        await context.Specializations.AddRangeAsync(specializations);
        await context.SaveChangesAsync();


        // Диагнозы
        var diagnoses = new List<Diagnosis>
        {
            new() { Name = "Острая инфекция верхних дыхательных путей (J06.9)" },
            new() { Name = "Эссенциальная гипертензия (I10)" },
            new() { Name = "Боль в пояснице (M54.5)" }
        };
        await context.Diagnoses.AddRangeAsync(diagnoses);
        await context.SaveChangesAsync();

        // Врачи 
        var doctors = new List<Doctor>
        {
            new()
            {
                LastName = "Иванов", FirstName = "Петр", MiddleName = "Сергеевич",
                SpecializationId = specializations[0].Id, OfficeNumber = "101",
                Login = "ivanov", PasswordHash = "hash1", Role = "Doctor", IsActive = true
            },
            new()
            {
                LastName = "Смирнова", FirstName = "Анна", MiddleName = "Ивановна",
                SpecializationId = specializations[1].Id, OfficeNumber = "205",
                Login = "smirnova", PasswordHash = "hash2", Role = "Doctor", IsActive = true
            }
        };
        await context.Doctors.AddRangeAsync(doctors);
        await context.SaveChangesAsync();

        // Пациенты
        var patients = new List<Patient>
        {
            new()
            {
                LastName = "Петров", FirstName = "Иван", MiddleName = "Петрович",
                BirthDate = new DateTime(1985, 5, 15),
                PolicyNumber = "1234567890123456", Phone = "+79001112233",
                Login = "petrov", PasswordHash = "hash3", Role = "Patient", IsActive = true
            },
            new()
            {
                LastName = "Сидорова", FirstName = "Мария", MiddleName = "Александровна",
                BirthDate = new DateTime(1992, 11, 3),
                PolicyNumber = "9876543210987654", Phone = "+79004445566",
                Login = "sidorova", PasswordHash = "hash4", Role = "Patient", IsActive = true
            }
        };
        await context.Patients.AddRangeAsync(patients);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ База данных успешно заполнена тестовыми данными!");
    }
}