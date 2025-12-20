using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models.ViewModels
{

    public class UyeEkleViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur")]
        [Display(Name = "Ad")]
        [StringLength(50)]
        public string Ad { get; set; } = "";

        [Required(ErrorMessage = "Soyad zorunludur")]
        [Display(Name = "Soyad")]
        [StringLength(50)]
        public string Soyad { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
        [Display(Name = "E-posta")]
        public string Eposta { get; set; } = "";

        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = "";

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        public int? BoyCm { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        public double? KiloKg { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        [Display(Name = "Cinsiyet")]
        public Cinsiyet? Cinsiyet { get; set; }
    }

    public class UyeDuzenleViewModel
    {
        public string Id { get; set; } = "";

        [Required(ErrorMessage = "Ad zorunludur")]
        [Display(Name = "Ad")]
        [StringLength(50)]
        public string Ad { get; set; } = "";

        [Required(ErrorMessage = "Soyad zorunludur")]
        [Display(Name = "Soyad")]
        [StringLength(50)]
        public string Soyad { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
        [Display(Name = "E-posta")]
        public string Eposta { get; set; } = "";

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        public int? BoyCm { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        public double? KiloKg { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DogumTarihi { get; set; }

        [Display(Name = "Cinsiyet")]
        public Cinsiyet? Cinsiyet { get; set; }

        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (boş bırakılırsa değişmez)")]
        public string? YeniSifre { get; set; }
    }
}


