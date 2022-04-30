using System.Linq;
using EtoroTaxes.Domain.Fns;
using NUnit.Framework;

namespace EtoroTaxes.Tests.Domain.Fns;

public class FnsDeductionCalculatorTests
{
    [Test]
    public void StockOrIndexDerivativesWithoutLoss_ShouldNotReturnDeduction()
    {
        var profitTotals = new[]
            {new FnsProfitTotal(FnsProfitTypes.NonStockOrIndexDerivatives, 1, 0)};

        var sut = new FnsDeductionCalculator();

        var actual = sut.CalculateDeductions(profitTotals);

        Assert.That(actual, Is.Empty);
    }

    [Test]
    public void StockOrIndexDerivativesWithLossAndWithoutProfit_ShouldNotReturnDeduction()
    {
        var profitTotals = new[]
            {new FnsProfitTotal(FnsProfitTypes.StockOrIndexDerivatives, 0, 1)};

        var sut = new FnsDeductionCalculator();

        var actual = sut.CalculateDeductions(profitTotals);

        Assert.That(actual, Is.Empty);
    }

    [TestCase(2, 1)]
    [TestCase(2, 2)]
    public void StockOrIndexDerivativesWithLossNotMoreThatProfit_ShouldReturnDeduction(decimal profit, decimal loss)
    {
        var profitTotals = new[]
            {new FnsProfitTotal(FnsProfitTypes.StockOrIndexDerivatives, profit, loss)};

        var sut = new FnsDeductionCalculator();

        var actual = sut.CalculateDeductions(profitTotals);

        Assert.That(actual, Has.Count.EqualTo(1));
        Assert.That(actual.Single().ProfitType, Is.EqualTo(FnsProfitTypes.StockOrIndexDerivatives));
        Assert.That(actual.Single().Type, Is.EqualTo(FnsTaxDeductionTypes.StockOrDerivativesExpenses));
        Assert.That(actual.Single().Amount, Is.EqualTo(loss));
    }

    [Test]
    public void StockOrIndexDerivativesWithLossMoreThatProfit_ShouldReturnDeductionForTheWholeProfit()
    {
        var profitTotals = new[]
            {new FnsProfitTotal(FnsProfitTypes.StockOrIndexDerivatives, 2, 3)};

        var sut = new FnsDeductionCalculator();

        var actual = sut.CalculateDeductions(profitTotals);

        Assert.That(actual, Has.Count.EqualTo(1));
        Assert.That(actual.Single().ProfitType, Is.EqualTo(FnsProfitTypes.StockOrIndexDerivatives));
        Assert.That(actual.Single().Type, Is.EqualTo(FnsTaxDeductionTypes.StockOrDerivativesExpenses));
        Assert.That(actual.Single().Amount, Is.EqualTo(2));
    }

    [Test]
    public void NonStockOrIndexDerivativesWithoutLoss_ShouldNotReturnDeduction()
    {
        var profitTotals = new[]
            {new FnsProfitTotal(FnsProfitTypes.NonStockOrIndexDerivatives, 1, 0)};

        var sut = new FnsDeductionCalculator();

        var actual = sut.CalculateDeductions(profitTotals);

        Assert.That(actual, Is.Empty);
    }

    [Test]
    public void NonStockOrIndexDerivativesWithLossAndWithoutProfit_ShouldNotReturnDeduction()
    {
        var profitTotals = new[]
            {new FnsProfitTotal(FnsProfitTypes.NonStockOrIndexDerivatives, 0, 1)};

        var sut = new FnsDeductionCalculator();

        var actual = sut.CalculateDeductions(profitTotals);

        Assert.That(actual, Is.Empty);
    }

    [TestCase(2, 1)]
    [TestCase(2, 2)]
    public void NonStockOrIndexDerivativesWithLossNotMoreThanProfit_ShouldReturnDeduction(decimal profit, decimal loss)
    {
        var profitTotals = new[]
            {new FnsProfitTotal(FnsProfitTypes.NonStockOrIndexDerivatives, profit, loss)};

        var sut = new FnsDeductionCalculator();

        var actual = sut.CalculateDeductions(profitTotals);

        Assert.That(actual, Has.Count.EqualTo(1));
        Assert.That(actual.Single().ProfitType, Is.EqualTo(FnsProfitTypes.NonStockOrIndexDerivatives));
        Assert.That(actual.Single().Type, Is.EqualTo(FnsTaxDeductionTypes.NonStockOrDerivativesExpenses));
        Assert.That(actual.Single().Amount, Is.EqualTo(loss));
    }

    [Test]
    public void NonStockOrIndexDerivativesWithLossMoreThanProfitWithoutNonDeductedStockDerivativesProfits_ShouldReturnOnlyNonStockDeductions()
    {
        var profitTotals = new[]
            {new FnsProfitTotal(FnsProfitTypes.NonStockOrIndexDerivatives, 2, 3)};

        var sut = new FnsDeductionCalculator();

        var actual = sut.CalculateDeductions(profitTotals);

        Assert.That(actual, Has.Count.EqualTo(1));
        Assert.That(actual.Single().ProfitType, Is.EqualTo(FnsProfitTypes.NonStockOrIndexDerivatives));
        Assert.That(actual.Single().Type, Is.EqualTo(FnsTaxDeductionTypes.NonStockOrDerivativesExpenses));
        Assert.That(actual.Single().Amount, Is.EqualTo(2));
    }

    [Test]
    public void NonStockOrIndexDerivativesWithLossMoreThanProfitWithNonDeductedStockDerivativesProfits_ShouldReturnOnlyNonStockDeductions()
    {
        var profitTotals = new[]
        {
            new FnsProfitTotal(FnsProfitTypes.NonStockOrIndexDerivatives, 2, 5),
            new FnsProfitTotal(FnsProfitTypes.StockOrIndexDerivatives, 3, 2),
        };

        var sut = new FnsDeductionCalculator();

        var actual = sut.CalculateDeductions(profitTotals);

        Assert.That(actual, Has.Count.EqualTo(3));

        var nonStockExpensesDeduction = actual.Single(d => d.Type == FnsTaxDeductionTypes.NonStockOrDerivativesExpenses);
        Assert.That(nonStockExpensesDeduction.Type, Is.EqualTo(FnsTaxDeductionTypes.NonStockOrDerivativesExpenses));
        Assert.That(nonStockExpensesDeduction.Amount, Is.EqualTo(2));

        var stockExpensesDeduction = actual.Single(d => d.Type == FnsTaxDeductionTypes.StockOrDerivativesExpenses);
        Assert.That(stockExpensesDeduction.Type, Is.EqualTo(FnsTaxDeductionTypes.StockOrDerivativesExpenses));
        Assert.That(stockExpensesDeduction.Amount, Is.EqualTo(2));

        var nonStocksLossesReducingTaxableBaseForStocksDeduction = actual.Single(
            d => d.Type == FnsTaxDeductionTypes.NonStockOrDerivativesLossesReducingTaxableBaseForStockOrDerivatives);

        Assert.That(
            nonStocksLossesReducingTaxableBaseForStocksDeduction.ProfitType,
            Is.EqualTo(FnsProfitTypes.StockOrIndexDerivatives));

        Assert.That(nonStocksLossesReducingTaxableBaseForStocksDeduction.Amount, Is.EqualTo(1));
    }
}
