using HospitalIS.Application.DTOs.Diagnoses;
using HospitalIS.Application.Services.Diagnoses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalIS.WebApi.Controllers;

public class DiagnosesController : Controller
{
    private readonly IDiagnosisService _service;
    public DiagnosesController(IDiagnosisService service) => _service = service;

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    public async Task<IActionResult> Details(long id)
    {
        var d = await _service.GetByIdAsync(id);
        return d == null ? NotFound() : View(d);
    }

    [Authorize(Roles = "Administrator")]
    public IActionResult Create() => View();

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(CreateDiagnosisDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _service.CreateAsync(dto);
            TempData["Success"] = "Диагноз добавлен в справочник";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(long id)
    {
        var d = await _service.GetByIdAsync(id);
        if (d == null) return NotFound();
        return View(new CreateDiagnosisDto
        {
            MKBCode = d.MKBCode,
            Name = d.Name,
            Category = d.Category,
            Description = d.Description
        });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Edit(long id, CreateDiagnosisDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _service.UpdateAsync(id, dto);
            TempData["Success"] = "Диагноз обновлён";
            return RedirectToAction(nameof(Details), new { id });
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
        var d = await _service.GetByIdAsync(id);
        return d == null ? NotFound() : View(d);
    }

    [HttpPost, ActionName("DeleteConfirm")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Диагноз удалён";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}