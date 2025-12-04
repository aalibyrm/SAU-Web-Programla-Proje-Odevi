using System.ComponentModel.DataAnnotations;

namespace FitnessCenter.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }

        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Gün seçimi zorunludur")]
        [Display(Name = "Gün")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur")]
        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        // Navigation Property
        public virtual Trainer? Trainer { get; set; }
    }
}


