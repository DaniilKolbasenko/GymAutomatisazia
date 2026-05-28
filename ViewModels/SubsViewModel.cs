using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManager.DB;
using GymManager.Models;

namespace GymManager.ViewModels
{
    public partial class SubsViewModel : ViewModelBase
    {
        private readonly SubRepo _subRepo;
        private readonly Action _onDataChanged;

        [ObservableProperty] ObservableCollection<Subscription> subscriptions;
        [ObservableProperty] Subscription? selectedSubscription;

        [ObservableProperty] string subId = "";
        [ObservableProperty] string subName = "";
        [ObservableProperty] string subDuration = "";
        [ObservableProperty] string subPrice = "";

        public SubsViewModel(SubRepo subRepo, Action onDataChanged)
        {
            _subRepo = subRepo;
            _onDataChanged = onDataChanged;

            Subscriptions = new ObservableCollection<Subscription>();
        }

        public void RefreshSubscriptions()
        {
            Subscriptions.Clear();
            var list = _subRepo.GetAll();
            foreach (var sub in list)
            {
                Subscriptions.Add(sub);
            }
        }

        partial void OnSelectedSubscriptionChanged(Subscription? value)
        {
            if (value != null)
            {
                SubId = value.Id.ToString();
                SubName = value.Name;
                SubDuration = value.DurationDays.ToString();
                SubPrice = value.Price.ToString();
            }
        }

        [RelayCommand]
        public void ClearSubscriptionForm()
        {
            SelectedSubscription = null;
            SubId = "";
            SubName = "";
            SubDuration = "";
            SubPrice = "";
        }

        [RelayCommand]
        public void SaveSubscription()
        {
            if (string.IsNullOrWhiteSpace(SubName) ||
                !int.TryParse(SubDuration, out int duration) ||
                !double.TryParse(SubPrice, out double price))
            {
                return;
            }

            var sub = new Subscription
            {
                Name = SubName.Trim(),
                DurationDays = duration,
                Price = price
            };

            if (string.IsNullOrEmpty(SubId))
            {
                _subRepo.Add(sub);
            }
            else
            {
                sub.Id = int.Parse(SubId);
                _subRepo.Update(sub);
            }

            ClearSubscriptionForm();
            RefreshSubscriptions();
            _onDataChanged?.Invoke();
        }

        [RelayCommand]
        public void DeleteSubscription()
        {
            if (SelectedSubscription != null)
            {
                _subRepo.Delete(SelectedSubscription.Id);
                ClearSubscriptionForm();
                RefreshSubscriptions();
                _onDataChanged?.Invoke();
            }
        }
    }
}
