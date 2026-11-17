using HospitalIS.Application.Services.Appointments;
using HospitalIS.Application.Services.Auth;
using HospitalIS.Application.Services.Consultations;
using HospitalIS.Application.Services.Doctor;
using HospitalIS.Application.Services.Patients;
using HospitalIS.Application.Services.Specializations;
using HospitalIS.Infrastructure.Data;
using HospitalIS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using HospitalIS.Application.Services.Dispensary;
using HospitalIS.Application.Services.Diagnoses;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();
builder.Services.AddScoped<IDispensaryService, DispensaryService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Cookie-аутентификация
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "HospitalIS.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddDbContext<HospitalDbContext>(opt => opt.UseSqlite("Data Source=hospital.db"));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<ISpecializationService, SpecializationService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();   // ← ДО UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
    await DbSeeder.SeedAsync(context);
    await ProcessSeeder.SeedAsync(context);
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await AuthSeeder.SeedAsync(context, hasher.Hash);
}

app.Run();