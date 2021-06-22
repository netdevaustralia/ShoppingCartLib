using System.Collections.Generic;

namespace Application.Core.Models
{
    public class ShoppingCart
    {
        public List<ShoppingCartProduct> ShoppingCartProducts { get; set; }
        public decimal SalesTaxAmount { get; set; }
        public string CustomerAccountId { get; set; }
    }
}