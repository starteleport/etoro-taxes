using System;
using System.Globalization;
using System.IO;
using System.Linq;
using EtoroTaxes.Domain;
using EtoroTaxes.Integration.Etoro;
using NUnit.Framework;

namespace EtoroTaxes.Tests.Integration.Etoro.Trades;

public class EtoroClosedPositionsCsvTradesProviderTests
{
    private const string CsvContent1 = @"Номер позиции (ID);Действие;Сумма;Единицы;Дата открытия;Дата закрытия;Кредитное плечо;Спред;Прибыль;Курс открытия;Курс закрытия;Курс тейк-профита;Курс стоп-лосса;Комиссии за перенос позиций и дивиденды;Скопировано с;Тип;ISIN;Примечания;
516990378;Buy EUR/NZD;1,61 ;7.225677;27/12/2019 10:25:52;27/12/2019 14:46:19;5;0,00 ;0,01 ;1,66 ;1,67 ;1,83 ;1,50 ;0,00 ;OlivierDanvel;CFD;;;
";

    private const string CsvContent2 = @"Номер позиции (ID);Действие;Сумма;Единицы;Дата открытия;Дата закрытия;Кредитное плечо;Спред;Прибыль;Курс открытия;Курс закрытия;Курс тейк-профита;Курс стоп-лосса;Комиссии за перенос позиций и дивиденды;Скопировано с;Тип;ISIN;Примечания;
477889992;Buy Fresenius SE & Co KGaA;3,44 ;0.068592;30/07/2019 00:48:34;16/10/2019 00:49:05;1;0,00 ;(0,10);45,00 ;43,64 ;44,00 ;0,00 ;0,00 ;FabianMarco;Акции;DE0005785604;;
";

    public EtoroClosedPositionsCsvTradesProvider CreateSut(string csvContent, Currencies currency)
    {
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms, leaveOpen: true);
        writer.Write(csvContent);
        writer.Close();
        ms.Position = 0;

        return new(ms, currency, CultureInfo.GetCultureInfo("ru-ru"));
    }

    [Test]
    public void GetTrades_NonStocks()
    {
        var sut = CreateSut(CsvContent1, Currencies.USD);

        var actual = sut.GetTrades();

        Assert.That(actual.Single().Id, Is.EqualTo("516990378"));
        Assert.That(actual.Single().Description, Is.EqualTo("Buy EUR/NZD"));
        Assert.That(actual.Single().OpenAmount, Is.EqualTo(1.61m));
        Assert.That(actual.Single().OpenedOn, Is.EqualTo(new DateOnly(2019, 12, 27)));
        Assert.That(actual.Single().ClosedOn, Is.EqualTo(new DateOnly(2019, 12, 27)));
        Assert.That(actual.Single().Currency, Is.EqualTo(Currencies.USD));
        Assert.That(actual.Single().Profit, Is.EqualTo(0.01m));
        Assert.That(actual.Single().Type, Is.EqualTo(TradeTypes.Buy));
        Assert.That(actual.Single().InstrumentType, Is.EqualTo(InstrumentTypes.NonStockDerivatives));
    }

    [Test]
    public void GetTrades_Stocks()
    {
        var sut = CreateSut(CsvContent2, Currencies.USD);

        var actual = sut.GetTrades();

        Assert.That(actual.Single().Id, Is.EqualTo("477889992"));
        Assert.That(actual.Single().Description, Is.EqualTo("Buy Fresenius SE & Co KGaA"));
        Assert.That(actual.Single().OpenAmount, Is.EqualTo(3.44m));
        Assert.That(actual.Single().OpenedOn, Is.EqualTo(new DateOnly(2019, 7, 30)));
        Assert.That(actual.Single().ClosedOn, Is.EqualTo(new DateOnly(2019, 10, 16)));
        Assert.That(actual.Single().Currency, Is.EqualTo(Currencies.USD));
        Assert.That(actual.Single().Profit, Is.EqualTo(-0.1m));
        Assert.That(actual.Single().Type, Is.EqualTo(TradeTypes.Buy));
        Assert.That(actual.Single().InstrumentType, Is.EqualTo(InstrumentTypes.StockOrIndexDerivatives));
    }
}
