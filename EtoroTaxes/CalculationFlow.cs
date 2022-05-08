using System.Globalization;
using EtoroTaxes.Domain;
using EtoroTaxes.Domain.Fns;
using EtoroTaxes.Integration.Cbr.Csv;
using EtoroTaxes.Integration.Etoro;
using EtoroTaxes.Integration.Etoro.Dividends;

namespace EtoroTaxes;

public static class CalculationFlow
{
    public static void Calculate(
        FileInfo closedPositions,
        FileInfo dividends,
        string culture,
        FileInfo currencyRates,
        DateTime dateTimeFrom,
        DateTime dateTimeTo)
    {
        using var etoroClosedTradesCsvStream = File.OpenRead(closedPositions.FullName);
        var tradesProvider = new EtoroClosedPositionsCsvTradesProvider(
            etoroClosedTradesCsvStream,
            Currencies.USD,
            CultureInfo.GetCultureInfo(culture));

        using var etoroDividendsCsvStream = File.OpenRead(dividends.FullName);
        var dividendsProvider = new EtoroDividendsCsvDividendsProvider(
            etoroDividendsCsvStream,
            Currencies.USD,
            CultureInfo.GetCultureInfo(culture));

        using var cbrSingleCurrencyCsvStream = File.OpenRead(currencyRates.FullName);
        var currencyRatesProvider =
            new CbrSingleCurrencyCsvCurrencyRateProvider(cbrSingleCurrencyCsvStream, Currencies.USD);

        var fnsProfitCalculator = new FnsProfitCalculator(currencyRatesProvider);

        var dateFrom = new DateOnly(dateTimeFrom.Year, dateTimeFrom.Month, dateTimeFrom.Day);
        var dateTo = new DateOnly(dateTimeTo.Year, dateTimeTo.Month, dateTimeTo.Day);

        var profits = fnsProfitCalculator.CalculateProfits(
            tradesProvider.GetTrades()
                .SkipWhile(t => t.ClosedOn > dateTo)
                .TakeWhile(t => t.ClosedOn >= dateFrom)
                .ToArray(),
            dividendsProvider.GetDividends()
                .SkipWhile(t => t.Date > dateTo)
                .TakeWhile(t => t.Date >= dateFrom)
                .ToArray());

        Console.WriteLine("Информация о доходах:");

        foreach (var profit in profits)
        {
            Console.WriteLine(profit.AsString());
        }

        var deductionCalculator = new FnsDeductionCalculator();

        var deductions = deductionCalculator.CalculateDeductions(profits);

        var fnsDeclarationItemProvider = new FnsDeclarationItemsProvider();
        var fnsDeclarationItems = fnsDeclarationItemProvider.GetDeclarationItems(profits, deductions);

        Console.WriteLine("\nПредлагаемые строки для декларации:");

        foreach (var declarationItem in fnsDeclarationItems)
        {
            Console.WriteLine(declarationItem.AsString());
        }
    }
}
