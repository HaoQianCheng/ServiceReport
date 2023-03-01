using Microsoft.EntityFrameworkCore;
using ServiceReport.Core.Model;
using ServiceReport.Ui.Storage.Model;

namespace ServiceReport.Ui.Storage
{
    public class ApplicationContext : DbContext
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="options"></param>
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
       : base(options)
        { }

        public virtual DbSet<RequestInfo> RequestInfo { get; set; }

        public virtual DbSet<RequestDetail> RequestDetail { get; set; }

        public virtual DbSet<GcState> GcState { get; set; }

        public virtual DbSet<MonitorTask> MonitorTask { get; set; }

        public virtual DbSet<MonitorWarning> MonitorWarning { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}