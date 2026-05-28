namespace GymManager.Models;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string BirthDate { get; set; } = "";
    public string JoinDate { get; set; } = "";
    public int? TrainerId { get; set; }
    public string TrainerName { get; set; } = "Не назначен";

    public string ActiveSubscriptionName { get; set; } = "Нет абонемента";
    public string SubscriptionEndDate { get; set; } = "-";
    public string VisitsLeftText { get; set; } = "-";
    public bool IsSubscriptionActive { get; set; } = false;
}
