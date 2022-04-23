namespace EtoroTaxes.Domain;

public interface ITradesProvider
{
    IEnumerable<Trade> GetTrades();
    Currencies Currency { get; }
}
