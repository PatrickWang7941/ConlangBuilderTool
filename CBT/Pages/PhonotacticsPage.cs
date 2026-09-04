using CBT.Dialogs;
using CBT.Models;
using CBT.Services;

namespace CBT.Pages;

public class PhonotacticsPage : UserControl
{
    private readonly TextBox patternTextBox = new();
    private readonly TextBox descriptionTextBox = new();
    private readonly Label previewLabel = new();
    private readonly ListView templateList = new();
    private readonly Button addButton = new();
    private readonly Button removeButton = new();

    private readonly ListBox onsetList = new();
    private readonly ListBox nucleusList = new();
    private readonly ListBox codaList = new();
    private readonly ListBox forbiddenList = new();

    private readonly Label validationLabel = new();

    private readonly ConlangProject project;
    private readonly Action? projectModified;

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
            Text =
                "在这里定义语言允许的音节结构。\n" +
                "Define the syllable structures allowed in the language.",
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
            Text =
                "例如  Examples:  V, CV, CVC, CCV, CV(C), (C)V(C), CVV\n" +
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
            Text =
                "定义允许出现在声首、音节核和韵尾中的具体音素序列。\n" +
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
            Text =
                "定义在任意位置、词边界或音节组成部分中禁止的音素序列。\n" +
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
        validationLabel.Margin = new Padding(0, 0, 0, 40);

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

        templateList.SelectedIndexChanged += (sender, e) =>
            removeButton.Enabled = templateList.SelectedItems.Count > 0;
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
            BuildSequenceGroup(
                "声首  Allowed Onsets",
                onsetList,
                PhonemeSequenceKind.Onset,
                project.Phonotactics.AllowedOnsets),
            0,
            0);

        panel.Controls.Add(
            BuildSequenceGroup(
                "音节核  Allowed Nuclei",
                nucleusList,
                PhonemeSequenceKind.Nucleus,
                project.Phonotactics.AllowedNuclei),
            1,
            0);

        panel.Controls.Add(
            BuildSequenceGroup(
                "韵尾  Allowed Codas",
                codaList,
                PhonemeSequenceKind.Coda,
                project.Phonotactics.AllowedCodas),
            2,
            0);

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
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        Button add = new()
        {
            Text = "添加  Add",
            Width = 125,
            Height = 34,
            Margin = new Padding(0, 6, 8, 0)
        };

        Button remove = new()
        {
            Text = "删除  Remove",
            Width = 135,
            Height = 34,
            Margin = new Padding(0, 6, 0, 0),
            Enabled = false
        };

        add.Click += (sender, e) => AddSequence(
            PhonemeSequenceKind.Forbidden,
            forbiddenList,
            project.Phonotactics.ForbiddenSequences);

        remove.Click += (sender, e) =>
            RemoveSequence(forbiddenList, project.Phonotactics.ForbiddenSequences);

        forbiddenList.SelectedIndexChanged += (sender, e) =>
            remove.Enabled = forbiddenList.SelectedIndex >= 0;

        buttons.Controls.Add(add);
        buttons.Controls.Add(remove);

        group.Controls.Add(forbiddenList);
        group.Controls.Add(buttons);

        return group;
    }

    private Control BuildSequenceGroup(
        string title,
        ListBox list,
        PhonemeSequenceKind kind,
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
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        Button add = new()
        {
            Text = "添加  Add",
            Width = 125,
            Height = 34,
            Margin = new Padding(0, 6, 8, 0)
        };

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
        list.SelectedIndexChanged += (sender, e) =>
            remove.Enabled = list.SelectedIndex >= 0;

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

    private static void LoadSequenceList(
        ListBox list,
        IEnumerable<PhonemeSequence> sequences,
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
            previewLabel.Text =
                "结构预览  Structure Preview:  " +
                "Onset —   |   Nucleus —   |   Coda —";
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
        var warningText =
            "以下规则之间可能存在冲突：\n" +
            "The following rules may conflict:\n\n" +
            string.Join("\n\n", result.Warnings.Select(x => $"• {x}"));

        validationLabel.Text = result.CanValidate || string.IsNullOrWhiteSpace(result.Message)
            ? warningText
            : $"{result.Message}\n\n{warningText}";
    }

    private void AddTemplate(object? sender, EventArgs e)
    {
        var pattern = patternTextBox.Text.Trim();
        var description = descriptionTextBox.Text.Trim();

        if (pattern.Length == 0)
        {
            MessageBox.Show(
                this,
                "请输入音节模板。\n\nPlease enter a syllable template.",
                "Phonotactics",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            patternTextBox.Focus();
            return;
        }

        var duplicate = project.Phonotactics.SyllableTemplates.Any(x =>
            string.Equals(x.Pattern.Trim(), pattern, StringComparison.Ordinal));

        if (duplicate)
        {
            MessageBox.Show(
                this,
                "该音节模板已经存在。\n\nThis syllable template already exists.",
                "Phonotactics",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        SyllableTemplate template = new()
        {
            Pattern = pattern,
            Description = description
        };

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

    private void AddSequence(
        PhonemeSequenceKind kind,
        ListBox list,
        List<PhonemeSequence> sequences)
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
            MessageBox.Show(
                this,
                "该音素序列已经存在。\n\nThis phoneme sequence already exists.",
                "Phonotactics",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        PhonemeSequence sequence = new()
        {
            Phonemes = phonemes,
            Environment = environment
        };

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

    private sealed class SequenceListItem
    {
        public PhonemeSequence Sequence { get; }
        private bool ShowEnvironment { get; }

        public SequenceListItem(PhonemeSequence sequence, bool showEnvironment)
        {
            Sequence = sequence;
            ShowEnvironment = showEnvironment;
        }

        public override string ToString()
        {
            //模型保存独立音素，这里只负责拼接显示。
            var display = string.Concat(Sequence.Phonemes);
            return ShowEnvironment
                ? $"{display}    [{GetEnvironmentDisplay(Sequence.Environment)}]"
                : display;
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
