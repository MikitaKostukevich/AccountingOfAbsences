using System;
using System.Windows;

namespace AccountingOfAbsences
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные из полей
            string login = LoginTextBox.Text.Trim();
            string newPassword = NewPasswordBox.Password.Trim();
            string confirmPassword = ConfirmPasswordBox.Password.Trim();

            // Проверка ввода
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Введите новый пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Логика изменения пароля
            try
            {
                // Пример: вызов метода для изменения пароля
                bool isChanged = ChangePasswordInDatabase(login, newPassword);

                if (isChanged)
                {
                    MessageBox.Show("Пароль успешно изменён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при изменении пароля. Проверьте логин.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрыть окно без изменений
            this.Close();
        }

        /// <summary>
        /// Метод для изменения пароля в базе данных.
        /// </summary>
        /// <param name="login">Логин пользователя.</param>
        /// <param name="newPassword">Новый пароль.</param>
        /// <returns>Возвращает true, если изменение прошло успешно.</returns>
        private bool ChangePasswordInDatabase(string login, string newPassword)
        {
            // Здесь должна быть логика изменения пароля в базе данных.
            // Например:
            //
            // 1. Проверить, существует ли пользователь с указанным логином.
            // 2. Хэшировать новый пароль.
            // 3. Сохранить новый пароль в базе данных.

            // В данном примере возвращаем true для успешного выполнения.
            return true;
        }
    }
}
