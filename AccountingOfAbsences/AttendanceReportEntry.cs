using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingOfAbsences
{
    public class AttendanceReportEntry
    {
        public string FullName { get; set; }
        public string ClassName { get; set; }
        public string Reason { get; set; }
        public DateTime Date { get; set; }
        public string Classification { get; set; }
    }

}
