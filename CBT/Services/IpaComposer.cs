using System.Text;

namespace CBT.Services;

//负责组合和规范化IPA字符串。
public static class IpaComposer
{
    //将基础IPA与附加符号组合，并统一规范化为Unicode NFC。
    public static string Compose(string baseSymbol, IEnumerable<string> diacritics)
    {
        if (string.IsNullOrWhiteSpace(baseSymbol)) return "";

        StringBuilder builder = new(baseSymbol.Trim());

        foreach (string diacritic in diacritics)
        {
            if (string.IsNullOrEmpty(diacritic)) continue;
            builder.Append(diacritic);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    //组合元音、Diacritics和长度标记。
    public static string ComposeVowel(string baseSymbol, IEnumerable<string> diacritics, string lengthMark)
    {
        if (string.IsNullOrWhiteSpace(baseSymbol)) return "";

        StringBuilder builder = new(baseSymbol.Trim());

        foreach (string diacritic in diacritics)
        {
            if (string.IsNullOrEmpty(diacritic)) continue;
            builder.Append(diacritic);
        }

        if (!string.IsNullOrEmpty(lengthMark))
            builder.Append(lengthMark);

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    //规范化用户输入或数据库中的IPA。
    public static string NormalizeSymbol(string symbol)
    {
        return string.IsNullOrWhiteSpace(symbol)
            ? ""
            : symbol.Trim().Normalize(NormalizationForm.FormC);
    }

    //判断两个IPA字符串规范化后是否相同。
    public static bool AreEquivalent(string first, string second)
    {
        return string.Equals(
            NormalizeSymbol(first),
            NormalizeSymbol(second),
            StringComparison.Ordinal);
    }
}