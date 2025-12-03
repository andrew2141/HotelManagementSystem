using Avalonia.Controls;

namespace Avalonia_application.Services
{
    public interface INavigationService
    {
        void NavigateTo<T>(object? parameter = null) where T : Control;
        void NavigateBack();
        void ShowDialog<T>(object? parameter = null) where T : Window;
    }
}