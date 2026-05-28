using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using GymManager.DB;
using GymManager.Models;

namespace GymManager.ViewModels
{
    public partial class StatsViewModel : ViewModelBase
    {
        private readonly VisitRepo _visitRepo;

        [ObservableProperty] string totalMembers = "0";
        [ObservableProperty] string activeSubscriptions = "0";
        [ObservableProperty] string todayVisits = "0";
        [ObservableProperty] string totalRevenue = "0 руб.";

        [ObservableProperty] ObservableCollection<ChartItem> chartItems;
        [ObservableProperty] ObservableCollection<Client> expiringClients;

        public StatsViewModel(VisitRepo visitRepo)
        {
            _visitRepo = visitRepo;

            ChartItems = new ObservableCollection<ChartItem>();
            ExpiringClients = new ObservableCollection<Client>();
        }

        public void RefreshStats()
        {
            var stats = _visitRepo.GetStats();

            TotalMembers = stats.ContainsKey("TotalMembers") ? stats["TotalMembers"].ToString() : "0";
            ActiveSubscriptions = stats.ContainsKey("ActiveSubscriptions") ? stats["ActiveSubscriptions"].ToString() : "0";
            TodayVisits = stats.ContainsKey("TodayVisits") ? stats["TodayVisits"].ToString() : "0";
            TotalRevenue = stats.ContainsKey("TotalRevenue") ? $"{stats["TotalRevenue"]:N0} руб." : "0 руб.";

            ExpiringClients.Clear();
            var expiring = _visitRepo.GetEndingSubs();
            foreach (var c in expiring)
            {
                ExpiringClients.Add(c);
            }

            ChartItems.Clear();
            var dist = _visitRepo.GetSubStats();
            int maxCount = dist.Values.Count > 0 ? dist.Values.Max() : 0;
            if (maxCount == 0) maxCount = 1;

            foreach (var kvp in dist)
            {
                ChartItems.Add(new ChartItem
                {
                    Name = kvp.Key,
                    Count = kvp.Value,
                    MaxCount = maxCount
                });
            }
        }
    }
}
