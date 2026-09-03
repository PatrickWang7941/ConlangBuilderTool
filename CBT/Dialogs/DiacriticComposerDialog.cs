using CBT.Data;
using CBT.Services;

namespace CBT.Dialogs;

// 用于给一个基础 IPA 音素添加或编辑一个或多个附加符号。
public sealed class DiacriticComposerDialog : Form
{
    private readonly Button applyButton = new();

    private readonly string baseSymbol;

    private readonly Label baseSymbolLabel = new();

    private readonly Button cancelButton = new();

    private readonly CheckedListBox diacriticList = new();

    private readonly Label previewLabel = new();

    // 打开窗口时已经存在的 Diacritics。
    private readonly List<string> initialDiacritics;

    // 防止初始化勾选项目时触发多余的预览更新。
    private bool isLoadingDiacritics;


    public DiacriticComposerDialog(
        string baseSymbol,
        IEnumerable<string>? selectedDiacritics = null)
    {
        this.baseSymbol =
            IpaComposer.NormalizeSymbol(
                baseSymbol);

        initialDiacritics =
            selectedDiacritics?
                .ToList()
            ?? new List<string>();


        Text =
            "IPA 附加符号  IPA Diacritics";

        StartPosition =
            FormStartPosition.CenterParent;

        FormBorderStyle =
            FormBorderStyle.FixedDialog;

        MaximizeBox =
            false;

        MinimizeBox =
            false;

        ClientSize =
            new Size(
                620,
                560);


        BuildLayout();

        LoadDiacritics();

        UpdatePreview();
    }


    // 用户最后选择的附加符号。
    public List<string> SelectedDiacritics
    {
        get;
        private set;
    } = new();


    // 最终组合后的 IPA。
    public string ResultSymbol
    {
        get;
        private set;
    } = "";


    private void BuildLayout()
    {
        Label baseTitle =
            new()
            {
                Text =
                    "基础音素  Base symbol",

                AutoSize =
                    true,

                Font =
                    new Font(
                        "Microsoft YaHei UI",
                        10,
                        FontStyle.Bold),

                Location =
                    new Point(
                        25,
                        22)
            };


        baseSymbolLabel.Text =
            baseSymbol;

        baseSymbolLabel.AutoSize =
            true;

        baseSymbolLabel.Font =
            new Font(
                "Microsoft YaHei UI",
                22);

        baseSymbolLabel.Location =
            new Point(
                25,
                52);


        Label diacriticTitle =
            new()
            {
                Text =
                    "附加符号  Diacritics",

                AutoSize =
                    true,

                Font =
                    new Font(
                        "Microsoft YaHei UI",
                        10,
                        FontStyle.Bold),

                Location =
                    new Point(
                        25,
                        105)
            };


        diacriticList.Location =
            new Point(
                25,
                135);

        diacriticList.Size =
            new Size(
                570,
                275);

        diacriticList.CheckOnClick =
            true;

        diacriticList.Font =
            new Font(
                "Microsoft YaHei UI",
                11);


        // 使用自定义 Tab 对齐 IPA 符号和名称。
        diacriticList.UseCompatibleTextRendering =
            true;

        diacriticList.UseTabStops =
            false;

        diacriticList.UseCustomTabOffsets =
            true;

        diacriticList.CustomTabOffsets.Clear();

        diacriticList.CustomTabOffsets.Add(
            24);


        diacriticList.ItemCheck +=
            (sender, e) =>
            {
                if (isLoadingDiacritics)
                {
                    return;
                }


                // ItemCheck 发生时 CheckedItems
                // 还没有更新，因此延迟到消息循环下一次执行。
                BeginInvoke(
                    UpdatePreview);
            };


        Label previewTitle =
            new()
            {
                Text =
                    "预览  Preview",

                AutoSize =
                    true,

                Font =
                    new Font(
                        "Microsoft YaHei UI",
                        10,
                        FontStyle.Bold),

                Location =
                    new Point(
                        25,
                        430)
            };


        previewLabel.Text =
            baseSymbol;

        previewLabel.Size =
            new Size(
                300,
                85);

        previewLabel.Location =
            new Point(
                25,
                450);

        previewLabel.Font =
            new Font(
                "Microsoft YaHei UI",
                28);

        previewLabel.TextAlign =
            ContentAlignment.MiddleLeft;

        previewLabel.Padding =
            new Padding(
                10,
                6,
                10,
                6);

        previewLabel.UseCompatibleTextRendering =
            true;


        applyButton.Text =
            "应用  Apply";

        applyButton.Size =
            new Size(
                120,
                36);

        applyButton.Location =
            new Point(
                345,
                505);

        applyButton.Click +=
            ApplyButton_Click;


        cancelButton.Text =
            "取消  Cancel";

        cancelButton.Size =
            new Size(
                120,
                36);

        cancelButton.Location =
            new Point(
                475,
                505);

        cancelButton.DialogResult =
            DialogResult.Cancel;


        AcceptButton =
            applyButton;

        CancelButton =
            cancelButton;


        Controls.Add(
            baseTitle);

        Controls.Add(
            baseSymbolLabel);

        Controls.Add(
            diacriticTitle);

        Controls.Add(
            diacriticList);

        Controls.Add(
            previewTitle);

        Controls.Add(
            previewLabel);

        Controls.Add(
            applyButton);

        Controls.Add(
            cancelButton);
    }


    private void LoadDiacritics()
    {
        isLoadingDiacritics =
            true;


        diacriticList.Items.Clear();


        foreach (
            IpaDiacritic diacritic
            in IpaDiacritics.All)
        {
            DiacriticListItem item =
                new(
                    diacritic);


            bool isSelected =
                initialDiacritics.Any(
                    existing =>
                        IpaComposer.AreEquivalent(
                            existing,
                            diacritic.Symbol));


            diacriticList.Items.Add(
                item,
                isSelected);
        }


        isLoadingDiacritics =
            false;
    }


    private void UpdatePreview()
    {
        List<string> selected =
            GetSelectedDiacritics();


        previewLabel.Text =
            IpaComposer.Compose(
                baseSymbol,
                selected);
    }


    private List<string> GetSelectedDiacritics()
    {
        List<string> selected =
            new();


        foreach (
            object checkedItem
            in diacriticList.CheckedItems)
        {
            if (
                checkedItem
                is DiacriticListItem item)
            {
                selected.Add(
                    item.Diacritic.Symbol);
            }
        }


        return selected;
    }


    private void ApplyButton_Click(
        object? sender,
        EventArgs e)
    {
        SelectedDiacritics =
            GetSelectedDiacritics();


        ResultSymbol =
            IpaComposer.Compose(
                baseSymbol,
                SelectedDiacritics);


        DialogResult =
            DialogResult.OK;

        Close();
    }


    // CheckedListBox 中实际保存的数据对象。
    private sealed class DiacriticListItem
    {
        public DiacriticListItem(
            IpaDiacritic diacritic)
        {
            Diacritic =
                diacritic;
        }


        public IpaDiacritic Diacritic
        {
            get;
        }


        public override string ToString()
        {
            return
                $"{Diacritic.DisplaySymbol}\t" +
                $"{Diacritic.Name}";
        }
    }
}