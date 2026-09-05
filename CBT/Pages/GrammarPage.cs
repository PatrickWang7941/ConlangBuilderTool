using CBT.Models;

namespace CBT.Pages;

//语法描述页：基本语序、形态类型、名词/动词系统与备注，结构化字段与自由说明并存。
public class GrammarPage : UserControl
{
    private readonly ConlangProject project;
    private readonly Action? projectModified;

    private readonly ComboBox wordOrderComboBox = new();
    private readonly TextBox wordOrderNotesTextBox = new();
    private readonly ComboBox morphologyComboBox = new();
    private readonly TextBox morphologyNotesTextBox = new();
    private readonly TextBox nounSystemTextBox = new();
    private readonly TextBox verbSystemTextBox = new();
    private readonly TextBox notesTextBox = new();

    private bool loadingData;

    public GrammarPage(ConlangProject project, Action? projectModified)
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
            ConnectEvents();
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
            Text = "记录语言的基础语法特征。所有栏目都可留空，未填写的部分不强制启用。\n" +
                   "Record the basic grammatical profile. Every section is optional.",
            AutoSize = true,
            MaximumSize = new Size(960, 0),
            Font = new Font("Microsoft YaHei UI", 10),
            Margin = new Padding(0, 0, 0, 20)
        };

        //基本语序
        contentPanel.Controls.Add(BuildSectionTitle("基本语序  Basic Word Order"));
        ConfigureWordOrderComboBox();
        contentPanel.Controls.Add(BuildLabel("主导语序  Dominant order"));
        contentPanel.Controls.Add(wordOrderComboBox);
        contentPanel.Controls.Add(BuildLabel("语序说明  Word order notes"));
        wordOrderNotesTextBox.Multiline = true;
        wordOrderNotesTextBox.Height = 70;
        wordOrderNotesTextBox.ScrollBars = ScrollBars.Vertical;
        wordOrderNotesTextBox.Margin = new Padding(0, 0, 0, 20);
        contentPanel.Controls.Add(wordOrderNotesTextBox);

        //形态类型
        contentPanel.Controls.Add(BuildSectionTitle("形态类型  Morphological Type"));
        ConfigureMorphologyComboBox();
        contentPanel.Controls.Add(BuildLabel("主要倾向  Primary tendency"));
        contentPanel.Controls.Add(morphologyComboBox);
        contentPanel.Controls.Add(BuildLabel("形态说明  Morphology notes"));
        morphologyNotesTextBox.Multiline = true;
        morphologyNotesTextBox.Height = 70;
        morphologyNotesTextBox.ScrollBars = ScrollBars.Vertical;
        morphologyNotesTextBox.Margin = new Padding(0, 0, 0, 20);
        contentPanel.Controls.Add(morphologyNotesTextBox);

        //名词系统
        contentPanel.Controls.Add(BuildSectionTitle("名词系统  Noun System"));
        contentPanel.Controls.Add(BuildLabel("数、格、一致等  Number, case, agreement, etc."));
        nounSystemTextBox.Multiline = true;
        nounSystemTextBox.Height = 110;
        nounSystemTextBox.ScrollBars = ScrollBars.Vertical;
        nounSystemTextBox.Margin = new Padding(0, 0, 0, 20);
        contentPanel.Controls.Add(nounSystemTextBox);

        //动词系统
        contentPanel.Controls.Add(BuildSectionTitle("动词系统  Verb System"));
        contentPanel.Controls.Add(BuildLabel("时、体、态、人称等  Tense, aspect, mood, person, etc."));
        verbSystemTextBox.Multiline = true;
        verbSystemTextBox.Height = 110;
        verbSystemTextBox.ScrollBars = ScrollBars.Vertical;
        verbSystemTextBox.Margin = new Padding(0, 0, 0, 20);
        contentPanel.Controls.Add(verbSystemTextBox);

        //备注
        contentPanel.Controls.Add(BuildSectionTitle("语法备注  Grammar Notes"));
        notesTextBox.Multiline = true;
        notesTextBox.Height = 110;
        notesTextBox.ScrollBars = ScrollBars.Vertical;
        notesTextBox.Margin = new Padding(0, 0, 0, 20);
        contentPanel.Controls.Add(notesTextBox);

        Controls.Add(contentPanel);
    }

    private void ConfigureWordOrderComboBox()
    {
        wordOrderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        wordOrderComboBox.Width = 520;
        wordOrderComboBox.Font = new Font("Microsoft YaHei UI", 11);
        wordOrderComboBox.Margin = new Padding(0, 0, 0, 8);
        wordOrderComboBox.Items.AddRange(new object[]
        {
            new ComboItem("", "未设置  Not set"),
            new ComboItem("SOV", "SOV"),
            new ComboItem("SVO", "SVO"),
            new ComboItem("VSO", "VSO"),
            new ComboItem("VOS", "VOS"),
            new ComboItem("OVS", "OVS"),
            new ComboItem("OSV", "OSV"),
            new ComboItem("Free", "无主导语序  Free"),
            new ComboItem("Other", "其他  Other")
        });
        wordOrderComboBox.SelectedIndex = 0;
    }

    private void ConfigureMorphologyComboBox()
    {
        morphologyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        morphologyComboBox.Width = 520;
        morphologyComboBox.Font = new Font("Microsoft YaHei UI", 11);
        morphologyComboBox.Margin = new Padding(0, 0, 0, 8);
        morphologyComboBox.Items.AddRange(new object[]
        {
            new ComboItem("", "未设置  Not set"),
            new ComboItem("Analytic", "分析语  Analytic"),
            new ComboItem("Isolating", "孤立语  Isolating"),
            new ComboItem("Agglutinative", "黏着语  Agglutinative"),
            new ComboItem("Fusional", "融合语  Fusional"),
            new ComboItem("Polysynthetic", "多式综合语  Polysynthetic"),
            new ComboItem("Mixed", "混合  Mixed"),
            new ComboItem("Other", "其他  Other")
        });
        morphologyComboBox.SelectedIndex = 0;
    }

    private static Label BuildSectionTitle(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 14),
            Margin = new Padding(0, 0, 0, 8)
        };
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
        loadingData = true;
        try
        {
            SelectComboItem(wordOrderComboBox, project.Grammar.BasicWordOrder);
            SelectComboItem(morphologyComboBox, project.Grammar.MorphologicalType);
            wordOrderNotesTextBox.Text = project.Grammar.WordOrderNotes;
            morphologyNotesTextBox.Text = project.Grammar.MorphologyNotes;
            nounSystemTextBox.Text = project.Grammar.NounSystem;
            verbSystemTextBox.Text = project.Grammar.VerbSystem;
            notesTextBox.Text = project.Grammar.Notes;
        }
        finally
        {
            loadingData = false;
        }
    }

    private static void SelectComboItem(ComboBox combo, string value)
    {
        foreach (var item in combo.Items)
        {
            if (item is ComboItem comboItem &&
                string.Equals(comboItem.Value, value, StringComparison.Ordinal))
            {
                combo.SelectedItem = item;
                return;
            }
        }

        //未知值回退到未设置，但保留原始数据直到用户改变选择。
        combo.SelectedIndex = 0;
    }

    private void ConnectEvents()
    {
        wordOrderComboBox.SelectedIndexChanged += (sender, e) =>
        {
            if (loadingData) return;
            project.Grammar.BasicWordOrder = GetSelectedValue(wordOrderComboBox);
            projectModified?.Invoke();
        };

        morphologyComboBox.SelectedIndexChanged += (sender, e) =>
        {
            if (loadingData) return;
            project.Grammar.MorphologicalType = GetSelectedValue(morphologyComboBox);
            projectModified?.Invoke();
        };

        wordOrderNotesTextBox.TextChanged += (sender, e) => SetGrammarField(g => g.WordOrderNotes = wordOrderNotesTextBox.Text);
        morphologyNotesTextBox.TextChanged += (sender, e) => SetGrammarField(g => g.MorphologyNotes = morphologyNotesTextBox.Text);
        nounSystemTextBox.TextChanged += (sender, e) => SetGrammarField(g => g.NounSystem = nounSystemTextBox.Text);
        verbSystemTextBox.TextChanged += (sender, e) => SetGrammarField(g => g.VerbSystem = verbSystemTextBox.Text);
        notesTextBox.TextChanged += (sender, e) => SetGrammarField(g => g.Notes = notesTextBox.Text);
    }

    private void SetGrammarField(Action<GrammarData> setter)
    {
        if (loadingData) return;
        setter(project.Grammar);
        projectModified?.Invoke();
    }

    private static string GetSelectedValue(ComboBox combo)
    {
        return combo.SelectedItem is ComboItem item ? item.Value : "";
    }

    private sealed class ComboItem
    {
        public string Value { get; }
        private string Text { get; }

        public ComboItem(string value, string text)
        {
            Value = value;
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
