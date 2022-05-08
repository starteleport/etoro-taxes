namespace EtoroTaxes.Domain.Fns;

public class FnsProfitTotal
{
    public FnsProfitTotal(
        FnsProfitTypes profitType,
        decimal grossProfit,
        decimal grossLoss,
        decimal foreignTaxAmount = 0,
        decimal foreignTaxRate = 0)
    {
        if (grossProfit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(grossProfit), "Gross profit should be non-negative");
        }

        if (grossLoss < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(grossLoss), "Gross loss should be non-negative");
        }

        if (foreignTaxAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(foreignTaxAmount), "Foreign tax amount should be non-negative");
        }

        if (foreignTaxRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(foreignTaxAmount), "Foreign tax rate should be non-negative");
        }

        ProfitType = profitType;
        GrossProfit = grossProfit;
        GrossLoss = grossLoss;
        ForeignTaxAmount = foreignTaxAmount;
        ForeignTaxRate = foreignTaxRate;
    }

    public FnsProfitTypes ProfitType { get; }
    public decimal GrossProfit { get; }
    public decimal GrossLoss { get; }

    public decimal NetProfit => GrossProfit - GrossLoss;

    public decimal ForeignTaxAmount { get; }
    public decimal ForeignTaxRate { get; }

    public string AsString()
    {
        var commonPart = $"[{ProfitType:D}] Доход: {GrossProfit:F}, ";

        if (ProfitType != FnsProfitTypes.Dividends)
        {
            return $"{commonPart}Расход: {GrossLoss:F}, Прибыль: {NetProfit:F}";
        }

        return $"{commonPart}Налог, уплаченный не в РФ: {ForeignTaxAmount:F} ({ForeignTaxRate * 100}%)";
    }
}
