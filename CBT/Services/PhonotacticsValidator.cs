using CBT.Models;

namespace CBT.Services;

public record PhonotacticsValidationResult(
    bool CanValidate,
    IReadOnlyList<string> Warnings,
    string Message = "");

public static class PhonotacticsValidator
{
    public static PhonotacticsValidationResult Validate(PhonotacticsData data)
    {
        List<string> warnings = new();

        //只把适用环境重叠的允许和禁止规则视为直接冲突。
        CheckAllowedForbiddenConflicts(data, warnings);

        if (data.SyllableTemplates.Count == 0)
        {
            return new(
                false,
                warnings,
                "尚未定义音节模板。No syllable templates have been defined.");
        }

        var analyses = data.SyllableTemplates
            .Select(x => SyllableTemplateParser.Analyze(x.Pattern))
            .ToList();

        //自定义模板无法可靠计算槽位数量，但其他明确冲突仍然可以检查。
        if (analyses.Any(x => !x.IsRecognized))
        {
            return new(
                false,
                warnings,
                "存在自定义模板，暂不进行槽位限制检查。" +
                " Custom templates are present, so slot-limit checking is skipped.");
        }

        var maxOnsetSlots = analyses.Max(x =>
            CountSlots(x.Onset, 'C'));

        var maxNucleusSlots = analyses.Max(x =>
            CountSlots(x.Nucleus, 'V'));

        var maxCodaSlots = analyses.Max(x =>
            CountSlots(x.Coda, 'C'));

        CheckSequences(
            data.AllowedOnsets,
            maxOnsetSlots,
            "声首",
            "Onset",
            "C",
            warnings);

        CheckSequences(
            data.AllowedNuclei,
            maxNucleusSlots,
            "音节核",
            "Nucleus",
            "V",
            warnings);

        CheckSequences(
            data.AllowedCodas,
            maxCodaSlots,
            "韵尾",
            "Coda",
            "C",
            warnings);

        return new(true, warnings);
    }

    private static void CheckAllowedForbiddenConflicts(
        PhonotacticsData data,
        List<string> warnings)
    {
        foreach (var forbidden in data.ForbiddenSequences)
        {
            switch (forbidden.Environment)
            {
                case PhonemeSequenceEnvironment.Anywhere:
                    CheckConflict(
                        data.AllowedOnsets,
                        forbidden,
                        "声首",
                        "Onset",
                        warnings);

                    CheckConflict(
                        data.AllowedNuclei,
                        forbidden,
                        "音节核",
                        "Nucleus",
                        warnings);

                    CheckConflict(
                        data.AllowedCodas,
                        forbidden,
                        "韵尾",
                        "Coda",
                        warnings);
                    break;

                case PhonemeSequenceEnvironment.Onset:
                    CheckConflict(
                        data.AllowedOnsets,
                        forbidden,
                        "声首",
                        "Onset",
                        warnings);
                    break;

                case PhonemeSequenceEnvironment.Nucleus:
                    CheckConflict(
                        data.AllowedNuclei,
                        forbidden,
                        "音节核",
                        "Nucleus",
                        warnings);
                    break;

                case PhonemeSequenceEnvironment.Coda:
                    CheckConflict(
                        data.AllowedCodas,
                        forbidden,
                        "韵尾",
                        "Coda",
                        warnings);
                    break;

                //词首和词尾限制可以与一般Onset/Coda许可同时存在。
                case PhonemeSequenceEnvironment.WordInitial:
                case PhonemeSequenceEnvironment.WordFinal:
                    break;
            }
        }
    }

    private static void CheckConflict(
        IEnumerable<PhonemeSequence> allowed,
        PhonemeSequence forbidden,
        string chineseName,
        string englishName,
        List<string> warnings)
    {
        var conflict = allowed.Any(allowedSequence =>
            TokensEquivalent(
                allowedSequence.Phonemes,
                forbidden.Phonemes));

        if (!conflict) return;

        var display = string.Concat(forbidden.Phonemes);
        var environment =
            GetEnvironmentDisplay(forbidden.Environment);

        warnings.Add(
            $"{display} 同时被定义为允许的{chineseName}和禁止规则（{environment}）。" +
            $" {display} is both an allowed {englishName} and a forbidden sequence ({environment}).");
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

    private static string GetEnvironmentDisplay(
        PhonemeSequenceEnvironment environment)
    {
        return environment switch
        {
            PhonemeSequenceEnvironment.Anywhere =>
                "任意位置 / Anywhere",

            PhonemeSequenceEnvironment.WordInitial =>
                "词首 / Word-initial",

            PhonemeSequenceEnvironment.WordFinal =>
                "词尾 / Word-final",

            PhonemeSequenceEnvironment.Onset =>
                "声首 / Onset",

            PhonemeSequenceEnvironment.Nucleus =>
                "音节核 / Nucleus",

            PhonemeSequenceEnvironment.Coda =>
                "韵尾 / Coda",

            _ => environment.ToString()
        };
    }

    private static void CheckSequences(
        IEnumerable<PhonemeSequence> sequences,
        int maxSlots,
        string chineseName,
        string englishName,
        string slotName,
        List<string> warnings)
    {
        foreach (var sequence in sequences)
        {
            //按音素数量比较，而不是按Unicode字符数比较。
            var phonemeCount = sequence.Phonemes.Count;
            if (phonemeCount <= maxSlots) continue;

            var display = string.Concat(sequence.Phonemes);

            warnings.Add(
                $"{chineseName}序列 {display} 包含{phonemeCount}个音素，" +
                $"但当前模板最多只有{maxSlots}个{slotName}位置。" +
                $" {englishName} sequence {display} contains {phonemeCount} phonemes, " +
                $"but the current templates provide at most {maxSlots} {slotName} slot(s).");
        }
    }

    private static int CountSlots(
        string structure,
        char slot)
    {
        return structure.Count(x => x == slot);
    }
}