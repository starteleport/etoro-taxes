namespace EtoroTaxes.Domain.Fns;

public class FnsProfitCalculator
{
    private readonly ICurrencyRateProvider _currencyRateProvider;

    public FnsProfitCalculator(ICurrencyRateProvider currencyRateProvider)
    {
        _currencyRateProvider = currencyRateProvider;
    }

    public ICollection<FnsProfitTotal> CalculateProfits(ICollection<Trade> trades, ICollection<Dividend> dividends)
    {
        return GetTradingTotals(trades).Concat(GetDividendsTotals(dividends)).ToArray();
    }

    private IEnumerable<FnsProfitTotal> GetDividendsTotals(ICollection<Dividend> dividends)
    {
        return dividends
            .Select(
                d =>
                {
                    var rateToRub = _currencyRateProvider.GetRateToRub(d.Currency, d.Date);
                    if (rateToRub == null)
                    {
                        throw new($"No rate for {d.Date}");
                    }

                    return new
                    {
                        Dividend = d,
                        Amount = d.GrossAmount * rateToRub.Value,
                        TaxAmount = d.TaxAmount * rateToRub.Value
                    };
                })
            .GroupBy(tn => tn.Dividend.TaxRate)
            .Select(
                g => new FnsProfitTotal(
                    FnsProfitTypes.Dividends,
                    g.Sum(tn => tn.Amount),
                    0,
                    g.Sum(tn => tn.TaxAmount),
                    g.Key
                ));
    }


    private IEnumerable<FnsProfitTotal> GetTradingTotals(ICollection<Trade> trades)
    {
        // Предполагаем, что данные eToro отсортированы по дате закрытия позиции
        // Тогда принцип FIFO соблюдается автоматически
        return trades
            .Select(
                t =>
                {
                    var openRate = _currencyRateProvider.GetRateToRub(t.Currency, t.OpenedOn);
                    var closeRate = _currencyRateProvider.GetRateToRub(t.Currency, t.ClosedOn);

                    if (openRate == null)
                    {
                        throw new($"No rate for {t.OpenedOn}");
                    }

                    if (closeRate == null)
                    {
                        throw new($"No rate for {t.ClosedOn}");
                    }

                    var netProfit = t.GetOperations().Sum(
                        o =>
                        {
                            var rate = _currencyRateProvider.GetRateToRub(o.Currency, o.Date);
                            return o.Amount * rate;
                        }) ?? 0;

                    return new {Trade = t, NetProfit = netProfit};
                })
            .GroupBy(tn => tn.Trade.InstrumentType)
            .Select(
                g => new FnsProfitTotal(
                    GetFnsProfitType(g.Key),
                    g.Where(tn => tn.NetProfit > 0).Sum(tn => tn.NetProfit),
                    -g.Where(tn => tn.NetProfit < 0).Sum(tn => tn.NetProfit)
                ));
    }

    private static FnsProfitTypes GetFnsProfitType(InstrumentTypes instrumentType)
    {
        return instrumentType switch
        {
            InstrumentTypes.Stocks => FnsProfitTypes.Stocks,
            InstrumentTypes.NonStockDerivatives => FnsProfitTypes.NonStockOrIndexDerivatives,
            InstrumentTypes.StockOrIndexDerivatives => FnsProfitTypes.StockOrIndexDerivatives,
            _ => throw new ArgumentOutOfRangeException(nameof(instrumentType), instrumentType, null)
        };
    }
}
