using EtoroTaxes.Domain;

namespace EtoroTaxes.Integration.Etoro;

public static class EtoroInstrumentTypeMapper
{
    public static InstrumentTypes Map(string instrumentType)
    {
        return instrumentType switch
        {
            "CFD" or "Криптоактивы" => InstrumentTypes.NonStockDerivatives,
            _ => InstrumentTypes.StockOrIndexDerivatives
        };
    }
}
