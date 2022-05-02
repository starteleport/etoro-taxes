// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using EtoroTaxes.Domain;
using EtoroTaxes.Domain.Fns;
using EtoroTaxes.Integration.Cbr.Csv;
using EtoroTaxes.Integration.Etoro;
using EtoroTaxes.Integration.Etoro.Dividends;

var closedPositionsFileOption = new Option<FileInfo>(
    "--closed-positions",
    "Closed positions CSV report");

closedPositionsFileOption.LegalFilePathsOnly().AddAlias("-c");

var dividendsFileOption = new Option<FileInfo>(
    "--dividends",
    "Dividends CSV report");

dividendsFileOption.LegalFilePathsOnly().AddAlias("-d");

var currencyRatesFileOption = new Option<FileInfo>(
    "--currency-rates",
    "CBR currency rates CSV report. Download for your dates from https://www.cbr.ru/currency_base/dynamics/.");

currencyRatesFileOption.LegalFilePathsOnly().AddAlias("-r");

var etoroCsvCultureOption = new Option<string>(
    "--culture",
    () => "ru-ru",
    "Culture for eToro CSV reports");

var dateFromOption = new Option<DateTime>(
    "--date-from",
    description: "Date to calculate from");

var dateToOption = new Option<DateTime>(
    "--date-to",
    description: "Date to calculate to");

var rootCommand = new RootCommand("Calculate taxes from eToro CSV reports")
{
    closedPositionsFileOption,
    dividendsFileOption,
    etoroCsvCultureOption,
    currencyRatesFileOption,
    dateFromOption,
    dateToOption
};

rootCommand.SetHandler(
    (
        FileInfo closedPositions,
        FileInfo dividends,
        string culture,
        FileInfo currencyRates,
        DateTime dateTimeFrom,
        DateTime dateTimeTo) =>
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
                    $"Налог, уплаченный не в РФ: {profit.ForeignTaxAmount:F} ({profit.ForeignTaxRate * 100}%)");
            }
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
    },
    closedPositionsFileOption,
    dividendsFileOption,
    etoroCsvCultureOption,
    currencyRatesFileOption,
    dateFromOption,
    dateToOption);

return rootCommand.Invoke(args);
