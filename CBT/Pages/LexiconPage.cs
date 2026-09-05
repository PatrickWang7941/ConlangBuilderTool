using CBT.Models;
using CBT.Services;

namespace CBT.Pages;

//词典页面：左侧浏览词条，右侧编辑；编辑用副本语义，保存才写回模型。
public class LexiconPage : UserControl
{
    private readonly ConlangProject project;
    private readonly Action? projectModified;

    private readonly TextBox searchTextBox = new();
    private readonly ListView lexemeListView = new();

    private readonly TextBox lemmaTextBox = new();
    private readonly TextBox posTextBox = new();
    private readonly TextBox pronunciationTextBox = new();
    private readonly TextBox glossTextBox = new();
    private readonly TextBox definitionsTextBox = new();
    private readonly ComboBox sourceComboBox = new();
    private readonly TextBox rootTextBox = new();
    private readonly TextBox notesTextBox = new();

    private readonly Button addButton = new();
    private readonly Button saveButton = new();
    private readonly Button cancelButton = new();
    private readonly Button deleteButton = new();
    private readonly Label statusLabel = new();
    private readonly Label pronunciationCheckLabel = new();

    //当前编辑区加载的词条；null表示编辑区为空或处于新建模式。
    private Lexeme? currentLexeme;
    private bool newMode;
    private bool editorDirty;
    private bool loadingData;

    public LexiconPage(ConlangProject project, Action? projectModified)
    {
        this.project = project;
        this.projectModified = projectModified;

        Dock = DockStyle.Fill;
        Padding = new Padding(0);

        //构建期间暂停布局，避免首次显示时出现中间态闪烁。
        SuspendLayout();
        try
        {
            BuildLayout();
            LoadProjectData();
        }
        finally
        {
            ResumeLayout(true);
        }
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
            Text = "管理语言词汇。每个词条用稳定的Id关联，同形异义可作为独立词条保存。\n" +
                   "Manage the language lexicon. Each entry keeps a stable Id; homographs stay as separate entries.",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 10),
            Margin = new Padding(0, 0, 0, 20)
        };

        var searchRow = BuildSearchRow();
        var split = BuildEditorSplit();
        statusLabel.AutoSize = true;
        statusLabel.MaximumSize = new Size(1200, 0);
        statusLabel.Font = new Font("Microsoft YaHei UI", 9);
        statusLabel.Margin = new Padding(0, 8, 0, 0);

        contentPanel.Controls.Add(introduction);
        contentPanel.Controls.Add(searchRow);
        contentPanel.Controls.Add(split);
        contentPanel.Controls.Add(statusLabel);
        Controls.Add(contentPanel);

        //初始化状态栏显示；这里不经过EditorTextChanged，避免误标记未保存。
        UpdateEditorState();
    }

    private FlowLayoutPanel BuildSearchRow()
    {
        FlowLayoutPanel row = new()
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };
        searchTextBox.Width = 420;
        searchTextBox.Font = new Font("Microsoft YaHei UI", 11);
        searchTextBox.PlaceholderText = "搜索词形、词类或释义  Search lemma, POS, or definition";
        searchTextBox.Margin = new Padding(0, 3, 12, 0);
        searchTextBox.TextChanged += (sender, e) => RefreshList();

        row.Controls.Add(searchTextBox);
        return row;
    }

    private TableLayoutPanel BuildEditorSplit()
    {
        TableLayoutPanel split = new()
        {
            Width = 1200,
            Height = 600,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0)
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56f));
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        ConfigureLexemeList();
        split.Controls.Add(lexemeListView, 0, 0);
        split.Controls.Add(BuildEditorPanel(), 1, 0);
        return split;
    }

    private void ConfigureLexemeList()
    {
        lexemeListView.Dock = DockStyle.Fill;
        lexemeListView.View = View.Details;
        lexemeListView.FullRowSelect = true;
        lexemeListView.MultiSelect = false;
        lexemeListView.GridLines = true;
        lexemeListView.HideSelection = false;
        lexemeListView.Font = new Font("Microsoft YaHei UI", 10);
        lexemeListView.Margin = new Padding(0, 0, 12, 0);
        lexemeListView.Columns.Add("词形  Lemma", 150);
        lexemeListView.Columns.Add("词类  POS", 110);
        lexemeListView.Columns.Add("释义  Definition", 250);
        lexemeListView.SelectedIndexChanged += LexemeList_SelectedIndexChanged;
    }

    private Control BuildEditorPanel()
    {
        FlowLayoutPanel editor = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Margin = new Padding(12, 0, 0, 0),
            Padding = new Padding(0)
        };

        editor.Controls.Add(BuildLabel("词形  Lemma"));
        lemmaTextBox.Width = 520;
        lemmaTextBox.Font = new Font("Microsoft YaHei UI", 11);
        lemmaTextBox.Margin = new Padding(0, 0, 0, 12);
        editor.Controls.Add(lemmaTextBox);

        editor.Controls.Add(BuildLabel("词类  Part of Speech"));
        posTextBox.Width = 520;
        posTextBox.Font = new Font("Microsoft YaHei UI", 11);
        posTextBox.PlaceholderText = "N, V, Noun, Verb…";
        posTextBox.Margin = new Padding(0, 0, 0, 12);
        editor.Controls.Add(posTextBox);

        editor.Controls.Add(BuildLabel("发音  Pronunciation (IPA)"));
        pronunciationTextBox.Width = 520;
        pronunciationTextBox.Font = new Font("Microsoft YaHei UI", 11);
        pronunciationTextBox.PlaceholderText = "例如  e.g.  tak";
        pronunciationTextBox.Margin = new Padding(0, 0, 0, 4);
        editor.Controls.Add(pronunciationTextBox);

        pronunciationCheckLabel.AutoSize = true;
        pronunciationCheckLabel.MaximumSize = new Size(560, 0);
        pronunciationCheckLabel.Font = new Font("Microsoft YaHei UI", 9);
        pronunciationCheckLabel.ForeColor = Color.FromArgb(90, 90, 90);
        pronunciationCheckLabel.Margin = new Padding(0, 0, 0, 12);
        editor.Controls.Add(pronunciationCheckLabel);

        editor.Controls.Add(BuildLabel("简短注释  Gloss"));
        glossTextBox.Width = 520;
        glossTextBox.Font = new Font("Microsoft YaHei UI", 11);
        glossTextBox.Margin = new Padding(0, 0, 0, 12);
        editor.Controls.Add(glossTextBox);

        editor.Controls.Add(BuildLabel("释义  Definitions（每行一个义项  one sense per line）"));
        definitionsTextBox.Width = 520;
        definitionsTextBox.Height = 110;
        definitionsTextBox.Multiline = true;
        definitionsTextBox.ScrollBars = ScrollBars.Vertical;
        definitionsTextBox.Font = new Font("Microsoft YaHei UI", 11);
        definitionsTextBox.Margin = new Padding(0, 0, 0, 12);
        editor.Controls.Add(definitionsTextBox);

        editor.Controls.Add(BuildLabel("来源  Source"));
        sourceComboBox.Width = 520;
        sourceComboBox.Font = new Font("Microsoft YaHei UI", 11);
        sourceComboBox.Margin = new Padding(0, 0, 0, 12);
        sourceComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        sourceComboBox.Items.AddRange(new object[]
        {
            "Root",
            "Compound",
            "Loan",
            "Derived"
        });
        editor.Controls.Add(sourceComboBox);

        editor.Controls.Add(BuildLabel("词根  Root"));
        rootTextBox.Width = 520;
        rootTextBox.Font = new Font("Microsoft YaHei UI", 11);
        rootTextBox.PlaceholderText = "自由说明  free-form，例如 ROOT-15";
        rootTextBox.Margin = new Padding(0, 0, 0, 12);
        editor.Controls.Add(rootTextBox);

        editor.Controls.Add(BuildLabel("备注  Notes"));
        notesTextBox.Width = 520;
        notesTextBox.Height = 70;
        notesTextBox.Multiline = true;
        notesTextBox.ScrollBars = ScrollBars.Vertical;
        notesTextBox.Font = new Font("Microsoft YaHei UI", 11);
        notesTextBox.Margin = new Padding(0, 0, 0, 12);
        editor.Controls.Add(notesTextBox);

        var buttonRow = BuildButtonRow();
        editor.Controls.Add(buttonRow);

        return editor;
    }

    private FlowLayoutPanel BuildButtonRow()
    {
        FlowLayoutPanel row = new()
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0)
        };
        addButton.Text = "新增  New";
        addButton.Width = 120;
        addButton.Height = 36;
        addButton.Font = new Font("Microsoft YaHei UI", 10);
        addButton.Margin = new Padding(0, 3, 8, 0);
        addButton.Click += (sender, e) => NewLexeme();

        saveButton.Text = "保存  Save";
        saveButton.Width = 120;
        saveButton.Height = 36;
        saveButton.Font = new Font("Microsoft YaHei UI", 10);
        saveButton.Margin = new Padding(0, 3, 8, 0);
        saveButton.Click += (sender, e) => SaveEdit();

        cancelButton.Text = "取消  Cancel";
        cancelButton.Width = 120;
        cancelButton.Height = 36;
        cancelButton.Font = new Font("Microsoft YaHei UI", 10);
        cancelButton.Margin = new Padding(0, 3, 8, 0);
        cancelButton.Click += (sender, e) => CancelEdit();

        deleteButton.Text = "删除  Delete";
        deleteButton.Width = 120;
        deleteButton.Height = 36;
        deleteButton.Font = new Font("Microsoft YaHei UI", 10);
        deleteButton.Margin = new Padding(0, 3, 0, 0);
        deleteButton.Enabled = false;
        deleteButton.Click += (sender, e) => DeleteLexeme();

        row.Controls.Add(addButton);
        row.Controls.Add(saveButton);
        row.Controls.Add(cancelButton);
        row.Controls.Add(deleteButton);
        return row;
    }

    private static Label BuildLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 10),
            Margin = new Padding(0, 0, 0, 4)
        };
    }

    private void LoadProjectData()
    {
        ConnectEditorEvents();
        RefreshList();
        UpdateEditorState();
        UpdatePronunciationCheck();
    }

    private void ConnectEditorEvents()
    {
        lemmaTextBox.TextChanged += EditorTextChanged;
        posTextBox.TextChanged += EditorTextChanged;
        pronunciationTextBox.TextChanged += EditorTextChanged;
        glossTextBox.TextChanged += EditorTextChanged;
        definitionsTextBox.TextChanged += EditorTextChanged;
        notesTextBox.TextChanged += EditorTextChanged;
        sourceComboBox.TextChanged += EditorTextChanged;
        rootTextBox.TextChanged += EditorTextChanged;
        pronunciationTextBox.TextChanged += (sender, e) => UpdatePronunciationCheck();
    }

    private void EditorTextChanged(object? sender, EventArgs e)
    {
        if (loadingData) return;
        editorDirty = true;
        UpdateEditorState();
    }

    private void RefreshList()
    {
        loadingData = true;
        try
        {
            lexemeListView.BeginUpdate();
            lexemeListView.Items.Clear();
            var query = searchTextBox.Text.Trim();
            foreach (var lexeme in project.Lexicon.Lexemes)
            {
                if (!MatchesSearch(lexeme, query)) continue;
                lexemeListView.Items.Add(CreateListItem(lexeme));
            }
            lexemeListView.EndUpdate();
        }
        finally
        {
            loadingData = false;
        }
    }

    private static bool MatchesSearch(Lexeme lexeme, string query)
    {
        if (query.Length == 0) return true;

        var definition = string.Join(" ", lexeme.Definitions);
        return ContainsIgnoreCase(lexeme.Lemma, query) ||
               ContainsIgnoreCase(lexeme.PartOfSpeech, query) ||
               ContainsIgnoreCase(lexeme.Gloss, query) ||
               ContainsIgnoreCase(lexeme.Pronunciation, query) ||
               ContainsIgnoreCase(definition, query);
    }

    private static bool ContainsIgnoreCase(string source, string query)
    {
        return source.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static ListViewItem CreateListItem(Lexeme lexeme)
    {
        var definition = lexeme.Definitions.Count > 0
            ? lexeme.Definitions[0]
            : lexeme.Gloss;

        ListViewItem item = new(lexeme.Lemma);
        item.SubItems.Add(lexeme.PartOfSpeech);
        item.SubItems.Add(definition);
        item.Tag = lexeme;
        return item;
    }

    private void LexemeList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (loadingData) return;
        if (lexemeListView.SelectedItems.Count == 0) return;
        if (lexemeListView.SelectedItems[0].Tag is not Lexeme selected) return;
        if (ReferenceEquals(selected, currentLexeme)) return;

        //切换词条前先处理未保存的编辑，避免静默丢失。
        if (!ResolvePendingEdit())
        {
            RestoreSelection(currentLexeme);
            return;
        }

        LoadLexemeToEditor(selected);
    }

    private void LoadLexemeToEditor(Lexeme lexeme)
    {
        loadingData = true;
        try
        {
            lemmaTextBox.Text = lexeme.Lemma;
            posTextBox.Text = lexeme.PartOfSpeech;
            pronunciationTextBox.Text = lexeme.Pronunciation;
            glossTextBox.Text = lexeme.Gloss;
            definitionsTextBox.Text = string.Join(Environment.NewLine, lexeme.Definitions);
            sourceComboBox.Text = lexeme.Source;
            rootTextBox.Text = lexeme.Root;
            notesTextBox.Text = lexeme.Notes;
        }
        finally
        {
            loadingData = false;
        }

        currentLexeme = lexeme;
        newMode = false;
        editorDirty = false;
        UpdateEditorState();
    }

    private void NewLexeme()
    {
        if (!ResolvePendingEdit()) return;

        loadingData = true;
        try
        {
            ClearEditor();
            lexemeListView.SelectedItems.Clear();
        }
        finally
        {
            loadingData = false;
        }

        currentLexeme = null;
        newMode = true;
        editorDirty = false;
        UpdateEditorState();
        lemmaTextBox.Focus();
    }

    private void SaveEdit()
    {
        var lemma = lemmaTextBox.Text.Trim();
        if (lemma.Length == 0)
        {
            MessageBox.Show(
                this,
                "请输入词形。\n\nPlease enter a lemma.",
                "Lexicon",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            lemmaTextBox.Focus();
            return;
        }

        Lexeme target;
        if (newMode || currentLexeme == null)
        {
            target = new Lexeme();
            project.Lexicon.Lexemes.Add(target);
        }
        else
        {
            target = currentLexeme;
        }

        target.Lemma = lemma;
        target.PartOfSpeech = posTextBox.Text.Trim();
        target.Pronunciation = pronunciationTextBox.Text.Trim();
        target.Gloss = glossTextBox.Text.Trim();
        target.Definitions = SplitLines(definitionsTextBox.Text);
        target.Source = sourceComboBox.Text.Trim();
        target.Root = rootTextBox.Text.Trim();
        target.Notes = notesTextBox.Text.Trim();
        //发音文本是权威值；token缓存是派生值，只在当前库存能完整分词时保存，否则留空待重新分析。
        var check = PronunciationCheckService.Check(project, target.Pronunciation);
        target.PronunciationTokens = check.Tokenization.Success
            ? check.Tokenization.Tokens.ToList()
            : new();

        projectModified?.Invoke();
        editorDirty = false;
        newMode = false;

        RefreshList();
        LoadLexemeToEditor(target);
        SelectListItem(target);
    }

    private static List<string> SplitLines(string text)
    {
        return text
            .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToList();
    }

    private void CancelEdit()
    {
        if (newMode || currentLexeme == null)
        {
            ClearEditor();
            currentLexeme = null;
            newMode = false;
        }
        else
        {
            LoadLexemeToEditor(currentLexeme);
        }

        editorDirty = false;
        UpdateEditorState();
    }

    private void DeleteLexeme()
    {
        if (newMode || currentLexeme == null) return;

        var confirm = MessageBox.Show(
            this,
            $"确定删除词条 {currentLexeme.Lemma}？\n\nDelete the lexeme {currentLexeme.Lemma}?",
            "Lexicon",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        project.Lexicon.Lexemes.Remove(currentLexeme);
        projectModified?.Invoke();

        loadingData = true;
        try
        {
            ClearEditor();
        }
        finally
        {
            loadingData = false;
        }

        currentLexeme = null;
        newMode = false;
        editorDirty = false;
        RefreshList();
        UpdateEditorState();
    }

    //返回true表示可以继续切换/新建/删除；false表示用户取消。
    private bool ResolvePendingEdit()
    {
        if (!editorDirty) return true;

        var result = MessageBox.Show(
            this,
            "当前词条有未保存的修改。是否保存？\n\n" +
            "The current lexeme has unsaved changes. Save before continuing?",
            "Lexicon",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Cancel) return false;

        if (result == DialogResult.No)
        {
            editorDirty = false;
            return true;
        }

        //保存失败（例如词形为空）时阻止继续。
        var lemma = lemmaTextBox.Text.Trim();
        if (lemma.Length == 0)
        {
            MessageBox.Show(
                this,
                "请输入词形后再保存。\n\nPlease enter a lemma before saving.",
                "Lexicon",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            lemmaTextBox.Focus();
            return false;
        }

        SaveEdit();
        return !editorDirty;
    }

    private void RestoreSelection(Lexeme? lexeme)
    {
        loadingData = true;
        try
        {
            if (lexeme == null)
            {
                lexemeListView.SelectedItems.Clear();
                return;
            }

            SelectListItem(lexeme);
        }
        finally
        {
            loadingData = false;
        }
    }

    private void SelectListItem(Lexeme lexeme)
    {
        foreach (ListViewItem item in lexemeListView.Items)
        {
            if (!ReferenceEquals(item.Tag, lexeme)) continue;
            item.Selected = true;
            item.EnsureVisible();
            return;
        }
    }

    private void ClearEditor()
    {
        lemmaTextBox.Clear();
        posTextBox.Clear();
        pronunciationTextBox.Clear();
        glossTextBox.Clear();
        definitionsTextBox.Clear();
        sourceComboBox.Text = "";
        rootTextBox.Clear();
        notesTextBox.Clear();
    }

    private void UpdateEditorState()
    {
        deleteButton.Enabled = !newMode && currentLexeme != null;
        cancelButton.Enabled = editorDirty || newMode || currentLexeme != null;

        string state;
        if (newMode)
            state = "正在新建词条  Creating a new lexeme.";
        else if (currentLexeme != null)
            state = $"正在编辑：{currentLexeme.Lemma}  Editing: {currentLexeme.Lemma}";
        else
            state = "未选择词条  No lexeme selected.";

        if (editorDirty)
            state += "  （有未保存修改  unsaved changes）";

        statusLabel.Text = state;
    }

    //发音检查是派生结果，只显示、不修改项目、不标记dirty、不弹窗。
    private void UpdatePronunciationCheck()
    {
        var input = pronunciationTextBox.Text.Trim();
        if (input.Length == 0)
        {
            pronunciationCheckLabel.Text = "发音检查  Pronunciation check:  —";
            return;
        }

        var result = PronunciationCheckService.Check(project, input);
        var tokenization = result.Tokenization;

        if (!tokenization.Success)
        {
            pronunciationCheckLabel.Text =
                $"⚠ 未识别第{tokenization.FailureIndex + 1}个字符，无法继续分词。" +
                $" Unrecognized text: {tokenization.RemainingText}";
            return;
        }

        var test = result.Test!;
        var lines = new List<string>
        {
            "✓ 分词  Tokens:  " + string.Join(" | ", tokenization.Tokens)
        };

        var syllabification = test.Syllabification;
        if (!syllabification.Success)
        {
            lines.Add("⚠ 音节划分  Syllabification:  " + syllabification.Message);
        }
        else
        {
            lines.Add("音节划分  Syllabification:  " + FormatSyllabifications(syllabification));
            if (syllabification.WasTruncated)
                lines.Add("⚠ 音节搜索达到上限，可能还存在其他合法分析。");
        }

        foreach (var match in test.WordMatches)
            lines.Add(FormatRuleMatch(match));

        foreach (var assessment in test.SyllableAssessments)
            lines.Add(FormatAssessmentLine(assessment));

        if (test.WordMatches.Count == 0 &&
            test.SyllableAssessments.Count == 0 &&
            syllabification.Success &&
            !syllabification.WasTruncated)
        {
            lines.Add("✓ 未发现当前规则下的音系配列冲突。");
        }

        pronunciationCheckLabel.Text = string.Join("\n", lines);
    }

    private static string FormatSyllabifications(SyllabificationResult result)
    {
        if (result.Analyses.Count == 1)
            return result.Analyses[0].Display;

        const int previewLimit = 3;
        var preview = string.Join(" / ", result.Analyses.Take(previewLimit).Select(a => a.Display));
        return result.Analyses.Count > previewLimit
            ? $"{preview} …（共{result.Analyses.Count}个）"
            : preview;
    }

    private static string FormatRuleMatch(PhonotacticsRuleMatch match)
    {
        var sequence = string.Concat(match.Rule.Phonemes);
        var first = match.StartIndex + 1;
        var last = match.StartIndex + match.Rule.Phonemes.Count;
        var position = first == last ? first.ToString() : $"{first}–{last}";
        return $"⚠ {sequence} — {EnvironmentDisplay(match.Environment)} — 位置 {position}";
    }

    private static string FormatAssessmentLine(SyllableRuleAssessment assessment)
    {
        var sequence = string.Concat(assessment.Rule.Phonemes);
        var environment = EnvironmentDisplay(assessment.Environment);
        var syllables = string.Join(", ", assessment.SyllableNumbers);

        var conclusion = assessment.Conclusion switch
        {
            SyllableAssessmentConclusion.Certain => "所有合法音节划分都会命中",
            SyllableAssessmentConclusion.Partial => "仅部分音节划分命中",
            SyllableAssessmentConclusion.IncompleteAllHit => "已检查候选均命中（搜索未穷尽）",
            SyllableAssessmentConclusion.IncompletePartial => "已检查候选中存在差异（搜索未穷尽）",
            _ => ""
        };

        var mark = assessment.Conclusion is
            SyllableAssessmentConclusion.Certain or
            SyllableAssessmentConclusion.IncompleteAllHit
            ? "⚠"
            : "△";

        return $"{mark} {sequence} — {environment} — {conclusion} — Syllable(s): {syllables}";
    }

    private static string EnvironmentDisplay(PhonemeSequenceEnvironment environment)
    {
        return environment switch
        {
            PhonemeSequenceEnvironment.Anywhere => "任意位置/Anywhere",
            PhonemeSequenceEnvironment.WordInitial => "词首/Word-initial",
            PhonemeSequenceEnvironment.WordFinal => "词尾/Word-final",
            PhonemeSequenceEnvironment.Onset => "声首/Onset",
            PhonemeSequenceEnvironment.Nucleus => "音节核/Nucleus",
            PhonemeSequenceEnvironment.Coda => "韵尾/Coda",
            _ => environment.ToString()
        };
    }
}
