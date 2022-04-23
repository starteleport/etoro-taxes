using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EtoroTaxes.Domain;
using EtoroTaxes.Integration.Cbr;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace EtoroTaxes.Tests.Integration.Cbr;

public class CbrCurrencyRateProviderTests
{
    private const string CbrUsdResponse = @"<?xml version=""1.0"" encoding=""windows-1251""?>
<ValCurs Date=""10.03.2022"" name=""Foreign Currency Market"">
    <Valute ID=""R01235"">
        <NumCode>840</NumCode>
        <CharCode>USD</CharCode>
        <Nominal>1</Nominal>
        <Name>Доллар США</Name>
        <Value>116,0847</Value>
    </Valute>
</ValCurs>";

    private const string CbrEurResponse = @"<?xml version=""1.0"" encoding=""windows-1251""?>
<ValCurs Date=""10.03.2022"" name=""Foreign Currency Market"">
    <Valute ID=""R01239"">
        <NumCode>978</NumCode>
        <CharCode>EUR</CharCode>
        <Nominal>1</Nominal>
        <Name>Евро</Name>
        <Value>126,4395</Value>
    </Valute>
</ValCurs>";

    public CbrApiCurrencyRateProvider CreateSut(Expression requestMessagePredicate, string cbrResponse)
    {
        var messageHandlerMock = new Mock<HttpMessageHandler>();
        messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                requestMessagePredicate,
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new() {Content = new StringContent(cbrResponse)});

        var httpClient = new HttpClient(messageHandlerMock.Object);
        httpClient.BaseAddress = new("http://www.cbr.ru/");

        return new(httpClient);
    }

    [Test]
    public void GetRateToRub_CurrencyExistsInResponse_ShouldReturnRate()
    {
        var sut = CreateSut(
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.Query.Contains("10/03/2022")),
            CbrUsdResponse);

        var rate = sut.GetRateToRub(Currencies.USD, new(2022, 3, 10));

        Assert.That(rate, Is.EqualTo(116.0847m));
    }

    [Test]
    public void GetRateToRub_CurrencyDoesNotExistInResponse_ShouldReturnNull()
    {
        var sut = CreateSut(
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.Query.Contains("10/03/2022")),
            CbrEurResponse);

        var rate = sut.GetRateToRub(Currencies.USD, new(2022, 3, 10));

        Assert.That(rate, Is.Null);
    }
}
