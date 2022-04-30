namespace EtoroTaxes.Domain.Fns;

public class FnsDeductionCalculator
{
    public ICollection<FnsTaxDeduction> CalculateDeductions(ICollection<FnsProfitTotal> profitTotals)
    {
        var result = new List<FnsTaxDeduction>();

        var stockOrIndexDerivatives =
            profitTotals.FirstOrDefault(t => t.ProfitType == FnsProfitTypes.StockOrIndexDerivatives);

        var nonStockOrIndexDerivatives =
            profitTotals.FirstOrDefault(t => t.ProfitType == FnsProfitTypes.NonStockOrIndexDerivatives);

        decimal unallocatedNonStocksDeduction = 0;
        if (nonStockOrIndexDerivatives != null && nonStockOrIndexDerivatives.GrossLoss > 0)
        {
            var nonStocksDeduction = Math.Min(
                nonStockOrIndexDerivatives.GrossLoss,
                nonStockOrIndexDerivatives.GrossProfit);

            unallocatedNonStocksDeduction = nonStockOrIndexDerivatives.GrossLoss - nonStocksDeduction;

            if (nonStocksDeduction > 0)
            {
                result.Add(
                    new()
                    {
                        ProfitType = FnsProfitTypes.NonStockOrIndexDerivatives,
                        Type = FnsTaxDeductionTypes.NonStockOrDerivativesExpenses,
                        Amount = nonStocksDeduction
                    });
            }
        }

        if (stockOrIndexDerivatives != null)
        {
            decimal undeductedStocksProfit = stockOrIndexDerivatives.GrossProfit;
            if (stockOrIndexDerivatives.GrossLoss > 0)
            {
                var stocksDeduction = Math.Min(stockOrIndexDerivatives.GrossLoss, stockOrIndexDerivatives.GrossProfit);
                undeductedStocksProfit = Math.Max(0, undeductedStocksProfit - stocksDeduction);

                if (stocksDeduction > 0)
                {
                    result.Add(
                        new()
                        {
                            ProfitType = FnsProfitTypes.StockOrIndexDerivatives,
                            Type = FnsTaxDeductionTypes.StockOrDerivativesExpenses,
                            Amount = stocksDeduction
                        });
                }
            }

            if (undeductedStocksProfit > 0 && unallocatedNonStocksDeduction > 0)
            {
                result.Add(new()
                {
                    ProfitType = FnsProfitTypes.StockOrIndexDerivatives,
                    Type = FnsTaxDeductionTypes.NonStockOrDerivativesLossesReducingTaxableBaseForStockOrDerivatives,
                    Amount = Math.Min(undeductedStocksProfit, unallocatedNonStocksDeduction)
                });
            }
        }

        return result;
    }
}
