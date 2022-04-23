using System.Globalization;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using EtoroTaxes.Domain;

namespace EtoroTaxes.Integration.Cbr;

public class CbrApiCurrencyRateProvider : ICurrencyRateProvider
{
    private readonly HttpClient _httpClient;

    public CbrApiCurrencyRateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public decimal? GetRateToRub(Currencies currency, DateOnly date)
    {
        var dateString = date.ToString("dd/MM/yyyy");
        var response = _httpClient.GetAsync($"scripts/XML_daily.asp?date_req={dateString}").Result;
        response.EnsureSuccessStatusCode();

        using var responseStream = response.Content.ReadAsStream();
        var xDocument = XDocument.Load(responseStream);
        var rateElement = xDocument.XPathSelectElement($"/ValCurs/Valute[NumCode={(int) currency}]/Value");
        var qtyElement = xDocument.XPathSelectElement($"/ValCurs/Valute[NumCode={(int) currency}]/Nominal");

        if (rateElement == null || qtyElement == null)
        {
            return null;
        }

        var rate = decimal.Parse(rateElement.Value, CultureInfo.GetCultureInfo("ru-ru"));
        var quantity = int.Parse(qtyElement.Value);

        return rate/quantity;
    }
}
