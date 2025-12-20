using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SporSalonu.Models.ViewModels
{
    
    public class RandevuOlusturViewModel
    {
        [Required(ErrorMessage = "Hizmet seçimi zorunludur")]
        [Display(Name = "Hizmet")]
        public int HizmetId { get; set; }

        [Required(ErrorMessage = "Antrenör seçimi zorunludur")]
        [Display(Name = "Antrenör")]
        public int AntrenorId { get; set; }

        [Required(ErrorMessage = "Tarih seçimi zorunludur")]
        [DataType(DataType.Date)]
        [Display(Name = "Tarih")]
        public DateTime Tarih { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Saat seçimi zorunludur")]
        [Display(Name = "Saat")]
        public string SecilenSaat { get; set; } = string.Empty;

        [Display(Name = "Notlar")]
        [StringLength(500)]
        public string? Notlar { get; set; }

        public List<SelectListItem> Hizmetler { get; set; } = new();
        public List<SelectListItem> Antrenorler { get; set; } = new();
        public List<SelectListItem> UygunSaatler { get; set; } = new();
    }

    public class RandevuDetayViewModel
    {
        public int Id { get; set; }
        public string HizmetAdi { get; set; } = string.Empty;
        public string AntrenorAdi { get; set; } = string.Empty;
        public DateTime Tarih { get; set; }
        public TimeSpan BaslangicSaati { get; set; }
        public TimeSpan BitisSaati { get; set; }
        public decimal Ucret { get; set; }
        public RandevuDurumu Durum { get; set; }
        public string? Notlar { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
    }

    public class RandevuListeViewModel
    {
        public List<RandevuDetayViewModel> Randevular { get; set; } = new();
        public int ToplamRandevu { get; set; }
        public int BekleyenRandevu { get; set; }
        public int TamamlananRandevu { get; set; }
    }
}


