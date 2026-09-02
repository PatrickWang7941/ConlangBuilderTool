using System;
using System.Collections.Generic;
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
        string json = JsonSerializer.Serialize(project, JsonOptions);

        File.WriteAllText(filePath, json);
    }

    public static ConlangProject Load(string filePath)
    {
        string json = File.ReadAllText(filePath);

        ConlangProject? project =
            JsonSerializer.Deserialize<ConlangProject>(json, JsonOptions);

        if (project == null)
        {
            throw new InvalidDataException(
                "The project file could not be read.");
        }

        // 防止未来旧版本项目中缺少某些部分。
        project.Phonology ??= new PhonologyData();
        project.Grammar ??= new GrammarData();
        project.Lexicon ??= new LexiconData();

        return project;
    }
}