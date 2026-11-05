using HrApiProject.Domain;
using Microsoft.EntityFrameworkCore;

namespace HrApiProject.Data
{
    public class HrDbContext : DbContext {

        public HrDbContext(DbContextOptions<HrDbContext> options) : base(options)
        {

        }

        //DbSet es una representación de codigo c# de una tabla en una base de datos.
        public DbSet<Region> Regions { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<Employees> Employees { get; set; }

        public DbSet<Departments> Departments { get; set; }

        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<Job_History> JobHistory { get; set; }
    }
}
