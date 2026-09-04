using CBT.Models;

namespace CBT.Services;

public record PhonotacticsReferenceSummary(
    int AllowedOnsets,
    int AllowedNuclei,
    int AllowedCodas,
    int ForbiddenSequences)
{
    public int Total => AllowedOnsets + AllowedNuclei + AllowedCodas + ForbiddenSequences;
    public bool HasReferences => Total > 0;
}

public static class PhonotacticsReferenceService
{
    public static PhonotacticsReferenceSummary FindReferences(
        PhonotacticsData data,
        string phoneme)
    {
        return new PhonotacticsReferenceSummary(
            CountReferences(data.AllowedOnsets, phoneme),
            CountReferences(data.AllowedNuclei, phoneme),
            CountReferences(data.AllowedCodas, phoneme),
            CountReferences(data.ForbiddenSequences, phoneme));
    }

    //删除的是包含该音素的整条规则，不修改规则中的其他音素。
    public static void RemoveReferences(
        PhonotacticsData data,
        string phoneme)
    {
        RemoveReferences(data.AllowedOnsets, phoneme);
        RemoveReferences(data.AllowedNuclei, phoneme);
        RemoveReferences(data.AllowedCodas, phoneme);
        RemoveReferences(data.ForbiddenSequences, phoneme);
    }

    private static int CountReferences(
        IEnumerable<PhonemeSequence> sequences,
        string phoneme)
    {
        return sequences.Count(sequence =>
            sequence.Phonemes.Any(x => IpaComposer.AreEquivalent(x, phoneme)));
    }

    private static void RemoveReferences(
        List<PhonemeSequence> sequences,
        string phoneme)
    {
        sequences.RemoveAll(sequence =>
            sequence.Phonemes.Any(x => IpaComposer.AreEquivalent(x, phoneme)));
    }
}