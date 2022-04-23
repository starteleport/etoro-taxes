// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Globalization;
using EtoroTaxes.Domain;
using EtoroTaxes.Domain.Fns;
using EtoroTaxes.Integration.Cbr.Csv;
using EtoroTaxes.Integration.Etoro;
using EtoroTaxes.Integration.Etoro.Dividends;

var closedPositionsFileOption = new Option<FileInfo>(
    "--closed-positions",
    "Closed positions CSV report");

var dividendsFileOption = new Option<FileInfo>(
    "--dividends",
    "Closed positions CSV report");

var currencyRatesFileOption = new Option<FileInfo>(
    "--currency-rates",
    "CBR currency rates CSV report. Download for your dates from https://www.cbr.ru/currency_base/dynamics/.");

var etoroCsvCultureOption = new Option<string>(
    "--culture",
    () => "ru-ru",
    "Culture for eToro CSV reports");

var rootCommand = new RootCommand("Calculate taxes from eToro CSV reports")
{
    closedPositionsFileOption,
    dividendsFileOption,
    etoroCsvCultureOption
};

rootCommand.SetHandler(
    (FileInfo closedPositions, FileInfo dividends, string culture, FileInfo currencyRates) =>
    {
        using var etoroClosedTradesCsvStream = File.OpenRead(closedPositions.FullName);
        var tradesProvider = new EtoroClosedPositionsCsvTradesProvider(
            etoroClosedTradesCsvStream,
            Currencies.USD,
            CultureInfo.GetCultureInfo(culture));

        using var etoroDividendsCsvStream = File.OpenRead(dividends.FullName);
        var dividendProvider = new EtoroDividendsCsvDividendsProvider(
            etoroDividendsCsvStream,
            Currencies.USD,
            CultureInfo.GetCultureInfo(culture));

        using var cbrSingleCurrencyCsvStream = File.OpenRead(currencyRates.FullName);
        var currencyRatesProvider = new CbrSingleCurrencyCsvCurrencyRateProvider(cbrSingleCurrencyCsvStream, Currencies.USD);

        var fnsProfitCalculator = new FnsProfitCalculator(tradesProvider, dividendProvider, currencyRatesProvider);
        var profits = fnsProfitCalculator.Calculate(new(2020, 1, 1), new(2020, 12, 31));

        foreach (var profit in profits)
        {
            if (profit.ProfitType != FnsProfitTypes.Dividends)
            {
                Console.WriteLine(
                    $"[{profit.ProfitType:D}] " +
                    $"Доход: {profit.GrossProfit:F}, " +
                    $"Расход: {profit.GrossLoss:F}, " +
                    $"Прибыль: {profit.NetProfit:F}");
            }
            else
            {
                Console.WriteLine(
                    $"[{profit.ProfitType:D}] " +
                    $"Доход: {profit.GrossProfit:F}, " +
                    $"Налог, уплаченный не в РФ: {profit.ForeignTaxAmount:F} ({profit.ForeignTaxRate * 100}%), ");
            }
        }
    }, closedPositionsFileOption, dividendsFileOption, etoroCsvCultureOption, currencyRatesFileOption);

return rootCommand.Invoke(args);
