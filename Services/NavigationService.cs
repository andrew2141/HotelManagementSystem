using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Avalonia_application.Services
{
    public class NavigationService : INavigationService
    {
        private Stack<Control> _navigationStack = new Stack<Control>();
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<T>(object? parameter = null) where T : Control
        {
            var window = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (window?.MainWindow is not MainWindow mainWindow) return;
            
            var view = _serviceProvider.GetService<T>();
            if (view == null) return;
            
            if (mainWindow.Content is Control currentView)
            {
                _navigationStack.Push(currentView);
            }
            
            mainWindow.Content = view;
        }

        public void NavigateBack()
        {
            var window = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (window?.MainWindow is not MainWindow mainWindow) return;
            
            if (_navigationStack.Count > 0)
            {
                mainWindow.Content = _navigationStack.Pop();
            }
        }

        public void ShowDialog<T>(object? parameter = null) where T : Window
        {
            var dialog = _serviceProvider.GetService<T>();
            if (dialog == null) return;
            
            var window = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (window?.MainWindow != null)
            {
                dialog.ShowDialog(window.MainWindow);
            }
        }
    }
}