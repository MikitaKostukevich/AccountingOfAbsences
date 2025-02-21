using AccountingOfAbsences;
using System;

public class Record
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Reason { get; set; }
    public DateTime Date { get; set; }
    public string Classification { get; set; }

    // Навигационное свойство для связи с таблицей Students
    public virtual Student Student { get; set; }
}
