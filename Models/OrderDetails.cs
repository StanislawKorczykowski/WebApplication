namespace WebApplication2.Models;

public class OrderDetails
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
    public List<Product> Products { get; set; }
}