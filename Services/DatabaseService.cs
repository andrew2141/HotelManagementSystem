using System;
using System.Collections.Generic;
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
                    Position VARCHAR(50) NOT NULL
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
                    GuestID INT NOT NULL,
                    RoomID INT NOT NULL,
                    CheckInDate DATE NOT NULL,
                    CheckOutDate DATE NOT NULL,
                    Status VARCHAR(20) DEFAULT 'Подтверждено',
                    TotalAmount DECIMAL(10,2) NOT NULL
                )",
                
                @"CREATE TABLE IF NOT EXISTS CleaningSchedule (
                    ScheduleID SERIAL PRIMARY KEY,
                    RoomID INT NOT NULL,
                    EmployeeID INT,
                    ScheduledDate DATE NOT NULL,
                    Status VARCHAR(20) DEFAULT 'Назначена'
                )",
                
                @"CREATE TABLE IF NOT EXISTS Payments (
                    PaymentID SERIAL PRIMARY KEY,
                    BookingID INT NOT NULL,
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
                ("Иванов Иван Иванович", "Администратор"),
                ("Петрова Мария Петровна", "Руководитель"),
                ("Сидоров Алексей Сидорович", "Уборщик")
            };

            foreach (var (name, position) in employees)
            {
                using var cmd = new NpgsqlCommand(
                    "INSERT INTO Employees (FullName, Position) VALUES (@name, @position)",
                    conn);
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("position", position);
                await cmd.ExecuteNonQueryAsync();
            }

            var rooms = new[]
            {
                ("101", "Стандарт", "Свободен", 2500.00m),
                ("102", "Стандарт", "Свободен", 2500.00m),
                ("201", "Люкс", "Свободен", 5000.00m),
                ("202", "Люкс", "Свободен", 5000.00m)
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

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            await InitializeAsync();
            var rooms = new List<Room>();
            
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            using var cmd = new NpgsqlCommand("SELECT * FROM Rooms ORDER BY RoomNumber", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                rooms.Add(new Room
                {
                    RoomID = reader.GetInt32(0),
                    RoomNumber = reader.GetString(1),
                    RoomType = reader.GetString(2),
                    Status = reader.GetString(3),
                    PricePerNight = reader.GetDecimal(4)
                });
            }
            
            return rooms;
        }
        
        // Реализация недостающих методов интерфейса
        public Task<Room> GetRoomByIdAsync(int roomId) => Task.FromResult<Room>(null);
        public Task AddRoomAsync(Room room) => Task.CompletedTask;
        public Task UpdateRoomAsync(Room room) => Task.CompletedTask;
        public Task DeleteRoomAsync(int roomId) => Task.CompletedTask;
        public Task<List<Booking>> GetAllBookingsAsync() => Task.FromResult(new List<Booking>());
        public Task<List<Guest>> GetAllGuestsAsync() => Task.FromResult(new List<Guest>());
        public Task<List<Employee>> GetAllEmployeesAsync() => Task.FromResult(new List<Employee>());
        public Task<List<CleaningTask>> GetCleaningScheduleAsync(DateTime date) => Task.FromResult(new List<CleaningTask>());
        public Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate) => Task.FromResult(0m);
        public Task<int> GetOccupancyRateAsync(DateTime date) => Task.FromResult(0);
        public Task<decimal> GetADRAsync(DateTime startDate, DateTime endDate) => Task.FromResult(0m);
        public Task<decimal> GetRevPARAsync(DateTime startDate, DateTime endDate) => Task.FromResult(0m);
    }
}