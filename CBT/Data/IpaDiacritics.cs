namespace CBT.Data;

//IPA附加符号。Symbol保存实际参与组合的Unicode内容。
public record IpaDiacritic(string Symbol, string Name, bool IsCombining, string AlternativeSymbol = "")
{
    //Combining mark单独显示时使用dotted circle。
    public string DisplaySymbol => IsCombining ? $"◌{Symbol}" : Symbol;
}

public static class IpaDiacritics
{
    public static readonly List<IpaDiacritic> All =
    [
        //Voicing/phonation
        new("\u0325", "清音化  Voiceless", true, "\u030A"),
        new("\u032C", "浊音化  Voiced", true),
        new("ʰ", "送气  Aspirated", false),
        new("\u0324", "气声  Breathy voiced", true),
        new("\u0330", "嘎裂声  Creaky voiced", true),

        //Lip and tongue articulation
        new("\u0339", "更圆唇  More rounded", true),
        new("\u031C", "较少圆唇  Less rounded", true),
        new("\u031F", "前移  Advanced", true),
        new("\u0320", "后移  Retracted", true),
        new("\u0308", "央化  Centralized", true),
        new("\u033D", "中央化  Mid-centralized", true),
        new("\u033C", "舌唇音化  Linguolabial", true),
        new("ʷ", "唇化  Labialized", false),
        new("ʲ", "腭化  Palatalized", false),
        new("ˠ", "软腭化  Velarized", false),
        new("ˤ", "咽化  Pharyngealized", false),
        new("\u0334", "软腭化或咽化  Velarized or pharyngealized", true),

        //Height/tongue root
        new("\u031D", "抬高  Raised", true),
        new("\u031E", "降低  Lowered", true),
        new("\u0318", "舌根前移  Advanced Tongue Root", true),
        new("\u0319", "舌根后移  Retracted Tongue Root", true),

        //Syllabicity/rhoticity
        new("\u0329", "成音节  Syllabic", true),
        new("\u032F", "非音节  Non-syllabic", true),
        new("˞", "儿化  Rhoticity", false),

        //Coronal articulation
        new("\u032A", "齿音化  Dental", true),
        new("\u033A", "舌尖音  Apical", true),
        new("\u033B", "舌叶音  Laminal", true),

        //Nasality/releases
        new("\u0303", "鼻化  Nasalized", true),
        new("ⁿ", "鼻释放  Nasal release", false),
        new("ˡ", "边释放  Lateral release", false),
        new("\u031A", "无可闻释放  No audible release", true)
    ];
}