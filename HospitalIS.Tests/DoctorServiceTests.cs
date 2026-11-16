using HospitalIS.Application.DTOs.Doctor;
using HospitalIS.Application.Services.Doctor;
using HospitalIS.Domain.Entities;
using HospitalIS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace HospitalIS.Tests;

public class DoctorServiceTests
{
    private readonly Mock<IRepository<HospitalIS.Domain.Entities.Doctor>> _doctorRepoMock;
    private readonly Mock<IRepository<HospitalIS.Domain.Entities.Specialization>> _specRepoMock;
    private readonly DoctorService _service;

    public DoctorServiceTests()
    {
        _doctorRepoMock = new Mock<IRepository<HospitalIS.Domain.Entities.Doctor>>();
        _specRepoMock = new Mock<IRepository<HospitalIS.Domain.Entities.Specialization>>();
        _service = new DoctorService(_doctorRepoMock.Object, _specRepoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenDoctorsExist_ReturnsAllDoctors()
    {
        
        var doctors = new List<Doctor>
        {
            new Doctor { Id = 1, FirstName = "Иван", LastName = "Иванов", SpecializationId = 1 },
            new Doctor { Id = 2, FirstName = "Петр", LastName = "Петров", SpecializationId = 1 }
        };
        _doctorRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Doctor, bool>>>())).ReturnsAsync(doctors);
        _specRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Specialization { Id = 1, Name = "Терапевт" });

        
        var result = (await _service.GetAllAsync()).ToList();

        
        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.Equal("Терапевт", d.SpecializationName));
    }

    [Fact]
    public async Task GetByIdAsync_WhenDoctorNotFound_ReturnsNull()
    {
        _doctorRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Doctor?)null);
        var result = await _service.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedDoctor()
    {
        var dto = new CreateDoctorDto { FirstName = "Анна", LastName = "Смирнова", SpecializationId = 1, OfficeNumber = "101" };
        _specRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Specialization { Id = 1, Name = "Терапевт" });
        _doctorRepoMock.Setup(r => r.AddAsync(It.IsAny<Doctor>())).Returns(Task.CompletedTask);
        _doctorRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto);

        Assert.Equal("Анна", result.FirstName);
        Assert.Equal("Терапевт", result.SpecializationName);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidSpecialization_ThrowsException()
    {
        var dto = new CreateDoctorDto { SpecializationId = 999 };
        _specRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Specialization?)null);

        await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(dto));
    }
}