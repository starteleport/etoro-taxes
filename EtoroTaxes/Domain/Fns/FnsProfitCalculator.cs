namespace EtoroTaxes.Domain.Fns;

public class FnsProfitCalculator
{
    private readonly ITradesProvider _tradesProvider;
    private readonly IDividendsProvider _dividendsProvider;
    private readonly ICurrencyRateProvider _currencyRateProvider;

    public FnsProfitCalculator(
        ITradesProvider tradesProvider,
        IDividendsProvider dividendsProvider,
        ICurrencyRateProvider currencyRateProvider)
    {
        _tradesProvider = tradesProvider;
        _dividendsProvider = dividendsProvider;
        _currencyRateProvider = currencyRateProvider;
    }

    public FnsProfitTotal[] Calculate(DateOnly dateFrom, DateOnly dateTo)
    {
        return GetTradingTotals(dateFrom, dateTo).Concat(GetDividendsTotals(dateFrom, dateTo)).ToArray();
    }

    private IEnumerable<FnsProfitTotal> GetDividendsTotals(DateOnly dateFrom, DateOnly dateTo)
    {
        // Предполагаем, что данные eToro отсортированы по дате закрытия позиции
        var dividends = _dividendsProvider.GetDividends()
            .SkipWhile(t => t.Date > dateTo)
            .TakeWhile(t => t.Date >= dateFrom)
            .ToArray();

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
                g => new FnsProfitTotal
                {
                    ProfitType = FnsProfitTypes.Dividends,
                    GrossProfit = g.Sum(tn => tn.Amount),
                    GrossLoss = 0,
                    ForeignTaxAmount = g.Sum(tn => tn.TaxAmount),
                    ForeignTaxRate = g.Key
                });
    }


    private IEnumerable<FnsProfitTotal> GetTradingTotals(DateOnly dateFrom, DateOnly dateTo)
    {
        // Предполагаем, что данные eToro отсортированы по дате закрытия позиции
        var trades = _tradesProvider.GetTrades()
            .SkipWhile(t => t.ClosedOn > dateTo)
            .TakeWhile(t => t.ClosedOn >= dateFrom)
            .ToArray();

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

                    return new {Trade = t, NetProfit = t.GetNetProfitInRub(openRate.Value, closeRate.Value)};
                })
            .GroupBy(tn => tn.Trade.InstrumentType)
            .Select(
                g => new FnsProfitTotal
                {
                    ProfitType = GetFnsProfitType(g.Key),
                    GrossProfit = g.Where(tn => tn.NetProfit > 0).Sum(tn => tn.NetProfit),
                    GrossLoss = g.Where(tn => tn.NetProfit < 0).Sum(tn => tn.NetProfit)
                });
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
