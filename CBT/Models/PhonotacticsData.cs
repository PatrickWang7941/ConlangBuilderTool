using System.Text.Json.Serialization;

namespace CBT.Models;

public class PhonotacticsData
{
    public List<SyllableTemplate> SyllableTemplates { get; set; } = new();

    public List<PhonemeSequence> AllowedOnsets { get; set; } = new();
    public List<PhonemeSequence> AllowedNuclei { get; set; } = new();
    public List<PhonemeSequence> AllowedCodas { get; set; } = new();

    public List<PhonemeSequence> ForbiddenSequences { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public SyllabificationPreference SyllabificationPreference { get; set; } = SyllabificationPreference.None;

    public string Notes { get; set; } = "";
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SyllabificationPreference
{
    None,
    PreferLargerOnset,
    PreferLargerCoda
}

//音节模板，例如V、CV、CVC、(C)(C)V(C)。
public class SyllableTemplate
{
    public string Pattern { get; set; } = "";
    public string Description { get; set; } = "";
}

//禁止序列可以限制在不同音系环境中。
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PhonemeSequenceEnvironment
{
    Anywhere,
    WordInitial,
    WordFinal,
    Onset,
    Nucleus,
    Coda
}

//音位序列按音素分开保存，避免重新拆分多字符IPA。
public class PhonemeSequence
{
    public List<string> Phonemes { get; set; } = new();
    public string Description { get; set; } = "";

    //目前Environment只用于ForbiddenSequences，旧项目默认为Anywhere。
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public PhonemeSequenceEnvironment Environment { get; set; } = PhonemeSequenceEnvironment.Anywhere;
}
