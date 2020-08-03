using Microsoft.EntityFrameworkCore;

namespace SimpleFrame.DB {
    internal class WindowDbContext : DbContext {

#pragma warning disable CS8618
        public DbSet<PhotoWindowData> Windows { get; set; }
#pragma warning restore CS8618

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=windows.db");
    }
}
