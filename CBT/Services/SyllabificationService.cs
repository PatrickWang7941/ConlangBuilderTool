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
                Message: "没有可进行音节划分的音素。No phonemes are available for syllabification.");
        }

        if (project.Phonotactics.SyllableTemplates.Count == 0)
        {
            return new(
                false,
                Array.Empty<SyllabificationAnalysis>(),
                Message: "尚未定义音节模板，无法自动进行音节划分。No syllable templates have been defined.");
        }

        var templateAnalyses = project.Phonotactics.SyllableTemplates
            .Select(x => SyllableTemplateParser.Analyze(x.Pattern))
            .ToList();

        //存在无法解释的模板时不猜测用户定义的结构。
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
        var analyses = context.Search(0);

        if (analyses.Count == 0)
        {
            var failureIndex = Math.Min(context.FarthestTokenIndex, phonemes.Count - 1);
            var failedToken = failureIndex >= 0 && failureIndex < phonemes.Count
                ? phonemes[failureIndex]
                : "—";

            return new(
                false,
                Array.Empty<SyllabificationAnalysis>(),
                context.WasTruncated,
                failureIndex,
                $"无法从第{failureIndex + 1}个音素（{failedToken}）继续找到合法音节划分。" +
                $" No valid syllabification could continue from token {failureIndex + 1} ({failedToken}).");
        }

        return new(
            true,
            analyses.Select(x => new SyllabificationAnalysis(x)).ToArray(),
            context.WasTruncated,
            Message: context.WasTruncated
                ? $"合法分析数量过多，仅保留前{MaxAnalyses}个。Too many valid analyses were found; only the first {MaxAnalyses} are retained."
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
    private static IReadOnlyList<int> GetPossibleSlotCounts(string structure, char slot)
    {
        if (string.IsNullOrWhiteSpace(structure) || structure == "—")
            return [0];

        HashSet<int> counts = [0];

        for (var index = 0; index < structure.Length;)
        {
            if (structure[index] == slot)
            {
                counts = counts.Select(x => x + 1).ToHashSet();
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

            var optionalCount = structure[(index + 1)..closing].Count(x => x == slot);
            var current = counts.ToArray();

            foreach (var count in current)
                counts.Add(count + optionalCount);

            index = closing + 1;
        }

        return counts.Order().ToArray();
    }

    private sealed class SearchContext
    {
        private readonly ConlangProject project;
        private readonly IReadOnlyList<string> phonemes;
        private readonly IReadOnlyList<SyllableShape> shapes;

        private readonly Dictionary<int, List<IReadOnlyList<SyllableAnalysis>>> memo = new();

        public bool WasTruncated { get; private set; }
        public int FarthestTokenIndex { get; private set; }

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

            FarthestTokenIndex = Math.Max(FarthestTokenIndex, startIndex);

            if (memo.TryGetValue(startIndex, out var cached))
                return cached;

            List<IReadOnlyList<SyllableAnalysis>> results = new();

            foreach (var shape in shapes)
            {
                if (!TryCreateSyllable(startIndex, shape, out var syllable))
                    continue;

                var nextIndex = startIndex +
                    shape.OnsetCount +
                    shape.NucleusCount +
                    shape.CodaCount;

                FarthestTokenIndex = Math.Max(FarthestTokenIndex, nextIndex);

                foreach (var remainder in Search(nextIndex))
                {
                    results.Add(new[] { syllable }.Concat(remainder).ToArray());

                    if (results.Count < MaxAnalyses) continue;

                    WasTruncated = true;
                    memo[startIndex] = results;
                    return results;
                }
            }

            memo[startIndex] = results;
            return results;
        }

        private bool TryCreateSyllable(
            int startIndex,
            SyllableShape shape,
            out SyllableAnalysis syllable)
        {
            syllable = new(
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>());

            var totalCount =
                shape.OnsetCount +
                shape.NucleusCount +
                shape.CodaCount;

            if (totalCount == 0 || startIndex + totalCount > phonemes.Count)
                return false;

            var onset = Slice(startIndex, shape.OnsetCount);

            var nucleusStart = startIndex + shape.OnsetCount;
            var nucleus = Slice(nucleusStart, shape.NucleusCount);

            var codaStart = nucleusStart + shape.NucleusCount;
            var coda = Slice(codaStart, shape.CodaCount);

            if (!onset.All(IsConsonant))
                return false;

            if (!nucleus.All(IsNucleusPhoneme))
                return false;

            if (!coda.All(IsConsonant))
                return false;

            if (!MatchesAllowed(onset, project.Phonotactics.AllowedOnsets))
                return false;

            if (!MatchesAllowed(nucleus, project.Phonotactics.AllowedNuclei))
                return false;

            if (!MatchesAllowed(coda, project.Phonotactics.AllowedCodas))
                return false;

            syllable = new(onset, nucleus, coda);
            return true;
        }

        private IReadOnlyList<string> Slice(int startIndex, int count)
        {
            if (count == 0) return Array.Empty<string>();

            return phonemes
                .Skip(startIndex)
                .Take(count)
                .ToArray();
        }

        private bool IsConsonant(string phoneme)
        {
            return project.Phonology.Consonants.Any(x =>
                IpaComposer.AreEquivalent(x.Symbol, phoneme));
        }

        private bool IsNucleusPhoneme(string phoneme)
        {
            if (project.Phonology.Vowels.Any(x =>
                    IpaComposer.AreEquivalent(x.Symbol, phoneme)))
                return true;

            //带Syllabic标记的辅音也可以充当nucleus。
            return project.Phonology.Consonants.Any(x =>
                IpaComposer.AreEquivalent(x.Symbol, phoneme) &&
                x.Diacritics.Contains("\u0329"));
        }

        private static bool MatchesAllowed(
            IReadOnlyList<string> candidate,
            IReadOnlyList<PhonemeSequence> allowed)
        {
            //空列表表示尚未定义限制；空onset/coda由模板本身决定是否合法。
            if (candidate.Count == 0 || allowed.Count == 0)
                return true;

            return allowed.Any(sequence =>
                TokensEquivalent(candidate, sequence.Phonemes));
        }
    }

    private static bool TokensEquivalent(
        IReadOnlyList<string> first,
        IReadOnlyList<string> second)
    {
        if (first.Count != second.Count) return false;

        for (var i = 0; i < first.Count; i++)
        {
            if (!IpaComposer.AreEquivalent(first[i], second[i]))
                return false;
        }

        return true;
    }

    private readonly record struct SyllableShape(
        int OnsetCount,
        int NucleusCount,
        int CodaCount);
}