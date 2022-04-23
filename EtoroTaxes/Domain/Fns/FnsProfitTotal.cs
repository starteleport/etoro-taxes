namespace EtoroTaxes.Domain.Fns;

public class FnsProfitTotal
{
    public FnsProfitTypes ProfitType { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossLoss { get; set; }

    public decimal NetProfit => GrossProfit + GrossLoss;

    public decimal ForeignTaxAmount { get; set; }
    public decimal ForeignTaxRate { get; set; }
}
