using System;
using System.Linq;
using System.Windows;

namespace AccountingOfAbsences
{
    public partial class AddClassesAndStudentsWindow : Window
    {
        public AddClassesAndStudentsWindow()
        {
            InitializeComponent();
            LoadClasses();
        }

        private void LoadClasses()
        {
            // Загрузка классов в ComboBox
            var classes = DatabaseHelper.GetClasses();
            ClassComboBox.ItemsSource = classes;
            ClassComboBox.DisplayMemberPath = "Name";
            ClassComboBox.SelectedValuePath = "Id";
        }

        private void AddClassButton_Click(object sender, RoutedEventArgs e)
        {
            string className = ClassNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(className))
            {
                MessageBox.Show("Название класса не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newClass = new Class { Name = className };
                DatabaseHelper.AddClass(newClass); // Добавление класса через DatabaseHelper
                MessageBox.Show("Класс успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                ClassNameTextBox.Clear();
                LoadClasses(); // Обновление списка классов
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении класса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            string studentName = StudentNameTextBox.Text.Trim();
            var selectedClass = ClassComboBox.SelectedItem as Class;

            if (string.IsNullOrEmpty(studentName))
            {
                MessageBox.Show("ФИО учащегося не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedClass == null)
            {
                MessageBox.Show("Выберите класс для учащегося.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newStudent = new Student
                {
                    FullName = studentName,
                    ClassId = selectedClass.Id
                };

                DatabaseHelper.AddStudent(newStudent); // Добавление студента через DatabaseHelper
                MessageBox.Show("Учащийся успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                StudentNameTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении учащегося: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
