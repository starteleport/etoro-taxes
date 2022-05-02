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
                        DeductionAmount = nonStocksDeduction,
                        ProfitBeforeDeduction = nonStockOrIndexDerivatives.GrossProfit
                    });
            }
        }

        if (stockOrIndexDerivatives != null)
        {
            var availableDeductions = new[]
                {
                    (FnsTaxDeductionTypes.NonStockOrDerivativesLossesReducingTaxableBaseForStockOrDerivatives,
                        unallocatedNonStocksDeduction),
                    (FnsTaxDeductionTypes.StockOrDerivativesExpenses, stockOrIndexDerivatives.GrossLoss)
                }.Where(d => d.Item2 > 0)
                .OrderByDescending(d => d.Item2);

            decimal undeductedStocksProfit = stockOrIndexDerivatives.GrossProfit;
            foreach (var deduction in availableDeductions)
            {
                if (undeductedStocksProfit == 0)
                {
                    break;
                }

                var thisDeductionAmount = Math.Min(undeductedStocksProfit, deduction.Item2);

                result.Add(
                    new()
                    {
                        ProfitType = FnsProfitTypes.StockOrIndexDerivatives,
                        Type = deduction.Item1,
                        DeductionAmount = thisDeductionAmount,
                        ProfitBeforeDeduction = undeductedStocksProfit
                    });

                undeductedStocksProfit -= thisDeductionAmount;
            }
        }

        return result;
    }
}
