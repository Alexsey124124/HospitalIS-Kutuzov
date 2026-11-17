using HospitalIS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Infrastructure.Data;

public static class DbSeeder
{
    private static readonly (string Name, string Description)[] SpecSeed =
    {
        ("Терапевт", "Первичный приём, диагностика и лечение заболеваний внутренних органов, ведение амбулаторных пациентов."),
        ("Кардиолог", "Диагностика и лечение заболеваний сердечно-сосудистой системы: гипертония, аритмии, ИБС."),
        ("Невролог", "Заболевания центральной и периферической нервной системы: мигрени, невралгии, остеохондроз."),
        ("Хирург", "Оперативное лечение заболеваний и травм, предоперационная подготовка и послеоперационное наблюдение."),
        ("Офтальмолог", "Диагностика и лечение заболеваний глаз, подбор коррекции зрения, профилактические осмотры.")
    };

    private static readonly (string Code, string Name, string Category, string Description)[] DiagnosisSeed =
    {
        ("J06.9", "Острая инфекция верхних дыхательных путей", "Болезни органов дыхания", "ОРВИ неуточнённой этиологии: насморк, фарингит, повышение температуры."),
        ("I10", "Эссенциальная гипертензия", "Болезни системы кровообращения", "Артериальная гипертензия без установленной вторичной причины."),
        ("M54.5", "Боль в пояснице", "Болезни костно-мышечной системы", "Люмбаго, дорсалгия поясничного отдела позвоночника."),
        ("H52.1", "Миопия", "Болезни глаза", "Близорукость: нарушение рефракции с фокусировкой изображения перед сетчаткой."),
        ("K29.7", "Гастрит неуточнённый", "Болезни органов пищеварения", "Воспаление слизистой оболочки желудка без уточнения формы."),
        ("G43.9", "Мигрень неуточнённая", "Болезни нервной системы", "Приступообразная головная боль, часто односторонняя, с тошнотой и светофобией.")
    };

    private static readonly (string Login, string Last, string First, string Middle, string Spec, string Office, string Phone)[] DoctorSeed =
    {
        ("ivanov", "Иванов", "Петр", "Сергеевич", "Терапевт", "101", "+79001000101"),
        ("smirnova", "Смирнова", "Анна", "Ивановна", "Кардиолог", "205", "+79001000205"),
        ("kuznetsov", "Кузнецов", "Дмитрий", "Алексеевич", "Хирург", "310", "+79001000310"),
        ("volkova", "Волкова", "Елена", "Павловна", "Невролог", "214", "+79001000214"),
        ("morozov", "Морозов", "Сергей", "Николаевич", "Офтальмолог", "118", "+79001000118"),
        ("lebedeva", "Лебедева", "Ольга", "Викторовна", "Терапевт", "103", "+79001000103")
    };

    private static readonly (string Login, string Last, string First, string Middle, DateTime Birth, string Policy, string Phone, string Email)[] PatientSeed =
    {
        ("petrov", "Петров", "Иван", "Петрович", new DateTime(1985, 5, 15), "1234567890123456", "+79001112233", "petrov@mail.ru"),
        ("sidorova", "Сидорова", "Мария", "Александровна", new DateTime(1992, 11, 3), "9876543210987654", "+79004445566", "sidorova@mail.ru"),
        ("sokolov", "Соколов", "Артём", "Игоревич", new DateTime(1978, 2, 21), "5544332211009988", "+79005556677", "sokolov@mail.ru"),
        ("egorova", "Егорова", "Наталья", "Сергеевна", new DateTime(1999, 7, 9), "1122334455667788", "+79006667788", "egorova@mail.ru"),
        ("nikitin", "Никитин", "Владимир", "Олегович", new DateTime(1965, 12, 30), "9988776655443322", "+79007778899", "nikitin@mail.ru"),
        ("orlova", "Орлова", "Ксения", "Дмитриевна", new DateTime(2001, 4, 17), "6677889900112233", "+79008889900", "orlova@mail.ru")
    };

    public static async Task SeedAsync(HospitalDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        var specs = await context.Specializations.ToListAsync();
        if (specs.Count == 0)
        {
            foreach (var (name, desc) in SpecSeed)
                context.Specializations.Add(new Specialization { Name = name, Description = desc });
            await context.SaveChangesAsync();
            specs = await context.Specializations.ToListAsync();
        }

        var existingNames = (await context.Diagnoses.ToListAsync()).Select(d => d.Name).ToHashSet();
        foreach (var (code, name, cat, desc) in DiagnosisSeed)
        {
            if (existingNames.Contains(name)) continue;
            context.Diagnoses.Add(new Diagnosis { MKBCode = code, Name = name, Category = cat, Description = desc });
        }

        var existingDoctors = (await context.Doctors.ToListAsync()).Select(d => d.Login).ToHashSet();
        foreach (var (login, last, first, middle, specName, office, phone) in DoctorSeed)
        {
            if (existingDoctors.Contains(login)) continue;
            var spec = specs.FirstOrDefault(s => s.Name == specName) ?? specs.First();
            context.Doctors.Add(new Doctor
            {
                Login = login,
                PasswordHash = "changeme",
                Role = "Doctor",
                IsActive = true,
                RegistrationDate = DateTime.Now,
                LastName = last,
                FirstName = first,
                MiddleName = middle,
                Phone = phone,
                SpecializationId = spec.Id,
                OfficeNumber = office
            });
        }

        var existingPatients = (await context.Patients.ToListAsync()).Select(p => p.Login).ToHashSet();
        foreach (var (login, last, first, middle, birth, policy, phone, email) in PatientSeed)
        {
            if (existingPatients.Contains(login)) continue;
            context.Patients.Add(new Patient
            {
                Login = login,
                PasswordHash = "changeme",
                Role = "Patient",
                IsActive = true,
                RegistrationDate = DateTime.Now,
                LastName = last,
                FirstName = first,
                MiddleName = middle,
                Phone = phone,
                BirthDate = birth,
                PolicyNumber = policy,
                Email = email
            });
        }

        await context.SaveChangesAsync();
    }
}