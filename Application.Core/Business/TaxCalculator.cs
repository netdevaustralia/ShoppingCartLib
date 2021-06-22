namespace Application.Core.Business
{
    public class TaxCalculator : ITaxCalculator
    {
        public decimal CalculateTax(decimal shoppingCartTotal, decimal salesTaxRate) =>
            shoppingCartTotal * salesTaxRate / 100;
    }
}