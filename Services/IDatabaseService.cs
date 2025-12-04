using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia_application.Models;

namespace Avalonia_application.Services
{
    public interface IDatabaseService
    {
        Task InitializeAsync();

        // Rooms
        Task<List<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(int roomId);
        Task AddRoomAsync(Room room);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(int roomId);

        // Bookings
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int bookingId);
        Task AddBookingAsync(Booking booking);
        Task UpdateBookingAsync(Booking booking);
        Task DeleteBookingAsync(int bookingId);

        // Guests
        Task<List<Guest>> GetAllGuestsAsync();
        Task<Guest?> GetGuestByIdAsync(int guestId);
        Task AddGuestAsync(Guest guest);
        Task UpdateGuestAsync(Guest guest);
        Task DeleteGuestAsync(int guestId);

        // Employees
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int employeeId);
        Task AddEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(int employeeId);

        // Cleaning
        Task<List<CleaningTask>> GetCleaningScheduleAsync(DateTime date);
        Task AddCleaningTaskAsync(CleaningTask task);
        Task UpdateCleaningTaskAsync(CleaningTask task);
        Task DeleteCleaningTaskAsync(int scheduleId);

        // Payments
        Task<List<Payment>> GetAllPaymentsAsync();
        Task AddPaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int paymentId);

        // Finance
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOccupancyRateAsync(DateTime date);
        Task<decimal> GetADRAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetRevPARAsync(DateTime startDate, DateTime endDate);
    }
}