namespace CBT.Data;

//IPA元音长度标记
public record IpaVowelLength(string Mark, string Name, bool IsCombining = false)
{
    public string DisplayMark => string.IsNullOrEmpty(Mark) ? "—" : IsCombining ? $"◌{Mark}" : Mark;
    public string DisplayText => string.IsNullOrEmpty(Mark) ? Name : $"{DisplayMark}   {Name}";
}

public static class IpaVowelLengths
{
    public static readonly List<IpaVowelLength> All =
    [
        new("", "普通  Normal"),
        new("ː", "长  Long"),
        new("ˑ", "半长  Half-long"),
        new("\u0306", "超短  Extra-short", true)
    ];
}