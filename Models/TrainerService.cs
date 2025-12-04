using System.ComponentModel.DataAnnotations;

namespace FitnessCenter.Models
{
    // Many-to-Many ilişkisi için ara tablo
    public class TrainerService
    {
        public int Id { get; set; }

        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }

        // Navigation Properties
        public virtual Trainer? Trainer { get; set; }
        public virtual Service? Service { get; set; }
    }
}


