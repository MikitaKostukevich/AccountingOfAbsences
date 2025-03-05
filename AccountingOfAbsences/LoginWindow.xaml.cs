using System;
using System.Windows;

namespace AccountingOfAbsences
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!DatabaseHelper.TestDatabaseConnection())
                {
                    MessageBox.Show("Не удалось установить подключение с БД. проверьте подключение!",
                        "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string username = TxtUsername.Text;
                string password = TxtPassword.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Имя пользователя и пароль не могут быть пустыми.");
                    return;
                }

                if (DatabaseHelper.AuthenticateUser(username, password))
                {
                    string role = DatabaseHelper.GetUserRole(username);
                    MessageBox.Show($"Вход успешен. Роль: {role}");

                    if (role == "Admin")
                    {
                        var adminWindow = new AdminWindow();
                        adminWindow.Show();
                    }
                    else
                    {
                        var userWindow = new UserWindow();
                        userWindow.Show();
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверные учетные данные. Пожалуйста, попробуйте еще раз.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при входе в систему: {ex.Message}");
            }
        }
        // Обработчик события открытия окна справки
        private void OpenHelpWindow_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр окна справки и открываем его
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.ShowDialog();
        }

    }
}
