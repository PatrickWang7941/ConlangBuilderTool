using CBT.Models;

namespace CBT.Services;

public sealed record PhonemeTokenizationResult(
    bool Success,
    IReadOnlyList<string> Tokens,
    int FailureIndex = -1,
    string RemainingText = "");

public static class PhonemeTokenizerService
{
    public static PhonemeTokenizationResult Tokenize(ConlangProject project, string input)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(input);

        var normalizedInput = IpaComposer.NormalizeSymbol(input);
        var inventory = project.Phonology.Consonants.Select(x => x.Symbol)
            .Concat(project.Phonology.Vowels.Select(x => x.Symbol))
            .Select(IpaComposer.NormalizeSymbol)
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderByDescending(x => x.Length)
            .ToList();

        List<string> tokens = new();

        for (var index = 0; index < normalizedInput.Length;)
        {
            //优先匹配最长音素，避免把复合音或带附加符号的音素提前拆开。
            var token = inventory.FirstOrDefault(symbol =>
                normalizedInput.AsSpan(index).StartsWith(symbol.AsSpan(), StringComparison.Ordinal));

            if (token is null)
            {
                return new(
                    false,
                    tokens.ToArray(),
                    index,
                    normalizedInput[index..]);
            }

            tokens.Add(token);
            index += token.Length;
        }

        return new(true, tokens.ToArray());
    }
}
