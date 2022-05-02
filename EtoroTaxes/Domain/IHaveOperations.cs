namespace EtoroTaxes.Domain;

public interface IHaveOperations
{
    IEnumerable<Operation> GetOperations();
}