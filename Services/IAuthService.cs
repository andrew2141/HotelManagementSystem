using Avalonia_application.Models;

namespace Avalonia_application.Services
{
    public interface IAuthService
    {
        bool IsAuthenticated { get; }
        Employee? CurrentUser { get; }
        
        bool Login(string username, string password);
        void Logout();
    }
}