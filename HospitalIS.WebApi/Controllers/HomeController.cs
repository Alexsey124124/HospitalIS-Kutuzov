using HospitalIS.Infrastructure.Data;
using HospitalIS.WebApi.Models;  
using HospitalIS.WebApi.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.WebApi.Controllers;

public class HomeController : Controller
{
    private static readonly string[] Hues =
    {
        "#0f7f8b", "#2f6bff", "#17c07a", "#ffb020", "#e0344a", "#7a5cff", "#22c3e6", "#ff8a3d"
    };

    private readonly HospitalDbContext _db;
    public HomeController(HospitalDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        if (User.IsPatient()) return RedirectToAction("Index", "Appointments");

        var specs = await _db.Specializations.OrderBy(s => s.Id).ToListAsync();
        var grouped = await _db.Doctors
            .GroupBy(d => d.SpecializationId)
            .Select(g => new { SpecId = g.Key, Count = g.Count() })
            .ToListAsync();

        var load = specs.Select((s, i) =>
        {
            var count = grouped.FirstOrDefault(g => g.SpecId == s.Id)?.Count ?? 0;
            return new DashboardViewModel.SpecLoad(s.Name, count, Hues[i % Hues.Length], 0);
        }).ToList();

        var max = Math.Max(1, load.Count == 0 ? 1 : load.Max(l => l.Count));
        load = load.Select(l => l with { Percent = (int)Math.Round(100.0 * l.Count / max) }).ToList();

        var recent = await _db.Patients
            .OrderByDescending(p => p.Id)
            .Take(5)
            .Select(p => new DashboardViewModel.RecentPatient(
                p.Id,
                p.LastName + " " + p.FirstName + " " + p.MiddleName,
                p.BirthDate.ToString("dd.MM.yyyy"),
                p.PolicyNumber))
            .ToListAsync();

        var diagnoses = await _db.Diagnoses
            .OrderBy(d => d.MKBCode)
            .Take(6)
            .Select(d => new DashboardViewModel.DiagnosisRow(d.MKBCode, d.Name, d.Category))
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            DoctorsCount = await _db.Doctors.CountAsync(),
            PatientsCount = await _db.Patients.CountAsync(),
            SpecializationsCount = specs.Count,
            DiagnosesCount = await _db.Diagnoses.CountAsync(),
            Load = load,
            Recent = recent,
            Diagnoses = diagnoses
        };
        vm.TotalRecords = vm.DoctorsCount + vm.PatientsCount + vm.SpecializationsCount + vm.DiagnosesCount;
        return View(vm);
    }
}