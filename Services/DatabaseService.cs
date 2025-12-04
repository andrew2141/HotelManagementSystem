using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia_application.Models;
using Npgsql;

namespace Avalonia_application.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private bool _isInitialized;

        public DatabaseService()
        {
            _connectionString = "Host=localhost;Port=5432;Database=hotel;Username=postgres;Password=password";
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await CreateTablesIfNotExistAsync(conn);
            
            _isInitialized = true;
        }

        private async Task CreateTablesIfNotExistAsync(NpgsqlConnection conn)
        {
            var commands = new[]
            {
                @"CREATE TABLE IF NOT EXISTS Rooms (
                    RoomID SERIAL PRIMARY KEY,
                    RoomNumber VARCHAR(10) NOT NULL UNIQUE,
                    RoomType VARCHAR(50) NOT NULL,
                    Status VARCHAR(20) DEFAULT 'Свободен',
                    PricePerNight DECIMAL(10,2) NOT NULL
                )",
                
                @"CREATE TABLE IF NOT EXISTS Employees (
                    EmployeeID SERIAL PRIMARY KEY,
                    FullName VARCHAR(100) NOT NULL,
                    Position VARCHAR(50) NOT NULL,
                    Role VARCHAR(20) DEFAULT 'Staff'
                )",
                
                @"CREATE TABLE IF NOT EXISTS Guests (
                    GuestID SERIAL PRIMARY KEY,
                    FullName VARCHAR(100) NOT NULL,
                    Email VARCHAR(100),
                    Phone VARCHAR(20),
                    Preferences TEXT
                )",
                
                @"CREATE TABLE IF NOT EXISTS Bookings (
                    BookingID SERIAL PRIMARY KEY,
                    GuestID INT NOT NULL REFERENCES Guests(GuestID),
                    RoomID INT NOT NULL REFERENCES Rooms(RoomID),
                    CheckInDate DATE NOT NULL,
                    CheckOutDate DATE NOT NULL,
                    Status VARCHAR(20) DEFAULT 'Подтверждено',
                    TotalAmount DECIMAL(10,2) NOT NULL
                )",
                
                @"CREATE TABLE IF NOT EXISTS CleaningSchedule (
                    ScheduleID SERIAL PRIMARY KEY,
                    RoomID INT NOT NULL REFERENCES Rooms(RoomID),
                    EmployeeID INT REFERENCES Employees(EmployeeID),
                    ScheduledDate DATE NOT NULL,
                    Status VARCHAR(20) DEFAULT 'Назначена'
                )",
                
                @"CREATE TABLE IF NOT EXISTS Payments (
                    PaymentID SERIAL PRIMARY KEY,
                    BookingID INT NOT NULL REFERENCES Bookings(BookingID),
                    Amount DECIMAL(10,2) NOT NULL,
                    PaymentDate DATE NOT NULL,
                    PaymentType VARCHAR(50) NOT NULL
                )"
            };

            foreach (var commandText in commands)
            {
                using var cmd = new NpgsqlCommand(commandText, conn);
                await cmd.ExecuteNonQueryAsync();
            }
            
            await SeedTestDataAsync(conn);
        }

        private async Task SeedTestDataAsync(NpgsqlConnection conn)
        {
            using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Employees", conn))
            {
                var count = (long)await cmd.ExecuteScalarAsync();
                if (count > 0) return;
            }

            var employees = new[]
            {
                ("Иванов Иван Иванович", "Администратор", "Admin"),
                ("Петрова Мария Петровна", "Руководитель", "Manager"),
                ("Сидоров Алексей Сидорович", "Уборщик", "Staff")
            };

            foreach (var (name, position, role) in employees)
            {
                using var cmd = new NpgsqlCommand(
                    "INSERT INTO Employees (FullName, Position, Role) VALUES (@name, @position, @role)",
                    conn);
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("position", position);
                cmd.Parameters.AddWithValue("role", role);
                await cmd.ExecuteNonQueryAsync();
            }

            var rooms = new[]
            {
                ("101", "Стандарт", "Свободен", 2500.00m),
                ("102", "Стандарт", "Занят", 2500.00m),
                ("201", "Люкс", "Свободен", 5000.00m),
                ("202", "Люкс", "Занят", 5000.00m)
            };

            foreach (var (number, type, status, price) in rooms)
            {
                using var cmd = new NpgsqlCommand(
                    "INSERT INTO Rooms (RoomNumber, RoomType, Status, PricePerNight) VALUES (@number, @type, @status, @price)",
                    conn);
                cmd.Parameters.AddWithValue("number", number);
                cmd.Parameters.AddWithValue("type", type);
                cmd.Parameters.AddWithValue("status", status);
                cmd.Parameters.AddWithValue("price", price);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Реализации методов (пример для GetAllRoomsAsync, аналогично для остальных)
        public async Task<List<Room>> GetAllRoomsAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM Rooms", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            var rooms = new List<Room>();
            while (await reader.ReadAsync())
            {
                rooms.Add(new Room
                {
                    RoomID = reader.GetInt32("RoomID"),
                    RoomNumber = reader.GetString("RoomNumber"),
                    RoomType = reader.GetString("RoomType"),
                    Status = reader.GetString("Status"),
                    PricePerNight = reader.GetDecimal("PricePerNight")
                });
            }
            return rooms;
        }

        public async Task<Room?> GetRoomByIdAsync(int roomId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM Rooms WHERE RoomID = @id", conn);
            cmd.Parameters.AddWithValue("id", roomId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Room
                {
                    RoomID = reader.GetInt32("RoomID"),
                    RoomNumber = reader.GetString("RoomNumber"),
                    RoomType = reader.GetString("RoomType"),
                    Status = reader.GetString("Status"),
                    PricePerNight = reader.GetDecimal("PricePerNight")
                };
            }
            return null;
        }

        public async Task AddRoomAsync(Room room)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO Rooms (RoomNumber, RoomType, Status, PricePerNight) VALUES (@number, @type, @status, @price)",
                conn);
            cmd.Parameters.AddWithValue("number", room.RoomNumber);
            cmd.Parameters.AddWithValue("type", room.RoomType);
            cmd.Parameters.AddWithValue("status", room.Status);
            cmd.Parameters.AddWithValue("price", room.PricePerNight);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateRoomAsync(Room room)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "UPDATE Rooms SET RoomNumber = @number, RoomType = @type, Status = @status, PricePerNight = @price WHERE RoomID = @id",
                conn);
            cmd.Parameters.AddWithValue("id", room.RoomID);
            cmd.Parameters.AddWithValue("number", room.RoomNumber);
            cmd.Parameters.AddWithValue("type", room.RoomType);
            cmd.Parameters.AddWithValue("status", room.Status);
            cmd.Parameters.AddWithValue("price", room.PricePerNight);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteRoomAsync(int roomId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM Rooms WHERE RoomID = @id", conn);
            cmd.Parameters.AddWithValue("id", roomId);
            await cmd.ExecuteNonQueryAsync();
        }

        // Аналогично реализовать остальные методы (GetAllBookingsAsync, etc.). Для краткости опущено, но в реальном проекте добавьте похожие запросы.
        // Пример стуба для GetTotalRevenueAsync
        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "SELECT SUM(Amount) FROM Payments p JOIN Bookings b ON p.BookingID = b.BookingID WHERE p.PaymentDate BETWEEN @start AND @end",
                conn);
            cmd.Parameters.AddWithValue("start", startDate.Date);
            cmd.Parameters.AddWithValue("end", endDate.Date);
            var result = await cmd.ExecuteScalarAsync();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
        }

        // Стубы для остальных
        public async Task<List<Booking>> GetAllBookingsAsync() => new List<Booking>();
        public async Task<Booking?> GetBookingByIdAsync(int bookingId) => null;
        public async Task AddBookingAsync(Booking booking) { }
        public async Task UpdateBookingAsync(Booking booking) { }
        public async Task DeleteBookingAsync(int bookingId) { }
        public async Task<List<Guest>> GetAllGuestsAsync() => new List<Guest>();
        public async Task<Guest?> GetGuestByIdAsync(int guestId) => null;
        public async Task AddGuestAsync(Guest guest) { }
        public async Task UpdateGuestAsync(Guest guest) { }
        public async Task DeleteGuestAsync(int guestId) { }
        public async Task<List<Employee>> GetAllEmployeesAsync() => new List<Employee>();
        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId) => null;
        public async Task AddEmployeeAsync(Employee employee) { }
        public async Task UpdateEmployeeAsync(Employee employee) { }
        public async Task DeleteEmployeeAsync(int employeeId) { }
        public async Task<List<CleaningTask>> GetCleaningScheduleAsync(DateTime date) => new List<CleaningTask>();
        public async Task AddCleaningTaskAsync(CleaningTask task) { }
        public async Task UpdateCleaningTaskAsync(CleaningTask task) { }
        public async Task DeleteCleaningTaskAsync(int scheduleId) { }
        public async Task<List<Payment>> GetAllPaymentsAsync() => new List<Payment>();
        public async Task AddPaymentAsync(Payment payment) { }
        public async Task UpdatePaymentAsync(Payment payment) { }
        public async Task DeletePaymentAsync(int paymentId) { }
        public async Task<int> GetOccupancyRateAsync(DateTime date) => 0;
        public async Task<decimal> GetADRAsync(DateTime startDate, DateTime endDate) => 0m;
        public async Task<decimal> GetRevPARAsync(DateTime startDate, DateTime endDate) => 0m;
    }
}