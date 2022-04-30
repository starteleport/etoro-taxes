using System;
using System.Linq;
using EtoroTaxes.Domain;
using EtoroTaxes.Domain.Fns;
using Moq;
using NUnit.Framework;

namespace EtoroTaxes.Tests.Domain.Fns;

public class FnsProfitCalculatorTests
{
    private Mock<ICurrencyRateProvider> _currencyRateProvider;

    private FnsProfitCalculator _sut;

    [SetUp]
    public void SetUp()
    {
        _currencyRateProvider = new();

        _sut = new(_currencyRateProvider.Object);
    }

    [Test]
    public void Calculate_Trades_ShouldSplitByProfitType()
    {
        _currencyRateProvider.Setup(c => c.GetRateToRub(Currencies.USD, new(2019, 1, 1))).Returns(69.4706m);
        _currencyRateProvider.Setup(c => c.GetRateToRub(Currencies.USD, new(2019, 1, 25))).Returns(66.0016m);
        _currencyRateProvider.Setup(c => c.GetRateToRub(Currencies.USD, new(2019, 2, 1))).Returns(65.3577m);
        _currencyRateProvider.Setup(c => c.GetRateToRub(Currencies.USD, new(2019, 2, 25))).Returns(65.5149m);
        _currencyRateProvider.Setup(c => c.GetRateToRub(Currencies.USD, new(2019, 3, 1))).Returns(65.8895m);
        _currencyRateProvider.Setup(c => c.GetRateToRub(Currencies.USD, new(2019, 3, 25))).Returns(63.7705m);
        _currencyRateProvider.Setup(c => c.GetRateToRub(Currencies.USD, new(2019, 4, 1))).Returns(64.7347m);
        _currencyRateProvider.Setup(c => c.GetRateToRub(Currencies.USD, new(2019, 4, 25))).Returns(63.9798m);

        var trades =
            new[]
            {
                new Trade
                {
                    InstrumentType = InstrumentTypes.StockOrIndexDerivatives,
                    Currency = Currencies.USD,
                    OpenAmount = 10,
                    OpenedOn = new(2019, 1, 1),
                    ClosedOn = new(2019, 1, 25),
                    Profit = 1
                },
                new Trade
                {
                    InstrumentType = InstrumentTypes.StockOrIndexDerivatives,
                    Currency = Currencies.USD,
                    OpenAmount = 10,
                    OpenedOn = new(2019, 2, 1),
                    ClosedOn = new(2019, 2, 25),
                    Profit = -2
                },
                new Trade
                {
                    InstrumentType = InstrumentTypes.NonStockDerivatives,
                    Currency = Currencies.USD,
                    OpenAmount = 10,
                    OpenedOn = new(2019, 3, 1),
                    ClosedOn = new(2019, 3, 25),
                    Profit = 1
                },
                new Trade
                {
                    InstrumentType = InstrumentTypes.NonStockDerivatives,
                    Currency = Currencies.USD,
                    OpenAmount = 10,
                    OpenedOn = new(2019, 4, 1),
                    ClosedOn = new(2019, 4, 25),
                    Profit = -2
                },
            };

        var actual = _sut.CalculateProfits(trades, Array.Empty<Dividend>());

        var stockDerivativesTotal = actual.FirstOrDefault(t => t.ProfitType == FnsProfitTypes.StockOrIndexDerivatives);
        var nonStockDerivativesTotal = actual.FirstOrDefault(t => t.ProfitType == FnsProfitTypes.NonStockOrIndexDerivatives);

        Assert.That(stockDerivativesTotal, Is.Not.Null);
        Assert.That(stockDerivativesTotal.NetProfit, Is.EqualTo(-98.1462m));

        Assert.That(nonStockDerivativesTotal, Is.Not.Null);
        Assert.That(nonStockDerivativesTotal.NetProfit, Is.EqualTo(-92.9281m));
    }

    [Test]
    public void Calculate_Dividends_ShouldBeCorrect()
    {
        _currencyRateProvider.Setup(c => c.GetRateToRub(Currencies.USD, new(2019, 1, 1))).Returns(69.4706m);

        var dividends =
            new[]
            {
                new Dividend
                {
                    Currency = Currencies.USD,
                    Date = new(2019, 1, 1),
                    GrossAmount = 3,
                    NetAmount = 2,
                    TaxAmount = 1,
                    TaxRate = 0.3m
                }
            };

        var actual = _sut.CalculateProfits(Array.Empty<Trade>(), dividends);

        var dividendsTotal = actual.FirstOrDefault(t => t.ProfitType == FnsProfitTypes.Dividends);

        Assert.That(dividendsTotal, Is.Not.Null);
        Assert.That(dividendsTotal.NetProfit, Is.EqualTo(208.4118m));
        Assert.That(dividendsTotal.ForeignTaxAmount, Is.EqualTo(69.4706m));
        Assert.That(dividendsTotal.ForeignTaxRate, Is.EqualTo(0.3m));
    }
}
