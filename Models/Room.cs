using System;

namespace Avalonia_application.Models
{
    public class Room
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = "Свободен";
        public decimal PricePerNight { get; set; }
    }
}