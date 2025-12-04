using System;

namespace Avalonia_application.Models
{
    public class Guest
    {
        public int GuestID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Preferences { get; set; } = string.Empty;
    }
}