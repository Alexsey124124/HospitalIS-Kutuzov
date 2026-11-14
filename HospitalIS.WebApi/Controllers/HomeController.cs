using HospitalIS.Application.DTOs.Doctor;
using HospitalIS.Application.Services.Doctor;
using Microsoft.AspNetCore.Mvc;

namespace HospitalIS.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors);
    }

    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null)
            return NotFound(new { message = "Врач не найден" });

        return Ok(doctor);
    }

    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var createdDoctor = await _doctorService.CreateAsync(dto);
            
            return CreatedAtAction(nameof(GetById), new { id = createdDoctor.Id }, createdDoctor);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}