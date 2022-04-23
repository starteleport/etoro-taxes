using System.Globalization;
using CsvHelper;
using EtoroTaxes.Domain;

namespace EtoroTaxes.Integration.Etoro;

public class EtoroClosedPositionsCsvTradesProvider : ITradesProvider
{
    private readonly IEnumerable<EtoroClosedPositionsCsvLine> _records;

    public EtoroClosedPositionsCsvTradesProvider(Stream csvStream, Currencies currency, CultureInfo culture)
    {
        Currency = currency;
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, culture);

        _records = csv.GetRecords<EtoroClosedPositionsCsvLine>().ToArray();
    }

    public Currencies Currency { get; }

    public IEnumerable<Trade> GetTrades()
    {
        foreach (var record in _records)
        {
            yield return new()
            {
                Id = record.Id,
                Currency = Currency,
                Type = record.Action.Contains("Sell") ? TradeTypes.Sell : TradeTypes.Buy,
                Description = record.Action,
                OpenedOn = DateOnly.FromDateTime(record.OpenedOn),
                ClosedOn = DateOnly.FromDateTime(record.ClosedAt),
                OpenAmount = record.OpenAmount,
                Profit = EtoroProfitStringParser.GetProfit(record.ProfitString),
                InstrumentType = EtoroInstrumentTypeMapper.Map(record.InstrumentType)
            };
        }
    }
}
