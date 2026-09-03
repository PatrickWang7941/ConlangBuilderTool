namespace CBT.Data;

//非肺部气流辅音，参考IPA Kiel 2015图表。
public record IpaNonPulmonicConsonant(string Symbol, string Category, string Description);

public static class IpaNonPulmonicConsonants
{
    public static readonly List<IpaNonPulmonicConsonant> All =
    [
        //Clicks
        new("ʘ", "搭嘴音  Click", "双唇搭嘴音  Bilabial click"),
        new("ǀ", "搭嘴音  Click", "齿搭嘴音  Dental click"),
        new("ǃ", "搭嘴音  Click", "（后）齿龈搭嘴音  (Post)alveolar click"),
        new("ǂ", "搭嘴音  Click", "腭龈搭嘴音  Palatoalveolar click"),
        new("ǁ", "搭嘴音  Click", "齿龈边搭嘴音  Alveolar lateral click"),

        //Voiced implosives
        new("ɓ", "浊内爆音  Voiced implosive", "浊双唇内爆音  Voiced bilabial implosive"),
        new("ɗ", "浊内爆音  Voiced implosive", "浊齿／齿龈内爆音  Voiced dental/alveolar implosive"),
        new("ʄ", "浊内爆音  Voiced implosive", "浊硬腭内爆音  Voiced palatal implosive"),
        new("ɠ", "浊内爆音  Voiced implosive", "浊软腭内爆音  Voiced velar implosive"),
        new("ʛ", "浊内爆音  Voiced implosive", "浊小舌内爆音  Voiced uvular implosive"),

        //Ejectives，IPA图表中的符号仅为示例。
        new("pʼ", "挤喉音  Ejective", "双唇挤喉音  Bilabial ejective"),
        new("tʼ", "挤喉音  Ejective", "齿／齿龈挤喉音  Dental/alveolar ejective"),
        new("kʼ", "挤喉音  Ejective", "软腭挤喉音  Velar ejective"),
        new("sʼ", "挤喉音  Ejective", "齿龈擦音挤喉音  Alveolar fricative ejective")
    ];
}