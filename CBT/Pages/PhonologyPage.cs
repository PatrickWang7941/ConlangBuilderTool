using CBT.Data;
using CBT.Models;

namespace CBT.Pages;
//————————————————————以下代码由AI辅助整理为更整洁的布局，包括统一换行和修改注释格式，以便以后的修改。——————————————————————
public class PhonologyPage : UserControl
{
    // 音素输入与操作控件
    private readonly TextBox phonemeInput = new();
    private readonly ComboBox phonemeType = new();
    private readonly Button addPhonemeButton = new();
    private readonly Button removePhonemeButton = new();
    private readonly Label noMatchingPhonemeLabel = new();

    // IPA 选择控件
    private readonly Button selectionModeButton = new();
    private readonly ComboBox ipaSymbolPicker = new();
    private readonly ComboBox ipaCategory = new();
    private readonly ComboBox ipaChoice = new();

    // 辅音控件
    private readonly ComboBox consonantPlace = new();
    private readonly ComboBox consonantManner = new();
    private readonly ComboBox consonantVoicing = new();
    private readonly ListBox consonantList = new();

    // 普通辅音表
    private readonly TableLayoutPanel consonantChart = new();

    private readonly Dictionary<
        (string Place, string Manner, string Voicing),
        Label> consonantChartCells = new();

    // 非肺部气流辅音表
    private readonly TableLayoutPanel nonPulmonicChart = new();

    private readonly Dictionary<string, Label>
        nonPulmonicChartCells = new();

    // Other IPA Symbols 显示区域
    private readonly Label otherSymbolsContent = new();

    // 元音控件
    private readonly ComboBox vowelHeight = new();
    private readonly ComboBox vowelBackness = new();
    private readonly ComboBox vowelRoundedness = new();
    private readonly ListBox vowelList = new();

    // 元音图
    private readonly Panel vowelChart = new();

    private readonly Dictionary<
        (string Height, string Backness, string Roundedness),
        Label> vowelChartCells = new();

    private readonly ConlangProject project;

    // 当音系数据发生修改时通知主窗口。
    private readonly Action? projectModified;

    private SelectionMode selectionMode =
        SelectionMode.Detailed;

    private bool isSynchronizingSelection;

    private int lastIpaSymbolIndex = -1;


    private enum SelectionMode
    {
        Detailed,
        Guided,
        List
    }


    // 下拉栏中的显示项目。
    private class IpaDisplayItem
    {
        public string Text { get; }

        public IpaConsonant? Consonant { get; }

        public IpaNonPulmonicConsonant?
            NonPulmonicConsonant
        { get; }

        public IpaOtherSymbol?
            OtherSymbol
        { get; }

        public IpaVowel? Vowel { get; }


        // 分组标题。
        public IpaDisplayItem(
            string text)
        {
            Text = text;
        }


        // 普通辅音。
        public IpaDisplayItem(
            string text,
            IpaConsonant consonant)
        {
            Text = text;
            Consonant = consonant;
        }


        // 非肺部气流辅音。
        public IpaDisplayItem(
            string text,
            IpaNonPulmonicConsonant consonant)
        {
            Text = text;
            NonPulmonicConsonant = consonant;
        }


        // Other IPA Symbol。
        public IpaDisplayItem(
            string text,
            IpaOtherSymbol symbol)
        {
            Text = text;
            OtherSymbol = symbol;
        }


        // 元音。
        public IpaDisplayItem(
            string text,
            IpaVowel vowel)
        {
            Text = text;
            Vowel = vowel;
        }


        public override string ToString()
        {
            return Text;
        }
    }


    private class IpaCategoryItem
    {
        public string Manner { get; }

        public string Text { get; }


        public IpaCategoryItem(
            string manner,
            string text)
        {
            Manner = manner;
            Text = text;
        }


        public override string ToString()
        {
            return Text;
        }
    }


    private class IpaVowelCategoryItem
    {
        public string Height { get; }

        public string Text { get; }


        public IpaVowelCategoryItem(
            string height,
            string text)
        {
            Height = height;
            Text = text;
        }


        public override string ToString()
        {
            return Text;
        }
    }


    // 项目辅音在左侧清单中的显示形式。
    private class ConsonantEntry
    {
        public string Symbol { get; set; } = "";

        public string Place { get; set; } = "";

        public string Manner { get; set; } = "";

        public string Voicing { get; set; } = "";

        public string Category { get; set; } = "";

        public string Description { get; set; } = "";


        public override string ToString()
        {
            // 非肺部辅音和 Other Symbols
            // 使用 Description 显示。
            if (!string.IsNullOrWhiteSpace(
                Description))
            {
                return
                    $"{Symbol}    " +
                    $"{GetChinesePart(Description)}";
            }

            string place =
                Place.Split("  ")[0];

            string manner =
                Manner.Split("  ")[0];

            string voicing =
                Voicing.StartsWith("清音")
                    ? "清"
                    : "浊";

            return
                $"{Symbol}    " +
                $"{place}{voicing}{manner}";
        }
    }


    // 项目元音在清单中的显示形式。
    private class VowelEntry
    {
        public string Symbol { get; set; } = "";

        public string Height { get; set; } = "";

        public string Backness { get; set; } = "";

        public string Roundedness { get; set; } = "";


        public override string ToString()
        {
            string height =
                GetChinesePart(Height);

            string backness =
                GetChinesePart(Backness);

            string roundedness =
                GetChinesePart(Roundedness);

            return
                $"{Symbol}    " +
                $"{roundedness}{backness}{height}";
        }
    }


    public PhonologyPage()
        : this(
            new ConlangProject(),
            null)
    {
    }


    public PhonologyPage(
        ConlangProject project,
        Action? projectModified)
    {
        this.project = project;
        this.projectModified =
            projectModified;

        Dock = DockStyle.Fill;

        Padding =
            new Padding(
                30,
                0,
                30,
                30);

        FlowLayoutPanel contentPanel =
            new()
            {
                Dock =
                    DockStyle.Fill,

                FlowDirection =
                    FlowDirection.TopDown,

                WrapContents =
                    false,

                AutoScroll =
                    true,

                Padding =
                    new Padding(0)
            };

        contentPanel.Controls.Add(
            BuildPhonemeInventory());

        Controls.Add(
            contentPanel);

        LoadProjectPhonology();
    }


    // 创建整个 Phonology 页面。
    private Control BuildPhonemeInventory()
    {
        FlowLayoutPanel section =
            new()
            {
                FlowDirection =
                    FlowDirection.TopDown,

                WrapContents =
                    false,

                AutoSize =
                    true,

                Margin =
                    new Padding(0)
            };


        Label sectionTitle =
            new()
            {
                Text =
                    "音素清单  Phoneme Inventory",

                AutoSize =
                    true,

                Font =
                    new Font(
                        "Microsoft YaHei UI",
                        14)
            };


        FlowLayoutPanel sectionHeader =
            new()
            {
                FlowDirection =
                    FlowDirection.LeftToRight,

                AutoSize =
                    true
            };


        FlowLayoutPanel inputRow =
            new()
            {
                FlowDirection =
                    FlowDirection.LeftToRight,

                AutoSize =
                    true,

                Margin =
                    new Padding(
                        0,
                        10,
                        0,
                        10)
            };


        Label consonantTitle =
            new()
            {
                Text =
                    "辅音  Consonants",

                AutoSize =
                    true
            };


        Label vowelTitle =
            new()
            {
                Text =
                    "元音  Vowels",

                AutoSize =
                    true
            };


        TableLayoutPanel phonemeLists =
            new()
            {
                ColumnCount =
                    3,

                RowCount =
                    2,

                AutoSize =
                    true,

                AutoSizeMode =
                    AutoSizeMode.GrowAndShrink,

                GrowStyle =
                    TableLayoutPanelGrowStyle.FixedSize,

                Margin =
                    new Padding(0)
            };


        phonemeLists.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                725));

        phonemeLists.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                30));

        phonemeLists.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                725));

        phonemeLists.RowStyles.Add(
            new RowStyle(
                SizeType.AutoSize));

        phonemeLists.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                180));


        Panel bottomSpacer =
            new()
            {
                Size =
                    new Size(
                        1,
                        60),

                Margin =
                    new Padding(0)
            };


        ConfigureSelectionModeButton();
        ConfigureNoMatchingPhonemeLabel();
        ConfigureIpaSymbolPicker();
        ConfigurePhonemeType();
        ConfigureGuidedIpaPickers();
        ConfigureConsonantFeatureSelectors();
        ConfigureVowelFeatureSelectors();
        ConfigurePhonemeInput();
        ConfigureActionButtons();
        ConfigurePhonemeLists();


        sectionHeader.Controls.Add(
            sectionTitle);

        sectionHeader.Controls.Add(
            selectionModeButton);


        inputRow.Controls.Add(
            phonemeInput);

        inputRow.Controls.Add(
            phonemeType);

        inputRow.Controls.Add(
            ipaSymbolPicker);

        inputRow.Controls.Add(
            consonantPlace);

        inputRow.Controls.Add(
            consonantManner);

        inputRow.Controls.Add(
            consonantVoicing);

        inputRow.Controls.Add(
            vowelHeight);

        inputRow.Controls.Add(
            vowelBackness);

        inputRow.Controls.Add(
            vowelRoundedness);

        inputRow.Controls.Add(
            ipaCategory);

        inputRow.Controls.Add(
            ipaChoice);

        inputRow.Controls.Add(
            addPhonemeButton);

        inputRow.Controls.Add(
            removePhonemeButton);

        inputRow.Controls.Add(
            noMatchingPhonemeLabel);


        UpdateSelectionMode();
        UpdateSymbolFromFeatures();


        consonantTitle.Margin =
            new Padding(
                0,
                0,
                0,
                3);

        vowelTitle.Margin =
            new Padding(
                0,
                0,
                0,
                3);

        consonantList.Margin =
            new Padding(0);

        vowelList.Margin =
            new Padding(0);


        phonemeLists.Controls.Add(
            consonantTitle,
            0,
            0);

        phonemeLists.Controls.Add(
            vowelTitle,
            2,
            0);

        phonemeLists.Controls.Add(
            consonantList,
            0,
            1);

        phonemeLists.Controls.Add(
            vowelList,
            2,
            1);


        section.Controls.Add(
            sectionHeader);

        section.Controls.Add(
            inputRow);

        section.Controls.Add(
            phonemeLists);

        section.Controls.Add(
            BuildConsonantChart());

        section.Controls.Add(
            BuildLowerChartsRow());

        section.Controls.Add(
            bottomSpacer);


        return section;
    }


    // 切换 Detailed / Guided / List。
    private void ConfigureSelectionModeButton()
    {
        selectionModeButton.Text =
            "切换到引导模式";

        selectionModeButton.AutoSize =
            true;

        selectionModeButton.Font =
            new Font(
                "Microsoft YaHei UI",
                9);

        selectionModeButton.Margin =
            new Padding(
                20,
                0,
                0,
                0);

        selectionModeButton.Click +=
            (sender, e) =>
            {
                selectionMode =
                    selectionMode switch
                    {
                        SelectionMode.Detailed =>
                            SelectionMode.Guided,

                        SelectionMode.Guided =>
                            SelectionMode.List,

                        _ =>
                            SelectionMode.Detailed
                    };

                UpdateSelectionMode();
            };
    }


    // List mode 下拉栏。
    private void ConfigureIpaSymbolPicker()
    {
        ipaSymbolPicker.DropDownStyle =
            ComboBoxStyle.DropDownList;

        ipaSymbolPicker.Width =
            250;

        ipaSymbolPicker.DropDownWidth =
            560;

        ipaSymbolPicker.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        ipaSymbolPicker.Margin =
            new Padding(
                0,
                3,
                6,
                0);


        LoadDetailedIpaPicker();


        ipaSymbolPicker.SelectedIndexChanged +=
            (sender, e) =>
            {
                if (
                    isSynchronizingSelection ||
                    ipaSymbolPicker.SelectedItem
                        is not IpaDisplayItem selectedItem)
                {
                    return;
                }


                // 分组标题不能作为音素选择。
                if (
                    selectedItem.Consonant == null &&
                    selectedItem.NonPulmonicConsonant == null &&
                    selectedItem.OtherSymbol == null &&
                    selectedItem.Vowel == null)
                {
                    isSynchronizingSelection =
                        true;

                    ipaSymbolPicker.SelectedIndex =
                        lastIpaSymbolIndex;

                    isSynchronizingSelection =
                        false;

                    return;
                }


                lastIpaSymbolIndex =
                    ipaSymbolPicker.SelectedIndex;


                if (
                    selectedItem.Consonant != null)
                {
                    ApplyConsonant(
                        selectedItem.Consonant);

                    return;
                }


                if (
                    selectedItem.NonPulmonicConsonant != null)
                {
                    ApplyNonPulmonicConsonant(
                        selectedItem.NonPulmonicConsonant);

                    return;
                }


                if (
                    selectedItem.OtherSymbol != null)
                {
                    ApplyOtherSymbol(
                        selectedItem.OtherSymbol);

                    return;
                }


                if (
                    selectedItem.Vowel != null)
                {
                    ApplyVowel(
                        selectedItem.Vowel);
                }
            };
    }


    private void ConfigurePhonemeType()
    {
        phonemeType.Items.Add(
            "辅音  Consonant");

        phonemeType.Items.Add(
            "元音  Vowel");

        phonemeType.SelectedIndex =
            0;

        phonemeType.DropDownStyle =
            ComboBoxStyle.DropDownList;

        phonemeType.Width =
            160;

        phonemeType.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        phonemeType.Margin =
            new Padding(
                0,
                3,
                6,
                0);

        phonemeType.SelectedIndexChanged +=
            (sender, e) =>
                UpdateSelectionMode();
    }


    private void ConfigureGuidedIpaPickers()
    {
        ipaCategory.DropDownStyle =
            ComboBoxStyle.DropDownList;

        ipaCategory.Width =
            220;

        ipaCategory.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        ipaCategory.Margin =
            new Padding(
                0,
                3,
                6,
                0);


        ipaChoice.DropDownStyle =
            ComboBoxStyle.DropDownList;

        ipaChoice.Width =
            240;

        ipaChoice.DropDownWidth =
            560;

        ipaChoice.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        ipaChoice.Margin =
            new Padding(
                0,
                3,
                6,
                0);


        LoadConsonantCategories();


        ipaCategory.SelectedIndexChanged +=
            (sender, e) =>
            {
                if (
                    isSynchronizingSelection)
                {
                    return;
                }


                if (
                    ipaCategory.SelectedItem
                        is IpaCategoryItem consonantCategory)
                {
                    PopulateGuidedChoices(
                        consonantCategory.Manner);

                    return;
                }


                if (
                    ipaCategory.SelectedItem
                        is IpaVowelCategoryItem vowelCategory)
                {
                    PopulateGuidedVowelChoices(
                        vowelCategory.Height);
                }
            };


        ipaChoice.SelectedIndexChanged +=
            (sender, e) =>
            {
                if (
                    isSynchronizingSelection ||
                    ipaChoice.SelectedItem
                        is not IpaDisplayItem selectedItem)
                {
                    return;
                }


                if (
                    selectedItem.Consonant != null)
                {
                    ApplyConsonant(
                        selectedItem.Consonant);

                    return;
                }


                if (
                    selectedItem.Vowel != null)
                {
                    ApplyVowel(
                        selectedItem.Vowel);
                }
            };
    }


    private void ConfigureConsonantFeatureSelectors()
    {
        consonantPlace.Items.AddRange(
            new object[]
            {
                "双唇  Bilabial",
                "唇齿  Labiodental",
                "齿  Dental",
                "齿龈  Alveolar",
                "龈后  Postalveolar",
                "龈腭  Alveolo-palatal",
                "卷舌  Retroflex",
                "硬腭  Palatal",
                "唇硬腭  Labial-palatal",
                "软腭  Velar",
                "小舌  Uvular",
                "咽  Pharyngeal",
                "会厌  Epiglottal",
                "声门  Glottal",
                "唇软腭  Labial-velar"
            });


        consonantPlace.DropDownStyle =
            ComboBoxStyle.DropDownList;

        consonantPlace.Width =
            180;

        consonantPlace.SelectedIndex =
            0;

        consonantPlace.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        consonantPlace.Margin =
            new Padding(
                0,
                3,
                6,
                0);


        consonantManner.Items.AddRange(
            new object[]
            {
                "塞音  Plosive",
                "鼻音  Nasal",
                "颤音  Trill",
                "闪音  Tap / Flap",
                "边闪音  Lateral flap",
                "擦音  Fricative",
                "边擦音  Lateral fricative",
                "近音  Approximant",
                "边近音  Lateral approximant",
                "塞擦音  Affricate"
            });


        consonantManner.DropDownStyle =
            ComboBoxStyle.DropDownList;

        consonantManner.Width =
            190;

        consonantManner.SelectedIndex =
            0;

        consonantManner.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        consonantManner.Margin =
            new Padding(
                0,
                3,
                6,
                0);


        consonantVoicing.Items.AddRange(
            new object[]
            {
                "清音  Voiceless",
                "浊音  Voiced"
            });


        consonantPlace.SelectedIndexChanged +=
            (sender, e) =>
                UpdateSymbolFromFeatures();

        consonantManner.SelectedIndexChanged +=
            (sender, e) =>
                UpdateSymbolFromFeatures();

        consonantVoicing.SelectedIndexChanged +=
            (sender, e) =>
                UpdateSymbolFromFeatures();


        consonantVoicing.DropDownStyle =
            ComboBoxStyle.DropDownList;

        consonantVoicing.Width =
            150;

        consonantVoicing.SelectedIndex =
            0;

        consonantVoicing.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        consonantVoicing.Margin =
            new Padding(
                0,
                3,
                6,
                0);
    }


    private void ConfigureVowelFeatureSelectors()
    {
        vowelHeight.Items.AddRange(
            new object[]
            {
                "闭  Close",
                "近闭  Near-close",
                "半闭  Close-mid",
                "中  Mid",
                "半开  Open-mid",
                "近开  Near-open",
                "开  Open"
            });


        vowelHeight.DropDownStyle =
            ComboBoxStyle.DropDownList;

        vowelHeight.Width =
            170;

        vowelHeight.SelectedIndex =
            0;

        vowelHeight.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        vowelHeight.Margin =
            new Padding(
                0,
                3,
                6,
                0);


        vowelBackness.Items.AddRange(
            new object[]
            {
                "前  Front",
                "央  Central",
                "后  Back"
            });


        vowelBackness.DropDownStyle =
            ComboBoxStyle.DropDownList;

        vowelBackness.Width =
            150;

        vowelBackness.SelectedIndex =
            0;

        vowelBackness.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        vowelBackness.Margin =
            new Padding(
                0,
                3,
                6,
                0);


        vowelRoundedness.Items.AddRange(
            new object[]
            {
                "不圆唇  Unrounded",
                "圆唇  Rounded"
            });


        vowelRoundedness.DropDownStyle =
            ComboBoxStyle.DropDownList;

        vowelRoundedness.Width =
            170;

        vowelRoundedness.SelectedIndex =
            0;

        vowelRoundedness.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        vowelRoundedness.Margin =
            new Padding(
                0,
                3,
                6,
                0);


        vowelHeight.SelectedIndexChanged +=
            (sender, e) =>
                UpdateVowelSymbolFromFeatures();

        vowelBackness.SelectedIndexChanged +=
            (sender, e) =>
                UpdateVowelSymbolFromFeatures();

        vowelRoundedness.SelectedIndexChanged +=
            (sender, e) =>
                UpdateVowelSymbolFromFeatures();
    }


    private void ConfigurePhonemeInput()
    {
        phonemeInput.Width =
            180;

        phonemeInput.Font =
            new Font(
                "Microsoft YaHei UI",
                12);

        phonemeInput.Margin =
            new Padding(
                0,
                3,
                6,
                0);

        phonemeInput.TextChanged +=
            (sender, e) =>
                UpdateFeaturesFromSymbol();
    }


    private void ConfigureActionButtons()
    {
        addPhonemeButton.Text =
            "添加";

        addPhonemeButton.Width =
            100;

        addPhonemeButton.Height =
            phonemeType.PreferredHeight;

        addPhonemeButton.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        addPhonemeButton.Margin =
            new Padding(
                0,
                3,
                6,
                0);

        addPhonemeButton.Click +=
            AddPhoneme;


        removePhonemeButton.Text =
            "删除";

        removePhonemeButton.Width =
            100;

        removePhonemeButton.Height =
            phonemeType.PreferredHeight;

        removePhonemeButton.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        removePhonemeButton.Margin =
            new Padding(
                0,
                3,
                0,
                0);

        removePhonemeButton.Click +=
            RemovePhoneme;
    }


    private void ConfigurePhonemeLists()
    {
        consonantList.Size =
            new Size(
                725,
                180);

        consonantList.Font =
            new Font(
                "Microsoft YaHei UI",
                12);


        vowelList.Size =
            new Size(
                725,
                180);

        vowelList.Font =
            new Font(
                "Microsoft YaHei UI",
                12);
    }


    private void ConfigureNoMatchingPhonemeLabel()
    {
        noMatchingPhonemeLabel.Text =
            "无此音素  No corresponding phoneme";

        noMatchingPhonemeLabel.AutoSize =
            true;

        noMatchingPhonemeLabel.Font =
            new Font(
                "Microsoft YaHei UI",
                9);

        noMatchingPhonemeLabel.Margin =
            new Padding(
                8,
                7,
                0,
                0);

        noMatchingPhonemeLabel.Visible =
            false;
    }


    // 普通肺部辅音表。
    private Control BuildConsonantChart()
    {
        Panel chartContainer =
            new()
            {
                Width =
                    1480,

                AutoScroll =
                    true,

                Margin =
                    new Padding(
                        0,
                        15,
                        0,
                        25)
            };


        string[] places =
        {
            "双唇\nBilabial",
            "唇齿\nLabiodental",
            "齿\nDental",
            "齿龈\nAlveolar",
            "龈后\nPostalveolar",
            "龈腭\nAlveolo-palatal",
            "卷舌\nRetroflex",
            "硬腭\nPalatal",
            "唇硬腭\nLabial-palatal",
            "软腭\nVelar",
            "小舌\nUvular",
            "咽\nPharyngeal",
            "会厌\nEpiglottal",
            "声门\nGlottal",
            "唇软腭\nLabial-velar"
        };


        string[] placeKeys =
        {
            "双唇  Bilabial",
            "唇齿  Labiodental",
            "齿  Dental",
            "齿龈  Alveolar",
            "龈后  Postalveolar",
            "龈腭  Alveolo-palatal",
            "卷舌  Retroflex",
            "硬腭  Palatal",
            "唇硬腭  Labial-palatal",
            "软腭  Velar",
            "小舌  Uvular",
            "咽  Pharyngeal",
            "会厌  Epiglottal",
            "声门  Glottal",
            "唇软腭  Labial-velar"
        };


        string[] manners =
        {
            "塞音\nPlosive",
            "塞擦音\nAffricate",
            "鼻音\nNasal",
            "颤音\nTrill",
            "闪音\nTap / Flap",
            "边闪音\nLateral flap",
            "擦音\nFricative",
            "边擦音\nLateral fricative",
            "近音\nApproximant",
            "边近音\nLateral approximant"
        };


        string[] mannerKeys =
        {
            "塞音  Plosive",
            "塞擦音  Affricate",
            "鼻音  Nasal",
            "颤音  Trill",
            "闪音  Tap / Flap",
            "边闪音  Lateral flap",
            "擦音  Fricative",
            "边擦音  Lateral fricative",
            "近音  Approximant",
            "边近音  Lateral approximant"
        };


        consonantChart.Controls.Clear();
        consonantChart.ColumnStyles.Clear();
        consonantChart.RowStyles.Clear();
        consonantChartCells.Clear();


        consonantChart.RowCount =
            manners.Length + 1;

        consonantChart.ColumnCount =
            places.Length + 1;

        consonantChart.AutoSize =
            true;

        consonantChart.CellBorderStyle =
            TableLayoutPanelCellBorderStyle.Single;

        consonantChart.Location =
            new Point(
                0,
                0);


        consonantChart.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                150));


        foreach (
            string place
            in places)
        {
            consonantChart.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    100));
        }


        consonantChart.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                45));


        for (
            int column = 0;
            column < places.Length;
            column++)
        {
            Label label =
                new()
                {
                    Text =
                        places[column],

                    Dock =
                        DockStyle.Fill,

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Font =
                        new Font(
                            "Microsoft YaHei UI",
                            9)
                };

            consonantChart.Controls.Add(
                label,
                column + 1,
                0);
        }


        for (
            int row = 0;
            row < manners.Length;
            row++)
        {
            consonantChart.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    40));


            Label mannerLabel =
                new()
                {
                    Text =
                        manners[row],

                    Dock =
                        DockStyle.Fill,

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Font =
                        new Font(
                            "Microsoft YaHei UI",
                            9)
                };


            consonantChart.Controls.Add(
                mannerLabel,
                0,
                row + 1);


            for (
                int column = 0;
                column < places.Length;
                column++)
            {
                TableLayoutPanel cell =
                    new()
                    {
                        Dock =
                            DockStyle.Fill,

                        ColumnCount =
                            2,

                        RowCount =
                            1,

                        Margin =
                            new Padding(0)
                    };


                cell.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        50));

                cell.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        50));


                Label voiceless =
                    new()
                    {
                        Dock =
                            DockStyle.Fill,

                        TextAlign =
                            ContentAlignment.MiddleCenter
                    };


                Label voiced =
                    new()
                    {
                        Dock =
                            DockStyle.Fill,

                        TextAlign =
                            ContentAlignment.MiddleCenter
                    };


                consonantChartCells[
                    (
                        placeKeys[column],
                        mannerKeys[row],
                        "清音  Voiceless"
                    )
                ] = voiceless;


                consonantChartCells[
                    (
                        placeKeys[column],
                        mannerKeys[row],
                        "浊音  Voiced"
                    )
                ] = voiced;


                cell.Controls.Add(
                    voiceless,
                    0,
                    0);

                cell.Controls.Add(
                    voiced,
                    1,
                    0);


                consonantChart.Controls.Add(
                    cell,
                    column + 1,
                    row + 1);
            }
        }


        chartContainer.Controls.Add(
            consonantChart);


        chartContainer.Height =
            consonantChart.PreferredSize.Height +
            SystemInformation.HorizontalScrollBarHeight +
            5;


        return chartContainer;
    }


    // 页面底部：
    // 左侧元音图，右侧 Non-pulmonic。
    private Control BuildLowerChartsRow()
    {
        TableLayoutPanel row =
            new()
            {
                Width =
                    1480,

                Height =
                    430,

                ColumnCount =
                    2,

                RowCount =
                    1,

                Margin =
                    new Padding(
                        0,
                        20,
                        0,
                        25),

                GrowStyle =
                    TableLayoutPanelGrowStyle.FixedSize
            };


        row.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                800));


        row.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Absolute,
                560));


        row.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                430));


        Control vowelArea =
            BuildVowelChart();

        vowelArea.Margin =
            new Padding(0);


        Control nonPulmonicArea =
            BuildNonPulmonicChart();

        nonPulmonicArea.Margin =
            new Padding(
                20,
                0,
                0,
                0);


        row.Controls.Add(
            vowelArea,
            0,
            0);


        row.Controls.Add(
            nonPulmonicArea,
            1,
            0);


        return row;
    }


    // Non-pulmonic + Other IPA Symbols。
    private Control BuildNonPulmonicChart()
    {
        Panel container =
            new()
            {
                Width =
                    650,

                Height =
                    400,

                Margin =
                    new Padding(0)
            };


        Label title =
            new()
            {
                Text =
                    "非肺部气流辅音  " +
                    "Non-pulmonic consonants",

                Location =
                    new Point(
                        0,
                        10),

                AutoSize =
                    true,

                Font =
                    new Font(
                        "Microsoft YaHei UI",
                        11,
                        FontStyle.Bold)
            };


        string[] categoryKeys =
        {
            "搭嘴音  Click",
            "浊内爆音  Voiced implosive",
            "挤喉音  Ejective"
        };


        string[] categoryTitles =
        {
            "搭嘴音\nClicks",
            "浊内爆音\nVoiced implosives",
            "挤喉音\nEjectives"
        };


        nonPulmonicChart.Controls.Clear();
        nonPulmonicChart.ColumnStyles.Clear();
        nonPulmonicChart.RowStyles.Clear();
        nonPulmonicChartCells.Clear();


        nonPulmonicChart.Location =
            new Point(
                0,
                45);


        nonPulmonicChart.Size =
            new Size(
                650,
                300);


        nonPulmonicChart.ColumnCount =
            3;

        nonPulmonicChart.RowCount =
            2;


        nonPulmonicChart.GrowStyle =
            TableLayoutPanelGrowStyle.FixedSize;


        nonPulmonicChart.CellBorderStyle =
            TableLayoutPanelCellBorderStyle.Single;


        nonPulmonicChart.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                33.333f));

        nonPulmonicChart.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                33.333f));

        nonPulmonicChart.ColumnStyles.Add(
            new ColumnStyle(
                SizeType.Percent,
                33.334f));


        nonPulmonicChart.RowStyles.Add(
            new RowStyle(
                SizeType.Absolute,
                50));


        nonPulmonicChart.RowStyles.Add(
            new RowStyle(
                SizeType.Percent,
                100));


        for (
            int column = 0;
            column < categoryKeys.Length;
            column++)
        {
            string categoryKey =
                categoryKeys[column];


            Label header =
                new()
                {
                    Text =
                        categoryTitles[column],

                    Dock =
                        DockStyle.Fill,

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Font =
                        new Font(
                            "Microsoft YaHei UI",
                            9,
                            FontStyle.Bold)
                };


            Label content =
                new()
                {
                    Text =
                        "",

                    Dock =
                        DockStyle.Fill,

                    TextAlign =
                        ContentAlignment.TopLeft,

                    Font =
                        new Font(
                            "Microsoft YaHei UI",
                            11),

                    Padding =
                        new Padding(
                            10,
                            10,
                            6,
                            6)
                };


            nonPulmonicChart.Controls.Add(
                header,
                column,
                0);


            nonPulmonicChart.Controls.Add(
                content,
                column,
                1);


            nonPulmonicChartCells[
                categoryKey] =
                content;
        }


        // Other IPA Symbols 区域放在 NP 表下面。
        Label otherSymbolsTitle =
            new()
            {
                Text =
                    "其他 IPA 符号  Other IPA symbols",

                Location =
                    new Point(
                        0,
                        350),

                AutoSize =
                    true,

                Font =
                    new Font(
                        "Microsoft YaHei UI",
                        9,
                        FontStyle.Bold)
            };


        otherSymbolsContent.Location =
            new Point(
                0,
                375);

        otherSymbolsContent.Size =
            new Size(
                650,
                25);

        otherSymbolsContent.Font =
            new Font(
                "Microsoft YaHei UI",
                10);

        otherSymbolsContent.Text =
            "";


        container.Controls.Add(
            title);

        container.Controls.Add(
            nonPulmonicChart);

        container.Controls.Add(
            otherSymbolsTitle);

        container.Controls.Add(
            otherSymbolsContent);


        return container;
    }


    // IPA 元音梯形图。
    private Control BuildVowelChart()
    {
        Panel chartContainer =
            new()
            {
                Width =
                    800,

                Height =
                    400,

                Margin =
                    new Padding(0)
            };


        vowelChart.Size =
            new Size(
                790,
                390);

        vowelChart.Location =
            new Point(
                0,
                0);


        vowelChartCells.Clear();


        vowelChart.Paint +=
            (sender, e) =>
            {
                using Pen pen =
                    new(
                        Color.Gray,
                        1);


                Point topLeft =
                    new(
                        250,
                        55);

                Point topRight =
                    new(
                        730,
                        55);

                Point bottomLeft =
                    new(
                        370,
                        340);

                Point bottomRight =
                    new(
                        730,
                        340);


                e.Graphics.DrawLine(
                    pen,
                    topLeft,
                    topRight);

                e.Graphics.DrawLine(
                    pen,
                    topLeft,
                    bottomLeft);

                e.Graphics.DrawLine(
                    pen,
                    topRight,
                    bottomRight);

                e.Graphics.DrawLine(
                    pen,
                    bottomLeft,
                    bottomRight);


                e.Graphics.DrawLine(
                    pen,
                    new Point(
                        490,
                        55),

                    new Point(
                        550,
                        340));
            };


        AddVowelChartHeader(
            "前  Front",
            new Point(
                220,
                10));

        AddVowelChartHeader(
            "央  Central",
            new Point(
                450,
                10));

        AddVowelChartHeader(
            "后  Back",
            new Point(
                700,
                10));


        AddVowelHeightLabel(
            "闭  Close",
            45);

        AddVowelHeightLabel(
            "近闭  Near-close",
            90);

        AddVowelHeightLabel(
            "半闭  Close-mid",
            135);

        AddVowelHeightLabel(
            "中  Mid",
            180);

        AddVowelHeightLabel(
            "半开  Open-mid",
            225);

        AddVowelHeightLabel(
            "近开  Near-open",
            270);

        AddVowelHeightLabel(
            "开  Open",
            315);


        foreach (
            IpaVowel vowel
            in IpaVowels.All)
        {
            Point anchor =
                GetVowelChartPosition(
                    vowel.Height,
                    vowel.Backness);


            bool hasRoundedPair =
                IpaVowels.All.Any(
                    x =>
                        x.Height ==
                            vowel.Height &&

                        x.Backness ==
                            vowel.Backness &&

                        x.Roundedness !=
                            vowel.Roundedness);


            int x;


            if (
                !hasRoundedPair)
            {
                x =
                    anchor.X - 20;
            }
            else if (
                vowel.Roundedness.StartsWith(
                    "不圆唇"))
            {
                x =
                    anchor.X - 42;
            }
            else
            {
                x =
                    anchor.X + 2;
            }


            Label cell =
                new()
                {
                    Text =
                        "",

                    Location =
                        new Point(
                            x,
                            anchor.Y - 14),

                    Size =
                        new Size(
                            40,
                            30),

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Font =
                        new Font(
                            "Microsoft YaHei UI",
                            14)
                };


            vowelChartCells[
                (
                    vowel.Height,
                    vowel.Backness,
                    vowel.Roundedness
                )
            ] = cell;


            vowelChart.Controls.Add(
                cell);
        }


        chartContainer.Controls.Add(
            vowelChart);


        return chartContainer;
    }


    private void AddVowelChartHeader(
        string text,
        Point location)
    {
        Label label =
            new()
            {
                Text =
                    text,

                AutoSize =
                    true,

                Location =
                    location,

                Font =
                    new Font(
                        "Microsoft YaHei UI",
                        10,
                        FontStyle.Bold)
            };


        vowelChart.Controls.Add(
            label);
    }


    private void AddVowelHeightLabel(
        string text,
        int y)
    {
        Label label =
            new()
            {
                Text =
                    text,

                Size =
                    new Size(
                        180,
                        30),

                Location =
                    new Point(
                        10,
                        y - 14),

                TextAlign =
                    ContentAlignment.MiddleRight,

                Font =
                    new Font(
                        "Microsoft YaHei UI",
                        9)
            };


        vowelChart.Controls.Add(
            label);
    }


    private void RefreshVowelChart()
    {
        foreach (
            Label cell
            in vowelChartCells.Values)
        {
            cell.Text =
                "";
        }


        foreach (
            VowelPhoneme vowel
            in project.Phonology.Vowels)
        {
            var key =
                (
                    vowel.Height,
                    vowel.Backness,
                    vowel.Roundedness
                );


            if (
                !vowelChartCells.TryGetValue(
                    key,
                    out Label? cell))
            {
                continue;
            }


            if (
                cell.Text.Length == 0)
            {
                cell.Text =
                    vowel.Symbol;
            }
            else
            {
                cell.Text +=
                    $" {vowel.Symbol}";
            }
        }
    }


    private static Point GetVowelChartPosition(
        string height,
        string backness)
    {
        int row =
            height switch
            {
                "闭  Close" =>
                    0,

                "近闭  Near-close" =>
                    1,

                "半闭  Close-mid" =>
                    2,

                "中  Mid" =>
                    3,

                "半开  Open-mid" =>
                    4,

                "近开  Near-open" =>
                    5,

                "开  Open" =>
                    6,

                _ =>
                    0
            };


        int y =
            55 +
            row * 45;


        int frontX =
            250 +
            row * 20;


        int backX =
            730;


        int centralX =
            (frontX + backX) /
            2;


        int x =
            backness switch
            {
                "前  Front" =>
                    frontX,

                "央  Central" =>
                    centralX,

                "后  Back" =>
                    backX,

                _ =>
                    centralX
            };


        return
            new Point(
                x,
                y);
    }


    // 根据音素类型和选择模式更新 UI。
    private void UpdateSelectionMode()
    {
        bool isConsonant =
            phonemeType.SelectedIndex == 0;

        bool isVowel =
            phonemeType.SelectedIndex == 1;


        IpaConsonant? currentConsonant =
            FindConsonantFromInput();


        bool detailedMode =
            selectionMode ==
            SelectionMode.Detailed;

        bool guidedMode =
            selectionMode ==
            SelectionMode.Guided;

        bool listMode =
            selectionMode ==
            SelectionMode.List;


        ipaSymbolPicker.Visible =
            listMode;

        consonantPlace.Visible =
            detailedMode &&
            isConsonant;

        consonantManner.Visible =
            detailedMode &&
            isConsonant;

        consonantVoicing.Visible =
            detailedMode &&
            isConsonant;

        vowelHeight.Visible =
            detailedMode &&
            isVowel;

        vowelBackness.Visible =
            detailedMode &&
            isVowel;

        vowelRoundedness.Visible =
            detailedMode &&
            isVowel;

        ipaCategory.Visible =
            guidedMode;

        ipaChoice.Visible =
            guidedMode;


        selectionModeButton.Text =
            selectionMode switch
            {
                SelectionMode.Detailed =>
                    "切换到引导模式",

                SelectionMode.Guided =>
                    "切换到列表模式",

                SelectionMode.List =>
                    "切换到详细模式",

                _ =>
                    "切换模式"
            };


        if (
            selectionMode ==
            SelectionMode.Detailed)
        {
            if (
                isConsonant)
            {
                if (
                    currentConsonant != null)
                {
                    SelectDetailedConsonant(
                        currentConsonant);
                }
            }
            else
            {
                UpdateVowelSymbolFromFeatures();
            }

            return;
        }


        if (
            selectionMode ==
            SelectionMode.List)
        {
            if (
                isConsonant)
            {
                LoadDetailedIpaPicker();

                if (
                    currentConsonant != null)
                {
                    SelectDetailedConsonant(
                        currentConsonant);
                }
            }
            else
            {
                LoadVowelListPicker();

                IpaVowel? currentVowel =
                    FindVowelFromInput();

                if (
                    currentVowel != null)
                {
                    SelectListVowel(
                        currentVowel);
                }
            }

            return;
        }


        if (
            isConsonant)
        {
            ipaCategory.Enabled =
                true;

            ipaChoice.Enabled =
                true;


            EnsureConsonantCategories();


            if (
                currentConsonant != null)
            {
                SelectGuidedConsonant(
                    currentConsonant);
            }
        }
        else
        {
            ipaCategory.Enabled =
                true;

            ipaChoice.Enabled =
                true;


            LoadVowelCategories();


            IpaVowel? currentVowel =
                FindVowelFromInput();


            if (
                currentVowel != null)
            {
                SelectGuidedVowel(
                    currentVowel);
            }
            else
            {
                UpdateVowelSymbolFromFeatures();
            }
        }
    }


    // List mode：普通辅音 + NP + Other Symbols。
    private void LoadDetailedIpaPicker()
    {
        ipaSymbolPicker.BeginUpdate();

        ipaSymbolPicker.Items.Clear();


        // 普通辅音
        foreach (
            IGrouping<string, IpaConsonant> category
            in IpaConsonants.All.GroupBy(
                x => x.Manner))
        {
            ipaSymbolPicker.Items.Add(
                new IpaDisplayItem(
                    $"── " +
                    $"{GetCategoryDisplayName(category.Key)} " +
                    $"──"));


            foreach (
                IpaConsonant consonant
                in category)
            {
                ipaSymbolPicker.Items.Add(
                    new IpaDisplayItem(
                        $"{consonant.Symbol}   " +
                        $"{GetConsonantDescription(consonant)}",

                        consonant));
            }
        }


        // 非肺部气流辅音
        foreach (
            IGrouping<
                string,
                IpaNonPulmonicConsonant> category
            in IpaNonPulmonicConsonants.All.GroupBy(
                x => x.Category))
        {
            ipaSymbolPicker.Items.Add(
                new IpaDisplayItem(
                    $"── {category.Key} ──"));


            foreach (
                IpaNonPulmonicConsonant consonant
                in category)
            {
                ipaSymbolPicker.Items.Add(
                    new IpaDisplayItem(
                        $"{consonant.Symbol}   " +
                        $"{consonant.Description}",

                        consonant));
            }
        }


        // Other IPA Symbols
        if (
            IpaOtherSymbols.All.Count > 0)
        {
            ipaSymbolPicker.Items.Add(
                new IpaDisplayItem(
                    "── 其他 IPA 符号  " +
                    "Other IPA symbols ──"));


            foreach (
                IpaOtherSymbol symbol
                in IpaOtherSymbols.All)
            {
                ipaSymbolPicker.Items.Add(
                    new IpaDisplayItem(
                        $"{symbol.Symbol}   " +
                        $"{symbol.Name}",

                        symbol));
            }
        }


        ipaSymbolPicker.SelectedIndex =
            -1;

        lastIpaSymbolIndex =
            -1;


        ipaSymbolPicker.EndUpdate();
    }


    private void LoadConsonantCategories()
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        ipaCategory.BeginUpdate();

        ipaCategory.Items.Clear();


        foreach (
            string manner
            in IpaConsonants.All
                .Select(
                    x => x.Manner)
                .Distinct())
        {
            ipaCategory.Items.Add(
                new IpaCategoryItem(
                    manner,
                    GetCategoryDisplayName(
                        manner)));
        }


        ipaCategory.SelectedIndex =
            -1;

        ipaCategory.EndUpdate();

        ipaChoice.Items.Clear();


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private void PopulateGuidedChoices(
        string manner)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        ipaChoice.BeginUpdate();

        ipaChoice.Items.Clear();


        foreach (
            IpaConsonant consonant
            in IpaConsonants.All.Where(
                x => x.Manner == manner))
        {
            ipaChoice.Items.Add(
                new IpaDisplayItem(
                    $"{consonant.Symbol}   " +
                    $"{GetConsonantDescription(consonant)}",

                    consonant));
        }


        ipaChoice.SelectedIndex =
            -1;

        ipaChoice.EndUpdate();


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private void EnsureConsonantCategories()
    {
        if (
            ipaCategory.Items
                .OfType<IpaCategoryItem>()
                .Any())
        {
            return;
        }


        LoadConsonantCategories();
    }


    private void ApplyConsonant(
        IpaConsonant consonant)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        phonemeType.SelectedIndex =
            0;

        phonemeInput.Text =
            consonant.Symbol;

        phonemeInput.SelectionStart =
            phonemeInput.Text.Length;


        consonantPlace.SelectedItem =
            consonant.Place;

        consonantManner.SelectedItem =
            consonant.Manner;

        consonantVoicing.SelectedItem =
            consonant.Voicing;


        SelectDetailedConsonant(
            consonant);

        SelectGuidedConsonant(
            consonant);


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private void ApplyNonPulmonicConsonant(
        IpaNonPulmonicConsonant consonant)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        phonemeType.SelectedIndex =
            0;

        phonemeInput.Text =
            consonant.Symbol;

        phonemeInput.SelectionStart =
            phonemeInput.Text.Length;


        isSynchronizingSelection =
            wasSynchronizing;


        noMatchingPhonemeLabel.Visible =
            false;

        addPhonemeButton.Enabled =
            true;
    }


    // Other IPA Symbol → 输入框。
    private void ApplyOtherSymbol(
        IpaOtherSymbol symbol)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        phonemeType.SelectedIndex =
            0;

        phonemeInput.Text =
            symbol.Symbol;

        phonemeInput.SelectionStart =
            phonemeInput.Text.Length;


        isSynchronizingSelection =
            wasSynchronizing;


        noMatchingPhonemeLabel.Visible =
            false;

        addPhonemeButton.Enabled =
            true;
    }


    private void SelectDetailedConsonant(
        IpaConsonant consonant)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        for (
            int index = 0;
            index < ipaSymbolPicker.Items.Count;
            index++)
        {
            if (
                ipaSymbolPicker.Items[index]
                    is IpaDisplayItem item &&

                item.Consonant?.Symbol ==
                    consonant.Symbol)
            {
                ipaSymbolPicker.SelectedIndex =
                    index;

                lastIpaSymbolIndex =
                    index;

                break;
            }
        }


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private void SelectGuidedConsonant(
        IpaConsonant consonant)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        EnsureConsonantCategories();


        for (
            int index = 0;
            index < ipaCategory.Items.Count;
            index++)
        {
            if (
                ipaCategory.Items[index]
                    is IpaCategoryItem category &&

                category.Manner ==
                    consonant.Manner)
            {
                ipaCategory.SelectedIndex =
                    index;

                break;
            }
        }


        PopulateGuidedChoices(
            consonant.Manner);


        for (
            int index = 0;
            index < ipaChoice.Items.Count;
            index++)
        {
            if (
                ipaChoice.Items[index]
                    is IpaDisplayItem item &&

                item.Consonant?.Symbol ==
                    consonant.Symbol)
            {
                ipaChoice.SelectedIndex =
                    index;

                break;
            }
        }


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private IpaConsonant?
        FindConsonantFromInput()
    {
        string symbol =
            NormalizeInputSymbol(
                phonemeInput.Text.Trim());


        return
            IpaConsonants.All.FirstOrDefault(
                x => x.Symbol == symbol);
    }


    private IpaNonPulmonicConsonant?
        FindNonPulmonicConsonantFromInput()
    {
        string symbol =
            phonemeInput.Text.Trim();


        return
            IpaNonPulmonicConsonants.All
                .FirstOrDefault(
                    x => x.Symbol == symbol);
    }


    private IpaOtherSymbol?
        FindOtherSymbolFromInput()
    {
        string symbol =
            phonemeInput.Text.Trim();


        return
            IpaOtherSymbols.All
                .FirstOrDefault(
                    x => x.Symbol == symbol);
    }


    private static string NormalizeInputSymbol(
        string symbol)
    {
        return
            symbol == "g"
                ? "ɡ"
                : symbol;
    }


    private void UpdateSymbolFromFeatures()
    {
        if (
            isSynchronizingSelection ||
            phonemeType.SelectedIndex != 0)
        {
            return;
        }


        IpaConsonant? match =
            IpaConsonants.All.FirstOrDefault(
                x =>
                    x.Place ==
                        consonantPlace.Text &&

                    x.Manner ==
                        consonantManner.Text &&

                    x.Voicing ==
                        consonantVoicing.Text);


        if (
            match == null)
        {
            bool wasSynchronizing =
                isSynchronizingSelection;

            isSynchronizingSelection =
                true;


            phonemeInput.Clear();


            isSynchronizingSelection =
                wasSynchronizing;


            ClearIpaSelections();


            noMatchingPhonemeLabel.Visible =
                true;

            addPhonemeButton.Enabled =
                false;

            return;
        }


        noMatchingPhonemeLabel.Visible =
            false;

        addPhonemeButton.Enabled =
            true;


        ApplyConsonant(
            match);
    }


    private void LoadVowelListPicker()
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        ipaSymbolPicker.BeginUpdate();

        ipaSymbolPicker.Items.Clear();


        foreach (
            IGrouping<string, IpaVowel> category
            in IpaVowels.All.GroupBy(
                x => x.Height))
        {
            ipaSymbolPicker.Items.Add(
                new IpaDisplayItem(
                    $"── " +
                    $"{GetVowelCategoryDisplayName(category.Key)} " +
                    $"──"));


            foreach (
                IpaVowel vowel
                in category)
            {
                ipaSymbolPicker.Items.Add(
                    new IpaDisplayItem(
                        $"{vowel.Symbol}   " +
                        $"{GetVowelDescription(vowel)}",

                        vowel));
            }
        }


        ipaSymbolPicker.SelectedIndex =
            -1;

        lastIpaSymbolIndex =
            -1;


        ipaSymbolPicker.EndUpdate();


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private void LoadVowelCategories()
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        ipaCategory.BeginUpdate();

        ipaCategory.Items.Clear();


        foreach (
            string height
            in IpaVowels.All
                .Select(
                    x => x.Height)
                .Distinct())
        {
            ipaCategory.Items.Add(
                new IpaVowelCategoryItem(
                    height,
                    GetVowelCategoryDisplayName(
                        height)));
        }


        ipaCategory.SelectedIndex =
            -1;

        ipaCategory.EndUpdate();

        ipaChoice.Items.Clear();


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private void PopulateGuidedVowelChoices(
        string height)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        ipaChoice.BeginUpdate();

        ipaChoice.Items.Clear();


        foreach (
            IpaVowel vowel
            in IpaVowels.All.Where(
                x => x.Height == height))
        {
            ipaChoice.Items.Add(
                new IpaDisplayItem(
                    $"{vowel.Symbol}   " +
                    $"{GetVowelDescription(vowel)}",

                    vowel));
        }


        ipaChoice.SelectedIndex =
            -1;

        ipaChoice.EndUpdate();


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private void ApplyVowel(
        IpaVowel vowel)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        phonemeInput.Text =
            vowel.Symbol;

        phonemeInput.SelectionStart =
            phonemeInput.Text.Length;


        vowelHeight.SelectedItem =
            vowel.Height;

        vowelBackness.SelectedItem =
            vowel.Backness;

        vowelRoundedness.SelectedItem =
            vowel.Roundedness;


        SelectListVowel(
            vowel);

        SelectGuidedVowel(
            vowel);


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private void SelectListVowel(
        IpaVowel vowel)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        for (
            int index = 0;
            index < ipaSymbolPicker.Items.Count;
            index++)
        {
            if (
                ipaSymbolPicker.Items[index]
                    is IpaDisplayItem item &&

                item.Vowel?.Symbol ==
                    vowel.Symbol)
            {
                ipaSymbolPicker.SelectedIndex =
                    index;

                lastIpaSymbolIndex =
                    index;

                break;
            }
        }


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private void SelectGuidedVowel(
        IpaVowel vowel)
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        if (
            !ipaCategory.Items
                .OfType<IpaVowelCategoryItem>()
                .Any())
        {
            LoadVowelCategories();
        }


        for (
            int index = 0;
            index < ipaCategory.Items.Count;
            index++)
        {
            if (
                ipaCategory.Items[index]
                    is IpaVowelCategoryItem category &&

                category.Height ==
                    vowel.Height)
            {
                ipaCategory.SelectedIndex =
                    index;

                break;
            }
        }


        PopulateGuidedVowelChoices(
            vowel.Height);


        for (
            int index = 0;
            index < ipaChoice.Items.Count;
            index++)
        {
            if (
                ipaChoice.Items[index]
                    is IpaDisplayItem item &&

                item.Vowel?.Symbol ==
                    vowel.Symbol)
            {
                ipaChoice.SelectedIndex =
                    index;

                break;
            }
        }


        isSynchronizingSelection =
            wasSynchronizing;
    }


    private IpaVowel?
        FindVowelFromInput()
    {
        string symbol =
            phonemeInput.Text.Trim();


        return
            IpaVowels.All.FirstOrDefault(
                x => x.Symbol == symbol);
    }


    private void UpdateVowelSymbolFromFeatures()
    {
        if (
            isSynchronizingSelection ||
            phonemeType.SelectedIndex != 1)
        {
            return;
        }


        IpaVowel? match =
            IpaVowels.All.FirstOrDefault(
                x =>
                    x.Height ==
                        vowelHeight.Text &&

                    x.Backness ==
                        vowelBackness.Text &&

                    x.Roundedness ==
                        vowelRoundedness.Text);


        if (
            match == null)
        {
            bool wasSynchronizing =
                isSynchronizingSelection;

            isSynchronizingSelection =
                true;


            phonemeInput.Clear();


            isSynchronizingSelection =
                wasSynchronizing;


            noMatchingPhonemeLabel.Visible =
                true;

            addPhonemeButton.Enabled =
                false;

            return;
        }


        noMatchingPhonemeLabel.Visible =
            false;

        addPhonemeButton.Enabled =
            true;


        ApplyVowel(
            match);
    }


    // 手动输入 IPA 后进行反向识别。
    private void UpdateFeaturesFromSymbol()
    {
        if (
            isSynchronizingSelection)
        {
            return;
        }


        string input =
            phonemeInput.Text.Trim();


        if (
            input.Length == 0)
        {
            ClearIpaSelections();


            noMatchingPhonemeLabel.Visible =
                false;

            addPhonemeButton.Enabled =
                false;

            return;
        }


        // 辅音
        if (
            phonemeType.SelectedIndex == 0)
        {
            // 普通辅音
            IpaConsonant? consonant =
                FindConsonantFromInput();


            if (
                consonant != null)
            {
                noMatchingPhonemeLabel.Visible =
                    false;

                addPhonemeButton.Enabled =
                    true;


                ApplyConsonant(
                    consonant);

                return;
            }


            // Non-pulmonic
            IpaNonPulmonicConsonant?
                nonPulmonic =
                    FindNonPulmonicConsonantFromInput();


            if (
                nonPulmonic != null)
            {
                ClearIpaSelections();


                noMatchingPhonemeLabel.Visible =
                    false;

                addPhonemeButton.Enabled =
                    true;

                return;
            }


            // Other IPA Symbols
            IpaOtherSymbol?
                otherSymbol =
                    FindOtherSymbolFromInput();


            if (
                otherSymbol != null)
            {
                ClearIpaSelections();


                noMatchingPhonemeLabel.Visible =
                    false;

                addPhonemeButton.Enabled =
                    true;

                return;
            }


            // 所有参考数据库都没有找到。
            ClearIpaSelections();


            noMatchingPhonemeLabel.Visible =
                true;

            addPhonemeButton.Enabled =
                false;

            return;
        }


        // 元音
        IpaVowel? vowel =
            FindVowelFromInput();


        if (
            vowel == null)
        {
            ClearIpaSelections();


            noMatchingPhonemeLabel.Visible =
                true;

            addPhonemeButton.Enabled =
                false;

            return;
        }


        noMatchingPhonemeLabel.Visible =
            false;

        addPhonemeButton.Enabled =
            true;


        ApplyVowel(
            vowel);
    }


    private void ClearIpaSelections()
    {
        bool wasSynchronizing =
            isSynchronizingSelection;

        isSynchronizingSelection =
            true;


        ipaSymbolPicker.SelectedIndex =
            -1;

        ipaChoice.SelectedIndex =
            -1;

        lastIpaSymbolIndex =
            -1;


        isSynchronizingSelection =
            wasSynchronizing;
    }


    // 从项目模型加载音系。
    private void LoadProjectPhonology()
    {
        consonantList.Items.Clear();

        vowelList.Items.Clear();


        foreach (
            ConsonantPhoneme consonant
            in project.Phonology.Consonants)
        {
            consonantList.Items.Add(
                new ConsonantEntry
                {
                    Symbol =
                        consonant.Symbol,

                    Place =
                        consonant.Place,

                    Manner =
                        consonant.Manner,

                    Voicing =
                        consonant.Voicing,

                    Category =
                        consonant.Category,

                    Description =
                        consonant.Description
                });
        }


        foreach (
            VowelPhoneme vowel
            in project.Phonology.Vowels)
        {
            vowelList.Items.Add(
                new VowelEntry
                {
                    Symbol =
                        vowel.Symbol,

                    Height =
                        vowel.Height,

                    Backness =
                        vowel.Backness,

                    Roundedness =
                        vowel.Roundedness
                });
        }


        RefreshConsonantChart();

        RefreshNonPulmonicChart();

        RefreshOtherSymbols();

        RefreshVowelChart();
    }


    // 将当前音素加入项目。
    private void AddPhoneme(
        object? sender,
        EventArgs e)
    {
        string phoneme =
            phonemeInput.Text.Trim();


        if (
            phoneme.Length == 0)
        {
            return;
        }


        // 辅音
        if (
            phonemeType.SelectedIndex == 0)
        {
            phoneme =
                NormalizeInputSymbol(
                    phoneme);


            // 防止重复。
            if (
                project.Phonology.Consonants.Any(
                    x => x.Symbol == phoneme))
            {
                return;
            }


            // 1. 普通辅音
            IpaConsonant? ipaConsonant =
                IpaConsonants.All.FirstOrDefault(
                    x => x.Symbol == phoneme);


            if (
                ipaConsonant != null)
            {
                ConsonantPhoneme projectConsonant =
                    new()
                    {
                        Symbol =
                            ipaConsonant.Symbol,

                        Place =
                            ipaConsonant.Place,

                        Manner =
                            ipaConsonant.Manner,

                        Voicing =
                            ipaConsonant.Voicing
                    };


                project.Phonology.Consonants.Add(
                    projectConsonant);


                consonantList.Items.Add(
                    new ConsonantEntry
                    {
                        Symbol =
                            projectConsonant.Symbol,

                        Place =
                            projectConsonant.Place,

                        Manner =
                            projectConsonant.Manner,

                        Voicing =
                            projectConsonant.Voicing
                    });


                RefreshConsonantChart();
            }
            else
            {
                // 2. Non-pulmonic
                IpaNonPulmonicConsonant?
                    nonPulmonic =
                        IpaNonPulmonicConsonants.All
                            .FirstOrDefault(
                                x =>
                                    x.Symbol ==
                                    phoneme);


                if (
                    nonPulmonic != null)
                {
                    ConsonantPhoneme projectConsonant =
                        new()
                        {
                            Symbol =
                                nonPulmonic.Symbol,

                            Category =
                                nonPulmonic.Category,

                            Description =
                                nonPulmonic.Description
                        };


                    project.Phonology.Consonants.Add(
                        projectConsonant);


                    consonantList.Items.Add(
                        new ConsonantEntry
                        {
                            Symbol =
                                projectConsonant.Symbol,

                            Category =
                                projectConsonant.Category,

                            Description =
                                projectConsonant.Description
                        });


                    RefreshNonPulmonicChart();
                }
                else
                {
                    // 3. Other IPA Symbols
                    IpaOtherSymbol? otherSymbol =
                        IpaOtherSymbols.All
                            .FirstOrDefault(
                                x =>
                                    x.Symbol ==
                                    phoneme);


                    if (
                        otherSymbol == null)
                    {
                        noMatchingPhonemeLabel.Visible =
                            true;

                        addPhonemeButton.Enabled =
                            false;

                        return;
                    }


                    ConsonantPhoneme projectConsonant =
                        new()
                        {
                            Symbol =
                                otherSymbol.Symbol,

                            Category =
                                "其他 IPA 符号  " +
                                "Other IPA symbol",

                            Description =
                                otherSymbol.Name
                        };


                    project.Phonology.Consonants.Add(
                        projectConsonant);


                    consonantList.Items.Add(
                        new ConsonantEntry
                        {
                            Symbol =
                                projectConsonant.Symbol,

                            Category =
                                projectConsonant.Category,

                            Description =
                                projectConsonant.Description
                        });


                    RefreshOtherSymbols();
                }
            }
        }

        // 元音
        else
        {
            IpaVowel? ipaVowel =
                IpaVowels.All.FirstOrDefault(
                    x => x.Symbol == phoneme);


            if (
                ipaVowel == null)
            {
                noMatchingPhonemeLabel.Visible =
                    true;

                addPhonemeButton.Enabled =
                    false;

                return;
            }


            if (
                project.Phonology.Vowels.Any(
                    x => x.Symbol == phoneme))
            {
                return;
            }


            VowelPhoneme projectVowel =
                new()
                {
                    Symbol =
                        ipaVowel.Symbol,

                    Height =
                        ipaVowel.Height,

                    Backness =
                        ipaVowel.Backness,

                    Roundedness =
                        ipaVowel.Roundedness
                };


            project.Phonology.Vowels.Add(
                projectVowel);


            vowelList.Items.Add(
                new VowelEntry
                {
                    Symbol =
                        projectVowel.Symbol,

                    Height =
                        projectVowel.Height,

                    Backness =
                        projectVowel.Backness,

                    Roundedness =
                        projectVowel.Roundedness
                });


            RefreshVowelChart();
        }


        projectModified?.Invoke();


        phonemeInput.Clear();

        phonemeInput.Focus();
    }


    // 删除音素。
    private void RemovePhoneme(
        object? sender,
        EventArgs e)
    {
        if (
            consonantList.SelectedItem
                is ConsonantEntry consonant)
        {
            project.Phonology.Consonants.RemoveAll(
                x =>
                    x.Symbol ==
                    consonant.Symbol);


            consonantList.Items.Remove(
                consonant);


            RefreshConsonantChart();

            RefreshNonPulmonicChart();

            RefreshOtherSymbols();


            projectModified?.Invoke();

            return;
        }


        if (
            vowelList.SelectedItem
                is VowelEntry vowel)
        {
            project.Phonology.Vowels.RemoveAll(
                x =>
                    x.Symbol ==
                    vowel.Symbol);


            vowelList.Items.Remove(
                vowel);


            RefreshVowelChart();


            projectModified?.Invoke();
        }
    }


    // 普通辅音表刷新。
    private void RefreshConsonantChart()
    {
        foreach (
            Label cell
            in consonantChartCells.Values)
        {
            cell.Text =
                "";
        }


        foreach (
            ConsonantEntry consonant
            in consonantList.Items)
        {
            var key =
                (
                    consonant.Place,
                    consonant.Manner,
                    consonant.Voicing
                );


            if (
                !consonantChartCells.TryGetValue(
                    key,
                    out Label? cell))
            {
                continue;
            }


            if (
                cell.Text.Length == 0)
            {
                cell.Text =
                    consonant.Symbol;
            }
            else
            {
                cell.Text +=
                    $" {consonant.Symbol}";
            }


            cell.Font =
                new Font(
                    "Microsoft YaHei UI",
                    12);
        }
    }


    // Non-pulmonic 表刷新。
    private void RefreshNonPulmonicChart()
    {
        foreach (
            Label cell
            in nonPulmonicChartCells.Values)
        {
            cell.Text =
                "";
        }


        foreach (
            ConsonantPhoneme consonant
            in project.Phonology.Consonants)
        {
            if (
                string.IsNullOrWhiteSpace(
                    consonant.Category))
            {
                continue;
            }


            if (
                !nonPulmonicChartCells.TryGetValue(
                    consonant.Category,
                    out Label? cell))
            {
                continue;
            }


            string description =
                string.IsNullOrWhiteSpace(
                    consonant.Description)
                    ? ""
                    : GetChinesePart(
                        consonant.Description);


            string line =
                description.Length == 0
                    ? consonant.Symbol
                    : $"{consonant.Symbol}    " +
                      $"{description}";


            if (
                cell.Text.Length == 0)
            {
                cell.Text =
                    line;
            }
            else
            {
                cell.Text +=
                    Environment.NewLine +
                    line;
            }
        }
    }


    // Other IPA Symbols 刷新。
    private void RefreshOtherSymbols()
    {
        otherSymbolsContent.Text =
            "";


        foreach (
            ConsonantPhoneme consonant
            in project.Phonology.Consonants)
        {
            IpaOtherSymbol? reference =
                IpaOtherSymbols.All
                    .FirstOrDefault(
                        x =>
                            x.Symbol ==
                            consonant.Symbol);


            if (
                reference == null)
            {
                continue;
            }


            string line =
                $"{reference.Symbol}    " +
                $"{reference.Name}";


            if (
                otherSymbolsContent.Text.Length == 0)
            {
                otherSymbolsContent.Text =
                    line;
            }
            else
            {
                otherSymbolsContent.Text +=
                    "    " +
                    line;
            }
        }
    }


    private static string GetCategoryDisplayName(
        string manner)
    {
        return
            manner switch
            {
                "塞音  Plosive" =>
                    "塞音  Plosives",

                "塞擦音  Affricate" =>
                    "塞擦音  Affricates",

                "鼻音  Nasal" =>
                    "鼻音  Nasals",

                "颤音  Trill" =>
                    "颤音  Trills",

                "闪音  Tap / Flap" =>
                    "闪音  Taps / Flaps",

                "边闪音  Lateral flap" =>
                    "边闪音  Lateral flaps",

                "擦音  Fricative" =>
                    "擦音  Fricatives",

                "边擦音  Lateral fricative" =>
                    "边擦音  Lateral fricatives",

                "近音  Approximant" =>
                    "近音  Approximants",

                "边近音  Lateral approximant" =>
                    "边近音  Lateral approximants",

                _ =>
                    manner
            };
    }


    private static string GetVowelCategoryDisplayName(
        string height)
    {
        return
            height switch
            {
                "闭  Close" =>
                    "闭元音  Close vowels",

                "近闭  Near-close" =>
                    "近闭元音  Near-close vowels",

                "半闭  Close-mid" =>
                    "半闭元音  Close-mid vowels",

                "中  Mid" =>
                    "中元音  Mid vowels",

                "半开  Open-mid" =>
                    "半开元音  Open-mid vowels",

                "近开  Near-open" =>
                    "近开元音  Near-open vowels",

                "开  Open" =>
                    "开元音  Open vowels",

                _ =>
                    height
            };
    }


    private static string GetConsonantDescription(
        IpaConsonant consonant)
    {
        string voicing =
            consonant.Voicing.StartsWith(
                "清音")
                ? "清"
                : "浊";


        return
            $"{voicing}" +
            $"{GetChinesePart(consonant.Place)}" +
            $"{GetChinesePart(consonant.Manner)}";
    }


    private static string GetVowelDescription(
        IpaVowel vowel)
    {
        return
            $"{GetChinesePart(vowel.Roundedness)}" +
            $"{GetChinesePart(vowel.Backness)}" +
            $"{GetChinesePart(vowel.Height)}元音";
    }


    private static string GetChinesePart(
        string bilingualText)
    {
        int separatorIndex =
            bilingualText.IndexOf(
                "  ",
                StringComparison.Ordinal);


        return
            separatorIndex >= 0
                ? bilingualText[
                    ..separatorIndex]
                : bilingualText;
    }
}