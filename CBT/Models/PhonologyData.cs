namespace CBT.Models;

public class PhonologyData
{
    public List<ConsonantPhoneme> Consonants { get; set; } = new();
    public List<VowelPhoneme> Vowels { get; set; } = new();
}

//辅音
public class ConsonantPhoneme
{
    //最终显示和保存的IPA，例如p、pʰ、n̥。
    public string Symbol { get; set; } = "";

    //添加附加符号前的基础IPA，例如pʰ的基础音素是p。
    public string BaseSymbol { get; set; } = "";

    //应用于基础音素的IPADiacritics。
    public List<string> Diacritics { get; set; } = new();

    //普通肺部辅音属性。
    public string Place { get; set; } = "";
    public string Manner { get; set; } = "";
    public string Voicing { get; set; } = "";

    //Non-pulmonic和Other IPA Symbols等特殊辅音使用的信息。
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
}

//元音
public class VowelPhoneme
{
    //最终显示和保存的IPA，例如a、ã、a̤。
    public string Symbol { get; set; } = "";

    //添加附加符号前的基础IPA。
    public string BaseSymbol { get; set; } = "";

    //应用于基础元音的IPADiacritics。
    public List<string> Diacritics { get; set; } = new();

    public string Height { get; set; } = "";
    public string Backness { get; set; } = "";
    public string Roundedness { get; set; } = "";
}