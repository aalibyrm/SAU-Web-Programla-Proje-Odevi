using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models
{

    public class Uye : IdentityUser
    {
        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime? DogumTarihi { get; set; }

        [Range(30, 300, ErrorMessage = "Boy 30-300 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public int? BoyCm { get; set; }

        [Range(20, 500, ErrorMessage = "Kilo 20-500 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public double? KiloKg { get; set; }

        [Display(Name = "Cinsiyet")]
        public Cinsiyet? Cinsiyet { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Fotoğraf")]
        public string? ProfilFotoYolu { get; set; }

        
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

    
        [Display(Name = "Tam Ad")]
        public string TamAd => $"{Ad} {Soyad}";
    }

    public enum Cinsiyet
    {
        [Display(Name = "Erkek")]
        Erkek = 0,

        [Display(Name = "Kadın")]
        Kadin = 1,

        [Display(Name = "Belirtmek İstemiyorum")]
        Diger = 2
    }
}


