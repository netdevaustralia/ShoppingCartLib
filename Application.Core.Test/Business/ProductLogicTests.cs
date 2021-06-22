using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Core.Business;
using Application.Core.Constants;
using Application.Core.Infrastructure;
using Application.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Application.Core.Test.Business
{
    public class ProductLogicTests
    {
        [Fact]
        public async Task AddProductsAsync_EmptyProductList_ReturnsEmptyShoppingCart()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var taxCalculator = new TaxCalculator();
            var productLogic = new ProductLogic(new CacheProvider(memoryCache), taxCalculator);
            var products = new List<Product>();
            const string customerAccountId = "12345";

            // Act
            var shoppingCart = await productLogic.AddProductsAsync(products, customerAccountId);

            // Assert
            shoppingCart.ShoppingCartProducts.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task AddProductsAsync_WithoutProductList_ReturnsEmptyShoppingCart()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var taxCalculator = new TaxCalculator();
            var productLogic = new ProductLogic(new CacheProvider(memoryCache), taxCalculator);

            // Act
            var shoppingCart = await productLogic.AddProductsAsync(null, "12345");

            // Assert
            shoppingCart.ShoppingCartProducts.Should().BeNullOrEmpty();
        }

        [Theory]
        [InlineData(1, 39.99, 39.99)]
        [InlineData(1, 0.565, 0.56)]
        [InlineData(1, 0.5649, 0.56)]
        [InlineData(5, 39.99, 199.95)]
        public async Task AddProductsAsync_WithProduct_ReturnsExpectedShoppingCart(int qty, decimal unitPrice,
            decimal total)
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var taxCalculator = new TaxCalculator();
            var productLogic = new ProductLogic(new CacheProvider(cache), taxCalculator);
            var products = new List<Product>
            {
                new()
                {
                    Code = ProductName.DoveSoap,
                    Qty = qty,
                    UnitPrice = unitPrice
                }
            };

            // Act
            var shoppingCart = await productLogic.AddProductsAsync(products, "12345");

            // Assert
            var expectedResult = new ShoppingCart
            {
                ShoppingCartProducts = new List<ShoppingCartProduct>
                {
                    new()
                    {
                        TotalAmount = total,
                        Qty = qty,
                        ProductName = ProductName.DoveSoap
                    }
                },
                CustomerAccountId = "12345"
            };

            shoppingCart.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task AddProductsAsync_WithAdditionalProduct_ReturnsExpectedShoppingCart()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var taxCalculator = new TaxCalculator();
            var productLogic = new ProductLogic(new CacheProvider(cache), taxCalculator);
            var products = new List<Product>
            {
                new()
                {
                    Code = ProductName.DoveSoap,
                    Qty = 5,
                    UnitPrice = 39.99m
                }
            };

            var additionalProduct = new List<Product>
            {
                new()
                {
                    Code = ProductName.DoveSoap,
                    Qty = 3,
                    UnitPrice = 39.99m
                }
            };

            // Act
            await productLogic.AddProductsAsync(products, "12345");
            var newShoppingCart = await productLogic.AddProductsAsync(additionalProduct, "12345");

            // Assert
            var expectedResult = new ShoppingCart
            {
                ShoppingCartProducts = new List<ShoppingCartProduct>
                {
                    new()
                    {
                        TotalAmount = 319.92m,
                        Qty = 8,
                        ProductName = ProductName.DoveSoap
                    }
                },
                CustomerAccountId = "12345"
            };

            newShoppingCart.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task AddProductsAsync_MultipleProduct_ReturnsCorrectSalesTaxAmount()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var taxCalculator = new TaxCalculator();
            var productLogic = new ProductLogic(new CacheProvider(cache), taxCalculator);
            var products = new List<Product>
            {
                new()
                {
                    Code = ProductName.DoveSoap,
                    Qty = 2,
                    UnitPrice = 39.99m
                },
                new()
                {
                    Code = ProductName.AxeDeos,
                    Qty = 2,
                    UnitPrice = 99.99m
                }
            };

            // Act
            var shoppingCart = await productLogic.AddProductsAsync(products, "12345");

            // Assert
            shoppingCart.SalesTaxAmount.Should().Be(35.00m);
        }
    }
}