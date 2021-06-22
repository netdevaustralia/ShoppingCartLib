namespace Application.Core.Business
{
    public interface ITaxCalculator
    {
        decimal CalculateTax(decimal shoppingCartTotal, decimal salesTaxRate);
    }
}