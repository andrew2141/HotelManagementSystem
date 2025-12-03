using System;

namespace Avalonia_application.Models
{
    public class CleaningTask
    {
        public int ScheduleID { get; set; }
        public int RoomID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = "Назначена";
        public string RoomNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }
}