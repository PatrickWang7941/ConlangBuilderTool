namespace CBT.Models;

public class PhonologyData
{
    public List<ConsonantPhoneme> Consonants { get; set; } = new();

    public List<VowelPhoneme> Vowels { get; set; } = new();
}


// 辅音
public class ConsonantPhoneme
{
    // 最终显示和保存的 IPA。
    //
    // 例如：
    // p
    // pʰ
    // n̥
    public string Symbol { get; set; } = "";


    // 未添加附加符号之前的基础 IPA。
    //
    // 例如：
    // pʰ → p
    // n̥ → n
    //
    // 旧项目没有此字段时可以保持为空。
    public string BaseSymbol { get; set; } = "";


    // 应用于基础音素的 IPA Diacritics。
    //
    // 例如：
    // pʰ → ["ʰ"]
    // n̥ → ["\u0325"]
    public List<string> Diacritics { get; set; } = new();


    // 普通肺部辅音使用的属性。
    public string Place { get; set; } = "";

    public string Manner { get; set; } = "";

    public string Voicing { get; set; } = "";


    // Non-pulmonic、Other IPA Symbols 等
    // 特殊辅音使用的附加信息。
    public string Category { get; set; } = "";

    public string Description { get; set; } = "";
}


// 元音
public class VowelPhoneme
{
    // 最终显示和保存的 IPA。
    //
    // 例如：
    // a
    // ã
    // a̤
    public string Symbol { get; set; } = "";


    // 未添加附加符号之前的基础 IPA。
    //
    // 例如：
    // ã → a
    public string BaseSymbol { get; set; } = "";


    // 应用于基础元音的 IPA Diacritics。
    //
    // 例如：
    // ã → ["\u0303"]
    public List<string> Diacritics { get; set; } = new();


    public string Height { get; set; } = "";

    public string Backness { get; set; } = "";

    public string Roundedness { get; set; } = "";
}