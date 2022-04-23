namespace EtoroTaxes.Domain;

public class Trade
{
    public string Id { get; set; }
    public TradeTypes Type { get; set; }
    public string Description { get; set; }
    public InstrumentTypes InstrumentType { get; set; }

    public Currencies Currency { get; set; }
    public decimal OpenAmount { get; set; }

    public decimal Profit { get; set; }

    public DateOnly OpenedOn { get; set; }
    public DateOnly ClosedOn { get; set; }

    public decimal GetNetProfitInRub(decimal openRate, decimal closeRate)
    {
        var openAmountInRub = OpenAmount * openRate;
        var closeAmountInRub = (OpenAmount + Profit) * closeRate;

        return closeAmountInRub - openAmountInRub;
    }
}
