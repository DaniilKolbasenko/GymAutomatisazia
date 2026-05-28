using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GymManager.DB;

namespace GymManager.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public ClientsViewModel ClientsTab { get; }
        public VisitsViewModel VisitsTab { get; }
        public SubsViewModel SubsTab { get; }
        public TrainersViewModel TrainersTab { get; }
        public AdminsViewModel AdminsTab { get; }
        public StatsViewModel StatsTab { get; }

        [ObservableProperty] string currentAdmin = "admin";

        public MainViewModel(
            ClientRepo clientRepo,
            SubRepo subRepo,
            TrainerRepo trainerRepo,
            VisitRepo visitRepo,
            UserRepo userRepo,
            IServiceProvider serviceProvider)
        {
            Action refreshAll = () => LoadAllData();

            ClientsTab = new ClientsViewModel(clientRepo, serviceProvider, refreshAll);
            VisitsTab = new VisitsViewModel(visitRepo, clientRepo, refreshAll);
            SubsTab = new SubsViewModel(subRepo, refreshAll);
            TrainersTab = new TrainersViewModel(trainerRepo, refreshAll);
            AdminsTab = new AdminsViewModel(userRepo, () => CurrentAdmin, refreshAll);
            StatsTab = new StatsViewModel(visitRepo);

            LoadAllData();
        }

        public void LoadAllData()
        {
            ClientsTab.RefreshClients();
            VisitsTab.RefreshVisits();
            VisitsTab.RefreshClients();
            SubsTab.RefreshSubscriptions();
            TrainersTab.RefreshTrainers();
            AdminsTab.RefreshAdmins();
            StatsTab.RefreshStats();
        }
    }
}
