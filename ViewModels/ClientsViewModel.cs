using System;
using System.Collections.ObjectModel;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManager.DB;
using GymManager.Models;
using GymManager.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GymManager.ViewModels
{
    public partial class ClientsViewModel : ViewModelBase
    {
        private readonly ClientRepo _clientRepo;
        private readonly IServiceProvider _serviceProvider;
        private readonly Action _onDataChanged;

        [ObservableProperty] ObservableCollection<Client> clients;
        [ObservableProperty] Client? selectedClient;
        [ObservableProperty] string searchText = "";

        public ClientsViewModel(
            ClientRepo clientRepo,
            IServiceProvider serviceProvider,
            Action onDataChanged)
        {
            _clientRepo = clientRepo;
            _serviceProvider = serviceProvider;
            _onDataChanged = onDataChanged;

            Clients = new ObservableCollection<Client>();
        }

        [RelayCommand]
        public void RefreshClients()
        {
            Clients.Clear();
            var list = _clientRepo.GetAll(SearchText);
            foreach (var client in list)
            {
                Clients.Add(client);
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            RefreshClients();
        }

        [RelayCommand]
        public void ClearSearch()
        {
            SearchText = "";
            RefreshClients();
        }

        [RelayCommand]
        public void AddClient()
        {
            var win = _serviceProvider.GetRequiredService<ClientWindow>();
            var vm = _serviceProvider.GetRequiredService<ClientWindowViewModel>();

            vm.Initialize(null, () => win.Close(), () => {
                RefreshClients();
                _onDataChanged?.Invoke();
            });

            win.DataContext = vm;

            var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (desktop?.MainWindow != null)
            {
                win.ShowDialog(desktop.MainWindow);
            }
            else
            {
                win.Show();
            }
        }

        [RelayCommand]
        public void EditClient()
        {
            if (SelectedClient == null)
                return;

            var win = _serviceProvider.GetRequiredService<ClientWindow>();
            var vm = _serviceProvider.GetRequiredService<ClientWindowViewModel>();

            vm.Initialize(SelectedClient, () => win.Close(), () => {
                RefreshClients();
                _onDataChanged?.Invoke();
            });

            win.DataContext = vm;

            var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (desktop?.MainWindow != null)
            {
                win.ShowDialog(desktop.MainWindow);
            }
            else
            {
                win.Show();
            }
        }

        [RelayCommand]
        public void BuySub()
        {
            if (SelectedClient == null)
                return;

            var win = _serviceProvider.GetRequiredService<BuySubWindow>();
            var vm = _serviceProvider.GetRequiredService<BuySubViewModel>();

            vm.Initialize(SelectedClient, () => win.Close(), () => {
                RefreshClients();
                _onDataChanged?.Invoke();
            });

            win.DataContext = vm;

            var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (desktop?.MainWindow != null)
            {
                win.ShowDialog(desktop.MainWindow);
            }
            else
            {
                win.Show();
            }
        }

        [RelayCommand]
        public void DeleteClient()
        {
            if (SelectedClient != null)
            {
                _clientRepo.Delete(SelectedClient.Id);
                RefreshClients();
                _onDataChanged?.Invoke();
            }
        }
    }
}
