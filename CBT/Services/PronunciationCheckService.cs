using CBT.Models;

namespace CBT.Services;

//输入IPA并分析的共享协调入口：先分词，再跑音系配列测试。
//页面只负责显示，本服务不弹窗、不标记dirty、不修改项目。
public sealed record PronunciationCheckResult(
    PhonemeTokenizationResult Tokenization,
    PhonotacticsTestResult? Test);

public static class PronunciationCheckService
{
    public static PronunciationCheckResult Check(ConlangProject project, string input)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(input);

        var tokenization = PhonemeTokenizerService.Tokenize(project, input);
        if (!tokenization.Success)
            return new(tokenization, null);

        var test = PhonotacticsTestService.Test(project, tokenization.Tokens);
        return new(tokenization, test);
    }
}
