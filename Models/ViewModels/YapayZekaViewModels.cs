using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models.ViewModels
{
    
    public class EgzersizOneriViewModel
    {
        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        public int? BoyCm { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        public double? KiloKg { get; set; }

        [Display(Name = "Yaş")]
        [Range(10, 100, ErrorMessage = "Yaş 10-100 arasında olmalıdır")]
        public int? Yas { get; set; }

        [Display(Name = "Cinsiyet")]
        public Cinsiyet? Cinsiyet { get; set; }

        [Display(Name = "Hedefiniz")]
        public HedefTipi Hedef { get; set; }

        [Display(Name = "Aktivite Seviyeniz")]
        public AktiviteSeviyesi AktiviteSeviye { get; set; }

        [Display(Name = "Sağlık Durumu/Notlar")]
        [StringLength(500)]
        public string? EkBilgi { get; set; }


        public string? OneriSonucu { get; set; }
    }


    public enum HedefTipi
    {
        [Display(Name = "Kilo Vermek")]
        KiloVermek = 0,

        [Display(Name = "Kas Yapmak")]
        KasYapmak = 1,

        [Display(Name = "Formda Kalmak")]
        FormdaKalmak = 2
    }

    public enum AktiviteSeviyesi
    {
        [Display(Name = "Hareketsiz (Masabaşı)")]
        Hareketsiz = 0,

        [Display(Name = "Az Aktif (Haftada 1-2 gün)")]
        AzAktif = 1,

        [Display(Name = "Orta Aktif (Haftada 3-4 gün)")]
        OrtaAktif = 2,

        [Display(Name = "Çok Aktif (Haftada 5-6 gün)")]
        CokAktif = 3,

        [Display(Name = "Profesyonel Sporcu")]
        Profesyonel = 4
    }

    public class DiyetOneriViewModel
    {
        [Display(Name = "Boy (cm)")]
        [Range(100, 250)]
        public int? BoyCm { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 300)]
        public double? KiloKg { get; set; }

        [Display(Name = "Yaş")]
        [Range(10, 100)]
        public int? Yas { get; set; }

        [Display(Name = "Hedefiniz")]
        public HedefTipi Hedef { get; set; }

        [Display(Name = "Yemek Tercihleriniz")]
        public YemekTercihi YemekTercihi { get; set; }

        [Display(Name = "Alerji veya kısıtlamalarınız var mı?")]
        [StringLength(300)]
        public string? Alerjiler { get; set; }

        public string? OneriSonucu { get; set; }
    }

    public enum YemekTercihi
    {
        [Display(Name = "Herşeyi Yerim")]
        Normal = 0,

        [Display(Name = "Vejetaryen")]
        Vejetaryen = 1,

        [Display(Name = "Vegan")]
        Vegan = 2,

        [Display(Name = "Düşük Karbonhidrat")]
        DusukKarb = 3
    }


    public class GorselDonusumViewModel
    {
        [Display(Name = "Hedefiniz")]
        public HedefTipi Hedef { get; set; }

        [Display(Name = "Fotoğrafınız")]
        public IFormFile? Fotograf { get; set; }

     
        public string? YuklenenFotograf { get; set; }

        public string? DonusturulmusFotograf { get; set; }


        public string? Aciklama { get; set; }

      
        public string? HataMesaji { get; set; }
    }
}

