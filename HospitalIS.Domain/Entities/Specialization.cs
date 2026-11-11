namespace HospitalIS.Domain.Entities;

public class Specialization
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}