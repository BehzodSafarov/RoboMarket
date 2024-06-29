namespace RoboMarketPro.Models;

public class User
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public ICollection<Order> Orders { get; set; }
}

