namespace EtoroTaxes.Domain;

public record Operation(DateOnly Date, decimal Amount, Currencies Currency);
