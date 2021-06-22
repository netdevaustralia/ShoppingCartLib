using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Core.Constants;
using Application.Core.Infrastructure;
using Application.Core.Models;

namespace Application.Core.Business
{
    public class ProductLogic : IProductLogic
    {
        private const decimal _salesTaxRate = 12.5m;
        private const int rounding = 2;
        private readonly ICacheProvider _cacheProvider;
        private readonly ITaxCalculator _taxCalculator;

        public ProductLogic(ICacheProvider cacheProvider, ITaxCalculator taxCalculator)
        {
            _cacheProvider = cacheProvider;
            _taxCalculator = taxCalculator;
        }

        public Task<ShoppingCart> AddProductsAsync(List<Product> products, string customerAccountId)
        {
            var shoppingCart = new ShoppingCart
            {
                CustomerAccountId = customerAccountId
            };

            if (products == null) return Task.FromResult(shoppingCart);

            var shoppingCartProducts = GetProductsWithTotalAmount(products);

            var cacheKey = $"{CacheKey.ShoppingCartKey}-{customerAccountId}";
            var existingShoppingCart = _cacheProvider.GetFromCache<ShoppingCart>(cacheKey);

            _cacheProvider.RemoveCache(cacheKey);

            if (existingShoppingCart?.ShoppingCartProducts != null
                && existingShoppingCart.ShoppingCartProducts.Any()
                && shoppingCartProducts.Any())
            {
                foreach (var product in shoppingCartProducts) UpdateProductInformation(existingShoppingCart, product);

                shoppingCart.ShoppingCartProducts = existingShoppingCart.ShoppingCartProducts;
            }
            else if (shoppingCartProducts.Any())
            {
                shoppingCart.ShoppingCartProducts = shoppingCartProducts;
            }

            ApplySalesTax(shoppingCart);

            if (shoppingCart.ShoppingCartProducts != null && shoppingCart.ShoppingCartProducts.Any())
                _cacheProvider.SetCache(cacheKey, shoppingCart, 600);

            return Task.FromResult(shoppingCart);
        }

        private void ApplySalesTax(ShoppingCart shoppingCart)
        {
            if (shoppingCart.ShoppingCartProducts == null || !shoppingCart.ShoppingCartProducts.Any()) return;
            ;

            var multipleProducts = shoppingCart.ShoppingCartProducts.GroupBy(x => x.ProductName);
            if (multipleProducts.Count() <= 1) return;
            {
                var totalAmount = shoppingCart.ShoppingCartProducts.Sum(x => x.TotalAmount);
                shoppingCart.SalesTaxAmount =
                    Math.Round(_taxCalculator.CalculateTax(totalAmount, _salesTaxRate), rounding);
            }
        }

        private static List<ShoppingCartProduct> GetProductsWithTotalAmount(IEnumerable<Product> products)
            => products.GroupBy(p => p.Code)
                .Select(x => new ShoppingCartProduct
                {
                    ProductName = x.First().Code,
                    Qty = x.First().Qty,
                    TotalAmount = Math.Round(x.First().UnitPrice * x.First().Qty, rounding)
                }).ToList();


        private static void UpdateProductInformation(ShoppingCart existingShoppingCart, ShoppingCartProduct product)
        {
            var existingProduct =
                existingShoppingCart.ShoppingCartProducts
                    .FirstOrDefault(x => x.ProductName == product.ProductName);
            if (existingProduct == null) return;

            existingProduct.Qty += product.Qty;
            existingProduct.TotalAmount += product.TotalAmount;
        }
    }
}