using CBT.Dialogs;
using CBT.Models;
using CBT.Services;

namespace CBT.Pages;

//拼写页面：管理音位序列到书写形式的映射，并提供音位→拼写预览。
public class OrthographyPage : UserControl
{
    private readonly ConlangProject project;
    private readonly Action? projectModified;

    private readonly ListView mappingList = new();
    private readonly TextBox graphemeTextBox = new();
    private readonly NumericUpDown priorityNumeric = new();
    private readonly TextBox mappingNotesTextBox = new();
    private readonly Label pendingPhonemesLabel = new();
    private readonly Button selectPhonemesButton = new();
    private readonly Button addButton = new();
    private readonly Button removeButton = new();
    private readonly TextBox previewTextBox = new();
    private readonly Button previewButton = new();
    private readonly Label previewResultLabel = new();

    //待添加映射的音素序列，选择后暂存直到用户点添加。
    private List<string> pendingPhonemes = new();

    public OrthographyPage(ConlangProject project, Action? projectModified)
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
            Text = "定义音位到书写形式的映射，例如 /ʃ/ → sh、/t͡ʃ/ → ch。支持一对多和多对一。\n" +
                   "Define phoneme-to-grapheme mappings, e.g. /ʃ/ → sh. Many-to-one and one-to-many are supported.",
            AutoSize = true,
            MaximumSize = new Size(960, 0),
            Font = new Font("Microsoft YaHei UI", 10),
            Margin = new Padding(0, 0, 0, 20)
        };

        var inputRow = BuildInputRow();
        pendingPhonemesLabel.AutoSize = true;
        pendingPhonemesLabel.Font = new Font("Microsoft YaHei UI", 10);
        pendingPhonemesLabel.Margin = new Padding(0, 0, 0, 8);
        UpdatePendingLabel();

        ConfigureMappingList();
        var removeRow = BuildRemoveRow();

        Label previewTitle = new()
        {
            Text = "拼写预览  Orthography Preview",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 14),
            Margin = new Padding(0, 30, 0, 8)
        };
        var previewRow = BuildPreviewRow();
        previewResultLabel.AutoSize = true;
        previewResultLabel.MaximumSize = new Size(960, 0);
        previewResultLabel.Font = new Font("Microsoft YaHei UI", 10);
        previewResultLabel.Margin = new Padding(0, 0, 0, 20);

        contentPanel.Controls.Add(introduction);
        contentPanel.Controls.Add(inputRow);
        contentPanel.Controls.Add(pendingPhonemesLabel);
        contentPanel.Controls.Add(mappingList);
        contentPanel.Controls.Add(removeRow);
        contentPanel.Controls.Add(previewTitle);
        contentPanel.Controls.Add(previewRow);
        contentPanel.Controls.Add(previewResultLabel);
        Controls.Add(contentPanel);
    }

    private FlowLayoutPanel BuildInputRow()
    {
        FlowLayoutPanel row = new()
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };

        selectPhonemesButton.Text = "选择音素序列  Select phonemes";
        selectPhonemesButton.Width = 180;
        selectPhonemesButton.Height = 34;
        selectPhonemesButton.Font = new Font("Microsoft YaHei UI", 10);
        selectPhonemesButton.Margin = new Padding(0, 3, 8, 0);
        selectPhonemesButton.Click += (sender, e) => SelectPhonemes();

        graphemeTextBox.Width = 200;
        graphemeTextBox.Font = new Font("Microsoft YaHei UI", 11);
        graphemeTextBox.PlaceholderText = "拼写  Grapheme";
        graphemeTextBox.Margin = new Padding(0, 3, 8, 0);

        priorityNumeric.Width = 100;
        priorityNumeric.Font = new Font("Microsoft YaHei UI", 10);
        priorityNumeric.Minimum = 0;
        priorityNumeric.Maximum = 1000;
        priorityNumeric.Value = 0;
        priorityNumeric.Margin = new Padding(0, 3, 8, 0);

        mappingNotesTextBox.Width = 240;
        mappingNotesTextBox.Font = new Font("Microsoft YaHei UI", 11);
        mappingNotesTextBox.PlaceholderText = "备注，可选  Notes, optional";
        mappingNotesTextBox.Margin = new Padding(0, 3, 8, 0);

        addButton.Text = "添加  Add";
        addButton.Width = 110;
        addButton.Height = 34;
        addButton.Font = new Font("Microsoft YaHei UI", 10);
        addButton.Margin = new Padding(0, 3, 0, 0);
        addButton.Click += (sender, e) => AddMapping();

        row.Controls.Add(selectPhonemesButton);
        row.Controls.Add(graphemeTextBox);
        row.Controls.Add(BuildInlineLabel("优先级  Priority"));
        row.Controls.Add(priorityNumeric);
        row.Controls.Add(mappingNotesTextBox);
        row.Controls.Add(addButton);
        return row;
    }

    private static Label BuildInlineLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 10),
            Margin = new Padding(0, 10, 4, 0)
        };
    }

    private FlowLayoutPanel BuildRemoveRow()
    {
        FlowLayoutPanel row = new()
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 0)
        };

        removeButton.Text = "删除选中映射  Remove selected mapping";
        removeButton.Width = 220;
        removeButton.Height = 34;
        removeButton.Font = new Font("Microsoft YaHei UI", 10);
        removeButton.Enabled = false;
        removeButton.Click += (sender, e) => RemoveSelectedMapping();

        row.Controls.Add(removeButton);
        return row;
    }

    private FlowLayoutPanel BuildPreviewRow()
    {
        FlowLayoutPanel row = new()
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };

        previewTextBox.Width = 600;
        previewTextBox.Font = new Font("Microsoft YaHei UI", 11);
        previewTextBox.PlaceholderText = "输入IPA词形  Enter IPA word form";
        previewTextBox.Margin = new Padding(0, 3, 8, 0);
        previewTextBox.KeyDown += (sender, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                RunPreview();
            }
        };

        previewButton.Text = "预览  Preview";
        previewButton.Width = 120;
        previewButton.Height = 34;
        previewButton.Font = new Font("Microsoft YaHei UI", 10);
        previewButton.Margin = new Padding(0, 3, 0, 0);
        previewButton.Click += (sender, e) => RunPreview();

        row.Controls.Add(previewTextBox);
        row.Controls.Add(previewButton);
        return row;
    }

    private void ConfigureMappingList()
    {
        mappingList.Width = 960;
        mappingList.Height = 280;
        mappingList.View = View.Details;
        mappingList.FullRowSelect = true;
        mappingList.MultiSelect = false;
        mappingList.GridLines = true;
        mappingList.HideSelection = false;
        mappingList.Font = new Font("Microsoft YaHei UI", 10);
        mappingList.Margin = new Padding(0);
        mappingList.Columns.Add("音素序列  Phonemes", 180);
        mappingList.Columns.Add("拼写  Grapheme", 180);
        mappingList.Columns.Add("优先级  Priority", 100);
        mappingList.Columns.Add("备注  Notes", 480);
        mappingList.SelectedIndexChanged += (sender, e) =>
            removeButton.Enabled = mappingList.SelectedItems.Count > 0;
    }

    private void LoadProjectData()
    {
        RefreshList();
    }

    private void RefreshList()
    {
        mappingList.BeginUpdate();
        mappingList.Items.Clear();
        foreach (var mapping in project.Orthography.Mappings)
            mappingList.Items.Add(CreateListItem(mapping));
        mappingList.EndUpdate();
    }

    private static ListViewItem CreateListItem(OrthographyMapping mapping)
    {
        ListViewItem item = new(string.Concat(mapping.Phonemes));
        item.SubItems.Add(mapping.Grapheme);
        item.SubItems.Add(mapping.Priority.ToString());
        item.SubItems.Add(mapping.Notes);
        item.Tag = mapping;
        return item;
    }

    private void SelectPhonemes()
    {
        using PhonemeSequenceDialog dialog = new(project, PhonemeSequenceKind.Orthography);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        pendingPhonemes = dialog.SelectedPhonemes.ToList();
        UpdatePendingLabel();
    }

    private void UpdatePendingLabel()
    {
        pendingPhonemesLabel.Text = pendingPhonemes.Count == 0
            ? "待添加音素序列  Pending sequence:  —"
            : $"待添加音素序列  Pending sequence:  {string.Concat(pendingPhonemes)}";
    }

    private void AddMapping()
    {
        if (pendingPhonemes.Count == 0)
        {
            MessageBox.Show(
                this,
                "请先选择音素序列。\n\nPlease select a phoneme sequence first.",
                "Orthography",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var grapheme = graphemeTextBox.Text.Trim();
        if (grapheme.Length == 0)
        {
            MessageBox.Show(
                this,
                "请输入拼写形式。\n\nPlease enter a grapheme.",
                "Orthography",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            graphemeTextBox.Focus();
            return;
        }

        //同一音素序列映射到不同拼写时提示冲突，但不强制禁止。
        var duplicate = project.Orthography.Mappings.Any(existing =>
            existing.Phonemes.Count == pendingPhonemes.Count &&
            existing.Phonemes.Zip(pendingPhonemes).All(pair =>
                IpaComposer.AreEquivalent(pair.First, pair.Second)));

        if (duplicate)
        {
            var confirm = MessageBox.Show(
                this,
                "该音素序列已存在映射。仍要添加吗？\n\n" +
                "A mapping for this phoneme sequence already exists. Add anyway?",
                "Orthography",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;
        }

        OrthographyMapping mapping = new()
        {
            Phonemes = new List<string>(pendingPhonemes),
            Grapheme = grapheme,
            Priority = (int)priorityNumeric.Value,
            Notes = mappingNotesTextBox.Text.Trim()
        };

        project.Orthography.Mappings.Add(mapping);
        mappingList.Items.Add(CreateListItem(mapping));

        pendingPhonemes = new();
        graphemeTextBox.Clear();
        mappingNotesTextBox.Clear();
        priorityNumeric.Value = 0;
        UpdatePendingLabel();
        projectModified?.Invoke();
    }

    private void RemoveSelectedMapping()
    {
        if (mappingList.SelectedItems.Count == 0) return;
        if (mappingList.SelectedItems[0].Tag is not OrthographyMapping mapping) return;

        project.Orthography.Mappings.Remove(mapping);
        mappingList.Items.Remove(mappingList.SelectedItems[0]);
        projectModified?.Invoke();
    }

    private void RunPreview()
    {
        var input = previewTextBox.Text.Trim();
        if (input.Length == 0)
        {
            previewResultLabel.Text = "请输入要预览的IPA词形。\nPlease enter an IPA word form to preview.";
            return;
        }

        var tokenization = PhonemeTokenizerService.Tokenize(project, input);
        if (!tokenization.Success)
        {
            previewResultLabel.Text =
                $"⚠ 未识别第{tokenization.FailureIndex + 1}个字符，无法分词。" +
                $" Unrecognized text: {tokenization.RemainingText}";
            return;
        }

        var result = OrthographyService.Preview(
            tokenization.Tokens,
            project.Orthography);

        var lines = new List<string>
        {
            $"分词  Tokens:  {string.Join(" | ", tokenization.Tokens)}",
            $"拼写预览  Grapheme:  {(result.Grapheme.Length == 0 ? "—" : result.Grapheme)}"
        };

        if (result.IsComplete)
        {
            lines.Add("✓ 所有音素都有对应映射。All phonemes are mapped.");
        }
        else
        {
            lines.Add("⚠ 以下音素没有对应映射：" + string.Join(" | ", result.UnmappedPhonemes) +
                      "  These phonemes have no mapping: " + string.Join(" | ", result.UnmappedPhonemes));
        }

        previewResultLabel.Text = string.Join("\n", lines);
    }
}
