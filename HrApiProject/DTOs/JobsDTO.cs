namespace HrApiProject.DTOs
{
    public sealed class JobsDTO
    {
        public string JobId { get; set; } = null!;
        public string JobTitle { get; set; } = null!;

        public decimal? MinSalary { get; set; }

        public decimal? MaxSalary { get; set; }

    }
}
