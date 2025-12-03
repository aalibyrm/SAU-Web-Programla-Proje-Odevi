using System.ComponentModel.DataAnnotations;

namespace FitnessCenter.Models
{
    public class AIExerciseRecommendation
    {
        public int Id { get; set; }

        [Display(Name = "Üye")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Vücut Tipi")]
        public BodyType? BodyType { get; set; }

        [Display(Name = "Boy (cm)")]
        public int? Height { get; set; }

        [Display(Name = "Kilo (kg)")]
        public double? Weight { get; set; }

        [Display(Name = "Hedef")]
        public FitnessGoal? Goal { get; set; }

        [Display(Name = "Yüklenen Fotoğraf")]
        public string? UploadedImageUrl { get; set; }

        [Display(Name = "AI Önerisi")]
        public string? Recommendation { get; set; }

        [Display(Name = "Oluşturulan Görsel")]
        public string? GeneratedImageUrl { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual ApplicationUser? User { get; set; }
    }

    public enum BodyType
    {
        [Display(Name = "Ektomorf (İnce yapılı)")]
        Ectomorph,
        [Display(Name = "Mezomorf (Atletik yapılı)")]
        Mesomorph,
        [Display(Name = "Endomorf (Geniş yapılı)")]
        Endomorph
    }

    public enum FitnessGoal
    {
        [Display(Name = "Kilo Verme")]
        WeightLoss,
        [Display(Name = "Kas Geliştirme")]
        MuscleGain,
        [Display(Name = "Kondisyon Artırma")]
        Endurance,
        [Display(Name = "Esneklik")]
        Flexibility,
        [Display(Name = "Genel Sağlık")]
        GeneralHealth
    }
}

