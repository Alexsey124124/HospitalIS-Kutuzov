using HospitalIS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Infrastructure.Data;

public static class AuthSeeder
{
    public static async Task SeedAsync(HospitalDbContext context, Func<string, string> hash)
    {
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Login == "admin");
        if (admin == null)
        {
            context.Users.Add(new Administrator
            {
                Login = "admin",
                PasswordHash = hash("admin123"),
                Role = "Administrator",
                LastName = "Администратор",
                FirstName = "Системный",
                MiddleName = "",
                Phone = "",
                Position = "Системный администратор",
                IsActive = true,
                RegistrationDate = DateTime.Now
            });
            Console.WriteLine(" AuthSeeder: админ СОЗДАН (admin / admin123)");
        }
        else
        {
            admin.PasswordHash = hash("admin123");
            admin.IsActive = true;
            Console.WriteLine(" AuthSeeder: пароль админа СБРОШЕН на admin123");
        }

        
        var users = await context.Users.ToListAsync();
        var fixedCount = 0;
        foreach (var u in users.Where(u => u.Login != "admin" && !u.PasswordHash.StartsWith("PBKDF2$")))
        {
            u.PasswordHash = hash("password123");
            fixedCount++;
        }
        if (fixedCount > 0)
            Console.WriteLine($" AuthSeeder: сброшено паролей демо-пользователей: {fixedCount} (пароль: password123)");

        await context.SaveChangesAsync();
    }
}