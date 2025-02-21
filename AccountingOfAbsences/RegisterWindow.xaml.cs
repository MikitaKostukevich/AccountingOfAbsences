using System;
using System.Windows;

namespace AccountingOfAbsences
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка соединения с БД
                if (!DatabaseHelper.TestDatabaseConnection())
                {
                    MessageBox.Show("Не удалось установить подключение с БД. проверьте подключение!",
                        "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string username = TxtUsername.Text;
                string password = TxtPassword.Password;
                string role = CmbRole.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (DatabaseHelper.IsUserExists(username))
                {
                    MessageBox.Show("Имя пользователя занято. Выберите другое!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool isRegistered = DatabaseHelper.RegisterUser(username, password, role);

                if (isRegistered)
                {
                    MessageBox.Show("Регистрация успешна!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Регистрация провалена. Попробуйте ещё раз!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
