using HospitalIS.Application.Services.Doctor;
using HospitalIS.Application.Services.Patients;
using HospitalIS.Application.Services.Specializations;
using HospitalIS.Infrastructure.Data;
using HospitalIS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(); 


builder.Services.AddDbContext<HospitalDbContext>(opt =>
    opt.UseSqlite("Data Source=hospital.db"));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<ISpecializationService, SpecializationService>();
builder.Services.AddScoped<IPatientService, PatientService>();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
}

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();
    await DbSeeder.SeedAsync(context);
}
app.Run();