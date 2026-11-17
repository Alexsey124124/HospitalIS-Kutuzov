using System.ComponentModel.DataAnnotations;

namespace HospitalIS.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Введите логин")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    [Required(ErrorMessage = "Придумайте логин")]
    [MinLength(3, ErrorMessage = "Логин минимум 3 символа")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Придумайте пароль")]
    [MinLength(6, ErrorMessage = "Пароль минимум 6 символов")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите пароль")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите фамилию")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите имя")]
    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль")]
    public string Role { get; set; } = "Patient";

    // Для пациента
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }
    public string? PolicyNumber { get; set; }
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string? Email { get; set; }

    // Для врача
    public long? SpecializationId { get; set; }
    public string? OfficeNumber { get; set; }
}

public record AuthUser(long Id, string Login, string Role, string FullName);