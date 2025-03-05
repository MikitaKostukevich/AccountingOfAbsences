using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingOfAbsences
{
    public class StatisticsViewModel
    {
        public SeriesCollection BarChartSeries { get; set; }
        public SeriesCollection PieChartSeries { get; set; }
        public SeriesCollection LineChartSeries { get; set; }
        public List<string> AbsenceReasonsKeys { get; set; }
        public List<string> AbsenceTrendDates { get; set; }

        public StatisticsViewModel()
        {
            // Инициализация данных
            BarChartSeries = new SeriesCollection();
            PieChartSeries = new SeriesCollection();
            LineChartSeries = new SeriesCollection();
            AbsenceReasonsKeys = new List<string>();
            AbsenceTrendDates = new List<string>();

            LoadChartData(); // Загружаем данные для диаграмм
        }

        private void LoadChartData()
        {
            // Пример данных
            AbsenceReasonsKeys = new List<string> { "Отсутствие по болезни", "Отпуск", "Прочее" };
            BarChartSeries.Add(new ColumnSeries
            {
                Title = "Отсутствия",
                Values = new ChartValues<int> { 5, 10, 3 }
            });

            PieChartSeries.Add(new PieSeries
            {
                Title = "Отсутствие по болезни",
                Values = new ChartValues<int> { 5 }
            });
            PieChartSeries.Add(new PieSeries
            {
                Title = "Отпуск",
                Values = new ChartValues<int> { 10 }
            });
            PieChartSeries.Add(new PieSeries
            {
                Title = "Прочее",
                Values = new ChartValues<int> { 3 }
            });

            AbsenceTrendDates = new List<string> { "2025-03-01", "2025-03-02", "2025-03-03" };
            LineChartSeries.Add(new LineSeries
            {
                Title = "Динамика отсутствий",
                Values = new ChartValues<int> { 2, 3, 5 },
                PointGeometrySize = 10
            });
        }
    }
}
