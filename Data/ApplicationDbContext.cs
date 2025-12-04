using FitnessCenter.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainerService> TrainerServices { get; set; }
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AIExerciseRecommendation> AIExerciseRecommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Gym yapılandırması
            builder.Entity<Gym>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            });

            // Service yapılandırması
            builder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Gym)
                    .WithMany(g => g.Services)
                    .HasForeignKey(e => e.GymId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Trainer yapılandırması
            builder.Entity<Trainer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Gym)
                    .WithMany(g => g.Trainers)
                    .HasForeignKey(e => e.GymId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TrainerService (Many-to-Many) yapılandırması
            builder.Entity<TrainerService>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Trainer)
                    .WithMany(t => t.TrainerServices)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Service)
                    .WithMany(s => s.TrainerServices)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TrainerAvailability yapılandırması
            builder.Entity<TrainerAvailability>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Trainer)
                    .WithMany(t => t.Availabilities)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Appointment yapılandırması
            builder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Appointments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Trainer)
                    .WithMany(t => t.Appointments)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Service)
                    .WithMany(s => s.Appointments)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AIExerciseRecommendation yapılandırması
            builder.Entity<AIExerciseRecommendation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}


