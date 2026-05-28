using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManager.DB;
using GymManager.Models;

namespace GymManager.ViewModels
{
    public partial class TrainersViewModel : ViewModelBase
    {
        private readonly TrainerRepo _trainerRepo;
        private readonly Action _onDataChanged;

        [ObservableProperty] ObservableCollection<Trainer> trainers;
        [ObservableProperty] ObservableCollection<Client> trainerClients;
        [ObservableProperty] Trainer? selectedTrainer;

        [ObservableProperty] string trainerId = "";
        [ObservableProperty] string trainerName = "";
        [ObservableProperty] string trainerSpec = "";
        [ObservableProperty] string trainerPhone = "";

        public TrainersViewModel(TrainerRepo trainerRepo, Action onDataChanged)
        {
            _trainerRepo = trainerRepo;
            _onDataChanged = onDataChanged;

            Trainers = new ObservableCollection<Trainer>();
            TrainerClients = new ObservableCollection<Client>();
        }

        public void RefreshTrainers()
        {
            Trainers.Clear();
            var list = _trainerRepo.GetAll();
            foreach (var t in list)
            {
                Trainers.Add(t);
            }
        }

        partial void OnSelectedTrainerChanged(Trainer? value)
        {
            if (value != null)
            {
                TrainerId = value.Id.ToString();
                TrainerName = value.FullName;
                TrainerSpec = value.Specialization;
                TrainerPhone = value.Phone;
                LoadTrainerClients(value.Id);
            }
            else
            {
                TrainerClients.Clear();
                TrainerId = "";
                TrainerName = "";
                TrainerSpec = "";
                TrainerPhone = "";
            }
        }

        private void LoadTrainerClients(int trainerId)
        {
            TrainerClients.Clear();
            var list = _trainerRepo.GetClientsForTrainer(trainerId);
            foreach (var c in list)
            {
                TrainerClients.Add(c);
            }
        }

        [RelayCommand]
        public void ClearTrainerForm()
        {
            SelectedTrainer = null;
            TrainerId = "";
            TrainerName = "";
            TrainerSpec = "";
            TrainerPhone = "";
            TrainerClients.Clear();
        }

        [RelayCommand]
        public void SaveTrainer()
        {
            if (string.IsNullOrWhiteSpace(TrainerName))
                return;

            var trainer = new Trainer
            {
                FullName = TrainerName.Trim(),
                Specialization = TrainerSpec?.Trim() ?? "",
                Phone = TrainerPhone?.Trim() ?? ""
            };

            if (string.IsNullOrEmpty(TrainerId))
            {
                _trainerRepo.Add(trainer);
            }
            else
            {
                trainer.Id = int.Parse(TrainerId);
                _trainerRepo.Update(trainer);
            }

            ClearTrainerForm();
            RefreshTrainers();
            _onDataChanged?.Invoke();
        }

        [RelayCommand]
        public void DeleteTrainer()
        {
            if (SelectedTrainer != null)
            {
                _trainerRepo.Delete(SelectedTrainer.Id);
                ClearTrainerForm();
                RefreshTrainers();
                _onDataChanged?.Invoke();
            }
        }
    }
}
