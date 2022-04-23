using System;
using System.Globalization;
using System.IO;
using System.Linq;
using EtoroTaxes.Domain;
using EtoroTaxes.Integration.Etoro.Dividends;
using NUnit.Framework;

namespace EtoroTaxes.Tests.Integration.Etoro.Dividends;

public class EtoroDividendsCsvDividendsProviderTests
{
        private const string CsvContent1 = @"Дата платежа;Название инструмента;Полученные чистые дивиденды (долл. США);Ставка налога, взимаемого у источника (%);Сумма налога, взимаемого у источника (долл. США);Номер позиции (ID);Тип;ISIN;
12/01/2019 01:14:07;Gazprom OAO;0,02;15 %;0,0086;368263311;CFD;US3682872078;
24/01/2019 01:03:38;CVS Health Corp;0,01;30 %;0,0043;427253384;Stocks;US1266501006;";

    public EtoroDividendsCsvDividendsProvider CreateSut(string csvContent, Currencies currency)
    {
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms, leaveOpen: true);
        writer.Write(csvContent);
        writer.Close();
        ms.Position = 0;

        return new(ms, currency, CultureInfo.GetCultureInfo("ru-ru"));
    }

    [Test]
    public void GetDividends_ShouldDeserialize()
    {
        var sut = CreateSut(CsvContent1, Currencies.USD);

        var actual = sut.GetDividends();

        Assert.That(actual.First().Date, Is.EqualTo(new DateOnly(2019, 1, 12)));
        Assert.That(actual.First().Instrument, Is.EqualTo("Gazprom OAO"));
        Assert.That(actual.First().NetAmount, Is.EqualTo(0.02m));
        Assert.That(actual.First().Currency, Is.EqualTo(Currencies.USD));
        Assert.That(actual.First().TaxAmount, Is.EqualTo(0.0086m));
        Assert.That(actual.First().TaxRate, Is.EqualTo(0.15m));

        Assert.That(actual.Last().Date, Is.EqualTo(new DateOnly(2019, 1, 24)));
        Assert.That(actual.Last().Instrument, Is.EqualTo("CVS Health Corp"));
        Assert.That(actual.Last().NetAmount, Is.EqualTo(0.01m));
        Assert.That(actual.Last().Currency, Is.EqualTo(Currencies.USD));
        Assert.That(actual.Last().TaxAmount, Is.EqualTo(0.0043m));
        Assert.That(actual.Last().TaxRate, Is.EqualTo(0.3m));
    }
}
