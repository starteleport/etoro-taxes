namespace EtoroTaxes.Domain.Fns;

public class FnsTaxDeduction
{
    public FnsTaxDeductionTypes Type { get; set; }
    public FnsProfitTypes ProfitType { get; set; }
    public decimal Amount { get; set; }
}
