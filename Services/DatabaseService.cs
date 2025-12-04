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
            // Проверяем, есть ли данные в Employees
            using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Employees", conn))
            {
                var count = (long)await cmd.ExecuteScalarAsync();
                if (count > 0) return;
            }

            // Добавляем сотрудников
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

            // Добавляем номера
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

            // Добавляем тестовых гостей
            var guests = new[]
            {
                ("Петров Сергей Владимирович", "petrov@example.com", "+79161234567", "Окно на юг"),
                ("Смирнова Анна Игоревна", "smirnova@example.com", "+79167654321", "Без подушки")
            };

            foreach (var (name, email, phone, preferences) in guests)
            {
                using var cmd = new NpgsqlCommand(
                    "INSERT INTO Guests (FullName, Email, Phone, Preferences) VALUES (@name, @email, @phone, @preferences)",
                    conn);
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("email", email);
                cmd.Parameters.AddWithValue("phone", phone);
                cmd.Parameters.AddWithValue("preferences", preferences);
                await cmd.ExecuteNonQueryAsync();
            }

            // Добавляем тестовые бронирования
            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO Bookings (GuestID, RoomID, CheckInDate, CheckOutDate, Status, TotalAmount) 
                VALUES (1, 2, @checkIn1, @checkOut1, 'Подтверждено', 5000)",
                conn))
            {
                cmd.Parameters.AddWithValue("checkIn1", DateTime.Today.AddDays(-2));
                cmd.Parameters.AddWithValue("checkOut1", DateTime.Today.AddDays(2));
                await cmd.ExecuteNonQueryAsync();
            }

            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO Bookings (GuestID, RoomID, CheckInDate, CheckOutDate, Status, TotalAmount) 
                VALUES (2, 4, @checkIn2, @checkOut2, 'Подтверждено', 10000)",
                conn))
            {
                cmd.Parameters.AddWithValue("checkIn2", DateTime.Today);
                cmd.Parameters.AddWithValue("checkOut2", DateTime.Today.AddDays(4));
                await cmd.ExecuteNonQueryAsync();
            }

            // Добавляем задачи по уборке
            var cleaningTasks = new[]
            {
                (2, 3, DateTime.Today.AddDays(1), "Назначена"),
                (4, 3, DateTime.Today, "В процессе")
            };

            foreach (var (roomId, employeeId, date, status) in cleaningTasks)
            {
                using var cmd = new NpgsqlCommand(
                    "INSERT INTO CleaningSchedule (RoomID, EmployeeID, ScheduledDate, Status) VALUES (@roomId, @employeeId, @date, @status)",
                    conn);
                cmd.Parameters.AddWithValue("roomId", roomId);
                cmd.Parameters.AddWithValue("employeeId", employeeId);
                cmd.Parameters.AddWithValue("date", date);
                cmd.Parameters.AddWithValue("status", status);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM Rooms ORDER BY RoomNumber", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            var rooms = new List<Room>();
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
                    RoomID = reader.GetInt32(0),
                    RoomNumber = reader.GetString(1),
                    RoomType = reader.GetString(2),
                    Status = reader.GetString(3),
                    PricePerNight = reader.GetDecimal(4)
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
            return result == DBNull.Value ? 0m : (decimal)result;
        }

        public async Task<int> GetTotalRoomsCountAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Rooms", conn);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> GetOccupiedRoomsCountAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Rooms WHERE Status = 'Занят'", conn);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> GetAvailableRoomsCountAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Rooms WHERE Status = 'Свободен'", conn);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<List<Booking>> GetRecentBookingsAsync(int limit = 10)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(@"
                SELECT b.BookingID, g.FullName, r.RoomNumber, b.CheckInDate, b.CheckOutDate, b.Status, b.TotalAmount
                FROM Bookings b
                JOIN Guests g ON b.GuestID = g.GuestID
                JOIN Rooms r ON b.RoomID = r.RoomID
                ORDER BY b.CheckInDate DESC
                LIMIT @limit", conn);
            cmd.Parameters.AddWithValue("limit", limit);
            
            using var reader = await cmd.ExecuteReaderAsync();
            var bookings = new List<Booking>();
            while (await reader.ReadAsync())
            {
                bookings.Add(new Booking
                {
                    BookingID = reader.GetInt32(0),
                    GuestName = reader.GetString(1),
                    RoomNumber = reader.GetString(2),
                    CheckInDate = reader.GetDateTime(3),
                    CheckOutDate = reader.GetDateTime(4),
                    Status = reader.GetString(5),
                    TotalAmount = reader.GetDecimal(6)
                });
            }
            return bookings;
        }

        public async Task<List<CleaningTask>> GetPendingCleaningTasksAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(@"
                SELECT cs.ScheduleID, r.RoomNumber, cs.ScheduledDate, cs.Status, e.FullName
                FROM CleaningSchedule cs
                JOIN Rooms r ON cs.RoomID = r.RoomID
                LEFT JOIN Employees e ON cs.EmployeeID = e.EmployeeID
                WHERE cs.Status IN ('Назначена', 'В процессе')
                ORDER BY cs.ScheduledDate", conn);
            
            using var reader = await cmd.ExecuteReaderAsync();
            var tasks = new List<CleaningTask>();
            while (await reader.ReadAsync())
            {
                tasks.Add(new CleaningTask
                {
                    ScheduleID = reader.GetInt32(0),
                    RoomNumber = reader.GetString(1),
                    ScheduledDate = reader.GetDateTime(2),
                    Status = reader.GetString(3),
                    EmployeeName = reader.IsDBNull(4) ? "Не назначен" : reader.GetString(4)
                });
            }
            return tasks;
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(@"
                SELECT b.BookingID, g.FullName, r.RoomNumber, b.CheckInDate, b.CheckOutDate, b.Status, b.TotalAmount
                FROM Bookings b
                JOIN Guests g ON b.GuestID = g.GuestID
                JOIN Rooms r ON b.RoomID = r.RoomID
                ORDER BY b.CheckInDate DESC", conn);
            
            using var reader = await cmd.ExecuteReaderAsync();
            var bookings = new List<Booking>();
            while (await reader.ReadAsync())
            {
                bookings.Add(new Booking
                {
                    BookingID = reader.GetInt32(0),
                    GuestName = reader.GetString(1),
                    RoomNumber = reader.GetString(2),
                    CheckInDate = reader.GetDateTime(3),
                    CheckOutDate = reader.GetDateTime(4),
                    Status = reader.GetString(5),
                    TotalAmount = reader.GetDecimal(6)
                });
            }
            return bookings;
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(@"
                SELECT b.BookingID, g.FullName, r.RoomNumber, b.CheckInDate, b.CheckOutDate, b.Status, b.TotalAmount
                FROM Bookings b
                JOIN Guests g ON b.GuestID = g.GuestID
                JOIN Rooms r ON b.RoomID = r.RoomID
                WHERE b.BookingID = @id", conn);
            cmd.Parameters.AddWithValue("id", bookingId);
            
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Booking
                {
                    BookingID = reader.GetInt32(0),
                    GuestName = reader.GetString(1),
                    RoomNumber = reader.GetString(2),
                    CheckInDate = reader.GetDateTime(3),
                    CheckOutDate = reader.GetDateTime(4),
                    Status = reader.GetString(5),
                    TotalAmount = reader.GetDecimal(6)
                };
            }
            return null;
        }

        public async Task AddBookingAsync(Booking booking)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            // Получаем GuestID по имени (упрощенная версия)
            using var guestCmd = new NpgsqlCommand("SELECT GuestID FROM Guests WHERE FullName = @name", conn);
            guestCmd.Parameters.AddWithValue("name", booking.GuestName);
            var guestId = await guestCmd.ExecuteScalarAsync();
            
            if (guestId == null)
            {
                // Создаем нового гостя
                using var newGuestCmd = new NpgsqlCommand(
                    "INSERT INTO Guests (FullName) VALUES (@name) RETURNING GuestID", conn);
                newGuestCmd.Parameters.AddWithValue("name", booking.GuestName);
                guestId = await newGuestCmd.ExecuteScalarAsync();
            }

            // Получаем RoomID по номеру
            using var roomCmd = new NpgsqlCommand("SELECT RoomID FROM Rooms WHERE RoomNumber = @number", conn);
            roomCmd.Parameters.AddWithValue("number", booking.RoomNumber);
            var roomId = await roomCmd.ExecuteScalarAsync();

            if (roomId != null)
            {
                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO Bookings (GuestID, RoomID, CheckInDate, CheckOutDate, Status, TotalAmount)
                    VALUES (@guestId, @roomId, @checkIn, @checkOut, @status, @amount)", conn);
                cmd.Parameters.AddWithValue("guestId", guestId);
                cmd.Parameters.AddWithValue("roomId", roomId);
                cmd.Parameters.AddWithValue("checkIn", booking.CheckInDate);
                cmd.Parameters.AddWithValue("checkOut", booking.CheckOutDate);
                cmd.Parameters.AddWithValue("status", booking.Status);
                cmd.Parameters.AddWithValue("amount", booking.TotalAmount);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateBookingAsync(Booking booking)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            // Получаем RoomID по номеру
            using var roomCmd = new NpgsqlCommand("SELECT RoomID FROM Rooms WHERE RoomNumber = @number", conn);
            roomCmd.Parameters.AddWithValue("number", booking.RoomNumber);
            var roomId = await roomCmd.ExecuteScalarAsync();

            if (roomId != null)
            {
                using var cmd = new NpgsqlCommand(@"
                    UPDATE Bookings 
                    SET CheckInDate = @checkIn, CheckOutDate = @checkOut, Status = @status, TotalAmount = @amount
                    WHERE BookingID = @id", conn);
                cmd.Parameters.AddWithValue("id", booking.BookingID);
                cmd.Parameters.AddWithValue("checkIn", booking.CheckInDate);
                cmd.Parameters.AddWithValue("checkOut", booking.CheckOutDate);
                cmd.Parameters.AddWithValue("status", booking.Status);
                cmd.Parameters.AddWithValue("amount", booking.TotalAmount);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteBookingAsync(int bookingId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM Bookings WHERE BookingID = @id", conn);
            cmd.Parameters.AddWithValue("id", bookingId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Guest>> GetAllGuestsAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM Guests ORDER BY FullName", conn);
            
            using var reader = await cmd.ExecuteReaderAsync();
            var guests = new List<Guest>();
            while (await reader.ReadAsync())
            {
                guests.Add(new Guest
                {
                    GuestID = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Preferences = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }
            return guests;
        }

        public async Task<Guest?> GetGuestByIdAsync(int guestId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM Guests WHERE GuestID = @id", conn);
            cmd.Parameters.AddWithValue("id", guestId);
            
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Guest
                {
                    GuestID = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Preferences = reader.IsDBNull(4) ? null : reader.GetString(4)
                };
            }
            return null;
        }

        public async Task AddGuestAsync(Guest guest)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO Guests (FullName, Email, Phone, Preferences) VALUES (@name, @email, @phone, @preferences)",
                conn);
            cmd.Parameters.AddWithValue("name", guest.FullName);
            cmd.Parameters.AddWithValue("email", (object?)guest.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("phone", (object?)guest.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("preferences", (object?)guest.Preferences ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateGuestAsync(Guest guest)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "UPDATE Guests SET FullName = @name, Email = @email, Phone = @phone, Preferences = @preferences WHERE GuestID = @id",
                conn);
            cmd.Parameters.AddWithValue("id", guest.GuestID);
            cmd.Parameters.AddWithValue("name", guest.FullName);
            cmd.Parameters.AddWithValue("email", (object?)guest.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("phone", (object?)guest.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("preferences", (object?)guest.Preferences ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteGuestAsync(int guestId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM Guests WHERE GuestID = @id", conn);
            cmd.Parameters.AddWithValue("id", guestId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM Employees ORDER BY FullName", conn);
            
            using var reader = await cmd.ExecuteReaderAsync();
            var employees = new List<Employee>();
            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    EmployeeID = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Position = reader.GetString(2),
                    Role = reader.GetString(3)
                });
            }
            return employees;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM Employees WHERE EmployeeID = @id", conn);
            cmd.Parameters.AddWithValue("id", employeeId);
            
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Employee
                {
                    EmployeeID = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Position = reader.GetString(2),
                    Role = reader.GetString(3)
                };
            }
            return null;
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO Employees (FullName, Position, Role) VALUES (@name, @position, @role)",
                conn);
            cmd.Parameters.AddWithValue("name", employee.FullName);
            cmd.Parameters.AddWithValue("position", employee.Position);
            cmd.Parameters.AddWithValue("role", employee.Role);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "UPDATE Employees SET FullName = @name, Position = @position, Role = @role WHERE EmployeeID = @id",
                conn);
            cmd.Parameters.AddWithValue("id", employee.EmployeeID);
            cmd.Parameters.AddWithValue("name", employee.FullName);
            cmd.Parameters.AddWithValue("position", employee.Position);
            cmd.Parameters.AddWithValue("role", employee.Role);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteEmployeeAsync(int employeeId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM Employees WHERE EmployeeID = @id", conn);
            cmd.Parameters.AddWithValue("id", employeeId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<CleaningTask>> GetCleaningScheduleAsync(DateTime date)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(@"
                SELECT cs.ScheduleID, r.RoomNumber, cs.ScheduledDate, cs.Status, e.FullName
                FROM CleaningSchedule cs
                JOIN Rooms r ON cs.RoomID = r.RoomID
                LEFT JOIN Employees e ON cs.EmployeeID = e.EmployeeID
                WHERE cs.ScheduledDate = @date
                ORDER BY r.RoomNumber", conn);
            cmd.Parameters.AddWithValue("date", date.Date);
            
            using var reader = await cmd.ExecuteReaderAsync();
            var tasks = new List<CleaningTask>();
            while (await reader.ReadAsync())
            {
                tasks.Add(new CleaningTask
                {
                    ScheduleID = reader.GetInt32(0),
                    RoomNumber = reader.GetString(1),
                    ScheduledDate = reader.GetDateTime(2),
                    Status = reader.GetString(3),
                    EmployeeName = reader.IsDBNull(4) ? "Не назначен" : reader.GetString(4)
                });
            }
            return tasks;
        }

        public async Task AddCleaningTaskAsync(CleaningTask task)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            // Получаем RoomID по номеру
            using var roomCmd = new NpgsqlCommand("SELECT RoomID FROM Rooms WHERE RoomNumber = @number", conn);
            roomCmd.Parameters.AddWithValue("number", task.RoomNumber);
            var roomId = await roomCmd.ExecuteScalarAsync();
            
            if (roomId != null)
            {
                // Получаем EmployeeID по имени (если указано)
                int? employeeId = null;
                if (!string.IsNullOrEmpty(task.EmployeeName) && task.EmployeeName != "Не назначен")
                {
                    using var empCmd = new NpgsqlCommand("SELECT EmployeeID FROM Employees WHERE FullName = @name", conn);
                    empCmd.Parameters.AddWithValue("name", task.EmployeeName);
                    var empId = await empCmd.ExecuteScalarAsync();
                    if (empId != null) employeeId = (int)empId;
                }

                using var cmd = new NpgsqlCommand(
                    "INSERT INTO CleaningSchedule (RoomID, EmployeeID, ScheduledDate, Status) VALUES (@roomId, @employeeId, @date, @status)",
                    conn);
                cmd.Parameters.AddWithValue("roomId", roomId);
                cmd.Parameters.AddWithValue("employeeId", (object?)employeeId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("date", task.ScheduledDate);
                cmd.Parameters.AddWithValue("status", task.Status);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateCleaningTaskAsync(CleaningTask task)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            // Получаем EmployeeID по имени (если указано)
            int? employeeId = null;
            if (!string.IsNullOrEmpty(task.EmployeeName) && task.EmployeeName != "Не назначен")
            {
                using var empCmd = new NpgsqlCommand("SELECT EmployeeID FROM Employees WHERE FullName = @name", conn);
                empCmd.Parameters.AddWithValue("name", task.EmployeeName);
                var empId = await empCmd.ExecuteScalarAsync();
                if (empId != null) employeeId = (int)empId;
            }

            using var cmd = new NpgsqlCommand(
                "UPDATE CleaningSchedule SET ScheduledDate = @date, Status = @status, EmployeeID = @employeeId WHERE ScheduleID = @id",
                conn);
            cmd.Parameters.AddWithValue("id", task.ScheduleID);
            cmd.Parameters.AddWithValue("date", task.ScheduledDate);
            cmd.Parameters.AddWithValue("status", task.Status);
            cmd.Parameters.AddWithValue("employeeId", (object?)employeeId ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteCleaningTaskAsync(int scheduleId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM CleaningSchedule WHERE ScheduleID = @id", conn);
            cmd.Parameters.AddWithValue("id", scheduleId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(@"
                SELECT p.PaymentID, b.BookingID, p.Amount, p.PaymentDate, p.PaymentType
                FROM Payments p
                JOIN Bookings b ON p.BookingID = b.BookingID
                ORDER BY p.PaymentDate DESC", conn);
            
            using var reader = await cmd.ExecuteReaderAsync();
            var payments = new List<Payment>();
            while (await reader.ReadAsync())
            {
                payments.Add(new Payment
                {
                    PaymentID = reader.GetInt32(0),
                    BookingID = reader.GetInt32(1),
                    Amount = reader.GetDecimal(2),
                    PaymentDate = reader.GetDateTime(3),
                    PaymentType = reader.GetString(4)
                });
            }
            return payments;
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO Payments (BookingID, Amount, PaymentDate, PaymentType) VALUES (@bookingId, @amount, @date, @type)",
                conn);
            cmd.Parameters.AddWithValue("bookingId", payment.BookingID);
            cmd.Parameters.AddWithValue("amount", payment.Amount);
            cmd.Parameters.AddWithValue("date", payment.PaymentDate);
            cmd.Parameters.AddWithValue("type", payment.PaymentType);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "UPDATE Payments SET Amount = @amount, PaymentDate = @date, PaymentType = @type WHERE PaymentID = @id",
                conn);
            cmd.Parameters.AddWithValue("id", payment.PaymentID);
            cmd.Parameters.AddWithValue("amount", payment.Amount);
            cmd.Parameters.AddWithValue("date", payment.PaymentDate);
            cmd.Parameters.AddWithValue("type", payment.PaymentType);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeletePaymentAsync(int paymentId)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM Payments WHERE PaymentID = @id", conn);
            cmd.Parameters.AddWithValue("id", paymentId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> GetOccupancyRateAsync(DateTime date)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            // Получаем общее количество номеров
            using var totalCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Rooms", conn);
            var totalRooms = Convert.ToInt32(await totalCmd.ExecuteScalarAsync());
            
            if (totalRooms == 0) return 0;
            
            // Получаем количество занятых номеров на дату
            using var occupiedCmd = new NpgsqlCommand(@"
                SELECT COUNT(DISTINCT r.RoomID)
                FROM Rooms r
                JOIN Bookings b ON r.RoomID = b.RoomID
                WHERE @date BETWEEN b.CheckInDate AND b.CheckOutDate
                AND b.Status != 'Отменено'", conn);
            occupiedCmd.Parameters.AddWithValue("date", date.Date);
            var occupiedRooms = Convert.ToInt32(await occupiedCmd.ExecuteScalarAsync());
            
            return (int)Math.Round((occupiedRooms / (double)totalRooms) * 100);
        }

        public async Task<decimal> GetADRAsync(DateTime startDate, DateTime endDate)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            using var cmd = new NpgsqlCommand(@"
                SELECT AVG(b.TotalAmount / (b.CheckOutDate - b.CheckInDate))
                FROM Bookings b
                WHERE b.CheckInDate BETWEEN @start AND @end
                AND b.Status != 'Отменено'", conn);
            cmd.Parameters.AddWithValue("start", startDate.Date);
            cmd.Parameters.AddWithValue("end", endDate.Date);
            
            var result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? 0m : (decimal)result;
        }

        public async Task<decimal> GetRevPARAsync(DateTime startDate, DateTime endDate)
        {
            await InitializeAsync();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            // Получаем общее количество номеров
            using var totalCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Rooms", conn);
            var totalRooms = Convert.ToInt32(await totalCmd.ExecuteScalarAsync());
            
            if (totalRooms == 0) return 0m;
            
            // Получаем общую выручку за период
            var totalRevenue = await GetTotalRevenueAsync(startDate, endDate);
            
            // Количество дней в периоде
            var days = (endDate - startDate).Days + 1;
            
            return totalRevenue / (totalRooms * days);
        }
    }
}