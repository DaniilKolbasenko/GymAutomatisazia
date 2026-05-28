using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManager.DB;
using GymManager.Models;

namespace GymManager.ViewModels
{
    public partial class VisitsViewModel : ViewModelBase
    {
        private readonly VisitRepo _visitRepo;
        private readonly ClientRepo _clientRepo;
        private readonly Action _onDataChanged;

        [ObservableProperty] ObservableCollection<Visit> visits;
        [ObservableProperty] ObservableCollection<Client> clients;
        [ObservableProperty] Client? selectedVisitClient;

        
        [ObservableProperty] string visitStatusText = "";
        [ObservableProperty] bool isVisitStatusVisible = false;

        public VisitsViewModel(VisitRepo visitRepo, ClientRepo clientRepo, Action onDataChanged)
        {
            _visitRepo = visitRepo;
            _clientRepo = clientRepo;
            _onDataChanged = onDataChanged;

            Visits = new ObservableCollection<Visit>();
            Clients = new ObservableCollection<Client>();
        }

        public void RefreshVisits()
        {
            Visits.Clear();
            var list = _visitRepo.GetAll();
            foreach (var v in list)
            {
                Visits.Add(v);
            }
        }

        public void RefreshClients()
        {
            Clients.Clear();
            var list = _clientRepo.GetAll("");
            foreach (var c in list)
            {
                Clients.Add(c);
            }
        }

        [RelayCommand]
        public void RegisterVisit()
        {
            if (SelectedVisitClient != null)
            {
                string result = _visitRepo.AddVisit(SelectedVisitClient.Id);
                VisitStatusText = $"Результат: {result}";
                IsVisitStatusVisible = true;

                if (result == "Успешно")
                {
                    Console.WriteLine("Четко");
                    RefreshVisits();
                    _onDataChanged?.Invoke();
                }
                else
                {
                    Console.WriteLine("Не получилось");
                }
            }
        }
    }
}
