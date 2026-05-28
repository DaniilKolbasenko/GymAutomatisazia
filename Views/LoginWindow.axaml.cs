using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using GymManager.DB;
using GymManager.ViewModels;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace GymManager.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IServiceProvider _serviceProvider = null!;
        private readonly UserRepo _userRepo = null!;

        public LoginWindow()
        {
            InitializeComponent();
        }

        public LoginWindow(IServiceProvider serviceProvider, UserRepo userRepo)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _userRepo = userRepo;
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text ?? "";
            string password = PasswordTextBox.Text ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Заполните все поля!");
                return;
            }

            var user = _userRepo.ValidateUser(username.Trim(), password);

            if (user != null)
            {
                ErrorTextBlock.IsVisible = false;

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                if (mainWindow.DataContext is MainViewModel vm)
                {
                    vm.CurrentAdmin = user.Username;
                }

                if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                }

                this.Close();
            }
            else
            {
                ShowError("Неверный логин или пароль!");
            }
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.IsVisible = true;
        }
    }
}
