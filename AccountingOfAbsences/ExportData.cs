using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingOfAbsences
{
    public class ExportData
    {
        public string District { get; set; } // Район
        public int TotalStudents { get; set; } // Общее количество учащихся
        public int StudentsPresent { get; set; } // Присутствующие
        public int StudentsAbsent { get; set; } // Отсутствующие
        public double AttendancePercentage { get; set; } // Процент посещаемости
        public double AbsencePercentage { get; set; } // Процент отсутствующих
        public Dictionary<string, int> AbsenceReasons { get; set; } // Причины отсутствия
    }

}
