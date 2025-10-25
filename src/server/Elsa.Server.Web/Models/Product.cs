namespace Elsa.Server.Web.Models;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Sku { get; set; } = null!;
}
