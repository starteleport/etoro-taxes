namespace EtoroTaxes.Domain.Fns;

public class FnsTaxDeduction
{
    public FnsTaxDeductionTypes Type { get; set; }
    public FnsProfitTypes ProfitType { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal ProfitBeforeDeduction { get; set; }
}
