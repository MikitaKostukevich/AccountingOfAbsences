using System.Windows;

namespace AccountingOfAbsences
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (!DatabaseHelper.TestDatabaseConnection())
            {
                MessageBox.Show("Не удалось установить подключение с БД. проверьте подключение!");
                Application.Current.Shutdown();
            }
            //else
            //{
            //    MessageBox.Show("All is good");
            //}

        }




            private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
            {
                // Открываем окно изменения пароля
                ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow();
                changePasswordWindow.ShowDialog();
            }


    private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Логика перехода на окно авторизации
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Логика перехода на окно регистрации
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}