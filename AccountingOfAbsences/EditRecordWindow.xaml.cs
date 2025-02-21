using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AccountingOfAbsences
{
    public partial class EditRecordWindow : Window
    {
        private Record _record;

        public EditRecordWindow(Record record)
        {
            InitializeComponent();
            _record = record;
            LoadData();
        }

        private void LoadData()
        {
            // Загрузка данных ученика
            var students = DatabaseHelper.GetStudents();
            StudentComboBox.ItemsSource = students;
            StudentComboBox.SelectedItem = students.FirstOrDefault(s => s.Id == _record.StudentId);

            // Установка значения причины
            ReasonTextBox.Text = _record.Reason;

            // Установка значения даты
            DatePicker.SelectedDate = _record.Date;

            // Установка классификации
            ClassificationComboBox.SelectedItem = ClassificationComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _record.Classification);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполненности данных
                if (StudentComboBox.SelectedItem == null || DatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Обновление данных записи
                _record.StudentId = ((Student)StudentComboBox.SelectedItem).Id;
                _record.Reason = ReasonTextBox.Text.Trim();
                _record.Date = DatePicker.SelectedDate.Value;
                _record.Classification = (ClassificationComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                // Сохранение изменений в базе данных
                DatabaseHelper.UpdateRecord(_record);

                MessageBox.Show("Запись успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
