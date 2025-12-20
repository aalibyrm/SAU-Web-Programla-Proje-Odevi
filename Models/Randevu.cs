using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonu.Models
{

    public class Randevu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur")]
        [DataType(DataType.Date)]
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        [DataType(DataType.Time)]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan BaslangicSaati { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan BitisSaati { get; set; }

        [Display(Name = "Durum")]
        public RandevuDurumu Durum { get; set; } = RandevuDurumu.Beklemede;

        [StringLength(500)]
        [Display(Name = "Notlar")]
        public string? Notlar { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Ödenen Tutar")]
        public decimal OdenenTutar { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

   
        [Required]
        [Display(Name = "Üye")]
        public string UyeId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }

        [Required]
        [Display(Name = "Hizmet")]
        public int HizmetId { get; set; }

        public virtual Uye? Uye { get; set; }
        public virtual Antrenor? Antrenor { get; set; }
        public virtual Hizmet? Hizmet { get; set; }
    }


    public enum RandevuDurumu
    {
        [Display(Name = "Beklemede")]
        Beklemede = 0,

        [Display(Name = "Onaylandı")]
        Onaylandi = 1,

        [Display(Name = "İptal Edildi")]
        IptalEdildi = 2,

        [Display(Name = "Tamamlandı")]
        Tamamlandi = 3
    }
}


