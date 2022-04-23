using CsvHelper.Configuration.Attributes;

namespace EtoroTaxes.Integration.Cbr.Csv;

public class CbrSingleCurrencyCsvLine
{
    [Name("nominal")]
    public int Quantity { get; set; }

    [Name("data")]
    [Format("M/d/yy")]
    public DateOnly Date { get; set; }

    [Name("curs")]
    [CultureInfo("ru-ru")]
    public decimal Rate { get; set; }
}
