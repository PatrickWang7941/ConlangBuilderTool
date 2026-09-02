//这一页用来实现元音功能
namespace CBT.Data;

public record IpaVowel(
    string Symbol,
    string Height,
    string Backness,
    string Roundedness
);

public static class IpaVowels
{
    public static readonly List<IpaVowel> All =
    [
        // 闭元音 Close
        new("i", "闭  Close", "前  Front", "不圆唇  Unrounded"),
        new("y", "闭  Close", "前  Front", "圆唇  Rounded"),
        new("ɨ", "闭  Close", "央  Central", "不圆唇  Unrounded"),
        new("ʉ", "闭  Close", "央  Central", "圆唇  Rounded"),
        new("ɯ", "闭  Close", "后  Back", "不圆唇  Unrounded"),
        new("u", "闭  Close", "后  Back", "圆唇  Rounded"),

        // 近闭元音 Near-close
        new("ɪ", "近闭  Near-close", "前  Front", "不圆唇  Unrounded"),
        new("ʏ", "近闭  Near-close", "前  Front", "圆唇  Rounded"),
        new("ʊ", "近闭  Near-close", "后  Back", "圆唇  Rounded"),

        // 半闭元音 Close-mid
        new("e", "半闭  Close-mid", "前  Front", "不圆唇  Unrounded"),
        new("ø", "半闭  Close-mid", "前  Front", "圆唇  Rounded"),
        new("ɘ", "半闭  Close-mid", "央  Central", "不圆唇  Unrounded"),
        new("ɵ", "半闭  Close-mid", "央  Central", "圆唇  Rounded"),
        new("ɤ", "半闭  Close-mid", "后  Back", "不圆唇  Unrounded"),
        new("o", "半闭  Close-mid", "后  Back", "圆唇  Rounded"),

        // 中元音 Mid
        new("ə", "中  Mid", "央  Central", "不圆唇  Unrounded"),

        // 半开元音 Open-mid
        new("ɛ", "半开  Open-mid", "前  Front", "不圆唇  Unrounded"),
        new("œ", "半开  Open-mid", "前  Front", "圆唇  Rounded"),
        new("ɜ", "半开  Open-mid", "央  Central", "不圆唇  Unrounded"),
        new("ɞ", "半开  Open-mid", "央  Central", "圆唇  Rounded"),
        new("ʌ", "半开  Open-mid", "后  Back", "不圆唇  Unrounded"),
        new("ɔ", "半开  Open-mid", "后  Back", "圆唇  Rounded"),

        // 近开元音 Near-open
        new("æ", "近开  Near-open", "前  Front", "不圆唇  Unrounded"),
        new("ɐ", "近开  Near-open", "央  Central", "不圆唇  Unrounded"),

        // 开元音 Open
        new("a", "开  Open", "前  Front", "不圆唇  Unrounded"),
        new("ɶ", "开  Open", "前  Front", "圆唇  Rounded"),
        new("ɑ", "开  Open", "后  Back", "不圆唇  Unrounded"),
        new("ɒ", "开  Open", "后  Back", "圆唇  Rounded")
    ];
}