namespace CollegePlacementSystem.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCompanies { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalApplications { get; set; }
        public int ShortlistedApplications { get; set; }
        public int SelectedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int PendingApplications { get; set; }

        public double SelectionRate { get; set; }

        public List<DepartmentPlacementViewModel> DepartmentStatistics { get; set; } = new();

        public List<RecentApplicationViewModel> RecentApplications { get; set; } = new();
    }

    public class RecentApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
    }

    public class DepartmentPlacementViewModel
    {
        public string Department { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalApplications { get; set; }
        public int SelectedApplications { get; set; }
    }
}