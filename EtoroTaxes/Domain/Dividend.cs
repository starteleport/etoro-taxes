namespace EtoroTaxes.Domain;

public class Dividend
{
    public DateOnly Date { get; set; }
    public string Instrument { get; set; }

    public Currencies Currency { get; set; }

    public decimal NetAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TaxRate { get; set; }
}
