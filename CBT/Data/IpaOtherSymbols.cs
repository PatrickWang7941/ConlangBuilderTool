namespace CBT.Data;

public record IpaOtherSymbol(string Symbol, string Name);

public static class IpaOtherSymbols
{
    public static readonly List<IpaOtherSymbol> All =
    [
        new("ɧ", "sje音  sj-sound Simultaneous ʃ and x")
    ];
}