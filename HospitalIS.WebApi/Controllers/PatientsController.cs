using HospitalIS.Application.Services.Patients;
using Microsoft.AspNetCore.Mvc;

namespace HospitalIS.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _service;
    public PatientsController(IPatientService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var patient = await _service.GetByIdAsync(id);
        return patient == null ? NotFound() : Ok(patient);
    }
}