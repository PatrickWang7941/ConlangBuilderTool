namespace CBT.Services;

public record SyllableTemplateAnalysis(
    bool IsRecognized,
    string Onset,
    string Nucleus,
    string Coda,
    string Message = "");

public static class SyllableTemplateParser
{
    public static SyllableTemplateAnalysis Analyze(string pattern)
    {
        var normalized = new string(pattern
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray())
            .ToUpperInvariant();

        if (normalized.Length == 0)
            return new(false, "", "", "");

        if (!TryTokenize(normalized, out var tokens))
            return CustomTemplate();

        var firstNucleus = tokens.FindIndex(x => x.Type == 'V');
        if (firstNucleus < 0) return CustomTemplate();

        var lastNucleus = tokens.FindLastIndex(x => x.Type == 'V');

        //单音节C/V模板中，nucleus必须是一段连续的V区域。
        for (var i = firstNucleus; i <= lastNucleus; i++)
        {
            if (tokens[i].Type != 'V')
                return CustomTemplate();
        }

        var onset = string.Concat(tokens.Take(firstNucleus).Select(x => x.Text));
        var nucleus = string.Concat(tokens
            .Skip(firstNucleus)
            .Take(lastNucleus - firstNucleus + 1)
            .Select(x => x.Text));
        var coda = string.Concat(tokens.Skip(lastNucleus + 1).Select(x => x.Text));

        if (tokens.Take(firstNucleus).Any(x => x.Type != 'C') ||
            tokens.Skip(lastNucleus + 1).Any(x => x.Type != 'C'))
            return CustomTemplate();

        return new(
            true,
            onset.Length == 0 ? "—" : onset,
            nucleus,
            coda.Length == 0 ? "—" : coda);
    }

    private static bool TryTokenize(string pattern, out List<TemplateToken> tokens)
    {
        tokens = new List<TemplateToken>();

        for (var i = 0; i < pattern.Length; i++)
        {
            var current = pattern[i];

            if (current is 'C' or 'V')
            {
                tokens.Add(new TemplateToken(current.ToString(), current));
                continue;
            }

            if (current != '(') return false;

            var closing = pattern.IndexOf(')', i + 1);
            if (closing < 0) return false;

            var content = pattern[(i + 1)..closing];
            if (content.Length == 0 || content.Contains('(')) return false;

            var allConsonants = content.All(x => x == 'C');
            var allVowels = content.All(x => x == 'V');

            if (!allConsonants && !allVowels) return false;

            tokens.Add(new TemplateToken(
                $"({content})",
                allConsonants ? 'C' : 'V'));

            i = closing;
        }

        return tokens.Count > 0;
    }

    private static SyllableTemplateAnalysis CustomTemplate()
    {
        return new(
            false,
            "—",
            "—",
            "—",
            "自定义模板  Custom template");
    }

    private record TemplateToken(string Text, char Type);
}