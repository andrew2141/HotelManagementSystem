using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia_application.Views;
using Avalonia_application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia_application
{
    public partial class App : Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ServiceProvider = ConfigureServices();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
                desktop.MainWindow = loginWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<DashboardViewModel>();

            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<DashboardView>();

            return services.BuildServiceProvider();
        }
    }
}