using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia_application.Services;
using Avalonia_application.Models;

namespace Avalonia_application.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly IDatabaseService _databaseService;

        [ObservableProperty]
        private int _totalRooms;

        [ObservableProperty]
        private int _occupiedRooms;

        [ObservableProperty]
        private int _availableRooms;

        [ObservableProperty]
        private int _occupancyRate;

        [ObservableProperty]
        private List<Booking> _recentBookings = new List<Booking>();

        [ObservableProperty]
        private List<CleaningTask> _pendingCleaningTasks = new List<CleaningTask>();

        public DashboardViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            LoadDashboardData();
        }

        private async void LoadDashboardData()
        {
            try
            {
                await LoadRoomStatistics();
                await LoadRecentBookings();
                await LoadPendingCleaningTasks();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных дашборда: {ex.Message}");
            }
        }

        private async Task LoadRoomStatistics()
        {
            var rooms = await _databaseService.GetAllRoomsAsync();
            TotalRooms = rooms.Count;
            OccupiedRooms = rooms.Count(r => r.Status == "Занят");
            AvailableRooms = TotalRooms - OccupiedRooms;
            OccupancyRate = TotalRooms > 0 ? (int)Math.Round((decimal)OccupiedRooms / TotalRooms * 100) : 0;
        }

        private async Task LoadRecentBookings()
        {
            var bookings = await _databaseService.GetAllBookingsAsync();
            RecentBookings = bookings.OrderByDescending(b => b.CheckInDate).Take(5).ToList();
        }

        private async Task LoadPendingCleaningTasks()
        {
            var today = DateTime.Today;
            PendingCleaningTasks = await _databaseService.GetCleaningScheduleAsync(today);
        }
    }
}