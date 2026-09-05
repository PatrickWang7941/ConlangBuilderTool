using CBT.Models;

namespace CBT.Services;

public sealed record SyllableAnalysis(
    IReadOnlyList<string> Onset,
    IReadOnlyList<string> Nucleus,
    IReadOnlyList<string> Coda)
{
    public IReadOnlyList<string> Tokens =>
        Onset.Concat(Nucleus).Concat(Coda).ToArray();

    public string Display => string.Concat(Tokens);
}

public sealed record SyllabificationAnalysis(
    IReadOnlyList<SyllableAnalysis> Syllables)
{
    public string Display => string.Join(".", Syllables.Select(x => x.Display));
}

public sealed record SyllabificationResult(
    bool Success,
    IReadOnlyList<SyllabificationAnalysis> Analyses,
    bool WasTruncated = false,
    int FailureTokenIndex = -1,
    string Message = "");

public static class SyllabificationService
{
    private const int MaxAnalyses = 256;
    private const int MaxFailureReasons = 6;

    public static SyllabificationResult Analyze(
        ConlangProject project,
        IReadOnlyList<string> phonemes)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(phonemes);

        if (phonemes.Count == 0)
        {
            return new(
                false,
                Array.Empty<SyllabificationAnalysis>(),
                Message:
                    "没有可进行音节划分的音素。" +
                    " No phonemes are available for syllabification.");
        }

        if (project.Phonotactics.SyllableTemplates.Count == 0)
        {
            return new(
                false,
                Array.Empty<SyllabificationAnalysis>(),
                Message:
                    "尚未定义音节模板，无法自动进行音节划分。" +
                    " No syllable templates have been defined.");
        }

        var templateAnalyses = project.Phonotactics.SyllableTemplates
            .Select(x => SyllableTemplateParser.Analyze(x.Pattern))
            .ToList();

        //无法解释自定义模板时不替用户猜测结构。
        if (templateAnalyses.Any(x => !x.IsRecognized))
        {
            return new(
                false,
                Array.Empty<SyllabificationAnalysis>(),
                Message:
                    "存在自定义音节模板，无法可靠完成自动音节划分。" +
                    " Custom syllable templates are present, so automatic syllabification cannot be performed reliably.");
        }

        var shapes = templateAnalyses
            .SelectMany(CreateShapes)
            .Distinct()
            .Where(x => x.NucleusCount > 0)
            .ToList();

        if (shapes.Count == 0)
        {
            return new(
                false,
                Array.Empty<SyllabificationAnalysis>(),
                Message:
                    "当前音节模板没有产生可用的音节结构。" +
                    " The current templates do not produce a usable syllable structure.");
        }

        var context = new SearchContext(project, phonemes, shapes);
        var rawAnalyses = context.Search(0);

        //不同模板路径可能产生完全相同的音节结构，只向用户保留一个。
        var analyses = DeduplicateAnalyses(rawAnalyses);

        if (analyses.Count == 0)
        {
            var failureIndex = Math.Clamp(
                context.FarthestFailureTokenIndex,
                0,
                phonemes.Count - 1);

            var failedToken = phonemes[failureIndex];
            var reasons = context.GetFailureReasons(failureIndex);

            var message =
                $"无法从第{failureIndex + 1}个音素（{failedToken}）继续找到合法音节划分。" +
                $" No valid syllabification could continue from token {failureIndex + 1} ({failedToken}).";

            if (reasons.Count > 0)
            {
                message +=
                    "\n\n可能原因  Possible reasons:\n" +
                    string.Join("\n", reasons.Select(x => $"• {x}"));
            }

            return new(
                false,
                Array.Empty<SyllabificationAnalysis>(),
                context.WasTruncated,
                failureIndex,
                message);
        }

        //偏好只排序已保留的合法候选，不参与搜索和剪枝；同分保持原顺序。
        analyses = project.Phonotactics.SyllabificationPreference switch
        {
            SyllabificationPreference.PreferLargerOnset => analyses
                .OrderByDescending(x => x.Sum(s => s.Onset.Count))
                .ThenBy(x => x.Sum(s => s.Coda.Count)).ToList(),
            SyllabificationPreference.PreferLargerCoda => analyses
                .OrderByDescending(x => x.Sum(s => s.Coda.Count))
                .ThenBy(x => x.Sum(s => s.Onset.Count)).ToList(),
            _ => analyses
        };

        return new(
            true,
            analyses
                .Select(x => new SyllabificationAnalysis(x))
                .ToArray(),
            context.WasTruncated,
            Message: context.WasTruncated
                ? $"合法分析数量过多，仅保留前{MaxAnalyses}个。" +
                  $" Too many valid analyses were found; only the first {MaxAnalyses} are retained."
                : "");
    }

    private static IEnumerable<SyllableShape> CreateShapes(
        SyllableTemplateAnalysis template)
    {
        var onsetCounts = GetPossibleSlotCounts(template.Onset, 'C');
        var nucleusCounts = GetPossibleSlotCounts(template.Nucleus, 'V');
        var codaCounts = GetPossibleSlotCounts(template.Coda, 'C');

        foreach (var onset in onsetCounts)
            foreach (var nucleus in nucleusCounts)
                foreach (var coda in codaCounts)
                    yield return new SyllableShape(onset, nucleus, coda);
    }

    //可选组整体出现或整体省略，例如(CC)产生0或2个槽位。
    private static IReadOnlyList<int> GetPossibleSlotCounts(
        string structure,
        char slot)
    {
        if (string.IsNullOrWhiteSpace(structure) || structure == "—")
            return [0];

        HashSet<int> counts = [0];

        for (var index = 0; index < structure.Length;)
        {
            if (structure[index] == slot)
            {
                counts = counts
                    .Select(x => x + 1)
                    .ToHashSet();

                index++;
                continue;
            }

            if (structure[index] != '(')
            {
                index++;
                continue;
            }

            var closing = structure.IndexOf(')', index + 1);
            if (closing < 0) break;

            var optionalCount = structure[(index + 1)..closing]
                .Count(x => x == slot);

            var current = counts.ToArray();

            foreach (var count in current)
                counts.Add(count + optionalCount);

            index = closing + 1;
        }

        return counts.Order().ToArray();
    }

    private static List<IReadOnlyList<SyllableAnalysis>> DeduplicateAnalyses(
        IEnumerable<IReadOnlyList<SyllableAnalysis>> analyses)
    {
        List<IReadOnlyList<SyllableAnalysis>> unique = [];
        HashSet<string> seen = new(StringComparer.Ordinal);

        foreach (var analysis in analyses)
        {
            var key = CreateAnalysisKey(analysis);
            if (!seen.Add(key)) continue;

            unique.Add(analysis);
        }

        return unique;
    }

    private static string CreateAnalysisKey(
        IReadOnlyList<SyllableAnalysis> analysis)
    {
        return string.Join(
            "||",
            analysis.Select(syllable =>
                $"O:{CreateTokenKey(syllable.Onset)}|" +
                $"N:{CreateTokenKey(syllable.Nucleus)}|" +
                $"C:{CreateTokenKey(syllable.Coda)}"));
    }

    private static string CreateTokenKey(
        IReadOnlyList<string> tokens)
    {
        //长度前缀避免多字符IPA和拼接边界产生相同key。
        return string.Concat(tokens.Select(token =>
        {
            var normalized = IpaComposer.NormalizeSymbol(token);
            return $"{normalized.Length}:{normalized};";
        }));
    }

    private sealed class SearchContext
    {
        private readonly ConlangProject project;
        private readonly IReadOnlyList<string> phonemes;
        private readonly IReadOnlyList<SyllableShape> shapes;

        private readonly Dictionary<int, List<IReadOnlyList<SyllableAnalysis>>> memo = new();
        private readonly Dictionary<int, HashSet<string>> failureReasons = new();

        public bool WasTruncated { get; private set; }
        public int FarthestFailureTokenIndex { get; private set; }

        public SearchContext(
            ConlangProject project,
            IReadOnlyList<string> phonemes,
            IReadOnlyList<SyllableShape> shapes)
        {
            this.project = project;
            this.phonemes = phonemes;
            this.shapes = shapes;
        }

        public List<IReadOnlyList<SyllableAnalysis>> Search(int startIndex)
        {
            if (startIndex == phonemes.Count)
                return [Array.Empty<SyllableAnalysis>()];

            if (memo.TryGetValue(startIndex, out var cached))
                return cached;

            List<IReadOnlyList<SyllableAnalysis>> results = [];
            HashSet<string> resultKeys = new(StringComparer.Ordinal);

            foreach (var shape in shapes)
            {
                if (!TryCreateSyllable(
                        startIndex,
                        shape,
                        out var syllable,
                        out var failureReason))
                {
                    RecordFailure(startIndex, failureReason);
                    continue;
                }

                var nextIndex =
                    startIndex +
                    shape.OnsetCount +
                    shape.NucleusCount +
                    shape.CodaCount;

                foreach (var remainder in Search(nextIndex))
                {
                    var candidate = new[] { syllable }
                        .Concat(remainder)
                        .ToArray();

                    var key = CreateAnalysisKey(candidate);
                    if (!resultKeys.Add(key)) continue;

                    results.Add(candidate);

                    if (results.Count < MaxAnalyses)
                        continue;

                    WasTruncated = true;
                    memo[startIndex] = results;
                    return results;
                }
            }

            if (results.Count == 0)
                FarthestFailureTokenIndex =
                    Math.Max(FarthestFailureTokenIndex, startIndex);

            memo[startIndex] = results;
            return results;
        }

        public IReadOnlyList<string> GetFailureReasons(int tokenIndex)
        {
            if (!failureReasons.TryGetValue(tokenIndex, out var reasons))
                return Array.Empty<string>();

            return reasons
                .Take(MaxFailureReasons)
                .ToArray();
        }

        private bool TryCreateSyllable(
            int startIndex,
            SyllableShape shape,
            out SyllableAnalysis syllable,
            out string failureReason)
        {
            syllable = new(
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>());

            failureReason = "";

            var totalCount =
                shape.OnsetCount +
                shape.NucleusCount +
                shape.CodaCount;

            if (totalCount == 0)
            {
                failureReason =
                    "模板产生了空音节。The template produced an empty syllable.";
                return false;
            }

            if (startIndex + totalCount > phonemes.Count)
            {
                failureReason =
                    "剩余音素数量不足以匹配当前音节模板。" +
                    " Too few phonemes remain to match the current syllable template.";
                return false;
            }

            var onset = Slice(startIndex, shape.OnsetCount);

            var nucleusStart = startIndex + shape.OnsetCount;
            var nucleus = Slice(nucleusStart, shape.NucleusCount);

            var codaStart = nucleusStart + shape.NucleusCount;
            var coda = Slice(codaStart, shape.CodaCount);

            if (!onset.All(IsConsonant))
            {
                failureReason =
                    $"声首候选 {DisplaySequence(onset)} 包含非辅音音素。" +
                    $" Onset candidate {DisplaySequence(onset)} contains a non-consonant phoneme.";
                return false;
            }

            if (!nucleus.All(IsNucleusPhoneme))
            {
                failureReason =
                    $"音节核候选 {DisplaySequence(nucleus)} 不能作为当前音节核。" +
                    $" Nucleus candidate {DisplaySequence(nucleus)} cannot serve as the current nucleus.";
                return false;
            }

            if (!coda.All(IsConsonant))
            {
                failureReason =
                    $"韵尾候选 {DisplaySequence(coda)} 包含非辅音音素。" +
                    $" Coda candidate {DisplaySequence(coda)} contains a non-consonant phoneme.";
                return false;
            }

            if (!MatchesAllowed(
                    onset,
                    project.Phonotactics.AllowedOnsets))
            {
                failureReason =
                    $"声首 {DisplaySequence(onset)} 不在当前Allowed Onsets中。" +
                    $" Onset {DisplaySequence(onset)} is not listed in Allowed Onsets.";
                return false;
            }

            if (!MatchesAllowed(
                    nucleus,
                    project.Phonotactics.AllowedNuclei))
            {
                failureReason =
                    $"音节核 {DisplaySequence(nucleus)} 不在当前Allowed Nuclei中。" +
                    $" Nucleus {DisplaySequence(nucleus)} is not listed in Allowed Nuclei.";
                return false;
            }

            if (!MatchesAllowed(
                    coda,
                    project.Phonotactics.AllowedCodas))
            {
                failureReason =
                    $"韵尾 {DisplaySequence(coda)} 不在当前Allowed Codas中。" +
                    $" Coda {DisplaySequence(coda)} is not listed in Allowed Codas.";
                return false;
            }

            syllable = new(onset, nucleus, coda);
            return true;
        }

        private void RecordFailure(int tokenIndex, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return;

            if (!failureReasons.TryGetValue(tokenIndex, out var reasons))
            {
                reasons = new HashSet<string>(StringComparer.Ordinal);
                failureReasons[tokenIndex] = reasons;
            }

            if (reasons.Count < MaxFailureReasons)
                reasons.Add(reason);
        }

        private IReadOnlyList<string> Slice(
            int startIndex,
            int count)
        {
            if (count == 0)
                return Array.Empty<string>();

            return phonemes
                .Skip(startIndex)
                .Take(count)
                .ToArray();
        }

        private bool IsConsonant(string phoneme)
        {
            return project.Phonology.Consonants.Any(x =>
                IpaComposer.AreEquivalent(
                    x.Symbol,
                    phoneme));
        }

        private bool IsNucleusPhoneme(string phoneme)
        {
            if (project.Phonology.Vowels.Any(x =>
                    IpaComposer.AreEquivalent(
                        x.Symbol,
                        phoneme)))
                return true;

            //带Syllabic标记的辅音也可以充当nucleus。
            return project.Phonology.Consonants.Any(x =>
                IpaComposer.AreEquivalent(x.Symbol, phoneme) &&
                x.Diacritics?.Contains("\u0329") == true);
        }

        private static bool MatchesAllowed(
            IReadOnlyList<string> candidate,
            IReadOnlyList<PhonemeSequence> allowed)
        {
            //空列表表示未定义具体限制；空onset/coda由模板决定是否合法。
            if (candidate.Count == 0 || allowed.Count == 0)
                return true;

            return allowed.Any(sequence =>
                TokensEquivalent(
                    candidate,
                    sequence.Phonemes));
        }

        private static string DisplaySequence(
            IReadOnlyList<string> phonemes)
        {
            return phonemes.Count == 0
                ? "—"
                : string.Concat(phonemes);
        }
    }

    private static bool TokensEquivalent(
        IReadOnlyList<string> first,
        IReadOnlyList<string> second)
    {
        if (first.Count != second.Count)
            return false;

        for (var index = 0; index < first.Count; index++)
        {
            if (!IpaComposer.AreEquivalent(
                    first[index],
                    second[index]))
                return false;
        }

        return true;
    }

    private readonly record struct SyllableShape(
        int OnsetCount,
        int NucleusCount,
        int CodaCount);
}