using CsvHelper.Configuration.Attributes;

namespace EtoroTaxes.Integration.Etoro;

public class EtoroClosedPositionsCsvLine
{
    [Index(0)]
    public string Id { get; set; }

    [Index(1)]
    public string Action { get; set; }

    [Index(2)]
    public decimal OpenAmount { get; set; }

    [Index(4)]
    public DateTime OpenedOn { get; set; }

    [Index(5)]
    public DateTime ClosedAt { get; set; }

    [Index(8)]
    public string ProfitString { get; set; }

    [Index(15)]
    public string InstrumentType { get; set; }
}
