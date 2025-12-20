using System.ComponentModel.DataAnnotations;

namespace SporSalonu.Models
{

    public class Salon
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur")]
        [StringLength(100, ErrorMessage = "Salon adı en fazla 100 karakter olabilir")]
        [Display(Name = "Salon Adı")]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Açılış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan AcilisSaati { get; set; } = new TimeSpan(8, 0, 0);

        [Display(Name = "Kapanış Saati")]
        [DataType(DataType.Time)]
        public TimeSpan KapanisSaati { get; set; } = new TimeSpan(22, 0, 0);

        [Display(Name = "Aktif Mi?")]
        public bool AktifMi { get; set; } = true;

        public virtual ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public virtual ICollection<Antrenor> Antrenorler { get; set; } = new List<Antrenor>();
    }
}


