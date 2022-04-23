using System.IO;
using EtoroTaxes.Domain;
using EtoroTaxes.Integration.Cbr.Csv;
using NUnit.Framework;

namespace EtoroTaxes.Tests.Integration.Cbr.Csv;

public class CbrSingleCurrencyCsvCurrencyRateProviderTests
{
    private const string CsvContent = @"nominal;data;curs;cdx;
1;9/1/21;74,2926;Доллар США;
1;9/3/21;74,3026;Доллар США;";

    public CbrSingleCurrencyCsvCurrencyRateProvider CreateSut(string csvContent, Currencies currency)
    {
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms, leaveOpen: true);
        writer.Write(csvContent);
        writer.Close();
        ms.Position = 0;

        return new(ms, currency);
    }

    [Test]
    public void GetRateToRub_RateExistsForADate_ShouldReturnRate()
    {
        var sut = CreateSut(CsvContent, Currencies.USD);

        var rate = sut.GetRateToRub(Currencies.USD, new(2021, 9, 1));

        Assert.That(rate, Is.EqualTo(74.2926m));
    }

    [Test]
    public void GetRateToRub_NoRateForADate_ShouldReturnLatestAvailableRate()
    {
        var sut = CreateSut(CsvContent, Currencies.USD);

        var rate = sut.GetRateToRub(Currencies.USD, new(2021, 9, 4));

        Assert.That(rate, Is.EqualTo(74.3026m));
    }

    [Test]
    public void GetRateToRub_NoRateForADateAndNoEarlierRates_ShouldReturnNull()
    {
        var sut = CreateSut(CsvContent, Currencies.USD);

        var rate = sut.GetRateToRub(Currencies.USD, new(2021, 8, 28));

        Assert.That(rate, Is.Null);
    }

    [Test]
    public void GetRateToRub_CurrencyDoesNotMatch_ShouldReturnNull()
    {
        var sut = CreateSut(CsvContent, Currencies.USD);

        var rate = sut.GetRateToRub(Currencies.RUB, new(2021, 12, 31));

        Assert.That(rate, Is.Null);
    }
}
