using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManager.DB;
using GymManager.Models;

namespace GymManager.ViewModels
{
    public partial class AdminsViewModel : ViewModelBase
    {
        private readonly UserRepo _userRepo;
        private readonly Func<string> _getCurrentAdmin;
        private readonly Action _onDataChanged;

        [ObservableProperty] ObservableCollection<User> administrators;
        [ObservableProperty] User? selectedAdmin;

        [ObservableProperty] string adminId = "";
        [ObservableProperty] string adminUsername = "";
        [ObservableProperty] string adminPassword = "";
        [ObservableProperty] string adminRole = "Администратор";

        public AdminsViewModel(UserRepo userRepo, Func<string> getCurrentAdmin, Action onDataChanged)
        {
            _userRepo = userRepo;
            _getCurrentAdmin = getCurrentAdmin;
            _onDataChanged = onDataChanged;

            Administrators = new ObservableCollection<User>();
        }

        public void RefreshAdmins()
        {
            Administrators.Clear();
            var list = _userRepo.GetAll();
            foreach (var user in list)
            {
                Administrators.Add(user);
            }
        }

        partial void OnSelectedAdminChanged(User? value)
        {
            if (value != null)
            {
                AdminId = value.Id.ToString();
                AdminUsername = value.Username;
                AdminRole = value.Role;
                AdminPassword = "";
            }
            else
            {
                AdminId = "";
                AdminUsername = "";
                AdminRole = "Администратор";
                AdminPassword = "";
            }
        }

        [RelayCommand]
        public void ClearAdminForm()
        {
            SelectedAdmin = null;
            AdminId = "";
            AdminUsername = "";
            AdminPassword = "";
            AdminRole = "Администратор";
        }

        [RelayCommand]
        public void SaveAdmin()
        {
            if (string.IsNullOrWhiteSpace(AdminUsername))
                return;

            if (string.IsNullOrEmpty(AdminId))
            {
                if (string.IsNullOrWhiteSpace(AdminPassword))
                    return;

                var user = new User
                {
                    Username = AdminUsername.Trim(),
                    Role = AdminRole.Trim()
                };

                _userRepo.Add(user, AdminPassword);
            }

            ClearAdminForm();
            RefreshAdmins();
            _onDataChanged?.Invoke();
        }

        [RelayCommand]
        public void DeleteAdmin()
        {
            if (SelectedAdmin != null)
            {
                string currentAdminName = _getCurrentAdmin?.Invoke() ?? "";
                if (SelectedAdmin.Username.Equals(currentAdminName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                _userRepo.Delete(SelectedAdmin.Id);
                ClearAdminForm();
                RefreshAdmins();
                _onDataChanged?.Invoke();
            }
        }
    }
}
