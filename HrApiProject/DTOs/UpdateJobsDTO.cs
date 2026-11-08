namespace HrApiProject.DTOs
{
    public sealed class UpdateJobsDTO
    {
        public string JobTitle { get; set; } = null!;
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
    }
}
