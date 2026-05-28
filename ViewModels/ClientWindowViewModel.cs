using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManager.DB;
using GymManager.Models;

namespace GymManager.ViewModels
{
    public partial class ClientWindowViewModel : ViewModelBase
    {
        private readonly ClientRepo _clientRepo;
        private readonly TrainerRepo _trainerRepo;
        private Action? _closeAction;
        private Action? _onSaveSuccess;

        [ObservableProperty] string titleText = "Добавить клиента";
        [ObservableProperty] int? clientId;
        [ObservableProperty] string fullName = "";
        [ObservableProperty] string phone = "";
        [ObservableProperty] string birthDate = "";
        [ObservableProperty] Trainer? selectedTrainer;

        [ObservableProperty] ObservableCollection<Trainer> trainers;

        public ClientWindowViewModel(ClientRepo clientRepo, TrainerRepo trainerRepo)
        {
            _clientRepo = clientRepo;
            _trainerRepo = trainerRepo;

            Trainers = new ObservableCollection<Trainer>();
        }

        public void Initialize(Client? client, Action closeAction, Action onSaveSuccess)
        {
            _closeAction = closeAction;
            _onSaveSuccess = onSaveSuccess;

            Trainers.Clear();
            foreach (var t in _trainerRepo.GetAll())
            {
                Trainers.Add(t);
            }

            if (client != null)
            {
                TitleText = "Редактировать клиента";
                ClientId = client.Id;
                FullName = client.FullName;
                Phone = client.Phone;
                BirthDate = client.BirthDate;
                
                if (client.TrainerId.HasValue)
                {
                    SelectedTrainer = Trainers.FirstOrDefault(t => t.Id == client.TrainerId.Value);
                }
            }
            else
            {
                TitleText = "Добавить клиента";
                ClientId = null;
                FullName = "";
                Phone = "";
                BirthDate = "";
                SelectedTrainer = null;
            }
        }

        [RelayCommand]
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Phone))
                return;

            var client = new Client
            {
                FullName = FullName.Trim(),
                Phone = Phone.Trim(),
                BirthDate = BirthDate?.Trim() ?? "",
                TrainerId = SelectedTrainer?.Id
            };

            if (ClientId == null)
            {
                _clientRepo.Add(client);
            }
            else
            {
                client.Id = ClientId.Value;
                _clientRepo.Update(client);
            }

            _onSaveSuccess?.Invoke();
            _closeAction?.Invoke();
        }

        [RelayCommand]
        public void Cancel()
        {
            _closeAction?.Invoke();
        }
    }
}
