using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenter.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Üye seçimi zorunludur")]
        [Display(Name = "Üye")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Antrenör seçimi zorunludur")]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Hizmet seçimi zorunludur")]
        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur")]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Durum")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [Display(Name = "Notlar")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Ücret")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Trainer? Trainer { get; set; }
        public virtual Service? Service { get; set; }
    }

    public enum AppointmentStatus
    {
        [Display(Name = "Beklemede")]
        Pending,
        [Display(Name = "Onaylandı")]
        Approved,
        [Display(Name = "İptal Edildi")]
        Cancelled,
        [Display(Name = "Tamamlandı")]
        Completed,
        [Display(Name = "Reddedildi")]
        Rejected
    }
}

