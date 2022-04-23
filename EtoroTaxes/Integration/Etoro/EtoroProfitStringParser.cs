using System.Globalization;

namespace EtoroTaxes.Integration.Etoro;

public static class EtoroProfitStringParser
{
    public static decimal GetProfit(string profitString)
    {
        string profitOrLossString = profitString;
        decimal sign = 1;

        if (profitString.StartsWith("("))
        {
            sign = -1;
            profitOrLossString = profitString.Substring(1, profitString.Length - 2);
        }

        return sign * decimal.Parse(profitOrLossString, CultureInfo.GetCultureInfo("ru-ru"));
    }
}
