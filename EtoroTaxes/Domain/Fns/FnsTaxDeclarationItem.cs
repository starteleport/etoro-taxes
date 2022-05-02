namespace EtoroTaxes.Domain.Fns;

public class FnsTaxDeclarationItem
{
    public FnsProfitTypes ProfitType { get; set; }
    public decimal Profit { get; set; }

    public FnsTaxDeductionTypes? DeductionType { get; set; }
    public decimal? DeductionAmount { get; set; }

    public decimal ForeignTaxAmount { get; set; }
    public decimal ForeignTaxRate { get; set; }

    public string AsString()
    {
        var commonPart = $"[{ProfitType:D}] Доход: {Profit:F}, ";

        if (ProfitType == FnsProfitTypes.Dividends)
        {
            return commonPart + $"Налог, уплаченный не в РФ: {ForeignTaxAmount:F} ({ForeignTaxRate * 100}%)";
        }

        var deductionPart = DeductionType != null
            ? $"Код вычета: {DeductionType:D}, Сумма к вычету: {DeductionAmount:F}"
            : "Без вычета";

        return commonPart + deductionPart;
    }
}
