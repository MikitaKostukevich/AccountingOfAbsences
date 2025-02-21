using System.Data.Entity;

namespace AccountingOfAbsences
{
    public class DbInitializer : CreateDatabaseIfNotExists<AppDbContext>
    {
        protected override void Seed(AppDbContext context)
        {
            // Добавление примеров пользователей
            context.Users.Add(new User { Username = "admin", Password = "admin123", Role = "admin" });
            context.Users.Add(new User { Username = "user", Password = "user123", Role = "user" });

            base.Seed(context);
        }
    }
}
