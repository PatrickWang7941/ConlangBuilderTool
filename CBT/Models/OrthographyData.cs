namespace CBT.Models;

//拼写系统：音位序列到书写形式的映射。允许一对多、多对一，不限制为一个字符。
public class OrthographyData
{
    public List<OrthographyMapping> Mappings { get; set; } = new();
}

//一条映射，例如 /ʃ/ → sh，或 /t͡ʃ/ → ch。
public class OrthographyMapping
{
    //音素token序列，按项目库存的token保存，避免重新拆分多码点IPA。
    public List<string> Phonemes { get; set; } = new();

    //书写字符串，可以是多字符，例如sh、ng。
    public string Grapheme { get; set; } = "";

    //匹配优先级，数值越大越优先；用于同长度序列的消歧。
    public int Priority { get; set; } = 0;

    public string Notes { get; set; } = "";
}
