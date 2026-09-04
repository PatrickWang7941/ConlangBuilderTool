using CBT.Models;

namespace CBT.Services;

public sealed record PhonotacticsRuleMatch(
    PhonemeSequence Rule,
    PhonemeSequenceEnvironment Environment,
    int StartIndex);

public static class PhonotacticsTestService
{
    public static IReadOnlyList<PhonotacticsRuleMatch> Test(
        PhonotacticsData data,
        IReadOnlyList<string> phonemes)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(phonemes);

        List<PhonotacticsRuleMatch> matches = new();

        foreach (var rule in data.ForbiddenSequences)
        {
            if (rule.Phonemes.Count == 0 || rule.Phonemes.Count > phonemes.Count) continue;

            switch (rule.Environment)
            {
                case PhonemeSequenceEnvironment.Anywhere:
                    for (var startIndex = 0; startIndex <= phonemes.Count - rule.Phonemes.Count; startIndex++)
                    {
                        if (MatchesAt(phonemes, rule.Phonemes, startIndex))
                            matches.Add(new(rule, rule.Environment, startIndex));
                    }
                    break;
                case PhonemeSequenceEnvironment.WordInitial:
                    if (MatchesAt(phonemes, rule.Phonemes, 0))
                        matches.Add(new(rule, rule.Environment, 0));
                    break;
                case PhonemeSequenceEnvironment.WordFinal:
                    var finalStart = phonemes.Count - rule.Phonemes.Count;
                    if (MatchesAt(phonemes, rule.Phonemes, finalStart))
                        matches.Add(new(rule, rule.Environment, finalStart));
                    break;
                //Onset、Nucleus和Coda需要可靠的音节划分，当前版本暂不检查。
                case PhonemeSequenceEnvironment.Onset:
                case PhonemeSequenceEnvironment.Nucleus:
                case PhonemeSequenceEnvironment.Coda:
                    break;
            }
        }

        return matches.ToArray();
    }

    private static bool MatchesAt(
        IReadOnlyList<string> phonemes,
        IReadOnlyList<string> rule,
        int startIndex)
    {
        for (var i = 0; i < rule.Count; i++)
        {
            if (!IpaComposer.AreEquivalent(phonemes[startIndex + i], rule[i])) return false;
        }

        return true;
    }
}
