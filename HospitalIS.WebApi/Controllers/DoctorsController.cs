using System.Text.Json;
using HospitalIS.Application.DTOs.Doctor;
using HospitalIS.Application.Services.Doctor;
using HospitalIS.Application.Services.Specializations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalIS.WebApi.Controllers;

public class DoctorsController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly ISpecializationService _specService;

    public DoctorsController(IDoctorService doctorService, ISpecializationService specService)
    {
        _doctorService = doctorService;
        _specService = specService;
    }

    // ===== ЧТЕНИЕ: доступно всем вошедшим =====
    public async Task<IActionResult> Index()
    {
        ViewBag.Specializations = await _specService.GetAllAsync();
        return View(await _doctorService.GetAllAsync());
    }

    public async Task<IActionResult> Details(long id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        return doctor == null ? NotFound() : View(doctor);
    }

    // ===== ЗАПИСЬ: только администратор =====
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Specializations = await _specService.GetAllAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(CreateDoctorDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Specializations = await _specService.GetAllAsync();
            return View(dto);
        }
        try
        {
            await _doctorService.CreateAsync(dto);
            TempData["Success"] = "Врач добавлен";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Specializations = await _specService.GetAllAsync();
            return View(dto);
        }
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(long id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null) return NotFound();
        ViewBag.Specializations = await _specService.GetAllAsync();
        return View(new CreateDoctorDto
        {
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            MiddleName = doctor.MiddleName,
            OfficeNumber = doctor.OfficeNumber,
            SpecializationId = doctor.SpecializationId
        });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(long id, CreateDoctorDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Specializations = await _specService.GetAllAsync();
            return View(dto);
        }
        try
        {
            await _doctorService.UpdateAsync(id, dto);
            TempData["Success"] = "Данные врача обновлены";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Specializations = await _specService.GetAllAsync();
            return View(dto);
        }
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirm(long id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        return doctor == null ? NotFound() : View(doctor);
    }

    [HttpPost, ActionName("DeleteConfirm")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(long id) => await RemoveRange(new[] { id });

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> BulkDelete(long[] ids)
    {
        if (ids == null || ids.Length == 0) return RedirectToAction(nameof(Index));
        return await RemoveRange(ids);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Restore(string payload)
    {
        var list = JsonSerializer.Deserialize<List<CreateDoctorDto>>(payload ?? "[]") ?? new();
        foreach (var dto in list) await _doctorService.CreateAsync(dto);
        TempData["Success"] = $"Восстановлено записей: {list.Count}";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> RemoveRange(long[] ids)
    {
        var captured = new List<CreateDoctorDto>();
        foreach (var id in ids)
        {
            var d = await _doctorService.GetByIdAsync(id);
            if (d == null) continue;
            captured.Add(new CreateDoctorDto
            {
                FirstName = d.FirstName,
                LastName = d.LastName,
                MiddleName = d.MiddleName,
                OfficeNumber = d.OfficeNumber,
                SpecializationId = d.SpecializationId
            });
            await _doctorService.DeleteAsync(id);
        }
        if (captured.Count > 0)
        {
            TempData["UndoDoctors"] = JsonSerializer.Serialize(captured);
            TempData["Success"] = $"Удалено записей: {captured.Count}";
        }
        return RedirectToAction(nameof(Index));
    }
}