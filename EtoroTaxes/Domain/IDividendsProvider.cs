namespace EtoroTaxes.Domain;

public interface IDividendsProvider
{
    IEnumerable<Dividend> GetDividends();
    Currencies Currency { get; }
}
