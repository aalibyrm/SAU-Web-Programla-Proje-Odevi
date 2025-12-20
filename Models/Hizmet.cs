using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonu.Models
{

    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur")]
        [StringLength(100)]
        [Display(Name = "Hizmet Adı")]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Süre zorunludur")]
        [Range(15, 180, ErrorMessage = "Süre 15-180 dakika arasında olmalıdır")]
        [Display(Name = "Süre (Dakika)")]
        public int SureDakika { get; set; }

        [Required(ErrorMessage = "Ücret zorunludur")]
        [Range(0, 10000, ErrorMessage = "Ücret 0-10000 TL arasında olmalıdır")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool AktifMi { get; set; } = true;

        [Display(Name = "Salon")]
        public int SalonId { get; set; }

        public virtual Salon? Salon { get; set; }
        public virtual ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}


