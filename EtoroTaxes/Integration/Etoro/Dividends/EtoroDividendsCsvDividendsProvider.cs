using System.Globalization;
using CsvHelper;
using EtoroTaxes.Domain;

namespace EtoroTaxes.Integration.Etoro.Dividends;

public class EtoroDividendsCsvDividendsProvider : IDividendsProvider
{
    private readonly IEnumerable<EtoroDividendsCsvLine> _records;

    public EtoroDividendsCsvDividendsProvider(Stream csvStream, Currencies currency, CultureInfo culture)
    {
        Currency = currency;
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, culture);

        _records = csv.GetRecords<EtoroDividendsCsvLine>().ToArray();
    }

    public IEnumerable<Dividend> GetDividends()
    {
        foreach (var record in _records)
        {
            yield return new()
            {
                Date = DateOnly.FromDateTime(record.Date),
                Currency = Currency,
                Instrument = record.Instrument,
                // Иногда налог почему-то берётся значительно больше, чем должен в соответствии с TaxRateString
                GrossAmount = record.NetAmount + record.TaxAmount,
                NetAmount = record.NetAmount,
                TaxAmount = record.TaxAmount,
                TaxRate = GetTaxRate(record.TaxRateString)
            };
        }
    }

    private decimal GetTaxRate(string taxRateString)
    {
        return decimal.Parse(taxRateString.TrimEnd('%'))/100;
    }

    public Currencies Currency { get; }
}
