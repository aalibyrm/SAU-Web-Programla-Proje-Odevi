using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FitnessCenter.Models.ViewModels
{
    public class AIRecommendationViewModel
    {
        [Display(Name = "Vücut Tipi")]
        public BodyType? BodyType { get; set; }

        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        public int? Height { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        public double? Weight { get; set; }

        [Display(Name = "Hedef")]
        public FitnessGoal? Goal { get; set; }

        [Display(Name = "Fotoğraf Yükle")]
        public IFormFile? Image { get; set; }

        [Display(Name = "Ek Bilgiler")]
        [StringLength(500)]
        public string? AdditionalInfo { get; set; }
    }

    public class AIRecommendationResultViewModel
    {
        public string Recommendation { get; set; } = string.Empty;
        public string? GeneratedImageUrl { get; set; }
        public string? UploadedImageUrl { get; set; }
        public BodyType? BodyType { get; set; }
        public FitnessGoal? Goal { get; set; }
        public int? Height { get; set; }
        public double? Weight { get; set; }
        public double? BMI { get; set; }
        public string? BMICategory { get; set; }
    }
}


