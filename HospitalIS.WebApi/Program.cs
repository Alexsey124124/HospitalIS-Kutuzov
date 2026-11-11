using Microsoft.EntityFrameworkCore;
using HospitalIS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(); 


builder.Services.AddDbContext<HospitalDbContext>(opt =>
    opt.UseSqlite("Data Source=hospital.db"));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
}

app.MapControllers();

app.Run();