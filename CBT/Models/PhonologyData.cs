namespace CBT.Models;

public class PhonologyData
{
    public List<ConsonantPhoneme> Consonants { get; set; } = new();

    public List<VowelPhoneme> Vowels { get; set; } = new();
}

public class ConsonantPhoneme
{
    public string Symbol { get; set; } = "";

    // 普通肺部辅音使用的属性。
    public string Place { get; set; } = "";

    public string Manner { get; set; } = "";

    public string Voicing { get; set; } = "";

    // 非肺部气流辅音等特殊辅音使用。
    // 普通肺部辅音可以保持为空。
    public string Category { get; set; } = "";

    public string Description { get; set; } = "";
}

public class VowelPhoneme
{
    public string Symbol { get; set; } = "";

    public string Height { get; set; } = "";

    public string Backness { get; set; } = "";

    public string Roundedness { get; set; } = "";
}