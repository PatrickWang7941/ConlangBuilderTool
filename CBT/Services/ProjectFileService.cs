using System.Text.Json;
using CBT.Models;

namespace CBT.Services;

public static class ProjectFileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static void Save(string filePath, ConlangProject project)
    {
        var json = JsonSerializer.Serialize(project, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public static ConlangProject Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var project = JsonSerializer.Deserialize<ConlangProject>(json, JsonOptions);

        if (project == null)
            throw new InvalidDataException("The project file could not be read.");

        //兼容可能缺少部分数据的旧项目。
        project.Phonology ??= new PhonologyData();
        project.Grammar ??= new GrammarData();
        project.Lexicon ??= new LexiconData();

        return project;
    }
}