using HospitalIS.Application.DTOs.Dispensary;
using HospitalIS.Application.Services.Dispensary;
using HospitalIS.WebApi.Models;
using HospitalIS.WebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalIS.WebApi.Controllers;

public class DispensaryController : Controller
{
    private readonly IDispensaryService _service;
    public DispensaryController(IDispensaryService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        if (User.IsPatient()) return View(await _service.GetByPatientAsync(User.UserId()));
        if (User.IsDoctor()) return View(await _service.GetByDoctorAsync(User.UserId()));
        return View(await _service.GetAllAsync());
    }

    public async Task<IActionResult> Details(long id)
    {
        var r = await _service.GetByIdAsync(id);
        if (r == null) return NotFound();
        if (!CanView(r)) return Forbid();
        return View(r);
    }

    [Authorize(Roles = "Doctor,Administrator")]
    public async Task<IActionResult> Create()
    {
        await FillForm();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Doctor,Administrator")]
    public async Task<IActionResult> Create(CreateDispensaryRecordDto dto)
    {
        if (User.IsDoctor()) dto.DoctorId = User.UserId(); // врач ставит на учёт только от своего имени
        if (!ModelState.IsValid) { await FillForm(); return View(dto); }
        try
        {
            await _service.CreateAsync(dto, User.IsDoctor() ? User.UserId() : null);
            TempData["Success"] = "Пациент поставлен на диспансерный учёт";
            return RedirectToAction(nameof(Index));
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
        var r = await _service.GetByIdAsync(id);
        if (r == null) return NotFound();
        if (User.IsDoctor() && r.DoctorId != User.UserId()) return Forbid();
        await FillForm();
        return View(r);
    }

    [HttpPost]
    [Authorize(Roles = "Doctor,Administrator")]
    public async Task<IActionResult> Edit(long id, UpdateDispensaryRecordDto dto)
    {
        var r = await _service.GetByIdAsync(id);
        if (r == null) return NotFound();
        if (User.IsDoctor() && r.DoctorId != User.UserId()) return Forbid();
        try
        {
            await _service.UpdateAsync(id, dto);
            TempData["Success"] = "Запись учёта обновлена";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await FillForm();
            return View(r);
        }
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirm(long id)
    {
        var r = await _service.GetByIdAsync(id);
        return r == null ? NotFound() : View(r);
    }

    [HttpPost, ActionName("DeleteConfirm")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Запись учёта удалена";
        return RedirectToAction(nameof(Index));
    }

    private bool CanView(DispensaryRecordDto r)
    {
        if (User.IsAdmin()) return true;
        if (User.IsDoctor()) return r.DoctorId == User.UserId();
        if (User.IsPatient()) return r.PatientId == User.UserId();
        return false;
    }

    private async Task FillForm()
    {
        ViewBag.Patients = await _service.GetPatientOptionsAsync();
        ViewBag.Doctors = await _service.GetDoctorOptionsAsync();
        ViewBag.Diagnoses = await _service.GetDiagnosisOptionsAsync();
        ViewBag.Statuses = new SelectList(DispensaryStatuses.All);
    }
}