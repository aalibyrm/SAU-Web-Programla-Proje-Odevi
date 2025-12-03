using System.ComponentModel.DataAnnotations;

namespace FitnessCenter.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Uzmanlık Alanları")]
        [StringLength(500)]
        public string? Specializations { get; set; }

        [Display(Name = "Biyografi")]
        [StringLength(1000)]
        public string? Biography { get; set; }

        [Display(Name = "Deneyim (Yıl)")]
        [Range(0, 50, ErrorMessage = "Deneyim 0-50 yıl arasında olmalıdır")]
        public int? ExperienceYears { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key
        [Display(Name = "Spor Salonu")]
        public int GymId { get; set; }

        // Navigation Properties
        public virtual Gym? Gym { get; set; }
        public virtual ICollection<TrainerService>? TrainerServices { get; set; }
        public virtual ICollection<TrainerAvailability>? Availabilities { get; set; }
        public virtual ICollection<Appointment>? Appointments { get; set; }

        // Tam Ad
        public string FullName => $"{FirstName} {LastName}";
    }
}

