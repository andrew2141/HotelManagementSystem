using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia_application.Services;
using Avalonia_application.Views;
using System;
using Microsoft.Extensions.DependencyInjection; // Добавлено для GetService<T>()

namespace Avalonia_application.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly System.Timers.Timer _timer;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _currentUserText = string.Empty;

        [ObservableProperty]
        private string _currentDateTimeText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

        public MainViewModel(
            IAuthService authService, 
            INavigationService navigationService,
            IServiceProvider serviceProvider)
        {
            _authService = authService;
            _navigationService = navigationService;
            _serviceProvider = serviceProvider;
            
            UpdateCurrentUserText();
            
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) => CurrentDateTimeText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            _timer.Start();
            
            NavigateToDashboard();
        }

        private void UpdateCurrentUserText()
        {
            if (_authService.CurrentUser != null)
            {
                CurrentUserText = $"Пользователь: {_authService.CurrentUser.FullName} ({_authService.CurrentUser.Position})";
            }
        }

        private void NavigateToDashboard()
        {
            var dashboard = _serviceProvider.GetService<DashboardView>();
            CurrentView = dashboard;
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
            _navigationService.NavigateTo<LoginWindow>();
        }
    }
}