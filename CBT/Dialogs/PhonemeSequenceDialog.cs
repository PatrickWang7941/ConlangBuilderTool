using CBT.Models;

namespace CBT.Dialogs;

public enum PhonemeSequenceKind
{
    Onset,
    Nucleus,
    Coda,
    Forbidden
}

public class PhonemeSequenceDialog : Form
{
    private readonly ListBox inventoryList = new();
    private readonly ListBox sequenceList = new();
    private readonly Label previewLabel = new();
    private readonly ComboBox environmentComboBox = new();

    private readonly ConlangProject project;
    private readonly PhonemeSequenceKind kind;

    public IReadOnlyList<string> SelectedPhonemes =>
        sequenceList.Items
            .OfType<PhonemeItem>()
            .Select(x => x.Symbol)
            .ToList();

    public PhonemeSequenceEnvironment SelectedEnvironment =>
        environmentComboBox.SelectedItem is EnvironmentItem item
            ? item.Environment
            : PhonemeSequenceEnvironment.Anywhere;

    public PhonemeSequenceDialog(ConlangProject project, PhonemeSequenceKind kind)
    {
        this.project = project;
        this.kind = kind;

        Text = GetWindowTitle();
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(840, 540);
        MinimumSize = new Size(780, 500);
        Font = new Font("Microsoft YaHei UI", 10);

        BuildLayout();
        LoadInventory();
        UpdatePreview();
    }

    private void BuildLayout()
    {
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(20)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));

        Label inventoryTitle = new()
        {
            Text = "可用音素  Inventory",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Microsoft YaHei UI", 11)
        };

        Label sequenceTitle = new()
        {
            Text = "当前序列  Sequence",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Microsoft YaHei UI", 11)
        };

        inventoryList.Dock = DockStyle.Fill;
        inventoryList.Font = new Font("Microsoft YaHei UI", 11);
        inventoryList.HorizontalScrollbar = true;
        inventoryList.DoubleClick += (sender, e) => AddSelectedPhoneme();

        sequenceList.Dock = DockStyle.Fill;
        sequenceList.Font = new Font("Microsoft YaHei UI", 14);
        sequenceList.HorizontalScrollbar = true;
        sequenceList.DoubleClick += (sender, e) => RemoveSelectedPhoneme();

        FlowLayoutPanel actionPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10, 40, 10, 0)
        };

        Button addButton = CreateSmallButton("→");
        Button moveUpButton = CreateSmallButton("↑");
        Button moveDownButton = CreateSmallButton("↓");
        Button removeButton = CreateSmallButton("←");

        addButton.Click += (sender, e) => AddSelectedPhoneme();
        moveUpButton.Click += (sender, e) => MoveSelectedPhoneme(-1);
        moveDownButton.Click += (sender, e) => MoveSelectedPhoneme(1);
        removeButton.Click += (sender, e) => RemoveSelectedPhoneme();

        actionPanel.Controls.Add(addButton);
        actionPanel.Controls.Add(moveUpButton);
        actionPanel.Controls.Add(moveDownButton);
        actionPanel.Controls.Add(removeButton);

        FlowLayoutPanel bottomPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        previewLabel.Width = kind == PhonemeSequenceKind.Forbidden ? 230 : 450;
        previewLabel.Height = 50;
        previewLabel.TextAlign = ContentAlignment.MiddleLeft;
        previewLabel.Font = new Font("Microsoft YaHei UI", 11);

        bottomPanel.Controls.Add(previewLabel);

        if (kind == PhonemeSequenceKind.Forbidden)
            ConfigureEnvironmentSelector(bottomPanel);

        Button okButton = new()
        {
            Text = "确定  OK",
            Width = 105,
            Height = 36,
            Margin = new Padding(10, 8, 8, 0)
        };

        Button cancelButton = new()
        {
            Text = "取消  Cancel",
            Width = 105,
            Height = 36,
            Margin = new Padding(0, 8, 0, 0),
            DialogResult = DialogResult.Cancel
        };

        okButton.Click += ConfirmSelection;

        bottomPanel.Controls.Add(okButton);
        bottomPanel.Controls.Add(cancelButton);

        layout.Controls.Add(inventoryTitle, 0, 0);
        layout.Controls.Add(sequenceTitle, 2, 0);
        layout.Controls.Add(inventoryList, 0, 1);
        layout.Controls.Add(actionPanel, 1, 1);
        layout.Controls.Add(sequenceList, 2, 1);
        layout.Controls.Add(bottomPanel, 0, 2);
        layout.SetColumnSpan(bottomPanel, 3);

        Controls.Add(layout);

        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    private void ConfigureEnvironmentSelector(FlowLayoutPanel panel)
    {
        Label label = new()
        {
            Text = "环境  Environment",
            AutoSize = true,
            Margin = new Padding(8, 16, 6, 0)
        };

        environmentComboBox.Width = 170;
        environmentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        environmentComboBox.Font = new Font("Microsoft YaHei UI", 9);
        environmentComboBox.Margin = new Padding(0, 10, 0, 0);

        environmentComboBox.Items.Add(new EnvironmentItem(
            PhonemeSequenceEnvironment.Anywhere,
            "任意位置  Anywhere"));
        environmentComboBox.Items.Add(new EnvironmentItem(
            PhonemeSequenceEnvironment.WordInitial,
            "词首  Word-initial"));
        environmentComboBox.Items.Add(new EnvironmentItem(
            PhonemeSequenceEnvironment.WordFinal,
            "词尾  Word-final"));
        environmentComboBox.Items.Add(new EnvironmentItem(
            PhonemeSequenceEnvironment.Onset,
            "声首  Onset"));
        environmentComboBox.Items.Add(new EnvironmentItem(
            PhonemeSequenceEnvironment.Nucleus,
            "音节核  Nucleus"));
        environmentComboBox.Items.Add(new EnvironmentItem(
            PhonemeSequenceEnvironment.Coda,
            "韵尾  Coda"));
        environmentComboBox.SelectedIndex = 0;

        panel.Controls.Add(label);
        panel.Controls.Add(environmentComboBox);
    }

    private static Button CreateSmallButton(string text)
    {
        return new Button
        {
            Text = text,
            Width = 60,
            Height = 38,
            Margin = new Padding(5)
        };
    }

    private void LoadInventory()
    {
        inventoryList.Items.Clear();

        if (kind == PhonemeSequenceKind.Nucleus)
        {
            foreach (var vowel in project.Phonology.Vowels)
                inventoryList.Items.Add(new PhonemeItem(vowel.Symbol, DescribeVowel(vowel)));

            //带Syllabic标记的辅音也可以作为nucleus。
            foreach (var consonant in project.Phonology.Consonants.Where(IsSyllabicConsonant))
                inventoryList.Items.Add(new PhonemeItem(consonant.Symbol, DescribeConsonant(consonant)));

            return;
        }

        if (kind == PhonemeSequenceKind.Forbidden)
        {
            //禁止序列允许跨越辅音和元音类别。
            foreach (var consonant in project.Phonology.Consonants)
                inventoryList.Items.Add(new PhonemeItem(consonant.Symbol, DescribeConsonant(consonant)));

            foreach (var vowel in project.Phonology.Vowels)
                inventoryList.Items.Add(new PhonemeItem(vowel.Symbol, DescribeVowel(vowel)));

            return;
        }

        foreach (var consonant in project.Phonology.Consonants)
            inventoryList.Items.Add(new PhonemeItem(consonant.Symbol, DescribeConsonant(consonant)));
    }

    private static bool IsSyllabicConsonant(ConsonantPhoneme consonant)
    {
        //U+0329是IPA的Syllabic combining mark。
        return consonant.Diacritics?.Contains("\u0329") == true;
    }

    private static string DescribeConsonant(ConsonantPhoneme consonant)
    {
        if (!string.IsNullOrWhiteSpace(consonant.Description))
            return consonant.Description;

        string[] parts =
        [
            consonant.Manner,
            consonant.Place,
            consonant.Voicing
        ];

        return string.Join(" · ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string DescribeVowel(VowelPhoneme vowel)
    {
        string[] parts =
        [
            vowel.Height,
            vowel.Backness,
            vowel.Roundedness
        ];

        return string.Join(" · ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private void AddSelectedPhoneme()
    {
        if (inventoryList.SelectedItem is not PhonemeItem item) return;

        //同一个音素可以重复出现在序列中。
        sequenceList.Items.Add(new PhonemeItem(item.Symbol, item.Description));
        sequenceList.SelectedIndex = sequenceList.Items.Count - 1;

        UpdatePreview();
    }

    private void RemoveSelectedPhoneme()
    {
        var index = sequenceList.SelectedIndex;
        if (index < 0) return;

        sequenceList.Items.RemoveAt(index);

        if (sequenceList.Items.Count > 0)
            sequenceList.SelectedIndex = Math.Min(index, sequenceList.Items.Count - 1);

        UpdatePreview();
    }

    private void MoveSelectedPhoneme(int offset)
    {
        var index = sequenceList.SelectedIndex;
        if (index < 0) return;

        var newIndex = index + offset;
        if (newIndex < 0 || newIndex >= sequenceList.Items.Count) return;

        var item = sequenceList.Items[index];
        sequenceList.Items.RemoveAt(index);
        sequenceList.Items.Insert(newIndex, item);
        sequenceList.SelectedIndex = newIndex;

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        var symbol = string.Concat(
            sequenceList.Items
                .OfType<PhonemeItem>()
                .Select(x => x.Symbol));

        previewLabel.Text = symbol.Length == 0
            ? "预览  Preview:  —"
            : $"预览  Preview:  {symbol}";
    }

    private void ConfirmSelection(object? sender, EventArgs e)
    {
        if (sequenceList.Items.Count == 0)
        {
            MessageBox.Show(
                this,
                "请至少选择一个音素。\n\nPlease select at least one phoneme.",
                "Phonotactics",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private string GetWindowTitle()
    {
        return kind switch
        {
            PhonemeSequenceKind.Onset => "添加声首序列  Add Onset Sequence",
            PhonemeSequenceKind.Nucleus => "添加音节核序列  Add Nucleus Sequence",
            PhonemeSequenceKind.Coda => "添加韵尾序列  Add Coda Sequence",
            PhonemeSequenceKind.Forbidden => "添加禁止序列  Add Forbidden Sequence",
            _ => "Phoneme Sequence"
        };
    }

    private sealed class EnvironmentItem
    {
        public PhonemeSequenceEnvironment Environment { get; }
        private string Text { get; }

        public EnvironmentItem(PhonemeSequenceEnvironment environment, string text)
        {
            Environment = environment;
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    private sealed class PhonemeItem
    {
        public string Symbol { get; }
        public string Description { get; }

        public PhonemeItem(string symbol, string description)
        {
            Symbol = symbol;
            Description = description;
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Description)
                ? Symbol
                : $"{Symbol}   {Description}";
        }
    }
}
