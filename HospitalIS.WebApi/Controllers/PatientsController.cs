using HospitalIS.Application.DTOs.Patients;
using HospitalIS.Application.Services.Patients;
using Microsoft.AspNetCore.Mvc;

namespace HospitalIS.WebApi.Controllers;

public class PatientsController : Controller
{
    private readonly IPatientService _service;
    public PatientsController(IPatientService service) => _service = service;

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    public async Task<IActionResult> Details(long id)
    {
        var p = await _service.GetByIdAsync(id);
        return p == null ? NotFound() : View(p);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(CreatePatientDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.CreateAsync(dto);
        TempData["Success"] = "Пациент добавлен";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var p = await _service.GetByIdAsync(id);
        if (p == null) return NotFound();
        return View(new CreatePatientDto
        {
            LastName = p.LastName,
            FirstName = p.FirstName,
            MiddleName = p.MiddleName,
            BirthDate = p.BirthDate,
            PolicyNumber = p.PolicyNumber,
            Phone = p.Phone,
            Email = p.Email
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, CreatePatientDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _service.UpdateAsync(id, dto);
            TempData["Success"] = "Данные пациента обновлены";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> DeleteConfirm(long id)
    {
        var p = await _service.GetByIdAsync(id);
        return p == null ? NotFound() : View(p);
    }

    [HttpPost, ActionName("DeleteConfirm")]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Пациент удалён";
        return RedirectToAction(nameof(Index));
    }
}