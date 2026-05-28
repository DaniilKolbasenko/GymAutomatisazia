using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManager.DB;
using GymManager.Models;

namespace GymManager.ViewModels
{
    public partial class BuySubViewModel : ViewModelBase
    {
        private readonly ClientRepo _clientRepo;
        private readonly SubRepo _subRepo;
        private Action? _closeAction;
        private Action? _onSaveSuccess;

        [ObservableProperty] int clientId;
        [ObservableProperty] string clientName = "";
        [ObservableProperty] Subscription? selectedSub;
        [ObservableProperty] ObservableCollection<Subscription> subscriptions;
        [ObservableProperty] string errorMessage = "";
        [ObservableProperty] bool isSubscriptionActive;

        public BuySubViewModel(ClientRepo clientRepo, SubRepo subRepo)
        {
            _clientRepo = clientRepo;
            _subRepo = subRepo;

            Subscriptions = new ObservableCollection<Subscription>();
        }

        public void Initialize(Client client, Action closeAction, Action onSaveSuccess)
        {
            _closeAction = closeAction;
            _onSaveSuccess = onSaveSuccess;

            ClientId = client.Id;
            ClientName = client.FullName;
            IsSubscriptionActive = client.IsSubscriptionActive;
            ErrorMessage = "";

            Subscriptions.Clear();
            foreach (var s in _subRepo.GetAll())
            {
                Subscriptions.Add(s);
            }
            SelectedSub = Subscriptions.FirstOrDefault();
        }

        partial void OnSelectedSubChanged(Subscription? value)
        {
            ErrorMessage = "";
        }

        [RelayCommand]
        public void Buy()
        {
            if (SelectedSub != null)
            {
                if (SelectedSub.Name.Contains("Заморозка") && !IsSubscriptionActive)
                {
                    ErrorMessage = "Заморозка доступна только при наличии активного абонемента!";
                    return;
                }

                ErrorMessage = "";
                _clientRepo.AddSub(ClientId, SelectedSub.Id);
                _onSaveSuccess?.Invoke();
                _closeAction?.Invoke();
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            _closeAction?.Invoke();
        }
    }
}
