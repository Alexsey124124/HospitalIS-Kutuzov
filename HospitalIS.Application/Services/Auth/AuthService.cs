using HospitalIS.Application.DTOs.Auth;
using HospitalIS.Infrastructure.Repositories;
using UserEntity = HospitalIS.Domain.Entities.User;
using PatientEntity = HospitalIS.Domain.Entities.Patient;
using DoctorEntity = HospitalIS.Domain.Entities.Doctor;

namespace HospitalIS.Application.Services.Auth;

public interface IAuthService
{
    Task<AuthUser> RegisterAsync(RegisterDto dto);
    Task<AuthUser?> LoginAsync(string login, string password);
}

public class AuthService : IAuthService
{
    private readonly IRepository<UserEntity> _users;
    private readonly IPasswordHasher _hasher;

    public AuthService(IRepository<UserEntity> users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<AuthUser> RegisterAsync(RegisterDto dto)
    {
        var login = dto.Login.Trim().ToLowerInvariant();
        if ((await _users.FindAsync(u => u.Login == login)).Any())
            throw new Exception("Этот логин уже занят");

        if (dto.Role != "Patient" && dto.Role != "Doctor")
            throw new Exception("Недопустимая роль");

        UserEntity user;
        if (dto.Role == "Doctor")
        {
            if (dto.SpecializationId is null or <= 0) throw new Exception("Выберите специальность");
            if (string.IsNullOrWhiteSpace(dto.OfficeNumber)) throw new Exception("Укажите кабинет");
            user = new DoctorEntity
            {
                SpecializationId = dto.SpecializationId.Value,
                OfficeNumber = dto.OfficeNumber!
            };
        }
        else
        {
            if (dto.BirthDate is null) throw new Exception("Укажите дату рождения");
            if (string.IsNullOrWhiteSpace(dto.PolicyNumber)) throw new Exception("Укажите номер полиса");
            user = new PatientEntity
            {
                BirthDate = dto.BirthDate.Value,
                PolicyNumber = dto.PolicyNumber!,
                Email = dto.Email ?? ""
            };
        }

        user.Login = login;
        user.PasswordHash = _hasher.Hash(dto.Password);
        user.Role = dto.Role;
        user.LastName = dto.LastName;
        user.FirstName = dto.FirstName;
        user.MiddleName = dto.MiddleName;
        user.Phone = dto.Phone;
        user.IsActive = true;
        user.RegistrationDate = DateTime.Now;

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();
        return ToAuthUser(user);
    }

    public async Task<AuthUser?> LoginAsync(string login, string password)
    {
        login = login.Trim().ToLowerInvariant();
        var user = (await _users.FindAsync(u => u.Login == login)).FirstOrDefault();
        if (user == null || !user.IsActive) return null;
        if (!_hasher.Verify(password, user.PasswordHash)) return null;
        return ToAuthUser(user);
    }

    private static AuthUser ToAuthUser(UserEntity u)
        => new(u.Id, u.Login, u.Role, $"{u.LastName} {u.FirstName} {u.MiddleName}".Trim());
}