namespace HospitalIS.Domain.Entities;

public class Administrator : User
{
    public string Position { get; set; } = string.Empty;
}