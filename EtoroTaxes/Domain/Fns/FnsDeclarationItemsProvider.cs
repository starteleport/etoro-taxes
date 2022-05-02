namespace EtoroTaxes.Domain.Fns;

public class FnsDeclarationItemsProvider
{
    public ICollection<FnsTaxDeclarationItem> GetDeclarationItems(
        ICollection<FnsProfitTotal> profits,
        ICollection<FnsTaxDeduction> deductions)
    {
        return profits.GroupJoin(
                deductions,
                t => t.ProfitType,
                d => d.ProfitType,
                (profit, deductions) =>
                {
                    if (!deductions.Any())
                    {
                        return new[]
                        {
                            new FnsTaxDeclarationItem
                            {
                                ProfitType = profit.ProfitType,
                                Profit = profit.GrossProfit,
                                ForeignTaxAmount = profit.ForeignTaxAmount,
                                ForeignTaxRate = profit.ForeignTaxRate
                            }
                        };
                    }

                    return deductions.Select(
                        d => new FnsTaxDeclarationItem()
                        {
                            ProfitType = profit.ProfitType,
                            DeductionType = d.Type,
                            Profit = d.ProfitBeforeDeduction,
                            DeductionAmount = d.DeductionAmount,
                            ForeignTaxAmount = profit.ForeignTaxAmount,
                            ForeignTaxRate = profit.ForeignTaxRate
                        }).ToArray();
                })
            .SelectMany(d => d)
            .ToArray();
    }
}
