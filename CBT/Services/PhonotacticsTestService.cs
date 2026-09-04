using CBT.Models;

namespace CBT.Services;

public sealed record PhonotacticsRuleMatch(
    PhonemeSequence Rule,
    PhonemeSequenceEnvironment Environment,
    int StartIndex);

public sealed record SyllableRuleMatch(
    PhonemeSequence Rule,
    PhonemeSequenceEnvironment Environment,
    int SyllableIndex);

public sealed record SyllableRuleAssessment(
    PhonemeSequence Rule,
    PhonemeSequenceEnvironment Environment,
    bool IsCertain,
    IReadOnlyList<int> SyllableNumbers);

public sealed record PhonotacticsTestResult(
    IReadOnlyList<PhonotacticsRuleMatch> WordMatches,
    SyllabificationResult Syllabification,
    IReadOnlyList<SyllableRuleAssessment> SyllableAssessments);

public static class PhonotacticsTestService
{
    public static PhonotacticsTestResult Test(
        ConlangProject project,
        IReadOnlyList<string> phonemes)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(phonemes);

        var wordMatches = TestWordEnvironments(
            project.Phonotactics,
            phonemes);

        var syllabification = SyllabificationService.Analyze(
            project,
            phonemes);

        if (!syllabification.Success)
        {
            return new(
                wordMatches,
                syllabification,
                Array.Empty<SyllableRuleAssessment>());
        }

        var matchesByAnalysis = syllabification.Analyses
            .Select(analysis => TestSyllableEnvironments(
                project.Phonotactics,
                analysis))
            .ToList();

        var assessments = BuildAssessments(
            project.Phonotactics,
            matchesByAnalysis);

        return new(
            wordMatches,
            syllabification,
            assessments);
    }

    private static IReadOnlyList<PhonotacticsRuleMatch> TestWordEnvironments(
        PhonotacticsData data,
        IReadOnlyList<string> phonemes)
    {
        List<PhonotacticsRuleMatch> matches = new();

        foreach (var rule in data.ForbiddenSequences)
        {
            if (rule.Phonemes.Count == 0 ||
                rule.Phonemes.Count > phonemes.Count)
                continue;

            switch (rule.Environment)
            {
                case PhonemeSequenceEnvironment.Anywhere:
                    for (var startIndex = 0;
                         startIndex <= phonemes.Count - rule.Phonemes.Count;
                         startIndex++)
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
            }
        }

        return matches;
    }

    private static IReadOnlyList<SyllableRuleMatch> TestSyllableEnvironments(
        PhonotacticsData data,
        SyllabificationAnalysis analysis)
    {
        List<SyllableRuleMatch> matches = new();

        for (var syllableIndex = 0;
             syllableIndex < analysis.Syllables.Count;
             syllableIndex++)
        {
            var syllable = analysis.Syllables[syllableIndex];

            foreach (var rule in data.ForbiddenSequences)
            {
                var target = rule.Environment switch
                {
                    PhonemeSequenceEnvironment.Onset => syllable.Onset,
                    PhonemeSequenceEnvironment.Nucleus => syllable.Nucleus,
                    PhonemeSequenceEnvironment.Coda => syllable.Coda,
                    _ => null
                };

                if (target == null || rule.Phonemes.Count == 0)
                    continue;

                //位置型规则只要出现在对应音节成分内部就算命中。
                if (ContainsSequence(target, rule.Phonemes))
                {
                    matches.Add(new(
                        rule,
                        rule.Environment,
                        syllableIndex));
                }
            }
        }

        return matches;
    }

    private static IReadOnlyList<SyllableRuleAssessment> BuildAssessments(
        PhonotacticsData data,
        IReadOnlyList<IReadOnlyList<SyllableRuleMatch>> matchesByAnalysis)
    {
        if (matchesByAnalysis.Count == 0)
            return Array.Empty<SyllableRuleAssessment>();

        List<SyllableRuleAssessment> assessments = [];

        var syllableRules = data.ForbiddenSequences
            .Where(x =>
                x.Environment is
                    PhonemeSequenceEnvironment.Onset or
                    PhonemeSequenceEnvironment.Nucleus or
                    PhonemeSequenceEnvironment.Coda)
            .ToList();

        foreach (var rule in syllableRules)
        {
            var matchingAnalyses = matchesByAnalysis
                .Where(matches => matches.Any(x => ReferenceEquals(x.Rule, rule)))
                .ToList();

            if (matchingAnalyses.Count == 0)
                continue;

            var syllableNumbers = matchingAnalyses
                .SelectMany(matches => matches
                    .Where(x => ReferenceEquals(x.Rule, rule))
                    .Select(x => x.SyllableIndex + 1))
                .Distinct()
                .Order()
                .ToArray();

            assessments.Add(new(
                rule,
                rule.Environment,
                matchingAnalyses.Count == matchesByAnalysis.Count,
                syllableNumbers));
        }

        return assessments;
    }

    private static bool ContainsSequence(
        IReadOnlyList<string> phonemes,
        IReadOnlyList<string> rule)
    {
        if (rule.Count == 0 || rule.Count > phonemes.Count)
            return false;

        for (var index = 0;
             index <= phonemes.Count - rule.Count;
             index++)
        {
            if (MatchesAt(phonemes, rule, index))
                return true;
        }

        return false;
    }

    private static bool MatchesAt(
        IReadOnlyList<string> phonemes,
        IReadOnlyList<string> rule,
        int startIndex)
    {
        for (var i = 0; i < rule.Count; i++)
        {
            if (!IpaComposer.AreEquivalent(
                    phonemes[startIndex + i],
                    rule[i]))
                return false;
        }

        return true;
    }
}