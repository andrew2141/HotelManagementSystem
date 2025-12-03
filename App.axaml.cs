using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia_application.Views;
using Avalonia_application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Avalonia_application
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            
            // Регистрация сервисов
            serviceCollection.AddSingleton<INavigationService, NavigationService>();
            serviceCollection.AddSingleton<IDatabaseService, DatabaseService>();
            
            // Регистрация вью-моделей
            serviceCollection.AddTransient<LoginViewModel>();
            serviceCollection.AddTransient<MainViewModel>();
            serviceCollection.AddTransient<DashboardViewModel>();
            
            // Регистрация представлений
            serviceCollection.AddTransient<LoginWindow>();
            serviceCollection.AddTransient<MainWindow>();
            serviceCollection.AddTransient<DashboardView>();
            
            Services = serviceCollection.BuildServiceProvider();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var loginWindow = Services.GetService<LoginWindow>();
                loginWindow.DataContext = Services.GetService<LoginViewModel>();
                desktop.MainWindow = loginWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}