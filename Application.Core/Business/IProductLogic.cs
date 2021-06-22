using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Core.Models;

namespace Application.Core.Business
{
    public interface IProductLogic
    {
        Task<ShoppingCart> AddProductsAsync(List<Product> products, string customerAccountId);
    }
}