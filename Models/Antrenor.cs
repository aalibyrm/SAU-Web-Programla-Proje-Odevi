using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models
{
    
    public class Antrenor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "E-posta")]
        public string? Eposta { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [StringLength(200)]
        [Display(Name = "Uzmanlık Alanları")]
        public string? UzmanlikAlanlari { get; set; } 

        [Display(Name = "Fotoğraf")]
        public string? FotografYolu { get; set; }

        [Display(Name = "Mesai Başlangıç")]
        [DataType(DataType.Time)]
        public TimeSpan MesaiBaslangic { get; set; } = new TimeSpan(9, 0, 0);

        [Display(Name = "Mesai Bitiş")]
        [DataType(DataType.Time)]
        public TimeSpan MesaiBitis { get; set; } = new TimeSpan(18, 0, 0);

        [Display(Name = "Aktif Mi?")]
        public bool AktifMi { get; set; } = true;


        [Display(Name = "Salon")]
        public int SalonId { get; set; }

      
        public virtual Salon? Salon { get; set; }
        public virtual ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();


        [Display(Name = "Tam Ad")]
        public string TamAd => $"{Ad} {Soyad}";
    }
}


