using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Avalonia_application.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            // Размеры окна входа
            Width = 400;
            Height = 300;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}