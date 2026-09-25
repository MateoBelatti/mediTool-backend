using Microsoft.EntityFrameworkCore;
using Biblioteca.Entities;

namespace Biblioteca.Repository
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Profesional> Profesionales { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<PacienteProfesional> PacienteProfesionales { get; set; }
        // Tablas Branch feature/turnos
        public DbSet<TurnoFijo> TurnosFijos { get; set; }
        public DbSet<Turno> Turnos { get; set; }

        //Tablas Branch feature/informe-reuniones
        public DbSet<Informe> Informes { get; set; }
        public DbSet<Reunion> Reuniones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique index for matricula in profesional table
            modelBuilder.Entity<Profesional>()
                .HasIndex(p => p.Matricula)
                .IsUnique();

            // Configure unique index for dni in paciente table
            modelBuilder.Entity<Paciente>()
                .HasIndex(p => p.Dni)
                .IsUnique();

            modelBuilder.Entity<Paciente>()
                .Property(p => p.Activo)
                .HasDefaultValue(true)
                .ValueGeneratedOnAdd();

            // Configure Many-to-Many relationship
            modelBuilder.Entity<PacienteProfesional>()
                .HasKey(pp => new { pp.PacienteId, pp.ProfesionalId });

            modelBuilder.Entity<PacienteProfesional>()
                .HasOne(pp => pp.Paciente)
                .WithMany(p => p.PacienteProfesionales)
                .HasForeignKey(pp => pp.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PacienteProfesional>()
                .HasOne(pp => pp.Profesional)
                .WithMany(p => p.PacienteProfesionales)
                .HasForeignKey(pp => pp.ProfesionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Informe relationships
            modelBuilder.Entity<Informe>()
                .HasOne(i => i.Paciente)
                .WithMany(p => p.Informes)
                .HasForeignKey(i => i.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Informe>()
                .HasOne(i => i.Profesional)
                .WithMany(p => p.Informes)
                .HasForeignKey(i => i.ProfesionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Reunion relationships
            modelBuilder.Entity<Reunion>()
                .HasOne(r => r.Profesional)
                .WithMany(p => p.Reuniones)
                .HasForeignKey(r => r.ProfesionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TurnoFijo relationships
            modelBuilder.Entity<TurnoFijo>()
                .HasOne(tf => tf.Paciente)
                .WithMany()
                .HasForeignKey(tf => tf.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TurnoFijo>()
                .HasOne(tf => tf.Profesional)
                .WithMany()
                .HasForeignKey(tf => tf.ProfesionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Turno relationships
            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Paciente)
                .WithMany()
                .HasForeignKey(t => t.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Profesional)
                .WithMany()
                .HasForeignKey(t => t.ProfesionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Turno>()
                .HasOne(t => t.TurnoFijo)
                .WithMany(tf => tf.Turnos)
                .HasForeignKey(t => t.TurnoFijoId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}

