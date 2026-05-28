namespace GymManager.Models;

public class Subscription
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DurationDays { get; set; }
    public double Price { get; set; }

    public string DisplayText => $"{Name} ({Price} руб., {DurationDays} дн.)";
}
