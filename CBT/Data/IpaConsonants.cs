namespace CBT.Data;
public record IpaConsonant(string Symbol, string Place, string Manner, string Voicing);
public static class IpaConsonants
{
    public static readonly List<IpaConsonant> All =
        //这段AI写的。还不如我的，赫赫。
        [
            // Plosives
            new("p", "双唇  Bilabial", "塞音  Plosive", "清音  Voiceless"),
            new("b", "双唇  Bilabial", "塞音  Plosive", "浊音  Voiced"),
            new("t", "齿龈  Alveolar", "塞音  Plosive", "清音  Voiceless"),
            new("d", "齿龈  Alveolar", "塞音  Plosive", "浊音  Voiced"),
            new("ʈ", "卷舌  Retroflex", "塞音  Plosive", "清音  Voiceless"),
            new("ɖ", "卷舌  Retroflex", "塞音  Plosive", "浊音  Voiced"),
            new("c", "硬腭  Palatal", "塞音  Plosive", "清音  Voiceless"),
            new("ɟ", "硬腭  Palatal", "塞音  Plosive", "浊音  Voiced"),
            new("k", "软腭  Velar", "塞音  Plosive", "清音  Voiceless"),
            new("ɡ", "软腭  Velar", "塞音  Plosive", "浊音  Voiced"),
            new("q", "小舌  Uvular", "塞音  Plosive", "清音  Voiceless"),
            new("ɢ", "小舌  Uvular", "塞音  Plosive", "浊音  Voiced"),
            new("ʔ", "声门  Glottal", "塞音  Plosive", "清音  Voiceless"),
            new("ʡ", "会厌  Epiglottal", "塞音  Plosive", "清音  Voiceless"),

            // Affricates
            new("t͡s", "齿龈  Alveolar", "塞擦音  Affricate", "清音  Voiceless"),
            new("d͡z", "齿龈  Alveolar", "塞擦音  Affricate", "浊音  Voiced"),
            new("t͡ʃ", "龈后  Postalveolar", "塞擦音  Affricate", "清音  Voiceless"),
            new("d͡ʒ", "龈后  Postalveolar", "塞擦音  Affricate", "浊音  Voiced"),
            new("t͡ɕ", "龈腭  Alveolo-palatal", "塞擦音  Affricate", "清音  Voiceless"),
            new("d͡ʑ", "龈腭  Alveolo-palatal", "塞擦音  Affricate", "浊音  Voiced"),
            new("ʈ͡ʂ", "卷舌  Retroflex", "塞擦音  Affricate", "清音  Voiceless"),
            new("ɖ͡ʐ", "卷舌  Retroflex", "塞擦音  Affricate", "浊音  Voiced"),

            // Nasals
            new("m", "双唇  Bilabial", "鼻音  Nasal", "浊音  Voiced"),
            new("ɱ", "唇齿  Labiodental", "鼻音  Nasal", "浊音  Voiced"),
            new("n", "齿龈  Alveolar", "鼻音  Nasal", "浊音  Voiced"),
            new("ɳ", "卷舌  Retroflex", "鼻音  Nasal", "浊音  Voiced"),
            new("ɲ", "硬腭  Palatal", "鼻音  Nasal", "浊音  Voiced"),
            new("ŋ", "软腭  Velar", "鼻音  Nasal", "浊音  Voiced"),
            new("ɴ", "小舌  Uvular", "鼻音  Nasal", "浊音  Voiced"),

            // Trills
            new("ʙ", "双唇  Bilabial", "颤音  Trill", "浊音  Voiced"),
            new("r", "齿龈  Alveolar", "颤音  Trill", "浊音  Voiced"),
            new("ʀ", "小舌  Uvular", "颤音  Trill", "浊音  Voiced"),

            // Taps / Flaps
            new("ⱱ", "唇齿  Labiodental", "闪音  Tap / Flap", "浊音  Voiced"),
            new("ɾ", "齿龈  Alveolar", "闪音  Tap / Flap", "浊音  Voiced"),
            new("ɽ", "卷舌  Retroflex", "闪音  Tap / Flap", "浊音  Voiced"),

            // Lateral flap
            new("ɺ", "齿龈  Alveolar", "边闪音  Lateral flap", "浊音  Voiced"),

            // Fricatives
            new("ʍ", "唇软腭  Labial-velar", "擦音  Fricative", "清音  Voiceless"),
            new("ɸ", "双唇  Bilabial", "擦音  Fricative", "清音  Voiceless"),
            new("β", "双唇  Bilabial", "擦音  Fricative", "浊音  Voiced"),
            new("f", "唇齿  Labiodental", "擦音  Fricative", "清音  Voiceless"),
            new("v", "唇齿  Labiodental", "擦音  Fricative", "浊音  Voiced"),
            new("θ", "齿  Dental", "擦音  Fricative", "清音  Voiceless"),
            new("ð", "齿  Dental", "擦音  Fricative", "浊音  Voiced"),
            new("s", "齿龈  Alveolar", "擦音  Fricative", "清音  Voiceless"),
            new("z", "齿龈  Alveolar", "擦音  Fricative", "浊音  Voiced"),
            new("ʃ", "龈后  Postalveolar", "擦音  Fricative", "清音  Voiceless"),
            new("ʒ", "龈后  Postalveolar", "擦音  Fricative", "浊音  Voiced"),
            new("ɕ", "龈腭  Alveolo-palatal", "擦音  Fricative", "清音  Voiceless"),
            new("ʑ", "龈腭  Alveolo-palatal", "擦音  Fricative", "浊音  Voiced"),
            new("ʂ", "卷舌  Retroflex", "擦音  Fricative", "清音  Voiceless"),
            new("ʐ", "卷舌  Retroflex", "擦音  Fricative", "浊音  Voiced"),
            new("ç", "硬腭  Palatal", "擦音  Fricative", "清音  Voiceless"),
            new("ʝ", "硬腭  Palatal", "擦音  Fricative", "浊音  Voiced"),
            new("x", "软腭  Velar", "擦音  Fricative", "清音  Voiceless"),
            new("ɣ", "软腭  Velar", "擦音  Fricative", "浊音  Voiced"),
            new("χ", "小舌  Uvular", "擦音  Fricative", "清音  Voiceless"),
            new("ʁ", "小舌  Uvular", "擦音  Fricative", "浊音  Voiced"),
            new("ħ", "咽  Pharyngeal", "擦音  Fricative", "清音  Voiceless"),
            new("ʕ", "咽  Pharyngeal", "擦音  Fricative", "浊音  Voiced"),
            new("ʜ", "会厌  Epiglottal", "擦音  Fricative", "清音  Voiceless"),
            new("ʢ", "会厌  Epiglottal", "擦音  Fricative", "浊音  Voiced"),
            new("h", "声门  Glottal", "擦音  Fricative", "清音  Voiceless"),
            new("ɦ", "声门  Glottal", "擦音  Fricative", "浊音  Voiced"),

            // Lateral fricatives
            new("ɬ", "齿龈  Alveolar", "边擦音  Lateral fricative", "清音  Voiceless"),
            new("ɮ", "齿龈  Alveolar", "边擦音  Lateral fricative", "浊音  Voiced"),

            // Approximants
            new("ʋ", "唇齿  Labiodental", "近音  Approximant", "浊音  Voiced"),
            new("ɹ", "齿龈  Alveolar", "近音  Approximant", "浊音  Voiced"),
            new("ɻ", "卷舌  Retroflex", "近音  Approximant", "浊音  Voiced"),
            new("j", "硬腭  Palatal", "近音  Approximant", "浊音  Voiced"),
            new("ɥ", "唇硬腭  Labial-palatal", "近音  Approximant", "浊音  Voiced"),
            new("ɰ", "软腭  Velar", "近音  Approximant", "浊音  Voiced"),
            new("w", "唇软腭  Labial-velar", "近音  Approximant", "浊音  Voiced"),

            // Lateral approximants
            new("l", "齿龈  Alveolar", "边近音  Lateral approximant", "浊音  Voiced"),
            new("ɭ", "卷舌  Retroflex", "边近音  Lateral approximant", "浊音  Voiced"),
            new("ʎ", "硬腭  Palatal", "边近音  Lateral approximant", "浊音  Voiced"),
            new("ʟ", "软腭  Velar", "边近音  Lateral approximant", "浊音  Voiced")
        ];
}