using CsvHelper.Configuration.Attributes;

namespace EtoroTaxes.Integration.Etoro.Dividends;

public class EtoroDividendsCsvLine
{
    [Index(0)]
    public DateTime Date { get; set; }

    [Index(1)]
    public string Instrument { get; set; }

    [Index(2)]
    public decimal NetAmount { get; set; }

    [Index(3)]
    public string TaxRateString { get; set; }

    [Index(4)]
    public decimal TaxAmount { get; set; }
}
