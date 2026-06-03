using Avalonia;
using System;
using GymManager.DB;
using GymManager.ViewModels;
using GymManager.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GymManager;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((c, s) =>
            {
                s.Configure<DbConfig>(c.Configuration.GetSection("DatabaseConnection"));

                s.AddSingleton<UserRepo>();
                s.AddSingleton<ClientRepo>();
                s.AddSingleton<SubRepo>();
                s.AddSingleton<TrainerRepo>();
                s.AddSingleton<VisitRepo>();

                s.AddTransient<LoginWindow>();
                s.AddTransient<MainWindow>();
                s.AddTransient<MainViewModel>();
                s.AddTransient<ClientWindow>();
                s.AddTransient<ClientWindowViewModel>();
                s.AddTransient<BuySubWindow>();
                s.AddTransient<BuySubViewModel>();
            })
            .Build();

        BuildAvaloniaApp(host.Services)
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp(IServiceProvider serviceProvider)
        => AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
