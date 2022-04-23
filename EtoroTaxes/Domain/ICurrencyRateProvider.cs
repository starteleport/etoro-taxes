namespace EtoroTaxes.Domain;

public interface ICurrencyRateProvider
{
    /// <summary>
    /// Возвращает курс <paramref name="currency"/> к рублю на заданную дату.
    /// </summary>
    decimal? GetRateToRub(Currencies currency, DateOnly date);
}
