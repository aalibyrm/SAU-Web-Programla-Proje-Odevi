namespace FitnessCenter.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalMembers { get; set; }
        public int TotalTrainers { get; set; }
        public int TotalServices { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<AppointmentListViewModel> RecentAppointments { get; set; } = new();
        public List<TrainerStatsViewModel> TopTrainers { get; set; } = new();
    }

    public class TrainerStatsViewModel
    {
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class MemberDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public List<AppointmentListViewModel> UpcomingAppointmentsList { get; set; } = new();
        public AIRecommendationResultViewModel? LastRecommendation { get; set; }
    }
}

