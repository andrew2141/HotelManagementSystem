using System;

namespace Avalonia_application.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff"; // Admin, Manager, Staff
    }
}