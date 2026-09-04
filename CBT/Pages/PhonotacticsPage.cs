using CBT.Dialogs;
using CBT.Models;
using CBT.Services;
namespace CBT.Pages;
public class PhonotacticsPage : UserControl
{
    private readonly Button addButton = new();
    private readonly ListBox codaList = new();
    private readonly TextBox descriptionTextBox = new();
    private readonly ListBox forbiddenList = new();
    private readonly ListBox nucleusList = new();
    private readonly ListBox onsetList = new();
    private readonly TextBox patternTextBox = new();
    private readonly Label previewLabel = new();
    private readonly ConlangProject project;
    private readonly Action? projectModified;
    private readonly Button removeButton = new();
    private readonly ListView templateList = new();
    private readonly Label testResultLabel = new();
    private readonly TextBox testWordTextBox = new();
    private readonly Label tokenizationLabel = new();
    private readonly Label validationLabel = new();
    private readonly Label syllabificationLabel = new();
    public PhonotacticsPage() : this(new ConlangProject(), null)
    {
    }
    public PhonotacticsPage(ConlangProject project, Action? projectModified)
    {
        this.project = project;
        this.projectModified = projectModified;
        Dock = DockStyle.Fill;
        Padding = new Padding(0);
        BuildLayout();
        LoadProjectData();
        UpdatePreview();
        UpdateValidation();
    }
    private void BuildLayout()
    {
        FlowLayoutPanel contentPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0)
        };
        Label introduction = new()
        {
            Text = "在这里定义语言允许的音节结构。\n" + "Define the syllable structures allowed in the language.",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 10),
            Margin = new Padding(0, 0, 0, 25)
        };
        Label templateTitle = new()
        {
            Text = "音节模板  Syllable Templates",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 14),
            Margin = new Padding(0, 0, 0, 8)
        };
        Label helpLabel = new()
        {
            Text = "例如  Examples:  V, CV, CVC, CCV, CV(C), (C)V(C), CVV\n" +
                   "C = consonant position    V = nucleus position    ( ) = optional",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 9),
            Margin = new Padding(0, 0, 0, 15)
        };
        var inputRow = BuildTemplateInputRow();
        previewLabel.AutoSize = true;
        previewLabel.Font = new Font("Microsoft YaHei UI", 10);
        previewLabel.Margin = new Padding(0, 0, 0, 15);
        ConfigureTemplateList();
        Label sequenceTitle = new()
        {
            Text = "音素序列限制  Phoneme Sequence Constraints",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 14),
            Margin = new Padding(0, 30, 0, 8)
        };
        Label sequenceHelp = new()
        {
            Text = "定义允许出现在声首、音节核和韵尾中的具体音素序列。\n" +
                   "留空表示尚未定义这一位置的具体限制。Empty means no specific restriction has been defined.",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 9),
            Margin = new Padding(0, 0, 0, 15)
        };
        var sequencePanel = BuildSequencePanel();
        Label forbiddenTitle = new()
        {
            Text = "禁止音素序列  Forbidden Sequences",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 14),
            Margin = new Padding(0, 10, 0, 8)
        };
        Label forbiddenHelp = new()
        {
            Text = "定义在任意位置、词边界或音节组成部分中禁止的音素序列。\n" +
                   "Define sequences forbidden anywhere, at word edges, or within a syllable component.",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 9),
            Margin = new Padding(0, 0, 0, 15)
        };
        var forbiddenPanel = BuildForbiddenPanel();
        Label validationTitle = new()
        {
            Text = "结构检查  Structural Check",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 14),
            Margin = new Padding(0, 10, 0, 8)
        };
        validationLabel.AutoSize = true;
        validationLabel.MaximumSize = new Size(960, 0);
        validationLabel.Font = new Font("Microsoft YaHei UI", 10);
        validationLabel.Margin = new Padding(0, 0, 0, 30);
        Label testTitle = new()
        {
            Text = "音系配列测试  Phonotactics Test",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 14),
            Margin = new Padding(0, 10, 0, 8)
        };
        Label testHelp = new()
        {
            Text = "输入IPA词形，程序会根据音系清单切分音素、尝试音节划分并检查当前规则。\n" +
                   "Enter an IPA word form to tokenize, syllabify, and test against the current phonotactic rules.",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 9),
            Margin = new Padding(0, 0, 0, 12)
        };
        var testPanel = BuildTestPanel();
        contentPanel.Controls.Add(introduction);
        contentPanel.Controls.Add(templateTitle);
        contentPanel.Controls.Add(helpLabel);
        contentPanel.Controls.Add(inputRow);
        contentPanel.Controls.Add(previewLabel);
        contentPanel.Controls.Add(templateList);
        contentPanel.Controls.Add(sequenceTitle);
        contentPanel.Controls.Add(sequenceHelp);
        contentPanel.Controls.Add(sequencePanel);
        contentPanel.Controls.Add(forbiddenTitle);
        contentPanel.Controls.Add(forbiddenHelp);
        contentPanel.Controls.Add(forbiddenPanel);
        contentPanel.Controls.Add(validationTitle);
        contentPanel.Controls.Add(validationLabel);
        contentPanel.Controls.Add(testTitle);
        contentPanel.Controls.Add(testHelp);
        contentPanel.Controls.Add(testPanel);
        Controls.Add(contentPanel);
        patternTextBox.TextChanged += (sender, e) => UpdatePreview();
        patternTextBox.KeyDown += InputTextBox_KeyDown;
        descriptionTextBox.KeyDown += InputTextBox_KeyDown;
    }
    private FlowLayoutPanel BuildTemplateInputRow()
    {
        FlowLayoutPanel inputRow = new()
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };
        patternTextBox.Width = 220;
        patternTextBox.Font = new Font("Microsoft YaHei UI", 11);
        patternTextBox.PlaceholderText = "模板  Pattern";
        patternTextBox.Margin = new Padding(0, 3, 8, 0);
        descriptionTextBox.Width = 460;
        descriptionTextBox.Font = new Font("Microsoft YaHei UI", 11);
        descriptionTextBox.PlaceholderText = "说明，可选  Description, optional";
        descriptionTextBox.Margin = new Padding(0, 3, 8, 0);
        addButton.Text = "添加  Add";
        addButton.Width = 120;
        addButton.Height = patternTextBox.PreferredHeight;
        addButton.Font = new Font("Microsoft YaHei UI", 10);
        addButton.Margin = new Padding(0, 3, 8, 0);
        addButton.Click += AddTemplate;
        removeButton.Text = "删除  Remove";
        removeButton.Width = 130;
        removeButton.Height = patternTextBox.PreferredHeight;
        removeButton.Font = new Font("Microsoft YaHei UI", 10);
        removeButton.Margin = new Padding(0, 3, 0, 0);
        removeButton.Enabled = false;
        removeButton.Click += RemoveSelectedTemplate;
        inputRow.Controls.Add(patternTextBox);
        inputRow.Controls.Add(descriptionTextBox);
        inputRow.Controls.Add(addButton);
        inputRow.Controls.Add(removeButton);
        return inputRow;
    }
    private void ConfigureTemplateList()
    {
        templateList.Width = 960;
        templateList.Height = 300;
        templateList.View = View.Details;
        templateList.FullRowSelect = true;
        templateList.MultiSelect = false;
        templateList.GridLines = true;
        templateList.HideSelection = false;
        templateList.Font = new Font("Microsoft YaHei UI", 10);
        templateList.Margin = new Padding(0);
        templateList.Columns.Add("模板  Pattern", 170);
        templateList.Columns.Add("声首  Onset", 140);
        templateList.Columns.Add("音节核  Nucleus", 140);
        templateList.Columns.Add("韵尾  Coda", 140);
        templateList.Columns.Add("说明  Description", 350);
        templateList.SelectedIndexChanged += (sender, e) => removeButton.Enabled = templateList.SelectedItems.Count > 0;
    }
    private Control BuildSequencePanel()
    {
        TableLayoutPanel panel = new()
        {
            Width = 960,
            Height = 290,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 30)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334f));
        panel.Controls.Add(
            BuildSequenceGroup("声首  Allowed Onsets", onsetList, PhonemeSequenceKind.Onset,
                project.Phonotactics.AllowedOnsets), 0, 0);
        panel.Controls.Add(
            BuildSequenceGroup("音节核  Allowed Nuclei", nucleusList, PhonemeSequenceKind.Nucleus,
                project.Phonotactics.AllowedNuclei), 1, 0);
        panel.Controls.Add(
            BuildSequenceGroup("韵尾  Allowed Codas", codaList, PhonemeSequenceKind.Coda,
                project.Phonotactics.AllowedCodas), 2, 0);
        return panel;
    }
    private Control BuildForbiddenPanel()
    {
        GroupBox group = new()
        {
            Text = "禁止序列及环境  Forbidden Sequences and Environments",
            Width = 960,
            Height = 220,
            Margin = new Padding(0, 0, 0, 30),
            Padding = new Padding(10),
            Font = new Font("Microsoft YaHei UI", 10)
        };
        forbiddenList.Dock = DockStyle.Top;
        forbiddenList.Height = 135;
        forbiddenList.Font = new Font("Microsoft YaHei UI", 13);
        forbiddenList.HorizontalScrollbar = true;
        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.LeftToRight, WrapContents = false
        };
        Button add = new() { Text = "添加  Add", Width = 125, Height = 34, Margin = new Padding(0, 6, 8, 0) };
        Button remove = new()
        {
            Text = "删除  Remove",
            Width = 135,
            Height = 34,
            Margin = new Padding(0, 6, 0, 0),
            Enabled = false
        };
        add.Click += (sender, e) => AddSequence(
            PhonemeSequenceKind.Forbidden, forbiddenList, project.Phonotactics.ForbiddenSequences);
        remove.Click += (sender, e) => RemoveSequence(forbiddenList, project.Phonotactics.ForbiddenSequences);
        forbiddenList.SelectedIndexChanged += (sender, e) => remove.Enabled = forbiddenList.SelectedIndex >= 0;
        buttons.Controls.Add(add);
        buttons.Controls.Add(remove);
        group.Controls.Add(forbiddenList);
        group.Controls.Add(buttons);
        return group;
    }
    private Control BuildTestPanel()
    {
        TableLayoutPanel panel = new()
        {
            Width = 960,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 4,
            Margin = new Padding(0, 0, 0, 40)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 4; i++)
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        FlowLayoutPanel inputRow = new()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 10)
        };
        testWordTextBox.Width = 790;
        testWordTextBox.Font = new Font("Microsoft YaHei UI", 12);
        testWordTextBox.PlaceholderText = "IPA词形  IPA word form";
        testWordTextBox.Margin = new Padding(0, 3, 12, 0);
        testWordTextBox.KeyDown += TestWordTextBox_KeyDown;
        Button testButton = new()
        {
            Text = "测试  Test",
            Width = 150,
            Height = testWordTextBox.PreferredHeight,
            Font = new Font("Microsoft YaHei UI", 10),
            Margin = new Padding(0, 3, 0, 0)
        };
        testButton.Click += RunPhonotacticsTest;
        inputRow.Controls.Add(testWordTextBox);
        inputRow.Controls.Add(testButton);
        tokenizationLabel.Text = "解析  Tokenization:  —";
        tokenizationLabel.AutoSize = true;
        tokenizationLabel.MaximumSize = new Size(960, 0);
        tokenizationLabel.Font = new Font("Microsoft YaHei UI", 10);
        tokenizationLabel.Margin = new Padding(0, 0, 0, 10);
        syllabificationLabel.Text = "音节划分  Syllabification:  —";
        syllabificationLabel.AutoSize = true;
        syllabificationLabel.MaximumSize = new Size(960, 0);
        syllabificationLabel.Font = new Font("Microsoft YaHei UI", 10);
        syllabificationLabel.Margin = new Padding(0, 0, 0, 12);
        testResultLabel.Text = "结果  Result:  —";
        testResultLabel.AutoSize = true;
        testResultLabel.MaximumSize = new Size(960, 0);
        testResultLabel.Font = new Font("Microsoft YaHei UI", 10);
        testResultLabel.Margin = new Padding(0);
        panel.Controls.Add(inputRow, 0, 0);
        panel.Controls.Add(tokenizationLabel, 0, 1);
        panel.Controls.Add(syllabificationLabel, 0, 2);
        panel.Controls.Add(testResultLabel, 0, 3);
        return panel;
    }
    private Control BuildSequenceGroup(string title, ListBox list, PhonemeSequenceKind kind,
        List<PhonemeSequence> sequences)
    {
        GroupBox group = new()
        {
            Text = title,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0),
            Padding = new Padding(10),
            Font = new Font("Microsoft YaHei UI", 10)
        };
        list.Dock = DockStyle.Top;
        list.Height = 190;
        list.Font = new Font("Microsoft YaHei UI", 13);
        list.HorizontalScrollbar = true;
        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Bottom, Height = 48, FlowDirection = FlowDirection.LeftToRight, WrapContents = false
        };
        Button add = new() { Text = "添加  Add", Width = 125, Height = 34, Margin = new Padding(0, 6, 8, 0) };
        Button remove = new()
        {
            Text = "删除  Remove",
            Width = 135,
            Height = 34,
            Margin = new Padding(0, 6, 0, 0),
            Enabled = false
        };
        add.Click += (sender, e) => AddSequence(kind, list, sequences);
        remove.Click += (sender, e) => RemoveSequence(list, sequences);
        list.SelectedIndexChanged += (sender, e) => remove.Enabled = list.SelectedIndex >= 0;
        buttons.Controls.Add(add);
        buttons.Controls.Add(remove);
        group.Controls.Add(list);
        group.Controls.Add(buttons);
        return group;
    }
    private void LoadProjectData()
    {
        templateList.Items.Clear();
        foreach (var template in project.Phonotactics.SyllableTemplates)
            AddTemplateToList(template);
        LoadSequenceList(onsetList, project.Phonotactics.AllowedOnsets);
        LoadSequenceList(nucleusList, project.Phonotactics.AllowedNuclei);
        LoadSequenceList(codaList, project.Phonotactics.AllowedCodas);
        LoadSequenceList(forbiddenList, project.Phonotactics.ForbiddenSequences, true);
    }
    private static void LoadSequenceList(ListBox list, IEnumerable<PhonemeSequence> sequences,
        bool showEnvironment = false)
    {
        list.Items.Clear();
        foreach (var sequence in sequences)
            list.Items.Add(new SequenceListItem(sequence, showEnvironment));
    }
    private void UpdatePreview()
    {
        var pattern = patternTextBox.Text.Trim();
        if (pattern.Length == 0)
        {
            previewLabel.Text = "结构预览  Structure Preview:  " + "Onset —   |   Nucleus —   |   Coda —";
            return;
        }
        var analysis = SyllableTemplateParser.Analyze(pattern);
        previewLabel.Text = analysis.IsRecognized
            ? $"结构预览  Structure Preview:  Onset {analysis.Onset}   |   " +
              $"Nucleus {analysis.Nucleus}   |   Coda {analysis.Coda}"
            : $"结构预览  Structure Preview:  {analysis.Message}";
    }
    private void UpdateValidation()
    {
        var result = PhonotacticsValidator.Validate(project.Phonotactics);
        if (result.Warnings.Count == 0)
        {
            validationLabel.Text = result.CanValidate
                ? "未发现明显的音系配列规则矛盾。\nNo obvious phonotactic rule conflicts were found."
                : result.Message;
            return;
        }

        //结构检查只提供提示，不阻止用户继续设计语言。
        var warningText = "以下规则之间可能存在冲突：\n" + "The following rules may conflict:\n\n" +
                          string.Join("\n\n", result.Warnings.Select(x => $"• {x}"));
        validationLabel.Text = result.CanValidate || string.IsNullOrWhiteSpace(result.Message)
            ? warningText
            : $"{result.Message}\n\n{warningText}";
    }
    private void RunPhonotacticsTest(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(testWordTextBox.Text))
        {
            tokenizationLabel.Text = "解析  Tokenization:  —";
            syllabificationLabel.Text = "音节划分  Syllabification:  —";
            testResultLabel.Text = "请输入要测试的IPA词形。\n" + "Please enter an IPA word form to test.";
            testWordTextBox.Focus();
            return;
        }
        var tokenization = PhonemeTokenizerService.Tokenize(project, testWordTextBox.Text);
        var tokenDisplay = tokenization.Tokens.Count == 0 ? "—" : string.Join(" | ", tokenization.Tokens);
        tokenizationLabel.Text = $"解析  Tokenization:  {tokenDisplay}";
        if (!tokenization.Success)
        {
            syllabificationLabel.Text = "音节划分  Syllabification:  —";
            testResultLabel.Text = $"无法从规范化输入的第{tokenization.FailureIndex + 1}个字符继续解析。" +
                                   $" Could not continue tokenizing at character {tokenization.FailureIndex + 1}.\n" +
                                   $"未识别文本  Unrecognized text:  {tokenization.RemainingText}";
            return;
        }
        var result = PhonotacticsTestService.Test(project, tokenization.Tokens);
        UpdateSyllabificationDisplay(result.Syllabification);
        UpdateTestResultDisplay(result);
    }
    private void UpdateSyllabificationDisplay(SyllabificationResult result)
    {
        if (!result.Success)
        {
            syllabificationLabel.Text = $"音节划分  Syllabification:  {result.Message}";
            return;
        }
        if (result.Analyses.Count == 1)
        {
            var analysis = result.Analyses[0];
            syllabificationLabel.Text = "音节划分  Syllabification:\n" + FormatSyllabificationAnalysis(analysis);
            if (result.WasTruncated && !string.IsNullOrWhiteSpace(result.Message))
                syllabificationLabel.Text += $"\n\n{result.Message}";
            return;
        }
        const int displayLimit = 8;
        var displayed = result.Analyses.Take(displayLimit)
            .Select((analysis, index) => $"{index + 1}. {analysis.Display}").ToList();
        var header = $"存在{result.Analyses.Count}个合法音节划分。" +
                     $" Multiple valid syllabifications: {result.Analyses.Count}.";
        var extra = result.Analyses.Count > displayLimit
            ? $"\n仅显示前{displayLimit}个。Only the first {displayLimit} are shown."
            : "";
        if (result.WasTruncated && !string.IsNullOrWhiteSpace(result.Message))
            extra += $"\n{result.Message}";
        syllabificationLabel.Text = $"{header}\n" + string.Join("\n", displayed) + extra;
    }
    private static string FormatSyllabificationAnalysis(SyllabificationAnalysis analysis)
    {
        List<string> lines =
        [
            analysis.Display
        ];
        for (var index = 0; index < analysis.Syllables.Count; index++)
        {
            var syllable = analysis.Syllables[index];
            var onset = syllable.Onset.Count == 0 ? "—" : string.Concat(syllable.Onset);
            var nucleus = syllable.Nucleus.Count == 0 ? "—" : string.Concat(syllable.Nucleus);
            var coda = syllable.Coda.Count == 0 ? "—" : string.Concat(syllable.Coda);
            lines.Add($"Syllable {index + 1}:  " + $"Onset {onset}   |   Nucleus {nucleus}   |   Coda {coda}");
        }
        return string.Join("\n", lines);
    }
    private void UpdateTestResultDisplay(PhonotacticsTestResult result)
    {
        List<string> warnings = [];
        warnings.AddRange(result.WordMatches.Select(FormatTestMatch));
        foreach (var assessment in result.SyllableAssessments)
        {
            var sequence = string.Concat(assessment.Rule.Phonemes);
            var environment = GetTestEnvironmentDisplay(assessment.Environment);
            var syllables = string.Join(", ", assessment.SyllableNumbers);
            var description = string.IsNullOrWhiteSpace(assessment.Rule.Description)
                ? ""
                : $" — {assessment.Rule.Description}";
            if (assessment.IsCertain)
                warnings.Add($"⚠ {sequence} — {environment} — " + $"所有合法音节划分都会命中此规则。" +
                             $" All valid syllabifications violate this rule." +
                             $" Syllable(s): {syllables}{description}");
            else
                warnings.Add($"△ {sequence} — {environment} — " + $"仅部分合法音节划分命中此规则。" +
                             $" Only some valid syllabifications violate this rule." +
                             $" Syllable(s): {syllables}{description}");
        }
        if (!result.Syllabification.Success)
        {
            if (warnings.Count == 0)
                testResultLabel.Text = "未发现词级禁止序列，但当前无法可靠完成音节级检查。\n" +
                                       "No word-level forbidden sequence was found, but syllable-level checking could not be completed reliably.";
            else
                testResultLabel.Text = "结果  Result:\n" + string.Join("\n", warnings) + "\n\n当前无法可靠完成音节级检查。" +
                                       " Syllable-level checking could not be completed reliably.";
            return;
        }
        testResultLabel.Text = warnings.Count == 0
            ? "✓ 未发现当前规则下的音系配列冲突。\n" + "No phonotactic conflict was found under the current rules."
            : "结果  Result:\n" + string.Join("\n", warnings);
    }
    private static string FormatTestMatch(PhonotacticsRuleMatch match)
    {
        var sequence = string.Concat(match.Rule.Phonemes);
        var firstPosition = match.StartIndex + 1;
        var lastPosition = match.StartIndex + match.Rule.Phonemes.Count;
        var position = firstPosition == lastPosition ? firstPosition.ToString() : $"{firstPosition}–{lastPosition}";
        var description = string.IsNullOrWhiteSpace(match.Rule.Description) ? "" : $" — {match.Rule.Description}";
        return $"⚠ {sequence} — {GetTestEnvironmentDisplay(match.Environment)} — " +
               $"音素位置 {position} / token position {position}{description}";
    }
    private static string GetTestEnvironmentDisplay(PhonemeSequenceEnvironment environment)
    {
        return environment switch
        {
            PhonemeSequenceEnvironment.Anywhere => "任意位置 / Anywhere",
            PhonemeSequenceEnvironment.WordInitial => "词首 / Word-initial",
            PhonemeSequenceEnvironment.WordFinal => "词尾 / Word-final",
            PhonemeSequenceEnvironment.Onset => "声首 / Onset",
            PhonemeSequenceEnvironment.Nucleus => "音节核 / Nucleus",
            PhonemeSequenceEnvironment.Coda => "韵尾 / Coda",
            _ => environment.ToString()
        };
    }
    private void AddTemplate(object? sender, EventArgs e)
    {
        var pattern = patternTextBox.Text.Trim();
        var description = descriptionTextBox.Text.Trim();
        if (pattern.Length == 0)
        {
            MessageBox.Show(this, "请输入音节模板。\n\nPlease enter a syllable template.", "Phonotactics", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            patternTextBox.Focus();
            return;
        }
        var duplicate = project.Phonotactics.SyllableTemplates.Any(x =>
            string.Equals(x.Pattern.Trim(), pattern, StringComparison.Ordinal));
        if (duplicate)
        {
            MessageBox.Show(this, "该音节模板已经存在。\n\nThis syllable template already exists.", "Phonotactics",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        SyllableTemplate template = new() { Pattern = pattern, Description = description };
        project.Phonotactics.SyllableTemplates.Add(template);
        AddTemplateToList(template);
        patternTextBox.Clear();
        descriptionTextBox.Clear();
        patternTextBox.Focus();
        projectModified?.Invoke();
        UpdateValidation();
    }
    private void AddTemplateToList(SyllableTemplate template)
    {
        var analysis = SyllableTemplateParser.Analyze(template.Pattern);
        ListViewItem item = new(template.Pattern);
        if (analysis.IsRecognized)
        {
            item.SubItems.Add(analysis.Onset);
            item.SubItems.Add(analysis.Nucleus);
            item.SubItems.Add(analysis.Coda);
        }
        else
        {
            item.SubItems.Add("—");
            item.SubItems.Add(analysis.Message);
            item.SubItems.Add("—");
        }
        item.SubItems.Add(template.Description);
        item.Tag = template;
        templateList.Items.Add(item);
    }
    private void RemoveSelectedTemplate(object? sender, EventArgs e)
    {
        if (templateList.SelectedItems.Count == 0) return;
        var item = templateList.SelectedItems[0];
        if (item.Tag is not SyllableTemplate template) return;
        project.Phonotactics.SyllableTemplates.Remove(template);
        templateList.Items.Remove(item);
        projectModified?.Invoke();
        UpdateValidation();
    }
    private void AddSequence(PhonemeSequenceKind kind, ListBox list, List<PhonemeSequence> sequences)
    {
        using PhonemeSequenceDialog dialog = new(project, kind);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var phonemes = dialog.SelectedPhonemes.ToList();
        var environment = kind == PhonemeSequenceKind.Forbidden
            ? dialog.SelectedEnvironment
            : PhonemeSequenceEnvironment.Anywhere;

        //比较独立音素token，避免多字符IPA造成误判。
        var duplicate = sequences.Any(existing =>
            existing.Phonemes.SequenceEqual(phonemes) &&
            (kind != PhonemeSequenceKind.Forbidden || existing.Environment == environment));
        if (duplicate)
        {
            MessageBox.Show(this, "该音素序列已经存在。\n\nThis phoneme sequence already exists.", "Phonotactics",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        PhonemeSequence sequence = new() { Phonemes = phonemes, Environment = environment };
        sequences.Add(sequence);
        list.Items.Add(new SequenceListItem(sequence, kind == PhonemeSequenceKind.Forbidden));
        projectModified?.Invoke();
        UpdateValidation();
    }
    private void RemoveSequence(ListBox list, List<PhonemeSequence> sequences)
    {
        if (list.SelectedItem is not SequenceListItem item) return;
        sequences.Remove(item.Sequence);
        list.Items.Remove(item);
        projectModified?.Invoke();
        UpdateValidation();
    }
    private void InputTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;
        AddTemplate(sender, EventArgs.Empty);
    }
    private void TestWordTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;
        RunPhonotacticsTest(sender, EventArgs.Empty);
    }
    private sealed class SequenceListItem
    {
        public SequenceListItem(PhonemeSequence sequence, bool showEnvironment)
        {
            Sequence = sequence;
            ShowEnvironment = showEnvironment;
        }
        public PhonemeSequence Sequence { get; }
        private bool ShowEnvironment { get; }
        public override string ToString()
        {
            //模型保存独立音素，这里只负责拼接显示。
            var display = string.Concat(Sequence.Phonemes);
            return ShowEnvironment ? $"{display}    [{GetEnvironmentDisplay(Sequence.Environment)}]" : display;
        }
        private static string GetEnvironmentDisplay(PhonemeSequenceEnvironment environment)
        {
            return environment switch
            {
                PhonemeSequenceEnvironment.Anywhere => "任意位置  Anywhere",
                PhonemeSequenceEnvironment.WordInitial => "词首  Word-initial",
                PhonemeSequenceEnvironment.WordFinal => "词尾  Word-final",
                PhonemeSequenceEnvironment.Onset => "声首  Onset",
                PhonemeSequenceEnvironment.Nucleus => "音节核  Nucleus",
                PhonemeSequenceEnvironment.Coda => "韵尾  Coda",
                _ => environment.ToString()
            };
        }
    }
}