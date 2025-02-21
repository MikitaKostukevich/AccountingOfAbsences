using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using Microsoft.Win32;
using Word = Microsoft.Office.Interop.Word;

namespace AccountingOfAbsences
{

    public partial class AdminWindow : Window
    {
        public ObservableCollection<Record> Records { get; set; }
        public ObservableCollection<Record> FilteredRecords { get; set; }

        public AdminWindow()
        {
            InitializeComponent();
            LoadRecords();
        }

        private void LoadRecords()
        {
            Records = DatabaseHelper.GetRecords();
            FilteredRecords = new ObservableCollection<Record>(Records);
            DataGridRecords.ItemsSource = FilteredRecords;

            UpdateCharts(); // Перестраиваем диаграммы
        }

        private void BtnDeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridRecords.SelectedItem is Record selectedRecord)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Вы уверены, что хотите удалить выбранную запись?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Удаление записи из базы данных
                        DatabaseHelper.DeleteRecord(selectedRecord.Id);

                        // Обновление данных в таблице
                        LoadRecords();

                        MessageBox.Show("Запись успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void FilterByDate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;

                if (startDate == null || endDate == null)
                {
                    MessageBox.Show("Пожалуйста, выберите обе даты для фильтрации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                FilteredRecords = new ObservableCollection<Record>(
                    Records.Where(record => record.Date >= startDate && record.Date <= endDate)
                );

                DataGridRecords.ItemsSource = FilteredRecords;

                if (FilteredRecords.Count == 0)
                {
                    MessageBox.Show("Записей за выбранный период не найдено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecord = (Record)DataGridRecords.SelectedItem;

            if (selectedRecord != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        DatabaseHelper.DeleteRecord(selectedRecord.Id);
                        MessageBox.Show("Запись успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Удаляем запись из коллекции и обновляем DataGrid
                        Records.Remove(selectedRecord);
                        FilteredRecords.Remove(selectedRecord);
                        DataGridRecords.ItemsSource = FilteredRecords;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void BtnEditRecord_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecord = DataGridRecords.SelectedItem as Record;
            if (selectedRecord == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new EditRecordWindow(selectedRecord); // Создаем окно редактирования записи
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем запись в базе данных
                DatabaseHelper.UpdateRecord(selectedRecord);
                MessageBox.Show("Запись успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DataGridRecords.Items.Refresh();
            }
        }

        private void BtnExportToWord_Click(object sender, RoutedEventArgs e)
        {
            if (FilteredRecords == null || FilteredRecords.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx",
                Title = "Сохранить файл"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                //ExportToWord(saveFileDialog.FileName);
            }
        }

        private (Dictionary<string, int> AbsenceReasons, Dictionary<DateTime, int> AbsenceTrend) CalculateStatistics()
        {
            var absenceReasons = Records
                .Where(r => !string.IsNullOrEmpty(r.Reason))
                .GroupBy(r => r.Reason)
                .ToDictionary(g => g.Key, g => g.Count());

            var absenceTrend = Records
                .GroupBy(r => r.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());

            return (absenceReasons, absenceTrend);
        }





        //private void ExportToWord(string filePath)
        //{
        //    try
        //    {
        //        if (FilteredRecords == null || !FilteredRecords.Any())
        //        {
        //            MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        //            return;
        //        }

        //        var statistics = CalculateStatistics(); // Получаем статистику
        //        var wordApp = new Word.Application();
        //        var document = wordApp.Documents.Add();

        //        // Заголовок документа
        //        var paragraph = document.Paragraphs.Add();
        //        paragraph.Range.Text = "Отчет по посещаемости";
        //        paragraph.Range.Font.Size = 16;
        //        paragraph.Range.Font.Bold = 1;
        //        paragraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
        //        paragraph.Range.InsertParagraphAfter();

        //        // Описание района
        //        var districtParagraph = document.Paragraphs.Add();
        //        districtParagraph.Range.Text = $"Район: {statistics.District}";
        //        districtParagraph.Range.Font.Size = 12;
        //        districtParagraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
        //        districtParagraph.Range.InsertParagraphAfter();

        //        // Добавление статистики
        //        var statsParagraph = document.Paragraphs.Add();
        //        statsParagraph.Range.Text = $"Общее количество учащихся: {statistics.TotalStudents}\n" +
        //                                    $"Присутствуют на занятиях: {statistics.StudentsPresent} ({statistics.AttendancePercentage:0.00}%)\n" +
        //                                    $"Отсутствуют на занятиях: {statistics.StudentsAbsent} ({statistics.AbsencePercentage:0.00}%)";
        //        statsParagraph.Range.Font.Size = 12;
        //        statsParagraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
        //        statsParagraph.Range.InsertParagraphAfter();

        //        // Список причин отсутствия
        //        var reasonsParagraph = document.Paragraphs.Add();
        //        reasonsParagraph.Range.Text = "Причины отсутствия:";
        //        reasonsParagraph.Range.Font.Size = 12;
        //        reasonsParagraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
        //        reasonsParagraph.Range.InsertParagraphAfter();

        //        foreach (var reason in statistics.AbsenceReasons)
        //        {
        //            reasonsParagraph.Range.Text += $"\n  - {reason.Key}: {reason.Value}";
        //            reasonsParagraph.Range.InsertParagraphAfter();
        //        }

        //        // Таблица с данными
        //        var table = document.Tables.Add(reasonsParagraph.Range, FilteredRecords.Count + 1, 5);
        //        table.Borders.Enable = 1;

        //        // Заполнение заголовков таблицы
        //        table.Cell(1, 1).Range.Text = "ФИО Ученика";
        //        table.Cell(1, 2).Range.Text = "Класс";
        //        table.Cell(1, 3).Range.Text = "Причина";
        //        table.Cell(1, 4).Range.Text = "Дата";
        //        table.Cell(1, 5).Range.Text = "Классификация";

        //        // Заполнение данных из коллекции
        //        int rowIndex = 2;
        //        foreach (var record in FilteredRecords)
        //        {
        //            table.Cell(rowIndex, 1).Range.Text = record.Student.FullName;
        //            table.Cell(rowIndex, 2).Range.Text = record.Student.Class.Name;
        //            table.Cell(rowIndex, 3).Range.Text = record.Reason;
        //            table.Cell(rowIndex, 4).Range.Text = record.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        //            table.Cell(rowIndex, 5).Range.Text = record.Classification;
        //            rowIndex++;
        //        }

        //        // Сохранение документа
        //        document.SaveAs2(filePath);
        //        document.Close();
        //        wordApp.Quit();

        //        MessageBox.Show("Данные успешно экспортированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void OpenAddClassesAndStudentsWindow_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddClassesAndStudentsWindow();
            addWindow.ShowDialog();
        }


        private void SearchByFullName_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // Фильтруем записи по совпадению в ФИО
                FilteredRecords = new ObservableCollection<Record>(
                    Records.Where(record => record.Student.FullName.ToLower().Contains(searchText))
                );
                DataGridRecords.ItemsSource = FilteredRecords;

                if (FilteredRecords.Count == 0)
                {
                    MessageBox.Show("Записи с указанным ФИО не найдены.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                // Если поле поиска пустое, сбрасываем фильтр
                FilteredRecords = new ObservableCollection<Record>(Records);
                DataGridRecords.ItemsSource = FilteredRecords;
            }
        }
        public SeriesCollection BarChartSeries { get; set; }
        public SeriesCollection PieChartSeries { get; set; }
        public List<string> AbsenceReasonsKeys { get; set; }
        public SeriesCollection LineChartSeries { get; set; }
        public List<string> AbsenceTrendDates { get; set; }



        private void UpdateCharts()
        {
            var statistics = CalculateStatistics();

            // Обновляем данные для столбчатой диаграммы
            AbsenceReasonsKeys = statistics.AbsenceReasons.Keys.ToList();
            BarChartSeries = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Отсутствия",
            Values = new ChartValues<int>(statistics.AbsenceReasons.Values)
        }
    };
            BarChart.AxisX[0].Labels = AbsenceReasonsKeys;
            BarChart.Series = BarChartSeries;

            // Обновляем данные для круговой диаграммы
            PieChartSeries = new SeriesCollection();
            foreach (var reason in statistics.AbsenceReasons)
            {
                PieChartSeries.Add(new PieSeries
                {
                    Title = reason.Key,
                    Values = new ChartValues<int> { reason.Value }
                });
            }
            PieChart.Series = PieChartSeries;

            // Обновляем данные для линейного графика
            AbsenceTrendDates = statistics.AbsenceTrend.Keys.Select(date => date.ToString("yyyy-MM-dd")).ToList();
            LineChartSeries = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Динамика отсутствий",
            Values = new ChartValues<int>(statistics.AbsenceTrend.Values),
            PointGeometrySize = 10
        }
    };

            LineChart.AxisX[0].Labels = AbsenceTrendDates;
            LineChart.Series = LineChartSeries;
        }

    }


}
