using System.Globalization;
using CsvHelper;
using EtoroTaxes.Domain;

namespace EtoroTaxes.Integration.Cbr.Csv;

public class CbrSingleCurrencyCsvCurrencyRateProvider : ICurrencyRateProvider
{
    private readonly Currencies _currency;
    private readonly Dictionary<DateOnly, decimal> _rates;

    public CbrSingleCurrencyCsvCurrencyRateProvider(Stream csvStream, Currencies currency)
    {
        _currency = currency;
        _rates = Read(csvStream);
    }

    public decimal? GetRateToRub(Currencies currency, DateOnly date)
    {
        if (currency == _currency && TryGetRate(date, out var rate))
        {
            return rate;
        }

        return null;
    }

    private bool TryGetRate(DateOnly date, out decimal rate)
    {
        if (_rates.TryGetValue(date, out rate))
        {
            return true;
        }

        var lastAvailableDate = _rates.Keys
            .Cast<DateOnly?>()
            .Where(k => k < date)
            .OrderByDescending(k => k)
            .FirstOrDefault();

        if (lastAvailableDate != null)
        {
            rate = _rates[lastAvailableDate.Value];
            return true;
        }

        return false;
    }

    private Dictionary<DateOnly, decimal> Read(Stream csvStream)
    {
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, CultureInfo.GetCultureInfo("ru-ru"));

        var records = csv.GetRecords<CbrSingleCurrencyCsvLine>();

        return records.ToDictionary(r => r.Date, r => r.Rate / r.Quantity);
    }
}
