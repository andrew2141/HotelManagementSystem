using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia_application.Services;
using Avalonia_application.Views;

namespace Avalonia_application.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void Login()
        {
            if (_authService.Login(Username, Password))
            {
                // Переходим на главный экран в зависимости от роли
                _navigationService.NavigateTo<MainWindow>();
            }
            else
            {
                ErrorMessage = "Неверное имя пользователя или пароль";
            }
        }
    }
}