namespace CBT.Models;

public class PhonologyData
{
    public List<ConsonantPhoneme> Consonants { get; set; } = new();
    public List<VowelPhoneme> Vowels { get; set; } = new();
}

//辅音
public class ConsonantPhoneme
{
    //最终显示和保存的IPA，例如p、pʰ、d͡z、n͡m。
    public string Symbol { get; set; } = "";

    //添加附加符号前的基础IPA。
    public string BaseSymbol { get; set; } = "";

    //应用于基础音素的IPADiacritics。
    public List<string> Diacritics { get; set; } = new();

    //Tie bar音素的组成部分，例如d͡z保存为[d, z]。
    public List<string> Components { get; set; } = new();

    //普通肺部辅音属性。
    public string Place { get; set; } = "";
    public string Manner { get; set; } = "";
    public string Voicing { get; set; } = "";

    //Non-pulmonic、Other IPA Symbols和复合调音使用的信息。
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
}

//元音
public class VowelPhoneme
{
    //最终显示和保存的IPA，例如a、ã、a̤、aː。
    public string Symbol { get; set; } = "";

    //添加附加符号和长度标记前的基础IPA。
    public string BaseSymbol { get; set; } = "";

    //应用于基础元音的IPADiacritics。
    public List<string> Diacritics { get; set; } = new();

    //元音长度标记。普通长度为空，长元音为ː，半长为ˑ，超短为U+0306。
    public string LengthMark { get; set; } = "";

    public string Height { get; set; } = "";
    public string Backness { get; set; } = "";
    public string Roundedness { get; set; } = "";
}