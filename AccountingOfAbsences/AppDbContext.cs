using System.Data.Entity;

namespace AccountingOfAbsences
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<Student> Students { get; set; } // Добавлено
        public DbSet<Class> Classes { get; set; }   // Добавлено

        public AppDbContext() : base("AttendanceDB") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasRequired(s => s.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassId)
                .WillCascadeOnDelete(false);
        }
    }
}
