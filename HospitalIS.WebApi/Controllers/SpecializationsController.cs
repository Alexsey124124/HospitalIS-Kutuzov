using System.Text.Json;
using HospitalIS.Application.DTOs.Specializations;
using HospitalIS.Application.Services.Specializations;
using HospitalIS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.WebApi.Controllers;

public class SpecializationsController : Controller
{
    private readonly ISpecializationService _service;
    private readonly HospitalDbContext _db;

    public SpecializationsController(ISpecializationService service, HospitalDbContext db)
    {
        _service = service;
        _db = db;
    }

    // ===== ЧТЕНИЕ: доступно всем вошедшим =====
    public async Task<IActionResult> Index()
    {
        ViewBag.Counts = await _db.Doctors
            .GroupBy(d => d.SpecializationId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
        return View(await _service.GetAllAsync());
    }

    public async Task<IActionResult> Details(long id)
    {
        var s = await _service.GetByIdAsync(id);
        return s == null ? NotFound() : View(s);
    }

    // ===== ЗАПИСЬ: только администратор =====
    [Authorize(Roles = "Administrator")]
    public IActionResult Create() => View();

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(CreateSpecializationDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.CreateAsync(dto);
        TempData["Success"] = "Специальность добавлена";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(long id)
    {
        var s = await _service.GetByIdAsync(id);
        if (s == null) return NotFound();
        return View(new CreateSpecializationDto { Name = s.Name, Description = s.Description });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(long id, CreateSpecializationDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _service.UpdateAsync(id, dto);
            TempData["Success"] = "Специальность обновлена";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirm(long id)
    {
        var s = await _service.GetByIdAsync(id);
        return s == null ? NotFound() : View(s);
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
        var list = JsonSerializer.Deserialize<List<CreateSpecializationDto>>(payload ?? "[]") ?? new();
        foreach (var dto in list) await _service.CreateAsync(dto);
        TempData["Success"] = $"Восстановлено записей: {list.Count}";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> RemoveRange(long[] ids)
    {
        var captured = new List<CreateSpecializationDto>();
        foreach (var id in ids)
        {
            var s = await _service.GetByIdAsync(id);
            if (s == null) continue;
            captured.Add(new CreateSpecializationDto { Name = s.Name, Description = s.Description });
            await _service.DeleteAsync(id);
        }
        if (captured.Count > 0)
        {
            TempData["UndoSpecializations"] = JsonSerializer.Serialize(captured);
            TempData["Success"] = $"Удалено записей: {captured.Count}";
        }
        return RedirectToAction(nameof(Index));
    }
}