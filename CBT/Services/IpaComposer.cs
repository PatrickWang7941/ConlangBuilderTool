using System.Text;

namespace CBT.Services;

// 负责组合和规范化 IPA 字符串。
// UI 不应该自己处理 combining marks。
public static class IpaComposer
{
    // 将基础 IPA 符号与若干附加符号组合成最终显示形式。
    //
    // 例如：
    // p + ʰ      → pʰ
    // a + ◌̃     → ã
    // n + ◌̥     → n̥
    public static string Compose(
        string baseSymbol,
        IEnumerable<string> diacritics)
    {
        if (string.IsNullOrWhiteSpace(baseSymbol))
        {
            return "";
        }

        StringBuilder builder =
            new(baseSymbol.Trim());

        foreach (string diacritic in diacritics)
        {
            if (string.IsNullOrEmpty(diacritic))
            {
                continue;
            }

            builder.Append(diacritic);
        }

        // 使用 Unicode NFC 规范化。
        //
        // 例如：
        // "a" + U+0303
        //
        // 在可能的情况下会规范化为：
        // "ã"
        //
        // 这可以避免视觉上相同的 IPA
        // 因 Unicode 编码方式不同而被当成两个音素。
        return builder
            .ToString()
            .Normalize(
                NormalizationForm.FormC);
    }


    // 统一规范化用户输入或数据库中的 IPA。
    public static string NormalizeSymbol(
        string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return "";
        }

        return symbol
            .Trim()
            .Normalize(
                NormalizationForm.FormC);
    }


    // 判断两个 IPA 字符串在 Unicode 规范化后
    // 是否代表相同的字符串。
    public static bool AreEquivalent(
        string first,
        string second)
    {
        return string.Equals(
            NormalizeSymbol(first),
            NormalizeSymbol(second),
            StringComparison.Ordinal);
    }
}