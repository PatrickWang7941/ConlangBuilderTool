using CBT.Models;

namespace CBT.Services;

public sealed record OrthographyPreviewResult(
    string Grapheme,
    IReadOnlyList<string> UnmappedPhonemes,
    bool IsComplete);

//把音素token序列按映射转成书写形式。贪心最长匹配，同长度按Priority消歧。
public static class OrthographyService
{
    public static OrthographyPreviewResult Preview(
        IReadOnlyList<string> phonemes,
        OrthographyData data)
    {
        ArgumentNullException.ThrowIfNull(phonemes);
        ArgumentNullException.ThrowIfNull(data);

        var mappings = data.Mappings
            .Where(x => x.Phonemes.Count > 0 && x.Grapheme.Length > 0)
            .OrderByDescending(x => x.Phonemes.Count)
            .ThenByDescending(x => x.Priority)
            .ToList();

        if (phonemes.Count == 0)
            return new("", Array.Empty<string>(), true);

        var builder = new System.Text.StringBuilder();
        var unmapped = new List<string>();

        for (var index = 0; index < phonemes.Count;)
        {
            var match = FindMatch(phonemes, index, mappings);
            if (match == null)
            {
                unmapped.Add(phonemes[index]);
                index++;
                continue;
            }

            builder.Append(match.Grapheme);
            index += match.Phonemes.Count;
        }

        return new(
            builder.ToString(),
            unmapped,
            unmapped.Count == 0);
    }

    private static OrthographyMapping? FindMatch(
        IReadOnlyList<string> phonemes,
        int index,
        IReadOnlyList<OrthographyMapping> mappings)
    {
        foreach (var mapping in mappings)
        {
            if (index + mapping.Phonemes.Count > phonemes.Count)
                continue;

            if (MatchesAt(phonemes, mapping.Phonemes, index))
                return mapping;
        }

        return null;
    }

    private static bool MatchesAt(
        IReadOnlyList<string> phonemes,
        IReadOnlyList<string> sequence,
        int index)
    {
        for (var i = 0; i < sequence.Count; i++)
        {
            if (!IpaComposer.AreEquivalent(
                    phonemes[index + i],
                    sequence[i]))
                return false;
        }

        return true;
    }
}
