namespace Application.Core.Models
{
    public class ShoppingCartProduct
    {
        public string ProductName { get; set; }
        public int Qty { get; set; }
        public decimal TotalAmount { get; set; }
    }
}