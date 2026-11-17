using HospitalIS.Application.DTOs.Consultations;
using HospitalIS.Application.Services.Consultations;
using HospitalIS.Domain.Enums;
using HospitalIS.WebApi.Models;
using HospitalIS.WebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalIS.WebApi.Controllers;

public class ConsultationsController : Controller
{
    private readonly IConsultationService _service;
    public ConsultationsController(IConsultationService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        if (User.IsPatient()) return View(await _service.GetByPatientAsync(User.UserId()));
        if (User.IsDoctor()) return View(await _service.GetByDoctorAsync(User.UserId()));
        return View(await _service.GetAllAsync());
    }

    public async Task<IActionResult> Details(long id)
    {
        var c = await _service.GetByIdAsync(id);
        if (c == null) return NotFound();
        if (!CanView(c)) return Forbid();
        return View(c);
    }

    [Authorize(Roles = "Doctor,Administrator")]
    public async Task<IActionResult> Create()
    {
        await FillForm();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Doctor,Administrator")]
    public async Task<IActionResult> Create(CreateConsultationDto dto)
    {
        if (!ModelState.IsValid) { await FillForm(); return View(dto); }
        try
        {
            var created = await _service.CreateAsync(dto, User.IsDoctor() ? User.UserId() : null);
            TempData["Success"] = "Консультация создана — внесите диагноз и лечение";
            return RedirectToAction(nameof(Edit), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await FillForm();
            return View(dto);
        }
    }

    [Authorize(Roles = "Doctor,Administrator")]
    public async Task<IActionResult> Edit(long id)
    {
        var c = await _service.GetByIdAsync(id);
        if (c == null) return NotFound();
        if (User.IsDoctor() && c.DoctorId != User.UserId()) return Forbid();
        await FillForm(c.Status);
        return View(c);
    }

    [HttpPost]
    [Authorize(Roles = "Doctor,Administrator")]
    public async Task<IActionResult> Edit(long id, UpdateConsultationDto dto)
    {
        var c = await _service.GetByIdAsync(id);
        if (c == null) return NotFound();
        if (User.IsDoctor() && c.DoctorId != User.UserId()) return Forbid();
        try
        {
            await _service.UpdateAsync(id, dto);
            TempData["Success"] = "Консультация обновлена";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await FillForm(c.Status);
            return View(c);
        }
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirm(long id)
    {
        var c = await _service.GetByIdAsync(id);
        return c == null ? NotFound() : View(c);
    }

    [HttpPost, ActionName("DeleteConfirm")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Консультация удалена";
        return RedirectToAction(nameof(Index));
    }

    private bool CanView(ConsultationDto c)
    {
        if (User.IsAdmin()) return true;
        if (User.IsDoctor()) return c.DoctorId == User.UserId();
        if (User.IsPatient()) return c.PatientId == User.UserId();
        return false;
    }

    private async Task FillForm(int currentStatus = 0)
    {
        ViewBag.Appointments = await _service.GetFreeAppointmentsAsync(User.IsDoctor() ? User.UserId() : null);
        ViewBag.Diagnoses = await _service.GetDiagnosisOptionsAsync();
        ViewBag.Statuses = new SelectList(
            Enum.GetValues<ConsultationStatus>().Select(s => new { Value = (int)s, Text = EnumDisplay.Text(s) }),
            "Value", "Text", currentStatus);
    }
}