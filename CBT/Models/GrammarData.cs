namespace CBT.Models;

//语法描述框架：结构化字段与自由说明并存。空字符串表示未设置。
public class GrammarData
{
    //基本语序，例如SOV、SVO、Free；空表示未设置。
    public string BasicWordOrder { get; set; } = "";

    //语序自由说明，选择Other或需要补充时使用。
    public string WordOrderNotes { get; set; } = "";

    //形态类型，例如Analytic、Agglutinative；空表示未设置。
    public string MorphologicalType { get; set; } = "";

    //形态类型自由说明，真实语言往往是连续谱。
    public string MorphologyNotes { get; set; } = "";

    //名词系统描述，允许自由说明，空表示未设置或未启用。
    public string NounSystem { get; set; } = "";

    //动词系统描述，允许自由说明，空表示未设置或未启用。
    public string VerbSystem { get; set; } = "";

    public string Notes { get; set; } = "";
}
