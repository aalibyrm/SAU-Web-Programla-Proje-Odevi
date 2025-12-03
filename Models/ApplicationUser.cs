using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenter.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        public int? Height { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        public double? Weight { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Appointment>? Appointments { get; set; }

        // Tam Ad
        public string FullName => $"{FirstName} {LastName}";
    }
}

