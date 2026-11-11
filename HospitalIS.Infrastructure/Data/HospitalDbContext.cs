using HospitalIS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalIS.Infrastructure.Data;

public class HospitalDbContext : DbContext
{
    public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Administrator> Administrators { get; set; }
    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<Diagnosis> Diagnoses { get; set; }
    public DbSet<AppointmentRequest> AppointmentRequests { get; set; }
    public DbSet<Consultation> Consultations { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<DispensaryRecord> DispensaryRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Patient>().ToTable("Patients");
        modelBuilder.Entity<Doctor>().ToTable("Doctors");
        modelBuilder.Entity<Administrator>().ToTable("Administrators");

        
        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.Specialization)
            .WithMany(s => s.Doctors)
            .HasForeignKey(d => d.SpecializationId);

        modelBuilder.Entity<AppointmentRequest>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.AppointmentRequests)
            .HasForeignKey(a => a.PatientId);

        modelBuilder.Entity<AppointmentRequest>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.AppointmentRequests)
            .HasForeignKey(a => a.DoctorId);

        modelBuilder.Entity<Consultation>()
            .HasOne(c => c.AppointmentRequest)
            .WithMany()
            .HasForeignKey(c => c.AppointmentRequestId);

        modelBuilder.Entity<Consultation>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.Consultations)
            .HasForeignKey(c => c.PatientId);

        modelBuilder.Entity<Consultation>()
            .HasOne(c => c.Doctor)
            .WithMany(d => d.Consultations)
            .HasForeignKey(c => c.DoctorId);

        modelBuilder.Entity<Consultation>()
            .HasOne(c => c.Diagnosis)
            .WithMany(d => d.Consultations)
            .HasForeignKey(c => c.DiagnosisId);

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Consultation)
            .WithOne(c => c.MedicalRecord)
            .HasForeignKey<MedicalRecord>(m => m.ConsultationId);

        modelBuilder.Entity<DispensaryRecord>()
            .HasOne(d => d.Patient)
            .WithMany(p => p.DispensaryRecords)
            .HasForeignKey(d => d.PatientId);

        modelBuilder.Entity<DispensaryRecord>()
            .HasOne(d => d.Doctor)
            .WithMany(d => d.DispensaryRecords)
            .HasForeignKey(d => d.DoctorId);

        modelBuilder.Entity<DispensaryRecord>()
            .HasOne(d => d.Diagnosis)
            .WithMany(d => d.DispensaryRecords)
            .HasForeignKey(d => d.DiagnosisId);

        base.OnModelCreating(modelBuilder);
    }
}