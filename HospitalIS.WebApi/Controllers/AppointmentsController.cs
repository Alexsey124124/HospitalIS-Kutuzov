using HospitalIS.Application.DTOs.Appointments;
using HospitalIS.Application.Services.Appointments;
using HospitalIS.Application.Services.Doctor;
using HospitalIS.Application.Services.Patients;
using HospitalIS.Domain.Enums;
using HospitalIS.WebApi.Models;
using HospitalIS.WebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalIS.WebApi.Controllers;

public class AppointmentsController : Controller
{
    private readonly IAppointmentService _service;
    private readonly IPatientService _patients;
    private readonly IDoctorService _doctors;

    public AppointmentsController(IAppointmentService service, IPatientService patients, IDoctorService doctors)
    {
        _service = service;
        _patients = patients;
        _doctors = doctors;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsPatient()) return View(await _service.GetByPatientAsync(User.UserId()));
        if (User.IsDoctor()) return View(await _service.GetByDoctorAsync(User.UserId()));
        return View(await _service.GetAllAsync());
    }

    public async Task<IActionResult> Details(long id)
    {
        var a = await _service.GetByIdAsync(id);
        if (a == null) return NotFound();
        if (!CanView(a)) return Forbid();
        ViewBag.Statuses = StatusList(a.Status);
        return View(a);
    }

    [Authorize(Roles = "Patient,Administrator")]
    public async Task<IActionResult> Create()
    {
        await FillLists();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Patient,Administrator")]
    public async Task<IActionResult> Create(CreateAppointmentDto dto)
    {
        // Пациент может записать только себя — сервер игнорирует подмену PatientId
        if (User.IsPatient()) dto.PatientId = User.UserId();

        if (!ModelState.IsValid) { await FillLists(); return View(dto); }
        try
        {
            var created = await _service.CreateAsync(dto);
            TempData["Success"] = "Запись на приём создана";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await FillLists();
            return View(dto);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Doctor,Administrator")]
    public async Task<IActionResult> ChangeStatus(long id, int status)
    {
        var a = await _service.GetByIdAsync(id);
        if (a == null) return NotFound();
        if (User.IsDoctor() && a.DoctorId != User.UserId()) return Forbid();
        try
        {
            await _service.UpdateStatusAsync(id, status);
            TempData["Success"] = "Статус записи обновлён";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirm(long id)
    {
        var a = await _service.GetByIdAsync(id);
        return a == null ? NotFound() : View(a);
    }

    [HttpPost, ActionName("DeleteConfirm")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Запись удалена";
        return RedirectToAction(nameof(Index));
    }

    private bool CanView(AppointmentDto a)
    {
        if (User.IsAdmin()) return true;
        if (User.IsDoctor()) return a.DoctorId == User.UserId();
        if (User.IsPatient()) return a.PatientId == User.UserId();
        return false;
    }

    private async Task FillLists()
    {
        ViewBag.Patients = (await _patients.GetAllAsync())
            .Select(p => new { p.Id, Name = $"{p.LastName} {p.FirstName} {p.MiddleName}".Trim() });
        ViewBag.Doctors = (await _doctors.GetAllAsync())
            .Select(d => new { d.Id, Name = $"{d.LastName} {d.FirstName} ({d.SpecializationName}, каб. {d.OfficeNumber})" });
    }

    private static SelectList StatusList(int current) => new(
        Enum.GetValues<AppointmentStatus>().Select(s => new { Value = (int)s, Text = EnumDisplay.Text(s) }),
        "Value", "Text", current);
}