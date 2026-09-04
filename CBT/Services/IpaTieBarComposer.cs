using CBT.Data;

namespace CBT.Services;

public record IpaTiedConsonant(IpaConsonant First, IpaConsonant Second)
{
    public string Symbol => IpaTieBarComposer.Compose(First.Symbol, Second.Symbol);
    public IReadOnlyList<string> Components => [First.Symbol, Second.Symbol];

    public string Place => First.Place == Second.Place ? First.Place : "";

    public string Manner
    {
        get
        {
            if (First.Manner == Second.Manner) return First.Manner;

            if (First.Manner == "塞音  Plosive" &&
                (Second.Manner == "擦音  Fricative" ||
                 Second.Manner == "边擦音  Lateral fricative"))
                return "塞擦音  Affricate";

            return "";
        }
    }

    public string Voicing => First.Voicing == Second.Voicing ? First.Voicing : "";

    public string Description =>
        $"复合调音 {First.Symbol} + {Second.Symbol}  Tied articulation {First.Symbol} + {Second.Symbol}";
}

public static class IpaTieBarComposer
{
    private const char TieBarAbove = '\u0361';
    private const char TieBarBelow = '\u035C';

    //已有tie bar的音素不再作为新的组合部件。
    private static readonly List<IpaConsonant> BaseConsonants = IpaConsonants.All
        .Where(x => !x.Symbol.Contains(TieBarAbove) && !x.Symbol.Contains(TieBarBelow))
        .ToList();

    //先保守收录有依据的双重调音，之后可以继续扩展。
    private static readonly HashSet<(string First, string Second)> DoubleArticulations =
    [
        //Labial-velar
        ("k", "p"),
        ("ɡ", "b"),
        ("ŋ", "m"),

        //Labial-alveolar
        ("t", "p"),
        ("d", "b"),
        ("n", "m"),

        //Labial-retroflex
        ("ʈ", "p"),
        ("ɖ", "b"),
        ("ɳ", "m"),

        //Uvular-epiglottal
        ("q", "ʡ")
    ];

    public static string Compose(string first, string second)
    {
        return IpaComposer.NormalizeSymbol($"{first}{TieBarAbove}{second}");
    }

    //用户输入只使用连字符，例如d-z、n-m、k-p。
    public static bool TryParse(string input, out IpaTiedConsonant? consonant)
    {
        consonant = null;

        var normalized = IpaComposer.NormalizeSymbol(input)
            .Replace('g', 'ɡ');

        if (normalized.Count(x => x == '-') != 1)
            return false;

        var separatorIndex = normalized.IndexOf('-');

        if (separatorIndex <= 0 || separatorIndex >= normalized.Length - 1)
            return false;

        var firstSymbol = normalized[..separatorIndex];
        var secondSymbol = normalized[(separatorIndex + 1)..];

        return TryCreate(firstSymbol, secondSymbol, out consonant);
    }

    //程序内部重新读取已经生成的tie bar音素。
    public static bool TryParseComposed(string symbol, out IpaTiedConsonant? consonant)
    {
        consonant = null;

        var normalized = IpaComposer.NormalizeSymbol(symbol)
            .Replace('g', 'ɡ')
            .Replace(TieBarBelow, TieBarAbove);

        if (normalized.Count(x => x == TieBarAbove) != 1)
            return false;

        var separatorIndex = normalized.IndexOf(TieBarAbove);

        if (separatorIndex <= 0 || separatorIndex >= normalized.Length - 1)
            return false;

        var firstSymbol = normalized[..separatorIndex];
        var secondSymbol = normalized[(separatorIndex + 1)..];

        return TryCreate(firstSymbol, secondSymbol, out consonant);
    }

    private static bool TryCreate(
        string firstSymbol,
        string secondSymbol,
        out IpaTiedConsonant? consonant)
    {
        consonant = null;

        var first = FindBaseConsonant(firstSymbol);
        var second = FindBaseConsonant(secondSymbol);

        if (first == null || second == null)
            return false;

        if (!IsValidCombination(first, second))
            return false;

        consonant = new IpaTiedConsonant(first, second);
        return true;
    }

    private static bool IsValidCombination(IpaConsonant first, IpaConsonant second)
    {
        //基础库中已经存在的标准tie bar音素直接接受。
        var composed = Compose(first.Symbol, second.Symbol);

        if (IpaConsonants.All.Any(x =>
                IpaComposer.AreEquivalent(x.Symbol, composed)))
            return true;

        if (IsValidAffricate(first, second))
            return true;

        return DoubleArticulations.Contains((first.Symbol, second.Symbol));
    }

    private static bool IsValidAffricate(IpaConsonant first, IpaConsonant second)
    {
        if (first.Manner != "塞音  Plosive") return false;

        if (second.Manner != "擦音  Fricative" &&
            second.Manner != "边擦音  Lateral fricative")
            return false;

        if (first.Voicing != second.Voicing)
            return false;

        if (first.Place == second.Place)
            return true;

        //部分塞擦音的闭塞和摩擦阶段位于相邻调音区域。
        return (first.Place, second.Place) switch
        {
            ("双唇  Bilabial", "唇齿  Labiodental") => true,
            ("齿龈  Alveolar", "龈后  Postalveolar") => true,
            ("齿龈  Alveolar", "龈腭  Alveolo-palatal") => true,
            _ => false
        };
    }

    private static IpaConsonant? FindBaseConsonant(string symbol)
    {
        symbol = symbol.Replace('g', 'ɡ');

        return BaseConsonants.FirstOrDefault(x =>
            IpaComposer.AreEquivalent(x.Symbol, symbol));
    }
}