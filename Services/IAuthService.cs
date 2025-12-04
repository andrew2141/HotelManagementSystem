using Avalonia_application.Models;

namespace Avalonia_application.Services
{
    public interface IAuthService
    {
        bool Login(string username, string password);
        void Logout();
        bool IsAuthenticated { get; }
        Employee? CurrentUser { get; }
    }
}