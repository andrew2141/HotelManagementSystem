using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Avalonia_application.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}