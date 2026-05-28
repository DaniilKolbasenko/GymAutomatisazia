namespace GymManager.Models;

public class Visit
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = "";
    public string VisitDate { get; set; } = "";
}
