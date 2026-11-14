using HospitalIS.Application.Services.Doctor;
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

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
}

app.MapControllers();

app.Run();