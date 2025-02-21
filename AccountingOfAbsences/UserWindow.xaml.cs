using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AccountingOfAbsences
{
    public partial class UserWindow : Window
    {
        public UserWindow()
        {
            InitializeComponent();
            LoadClasses();
        }

        private void LoadClasses()
        {
            try
            {
                // Загрузка списка классов
                var classes = DatabaseHelper.GetClasses();
                ClassComboBox.ItemsSource = classes;
                ClassComboBox.DisplayMemberPath = "Name"; // Отображаемое поле
                ClassComboBox.SelectedValuePath = "Id";   // Значение, которое будет передаваться
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки классов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassComboBox.SelectedValue is int classId)
            {
                try
                {
                    // Загрузка учеников выбранного класса
                    var students = DatabaseHelper.GetStudentsByClass(classId);
                    StudentComboBox.ItemsSource = students;
                    StudentComboBox.DisplayMemberPath = "FullName"; // Отображаемое поле
                    StudentComboBox.SelectedValuePath = "Id";       // Значение, которое будет передаваться
                    StudentComboBox.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки учеников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAddRecord_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех обязательных полей
            if (StudentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите ученика.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtReason.Text))
            {
                MessageBox.Show("Пожалуйста, введите причину.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ComboClassification.Text))
            {
                MessageBox.Show("Пожалуйста, выберите классификацию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Создание новой записи
                var record = new Record
                {
                    StudentId = (int)StudentComboBox.SelectedValue,
                    Reason = TxtReason.Text,
                    Date = DatePicker.SelectedDate.Value,
                    Classification = ComboClassification.Text
                };

                // Добавление записи в базу данных
                DatabaseHelper.AddRecord(record);

                MessageBox.Show("Запись успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистка полей
                StudentComboBox.SelectedIndex = -1;
                TxtReason.Text = string.Empty;
                ComboClassification.SelectedIndex = -1;
                DatePicker.SelectedDate = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
