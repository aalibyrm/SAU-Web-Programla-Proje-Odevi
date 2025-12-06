using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessCenter.Models.ViewModels
{
    public class AppointmentCreateViewModel
    {
        [Required(ErrorMessage = "Antrenör seçimi zorunludur")]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Hizmet seçimi zorunludur")]
        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur")]
        [Display(Name = "Randevu Tarihi")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        [Display(Name = "Saat")]
        public string StartTime { get; set; } = string.Empty;

        [Display(Name = "Notlar")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // SelectList'ler
        public SelectList? Trainers { get; set; }
        public SelectList? Services { get; set; }
        public List<string>? AvailableTimes { get; set; }
    }

    public class AppointmentListViewModel
    {
        public int Id { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
    }
}



