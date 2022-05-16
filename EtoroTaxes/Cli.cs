using System.CommandLine;
using System.Globalization;

namespace EtoroTaxes;

public static class Cli
{
    public static RootCommand CreateRootCommand()
    {
        var closedPositionsFileOption = new Option<FileInfo>(
            "--closed-positions",
            "Closed positions CSV report");

        closedPositionsFileOption.LegalFilePathsOnly().AddAlias("-c");
        closedPositionsFileOption.AddValidator(
            v =>
            {
                if (!File.Exists(v.Tokens[0].Value))
                {
                    v.ErrorMessage = "Closed positions report file not found";
                }
            });

        var dividendsFileOption = new Option<FileInfo>(
            "--dividends",
            "Dividends CSV report");

        dividendsFileOption.LegalFilePathsOnly().AddAlias("-d");
        dividendsFileOption.AddValidator(
            v =>
            {
                if (!File.Exists(v.Tokens[0].Value))
                {
                    v.ErrorMessage = "Dividend report file not found";
                }
            });

        var currencyRatesFileOption = new Option<FileInfo>(
            "--currency-rates",
            "CBR currency rates CSV report. Download for your dates from " +
            "https://www.cbr.ru/currency_base/dynamics/. Be sure to include one week of the previous year into report " +
            "because currency rates for New Year holidays are set on the last week of the previous year.");

        currencyRatesFileOption.LegalFilePathsOnly().AddAlias("-r");
        currencyRatesFileOption.AddValidator(
            v =>
            {
                if (!File.Exists(v.Tokens[0].Value))
                {
                    v.ErrorMessage = "CBR currency rates report file not found";
                }
            });

        var etoroCsvCultureOption = new Option<string>(
            "--culture",
            () => "ru-ru",
            "Culture for eToro CSV reports");

        etoroCsvCultureOption.AddValidator(
            v =>
            {
                var cultureExists = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Any(c => c.Name.Equals(v.Tokens[0].Value, StringComparison.OrdinalIgnoreCase));
                if (!cultureExists)
                {
                    v.ErrorMessage = "Invalid culture specified";
                }
            });

        var dateFromOption = new Option<DateTime>(
            "--date-from",
            description: "Date to calculate from");

        dateFromOption.AddAlias("-f");

        var dateToOption = new Option<DateTime>(
            "--date-to",
            description: "Date to calculate to");

        dateToOption.AddAlias("-t");

        var rootCommand1 = new RootCommand("Calculate taxes from eToro CSV reports")
        {
            closedPositionsFileOption,
            dividendsFileOption,
            etoroCsvCultureOption,
            currencyRatesFileOption,
            dateFromOption,
            dateToOption
        };

        rootCommand1.SetHandler<FileInfo, FileInfo, string, FileInfo, DateTime, DateTime>(
            CalculationFlow.Calculate,
            closedPositionsFileOption,
            dividendsFileOption,
            etoroCsvCultureOption,
            currencyRatesFileOption,
            dateFromOption,
            dateToOption);
        return rootCommand1;
    }
}
