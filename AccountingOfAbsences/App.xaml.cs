using System;
using System.Windows;

namespace AccountingOfAbsences
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!DatabaseHelper.TestDatabaseConnection())
            {
                MessageBox.Show("Unable to connect to the database. The application will now close.",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown(); // Закрываем приложение
            }
        }
    }
}
