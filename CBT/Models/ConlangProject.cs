namespace CBT.Models;

public class ConlangProject
{
    //项目文件格式版本，用于以后兼容旧项目。
    public int FormatVersion { get; set; } = 1;

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public PhonologyData Phonology { get; set; } = new();
    public PhonotacticsData Phonotactics { get; set; } = new();
    public GrammarData Grammar { get; set; } = new();
    public LexiconData Lexicon { get; set; } = new();
}