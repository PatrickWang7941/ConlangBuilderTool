namespace CBT.Models;

//词汇表数据。每个词条用稳定Id区分，同形异义可以是多个独立词条。
public class LexiconData
{
    public List<Lexeme> Lexemes { get; set; } = new();
}

//一个词条。Id在保存重开后保持一致，即使Lemma后来被改名。
public class Lexeme
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    //词形/拼写，例如tak。
    public string Lemma { get; set; } = "";

    //权威发音文本（IPA）。PronunciationTokens是它的派生缓存，改动文本后应重新分析。
    public string Pronunciation { get; set; } = "";

    //已确认的音素token列表，例如n͡m、aː、pʰ。
    public List<string> PronunciationTokens { get; set; } = new();

    //简单词类标识，允许自定义，例如N、V、Noun、Verb。
    public string PartOfSpeech { get; set; } = "";

    //完整释义，可以有多个义项。
    public List<string> Definitions { get; set; } = new();

    //简短Gloss，用于逐词注释等场景。
    public string Gloss { get; set; } = "";

    //来源类型，先允许自由说明，例如Root、Compound、Loan、Derived或自定义。
    public string Source { get; set; } = "";

    //词根的自由说明，结构引用在形态阶段增强。
    public string Root { get; set; } = "";

    public string Notes { get; set; } = "";
}
