using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.IO;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using Microsoft.Win32;
using Word = Microsoft.Office.Interop.Word;
using System.Windows.Media;
using Excel = Microsoft.Office.Interop.Excel;


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
            DataContext = new StatisticsViewModel();
        }

        private void LoadRecords()
        {
            Records = DatabaseHelper.GetRecords();
            FilteredRecords = new ObservableCollection<Record>(Records);
            DataGridRecords.ItemsSource = FilteredRecords;
            UpdateAbsenceStatistics();
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

                // Update statistics after filtering
                UpdateAbsenceStatistics();
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

                        // Update statistics after deleting
                        UpdateAbsenceStatistics();
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


        private void UpdateAbsenceStatistics()
        {
            int totalAbsences = FilteredRecords.Count;

            int totalStudents = 784;  // Fixed total number of students

            double percentage = 0;
            if (totalStudents > 0)
            {
                percentage = ((double)totalAbsences / totalStudents) * 100;
            }

            // Update the TextBlocks in your StackPanel with the calculated values
            TotalAbsencesText.Text = totalAbsences.ToString();
            AbsencesPercentageText.Text = $"{percentage:0.00}%";

            // Apply color gradation based on the absence percentage
            if (percentage <= 10)
            {
                AbsencesPercentageText.Foreground = new SolidColorBrush(Colors.Green);  // Low absences - Green
            }
            else if (percentage <= 30)
            {
                AbsencesPercentageText.Foreground = new SolidColorBrush(Colors.Yellow);  // Moderate absences - Yellow
            }
            else if (percentage <= 50)
            {
                AbsencesPercentageText.Foreground = new SolidColorBrush(Colors.Orange);  // Higher absences - Orange
            }
            else
            {
                AbsencesPercentageText.Foreground = new SolidColorBrush(Colors.Red);  // Very high absences - Red
            }
        }

        // Метод для формирования отчета по ученику, классу или параллели
        public void GenerateReportByStudentClassParallel(string reportType)
        {
            try
            {
                var groupedRecords = new List<Record>();

                switch (reportType)
                {
                    case "Student":
                        // Группировка по ученикам
                        groupedRecords = FilteredRecords
                            .GroupBy(record => record.Student.FullName)
                            .SelectMany(group => group)
                            .ToList();
                        break;
                    case "Class":
                        // Группировка по классу
                        groupedRecords = FilteredRecords
                            .GroupBy(record => record.Student.Class.Name)
                            .SelectMany(group => group)
                            .ToList();
                        break;
                    default:
                        MessageBox.Show("Неизвестный тип отчета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                ExportReportToWord(groupedRecords, "Отчет по пропускам");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public void ExportReportToWord(List<Record> records, string reportTitle)
        {
            try
            {
                // Создаем приложение Word
                var wordApp = new Word.Application();
                wordApp.Visible = false; // Не показываем Word (можно сделать True для отображения)

                // Создаем новый документ
                var document = wordApp.Documents.Add();

                // Добавляем заголовок отчета
                var paragraph = document.Paragraphs.Add();
                paragraph.Range.Text = reportTitle;
                paragraph.Range.Font.Size = 16;
                paragraph.Range.Font.Bold = 1; // Жирный текст
                paragraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                paragraph.Range.InsertParagraphAfter();

                // Добавляем таблицу с данными
                if (records != null && records.Any())
                {
                    // Создаем таблицу с количеством строк равным числу записей + 1 (для заголовков столбцов)
                    var table = document.Tables.Add(paragraph.Range, records.Count + 1, 5); // 5 столбцов (ФИО, Класс, Причина, Дата, Классификация)

                    // Устанавливаем стиль для таблицы
                    table.Borders.Enable = 1;

                    // Заполняем заголовки столбцов
                    table.Cell(1, 1).Range.Text = "ФИО Ученика";
                    table.Cell(1, 2).Range.Text = "Класс";
                    table.Cell(1, 3).Range.Text = "Причина";
                    table.Cell(1, 4).Range.Text = "Дата";
                    table.Cell(1, 5).Range.Text = "Классификация";

                    // Заполняем таблицу данными
                    int rowIndex = 2; // Начинаем с второй строки, так как первая - это заголовки
                    foreach (var record in records)
                    {
                        table.Cell(rowIndex, 1).Range.Text = record.Student.FullName;
                        table.Cell(rowIndex, 2).Range.Text = record.Student.Class.Name;
                        table.Cell(rowIndex, 3).Range.Text = record.Reason;
                        table.Cell(rowIndex, 4).Range.Text = record.Date.ToString("yyyy-MM-dd");
                        table.Cell(rowIndex, 5).Range.Text = record.Classification;
                        rowIndex++;
                    }
                }

                // Генерация имени файла на основе типа отчета, даты и времени
                string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); // Форматируем текущую дату и время
                string fileName = $"{reportTitle}_{currentDateTime}.docx"; // Имя файла

                // Путь для сохранения в папку Загрузки
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);

                // Сохраняем документ
                document.SaveAs2(filePath);
                document.Close();
                wordApp.Quit();

                MessageBox.Show($"Отчет успешно экспортирован в Word. Файл сохранен как {fileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    
    private void GenerateStatisticalReport(string reportType)
        {
            try
            {
                var statistics = CalculateStatistics();
                var reportData = new List<string>();

                switch (reportType)
                {
                    case "Month":
                        // Группировка по месяцам
                        var monthlyStats = FilteredRecords
                            .GroupBy(record => record.Date.Month)
                            .Select(g => new
                            {
                                Month = g.Key,
                                AbsenceCount = g.Count()
                            })
                            .ToList();

                        reportData.Add("Статистика по месяцам:");
                        foreach (var monthStat in monthlyStats)
                        {
                            reportData.Add($"Месяц {monthStat.Month}: {monthStat.AbsenceCount} пропусков");
                        }
                        break;

                    case "Quarter":
                        // Группировка по кварталам
                        var quarterlyStats = FilteredRecords
                            .GroupBy(record => (record.Date.Month - 1) / 3 + 1)
                            .Select(g => new
                            {
                                Quarter = g.Key,
                                AbsenceCount = g.Count()
                            })
                            .ToList();

                        reportData.Add("Статистика по кварталам:");
                        foreach (var quarterStat in quarterlyStats)
                        {
                            reportData.Add($"Квартал {quarterStat.Quarter}: {quarterStat.AbsenceCount} пропусков");
                        }
                        break;

                    case "Reason":
                        // Группировка по причинам
                        var reasonStats = statistics.AbsenceReasons
                            .Select(r => $"{r.Key}: {r.Value}")
                            .ToList();

                        reportData.Add("Статистика по причинам пропусков:");
                        foreach (var reasonStat in reasonStats)
                        {
                            reportData.Add(reasonStat);
                        }
                        break;

                    default:
                        MessageBox.Show("Неизвестный тип отчета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                ExportStatisticalReportToWord(reportData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании статистического отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


private void ExportStatisticalReportToWord(List<string> reportData)
    {
        try
        {
            // Создаем приложение Word
            var wordApp = new Word.Application();
            wordApp.Visible = false; // Сделать Word невидимым во время экспорта (можно сделать True для отображения)

            // Создаем новый документ
            var document = wordApp.Documents.Add();

            // Добавляем заголовок
            var paragraph = document.Paragraphs.Add();
            paragraph.Range.Text = "Статистический отчет";
            paragraph.Range.Font.Size = 16;
            paragraph.Range.Font.Bold = 1; // Жирный текст
            paragraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            paragraph.Range.InsertParagraphAfter();

            // Добавляем данные отчета
            foreach (var line in reportData)
            {
                paragraph = document.Paragraphs.Add();
                paragraph.Range.Text = line;
                paragraph.Range.Font.Size = 12; // Размер шрифта
                paragraph.Range.InsertParagraphAfter();
            }

            // Генерация имени файла на основе типа отчета, даты и времени
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); // Форматируем текущую дату и время
            string fileName = $"Статистический отчёт_{currentDateTime}.docx"; // Имя файла

            // Путь для сохранения в папку Загрузки
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);

            // Сохраняем документ
            document.SaveAs2(filePath);
            document.Close();
            wordApp.Quit();

            MessageBox.Show($"Статистический отчет успешно экспортирован в Word. Файл сохранен как {fileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте статистического отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }




    private void GenerateAttendanceRegister()
        {
            try
            {
                var reportData = FilteredRecords
                    .Select(r => new AttendanceReportEntry
                    {
                        FullName = r.Student.FullName,
                        ClassName = r.Student.Class.Name,
                        Reason = r.Reason,
                        Date = r.Date,
                        Classification = r.Classification
                    })
                    .ToList();

                ExportAttendanceRegisterToWord(reportData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании ведомости пропусков: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


private void ExportAttendanceRegisterToWord(List<AttendanceReportEntry> reportData)
    {
        try
        {
            // Создаем приложение Word
            var wordApp = new Word.Application();
            wordApp.Visible = false; // Сделать Word невидимым во время экспорта (можно сделать True для отображения)

            // Создаем новый документ
            var document = wordApp.Documents.Add();

            // Добавляем заголовок
            var paragraph = document.Paragraphs.Add();
            paragraph.Range.Text = "Ведомость пропусков";
            paragraph.Range.Font.Size = 16;
            paragraph.Range.Font.Bold = 1; // Жирный текст
            paragraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            paragraph.Range.InsertParagraphAfter();

            // Добавляем таблицу с данными
            var table = document.Tables.Add(paragraph.Range, reportData.Count + 1, 5); // +1 для заголовков
            table.Range.Font.Size = 12;
            table.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

            // Устанавливаем заголовки таблицы
            table.Cell(1, 1).Range.Text = "ФИО";
            table.Cell(1, 2).Range.Text = "Класс";
            table.Cell(1, 3).Range.Text = "Дата";
            table.Cell(1, 4).Range.Text = "Причина";
            table.Cell(1, 5).Range.Text = "Классификация";

            // Заполняем таблицу данными
            for (int i = 0; i < reportData.Count; i++)
            {
                table.Cell(i + 2, 1).Range.Text = reportData[i].FullName;
                table.Cell(i + 2, 2).Range.Text = reportData[i].ClassName;
                table.Cell(i + 2, 3).Range.Text = reportData[i].Date.ToString("dd/MM/yyyy");
                table.Cell(i + 2, 4).Range.Text = reportData[i].Reason;
                table.Cell(i + 2, 5).Range.Text = reportData[i].Classification;
            }

            // Стилизация таблицы
            table.Borders.Enable = 1; // Включаем границы таблицы

            // Генерация имени файла на основе типа отчета, даты и времени
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); // Форматируем текущую дату и время
            string fileName = $"Ведомость пропусков_{currentDateTime}.docx"; // Имя файла

            // Путь для сохранения в папку Загрузки
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);

            // Сохраняем документ
            document.SaveAs2(filePath);
            document.Close();
            wordApp.Quit();

            MessageBox.Show($"Ведомость пропусков успешно экспортирована в Word. Файл сохранен как {fileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте ведомости пропусков: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }




    // Метод для формирования аналитического отчета
    private void GenerateAnalyticalAttendanceReport()
        {
            try
            {
                var statistics = CalculateStatistics();
                var chartData = statistics.AbsenceTrend;

                ExportAnalyticalReportToWord(chartData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании аналитического отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Экспорт аналитического отчета в Word


        private void ExportAnalyticalReportToWord(Dictionary<DateTime, int> chartData)
        {
            try
            {
                // Создаем приложение Word
                var wordApp = new Word.Application();
                wordApp.Visible = false; // Сделать Word невидимым во время экспорта (можно сделать True для отображения)

                // Создаем новый документ
                var document = wordApp.Documents.Add();

                // Добавляем заголовок
                var paragraph = document.Paragraphs.Add();
                paragraph.Range.Text = "Аналитический отчет по тенденции пропусков";
                paragraph.Range.Font.Size = 16;
                paragraph.Range.Font.Bold = 1; // Жирный текст
                paragraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                paragraph.Range.InsertParagraphAfter();

                // Создаем таблицу для отображения данных
                var table = document.Tables.Add(paragraph.Range, chartData.Count + 1, 2); // +1 для заголовков
                table.Range.Font.Size = 12;
                table.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                // Устанавливаем заголовки таблицы
                table.Cell(1, 1).Range.Text = "Дата";
                table.Cell(1, 2).Range.Text = "Количество пропусков";

                // Заполняем таблицу данными
                var sortedChartData = chartData.OrderBy(d => d.Key).ToList(); // Сортируем данные по дате
                for (int i = 0; i < sortedChartData.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text = sortedChartData[i].Key.ToString("dd/MM/yyyy");
                    table.Cell(i + 2, 2).Range.Text = sortedChartData[i].Value.ToString();
                }

                // Добавляем график для визуализации тенденции
                var chartShape = document.Shapes.AddChart(
                    (Microsoft.Office.Core.XlChartType)Excel.XlChartType.xlLine,  // Используем XlChartType из Excel
                    50, 200, 500, 300  // Позиция и размер графика
                );

                // Получаем график из Shape
                var chart = chartShape.Chart;

                // Создаем серию данных
                var series = chart.SeriesCollection().NewSeries();  // Используем метод SeriesCollection() и NewSeries()

                // Устанавливаем значения для оси X и оси Y
                series.XValues = sortedChartData.Select(d => d.Key.ToString("dd/MM/yyyy")).ToArray(); // Даты по оси X
                series.Values = sortedChartData.Select(d => d.Value).ToArray(); // Количество пропусков по оси Y

                // Стилизация графика
                chart.HasTitle = true;
                chart.ChartTitle.Text = "Тенденция пропусков";

                chart.Axes(Excel.XlAxisType.xlCategory).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlCategory).AxisTitle.Text = "Дата";
                chart.Axes(Excel.XlAxisType.xlValue).HasTitle = true;
                chart.Axes(Excel.XlAxisType.xlValue).AxisTitle.Text = "Количество пропусков";

                // Генерация имени файла на основе типа отчета, даты и времени
                string reportType = "Аналитический отчет";
                string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); // Форматируем текущую дату и время
                string fileName = $"{reportType}_{currentDateTime}.docx"; // Имя файла

                // Путь для сохранения (используем директорию, где находится приложение)
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);


                // Сохраняем документ
                document.SaveAs2(filePath);
                document.Close();
                wordApp.Quit();

                MessageBox.Show($"Аналитический отчет успешно экспортирован в Word. Файл сохранен как {fileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте аналитического отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void UpdateCharts()
        {
            var statistics = CalculateStatistics();

            // Проверяем, что BarChart не null и его оси инициализированы
            if (BarChart != null && BarChart.AxisX.Count > 0)
            {
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
                //BarChart.Series = BarChartSeries;
            }
            else
            {
                // Логируем или обрабатываем случай, если BarChart не инициализирован
                Console.WriteLine("BarChart или его оси не инициализированы.");
            }

            // Проверяем, что PieChart не null
            if (PieChart != null)
            {
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
                //PieChart.Series = PieChartSeries;
            }
            else
            {
                // Логируем или обрабатываем случай, если PieChart не инициализирован
                Console.WriteLine("PieChart не инициализирован.");
            }

            // Проверяем, что LineChart не null и его оси инициализированы
            if (LineChart != null && LineChart.AxisX.Count > 0)
            {
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
            else
            {
                // Логируем или обрабатываем случай, если LineChart не инициализирован
                Console.WriteLine("LineChart или его оси не инициализированы.");
            }
        }




        // Обработчики для кнопок
        private void GenerateReportByStudent_Click(object sender, RoutedEventArgs e)
        {
            GenerateReportByStudentClassParallel("Student");
        }

        private void GenerateStatisticalReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateStatisticalReport("Month");
        }

        private void GenerateAttendanceRegister_Click(object sender, RoutedEventArgs e)
        {
            GenerateAttendanceRegister();
        }

        private void GenerateAnalyticalAttendanceReport_Click(object sender, RoutedEventArgs e)
        {
            GenerateAnalyticalAttendanceReport();
        }
    }

}
