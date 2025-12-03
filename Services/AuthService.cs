using Avalonia_application.Models;
using System.Linq;

namespace Avalonia_application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDatabaseService _databaseService;
        private Employee? _currentUser;

        public AuthService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public bool IsAuthenticated => _currentUser != null;
        public Employee? CurrentUser => _currentUser;

        public bool Login(string username, string password)
        {
            // В реальном приложении здесь будет проверка учетных данных
            if (username == "admin" && password == "admin")
            {
                _currentUser = new Employee 
                { 
                    EmployeeID = 1, 
                    FullName = "Иванов Иван Иванович", 
                    Position = "Администратор" 
                };
                return true;
            }
            else if (username == "manager" && password == "manager")
            {
                _currentUser = new Employee 
                { 
                    EmployeeID = 2, 
                    FullName = "Петрова Мария Петровна", 
                    Position = "Руководитель" 
                };
                return true;
            }
            
            return false;
        }

        public void Logout()
        {
            _currentUser = null;
        }
    }
}