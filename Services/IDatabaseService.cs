using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia_application.Models;

namespace Avalonia_application.Services
{
    public interface IDatabaseService
    {
        Task InitializeAsync();
        
        // Управление номерами
        Task<List<Room>> GetAllRoomsAsync();
        
        // Заглушки для остальных методов
        Task<Room> GetRoomByIdAsync(int roomId);
        Task AddRoomAsync(Room room);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(int roomId);
        
        // Управление уборкой
        Task<List<CleaningTask>> GetCleaningScheduleAsync(DateTime date);
        
        // Финансовые показатели
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOccupancyRateAsync(DateTime date);
        Task<decimal> GetADRAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetRevPARAsync(DateTime startDate, DateTime endDate);
        
        // Другие методы, которые могут понадобиться
        Task<List<Booking>> GetAllBookingsAsync();
        Task<List<Guest>> GetAllGuestsAsync();
        Task<List<Employee>> GetAllEmployeesAsync();
    }
}