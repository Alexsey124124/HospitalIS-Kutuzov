using HospitalIS.Application.Services.Specializations;
using Microsoft.AspNetCore.Mvc;

namespace HospitalIS.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecializationsController : ControllerBase
{
    private readonly ISpecializationService _service;
    public SpecializationsController(ISpecializationService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
}