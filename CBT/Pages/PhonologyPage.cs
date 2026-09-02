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
    private readonly TableLayoutPanel consonantChart = new();
    private readonly Dictionary<(string Place, string Manner, string Voicing), Label>
        consonantChartCells = new();

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

    private SelectionMode selectionMode = SelectionMode.Detailed;
    private bool isSynchronizingSelection;
    private int lastIpaSymbolIndex = -1;

    private enum SelectionMode
    {
        Detailed,
        Guided,
        List
    }

    // 分组标题和 IPA 项目共用的下拉栏显示类型
    private class IpaDisplayItem
    {
        public string Text { get; }
        public IpaConsonant? Consonant { get; }
        public IpaVowel? Vowel { get; }

        // 创建只显示文字、不关联具体音素的分组标题。
        public IpaDisplayItem(string text)
        {
            Text = text;
        }

        // 创建关联辅音数据的下拉栏项目。
        public IpaDisplayItem(string text, IpaConsonant consonant)
        {
            Text = text;
            Consonant = consonant;
        }

        // 创建关联元音数据的下拉栏项目。
        public IpaDisplayItem(string text, IpaVowel vowel)
        {
            Text = text;
            Vowel = vowel;
        }

        // 让下拉栏直接显示项目的双语说明文字。
        public override string ToString()
        {
            return Text;
        }
    }

    private class IpaCategoryItem
    {
        public string Manner { get; }
        public string Text { get; }

        // 保存引导模式中的辅音类别及其显示文字。
        public IpaCategoryItem(string manner, string text)
        {
            Manner = manner;
            Text = text;
        }

        // 返回辅音类别在下拉栏中的显示文字。
        public override string ToString()
        {
            return Text;
        }
    }

    private class IpaVowelCategoryItem
    {
        public string Height { get; }
        public string Text { get; }

        // 保存引导模式中的元音高度类别及其显示文字。
        public IpaVowelCategoryItem(string height, string text)
        {
            Height = height;
            Text = text;
        }

        // 返回元音类别在下拉栏中的显示文字。
        public override string ToString()
        {
            return Text;
        }
    }

    // 辅音和它的语言学属性
    private class ConsonantEntry
    {
        public string Symbol { get; set; } = "";
        public string Place { get; set; } = "";
        public string Manner { get; set; } = "";
        public string Voicing { get; set; } = "";

        // 把辅音符号和主要属性组合成清单中的简短说明。
        public override string ToString()
        {
            string place = Place.Split("  ")[0];
            string manner = Manner.Split("  ")[0];
            string voicing = Voicing.StartsWith("清音") ? "清" : "浊";

            return $"{Symbol}    {place}{voicing}{manner}";
        }
    }
    // 元音和它的语言学属性
    private class VowelEntry
    {
        public string Symbol { get; set; } = "";
        public string Height { get; set; } = "";
        public string Backness { get; set; } = "";
        public string Roundedness { get; set; } = "";

        // 把元音符号和主要属性组合成清单中的简短说明。
        public override string ToString()
        {
            string height = GetChinesePart(Height);
            string backness = GetChinesePart(Backness);
            string roundedness = GetChinesePart(Roundedness);

            return $"{Symbol}    {roundedness}{backness}{height}";
        }
    }

    // 初始化页面布局并创建音素清单区域。
    public PhonologyPage()
        : this(new ConlangProject(), null)
    {
    }

    public PhonologyPage(ConlangProject project, Action? projectModified)
    {
        this.project = project;
        this.projectModified =
            projectModified;

        Dock = DockStyle.Fill;
        Padding = new Padding(30, 0, 30, 30);

        FlowLayoutPanel contentPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0)
        };

        contentPanel.Controls.Add(BuildPhonemeInventory());
        Controls.Add(contentPanel);
        LoadProjectPhonology();
    }

    // 组装输入区、音素清单和辅音音系表的整体界面。
    private Control BuildPhonemeInventory()
    {
        FlowLayoutPanel section = new()
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0)
        };

        Label sectionTitle = new()
        {
            Text = "音素清单  Phoneme Inventory",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 14)
        };

        FlowLayoutPanel sectionHeader = new()
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };

        FlowLayoutPanel inputRow = new()
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 10)
        };

        Label consonantTitle = new()
        {
            Text = "辅音  Consonants",
            AutoSize = true
        };

        Label vowelTitle = new()
        {
            Text = "元音  Vowels",
            AutoSize = true
        };

        TableLayoutPanel phonemeLists = new()
        {
            ColumnCount = 3,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Margin = new Padding(0)
        };

        phonemeLists.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 725));
        phonemeLists.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 30));
        phonemeLists.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 725));
        phonemeLists.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        phonemeLists.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));

        Panel bottomSpacer = new()
        {
            Size = new Size(1, 60),
            Margin = new Padding(0)
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

        sectionHeader.Controls.Add(sectionTitle);
        sectionHeader.Controls.Add(selectionModeButton);

        inputRow.Controls.Add(phonemeInput);
        inputRow.Controls.Add(phonemeType);
        inputRow.Controls.Add(ipaSymbolPicker);
        inputRow.Controls.Add(consonantPlace);
        inputRow.Controls.Add(consonantManner);
        inputRow.Controls.Add(consonantVoicing);
        inputRow.Controls.Add(vowelHeight);
        inputRow.Controls.Add(vowelBackness);
        inputRow.Controls.Add(vowelRoundedness);
        inputRow.Controls.Add(ipaCategory);
        inputRow.Controls.Add(ipaChoice);
        inputRow.Controls.Add(addPhonemeButton);
        inputRow.Controls.Add(removePhonemeButton);
        inputRow.Controls.Add(noMatchingPhonemeLabel);

        UpdateSelectionMode();
        UpdateSymbolFromFeatures();

        consonantTitle.Margin = new Padding(0, 0, 0, 3);
        vowelTitle.Margin = new Padding(0, 0, 0, 3);
        consonantList.Margin = new Padding(0);
        vowelList.Margin = new Padding(0);

        phonemeLists.Controls.Add(consonantTitle, 0, 0);
        phonemeLists.Controls.Add(vowelTitle, 2, 0);
        phonemeLists.Controls.Add(consonantList, 0, 1);
        phonemeLists.Controls.Add(vowelList, 2, 1);

        section.Controls.Add(sectionHeader);
        section.Controls.Add(inputRow);
        section.Controls.Add(phonemeLists);
        section.Controls.Add(BuildConsonantChart());
        section.Controls.Add(BuildVowelChart());
        section.Controls.Add(bottomSpacer);

        return section;
    }

    // 配置三种 IPA 选择模式之间的循环切换按钮。
    private void ConfigureSelectionModeButton()
    {
        selectionModeButton.Text = "切换到引导模式";
        selectionModeButton.AutoSize = true;
        selectionModeButton.Font = new Font("Microsoft YaHei UI", 9);
        selectionModeButton.Margin = new Padding(20, 0, 0, 0);
        selectionModeButton.Click += (sender, e) =>
        {
            selectionMode = selectionMode switch
            {
                SelectionMode.Detailed => SelectionMode.Guided,
                SelectionMode.Guided => SelectionMode.List,
                _ => SelectionMode.Detailed
            };

            UpdateSelectionMode();
        };
    }

    // 配置列表模式的 IPA 下拉栏及其选择事件。
    private void ConfigureIpaSymbolPicker()
    {
        ipaSymbolPicker.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaSymbolPicker.Width = 250;
        ipaSymbolPicker.Font = new Font("Microsoft YaHei UI", 10);
        ipaSymbolPicker.Margin = new Padding(0, 3, 6, 0);

        LoadDetailedIpaPicker();

        ipaSymbolPicker.SelectedIndexChanged += (sender, e) =>
        {
            if (isSynchronizingSelection ||
                ipaSymbolPicker.SelectedItem is not IpaDisplayItem selectedItem)
            {
                return;
            }

            // 分组标题只负责显示，不能成为实际音素。
            if (selectedItem.Consonant == null && selectedItem.Vowel == null)
            {
                isSynchronizingSelection = true;
                ipaSymbolPicker.SelectedIndex = lastIpaSymbolIndex;
                isSynchronizingSelection = false;
                return;
            }

            lastIpaSymbolIndex = ipaSymbolPicker.SelectedIndex;

            if (selectedItem.Consonant != null)
            {
                ApplyConsonant(selectedItem.Consonant);
                return;
            }

            if (selectedItem.Vowel != null)
            {
                ApplyVowel(selectedItem.Vowel);
            }
        };
    }

    // 配置辅音与元音类型选择器，并在类型变化时刷新当前模式。
    private void ConfigurePhonemeType()
    {
        phonemeType.Items.Add("辅音  Consonant");
        phonemeType.Items.Add("元音  Vowel");
        phonemeType.SelectedIndex = 0;
        phonemeType.DropDownStyle = ComboBoxStyle.DropDownList;
        phonemeType.Width = 160;
        phonemeType.Font = new Font("Microsoft YaHei UI", 10);
        phonemeType.Margin = new Padding(0, 3, 6, 0);
        phonemeType.SelectedIndexChanged += (sender, e) => UpdateSelectionMode();
    }

    // 配置引导模式的类别和具体 IPA 两级选择器。
    private void ConfigureGuidedIpaPickers()
    {
        ipaCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaCategory.Width = 220;
        ipaCategory.Font = new Font("Microsoft YaHei UI", 10);
        ipaCategory.Margin = new Padding(0, 3, 6, 0);

        ipaChoice.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaChoice.Width = 240;
        ipaChoice.Font = new Font("Microsoft YaHei UI", 10);
        ipaChoice.Margin = new Padding(0, 3, 6, 0);

        LoadConsonantCategories();

        ipaCategory.SelectedIndexChanged += (sender, e) =>
        {
            if (isSynchronizingSelection)
            {
                return;
            }

            if (ipaCategory.SelectedItem is IpaCategoryItem consonantCategory)
            {
                PopulateGuidedChoices(consonantCategory.Manner);
                return;
            }

            if (ipaCategory.SelectedItem is IpaVowelCategoryItem vowelCategory)
            {
                PopulateGuidedVowelChoices(vowelCategory.Height);
            }
        };

        ipaChoice.SelectedIndexChanged += (sender, e) =>
        {
            if (isSynchronizingSelection ||
                ipaChoice.SelectedItem is not IpaDisplayItem selectedItem)
            {
                return;
            }

            if (selectedItem.Consonant != null)
            {
                ApplyConsonant(selectedItem.Consonant);
                return;
            }

            if (selectedItem.Vowel != null)
            {
                ApplyVowel(selectedItem.Vowel);
            }
        };
    }

    // 配置辅音的发音部位、发音方法和清浊属性选择器。
    private void ConfigureConsonantFeatureSelectors()
    {
        consonantPlace.Items.AddRange(new object[]
        {
            "双唇  Bilabial",
            "唇齿  Labiodental",
            "齿  Dental",
            "齿龈  Alveolar",
            "龈后  Postalveolar",
            "龈腭  Alveolo-palatal",
            "卷舌  Retroflex",
            "硬腭  Palatal",
            "软腭  Velar",
            "小舌  Uvular",
            "咽  Pharyngeal",
            "声门  Glottal",
            "唇软腭  Labial-velar"
        });

        consonantPlace.DropDownStyle = ComboBoxStyle.DropDownList;
        consonantPlace.Width = 180;
        consonantPlace.SelectedIndex = 0;
        consonantPlace.Font = new Font("Microsoft YaHei UI", 10);
        consonantPlace.Margin = new Padding(0, 3, 6, 0);

        consonantManner.Items.AddRange(new object[]
        {
            "塞音  Plosive",
            "鼻音  Nasal",
            "颤音  Trill",
            "闪音  Tap / Flap",
            "擦音  Fricative",
            "边擦音  Lateral fricative",
            "近音  Approximant",
            "边近音  Lateral approximant",
            "塞擦音  Affricate"
        });

        consonantManner.DropDownStyle = ComboBoxStyle.DropDownList;
        consonantManner.Width = 190;
        consonantManner.SelectedIndex = 0;
        consonantManner.Font = new Font("Microsoft YaHei UI", 10);
        consonantManner.Margin = new Padding(0, 3, 6, 0);

        consonantVoicing.Items.AddRange(new object[]
        {
            "清音  Voiceless",
            "浊音  Voiced"
        });

        consonantPlace.SelectedIndexChanged +=
            (sender, e) => UpdateSymbolFromFeatures();
        consonantManner.SelectedIndexChanged +=
            (sender, e) => UpdateSymbolFromFeatures();
        consonantVoicing.SelectedIndexChanged +=
            (sender, e) => UpdateSymbolFromFeatures();

        consonantVoicing.DropDownStyle = ComboBoxStyle.DropDownList;
        consonantVoicing.Width = 150;
        consonantVoicing.SelectedIndex = 0;
        consonantVoicing.Font = new Font("Microsoft YaHei UI", 10);
        consonantVoicing.Margin = new Padding(0, 3, 6, 0);
    }

    // 配置元音的高度、前后度和圆唇属性选择器。
    private void ConfigureVowelFeatureSelectors()
    {
        vowelHeight.Items.AddRange(new object[]
        {
            "闭  Close",
            "近闭  Near-close",
            "半闭  Close-mid",
            "中  Mid",
            "半开  Open-mid",
            "近开  Near-open",
            "开  Open"
        });

        vowelHeight.DropDownStyle = ComboBoxStyle.DropDownList;
        vowelHeight.Width = 170;
        vowelHeight.SelectedIndex = 0;
        vowelHeight.Font = new Font("Microsoft YaHei UI", 10);
        vowelHeight.Margin = new Padding(0, 3, 6, 0);

        vowelBackness.Items.AddRange(new object[]
        {
            "前  Front",
            "央  Central",
            "后  Back"
        });

        vowelBackness.DropDownStyle = ComboBoxStyle.DropDownList;
        vowelBackness.Width = 150;
        vowelBackness.SelectedIndex = 0;
        vowelBackness.Font = new Font("Microsoft YaHei UI", 10);
        vowelBackness.Margin = new Padding(0, 3, 6, 0);

        vowelRoundedness.Items.AddRange(new object[]
        {
            "不圆唇  Unrounded",
            "圆唇  Rounded"
        });

        vowelRoundedness.DropDownStyle = ComboBoxStyle.DropDownList;
        vowelRoundedness.Width = 170;
        vowelRoundedness.SelectedIndex = 0;
        vowelRoundedness.Font = new Font("Microsoft YaHei UI", 10);
        vowelRoundedness.Margin = new Padding(0, 3, 6, 0);

        vowelHeight.SelectedIndexChanged +=
            (sender, e) => UpdateVowelSymbolFromFeatures();
        vowelBackness.SelectedIndexChanged +=
            (sender, e) => UpdateVowelSymbolFromFeatures();
        vowelRoundedness.SelectedIndexChanged +=
            (sender, e) => UpdateVowelSymbolFromFeatures();
    }

    // 配置音素输入框，并在手动输入时尝试识别 IPA 属性。
    private void ConfigurePhonemeInput()
    {
        phonemeInput.Width = 180;
        phonemeInput.Font = new Font("Microsoft YaHei UI", 12);
        phonemeInput.Margin = new Padding(0, 3, 6, 0);
        phonemeInput.TextChanged += (sender, e) => UpdateFeaturesFromSymbol();
    }

    // 配置音素的添加和删除按钮及其点击事件。
    private void ConfigureActionButtons()
    {
        addPhonemeButton.Text = "添加";
        addPhonemeButton.Width = 100;
        addPhonemeButton.Height = phonemeType.PreferredHeight;
        addPhonemeButton.Font = new Font("Microsoft YaHei UI", 10);
        addPhonemeButton.Margin = new Padding(0, 3, 6, 0);
        addPhonemeButton.Click += AddPhoneme;

        removePhonemeButton.Text = "删除";
        removePhonemeButton.Width = 100;
        removePhonemeButton.Height = phonemeType.PreferredHeight;
        removePhonemeButton.Font = new Font("Microsoft YaHei UI", 10);
        removePhonemeButton.Margin = new Padding(0, 3, 0, 0);
        removePhonemeButton.Click += RemovePhoneme;
    }

    // 配置辅音与元音清单的尺寸和字体。
    private void ConfigurePhonemeLists()
    {
        consonantList.Size = new Size(725, 180);
        consonantList.Font = new Font("Microsoft YaHei UI", 12);

        vowelList.Size = new Size(725, 180);
        vowelList.Font = new Font("Microsoft YaHei UI", 12);
    }

    // 配置找不到属性组合时显示的提示标签。
    private void ConfigureNoMatchingPhonemeLabel()
    {
        noMatchingPhonemeLabel.Text = "无此音素  No corresponding phoneme";
        noMatchingPhonemeLabel.AutoSize = true;
        noMatchingPhonemeLabel.Font = new Font("Microsoft YaHei UI", 9);
        noMatchingPhonemeLabel.Margin = new Padding(8, 7, 0, 0);
        noMatchingPhonemeLabel.Visible = false;
    }

    // 创建按发音方法、部位和清浊排列的辅音音系表。
    private Control BuildConsonantChart()
    {
        Panel chartContainer = new()
        {
            Width = 1480,
            AutoScroll = true,
            Margin = new Padding(0, 15, 0, 25)
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
            "软腭\nVelar",
            "小舌\nUvular",
            "咽\nPharyngeal",
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
            "软腭  Velar",
            "小舌  Uvular",
            "咽  Pharyngeal",
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
            "擦音  Fricative",
            "边擦音  Lateral fricative",
            "近音  Approximant",
            "边近音  Lateral approximant"
        };

        consonantChart.Controls.Clear();
        consonantChart.ColumnStyles.Clear();
        consonantChart.RowStyles.Clear();
        consonantChartCells.Clear();

        consonantChart.RowCount = manners.Length + 1;
        consonantChart.ColumnCount = places.Length + 1;
        consonantChart.AutoSize = true;
        consonantChart.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
        consonantChart.Location = new Point(0, 0);

        consonantChart.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 150));

        foreach (string place in places)
        {
            consonantChart.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 100));
        }

        consonantChart.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 45));

        for (int column = 0; column < places.Length; column++)
        {
            Label label = new()
            {
                Text = places[column],
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 9)
            };

            consonantChart.Controls.Add(label, column + 1, 0);
        }

        for (int row = 0; row < manners.Length; row++)
        {
            consonantChart.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            Label mannerLabel = new()
            {
                Text = manners[row],
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 9)
            };

            consonantChart.Controls.Add(mannerLabel, 0, row + 1);

            // 每个格子预留清音和浊音两个位置。
            for (int column = 0; column < places.Length; column++)
            {
                TableLayoutPanel cell = new()
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Margin = new Padding(0)
                };

                cell.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 50));
                cell.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 50));

                Label voiceless = new()
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                Label voiced = new()
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                consonantChartCells[
                    (placeKeys[column], mannerKeys[row], "清音  Voiceless")
                ] = voiceless;
                consonantChartCells[
                    (placeKeys[column], mannerKeys[row], "浊音  Voiced")
                ] = voiced;

                cell.Controls.Add(voiceless, 0, 0);
                cell.Controls.Add(voiced, 1, 0);
                consonantChart.Controls.Add(cell, column + 1, row + 1);
            }
        }

        chartContainer.Controls.Add(consonantChart);
        chartContainer.Height =
            consonantChart.PreferredSize.Height +
            SystemInformation.HorizontalScrollBarHeight + 5;

        return chartContainer;
    }

    // 创建 IPA 风格的元音梯形图。
    private Control BuildVowelChart()
    {
        Panel chartContainer = new()
        {
            Width = 1000,
            Height = 430,
            Margin = new Padding(0, 20, 0, 25)
        };

        vowelChart.Size = new Size(900, 390);
        vowelChart.Location = new Point(0, 0);

        vowelChartCells.Clear();

        // 绘制元音梯形的结构线。
        vowelChart.Paint += (sender, e) =>
        {
            using Pen pen = new(Color.Gray, 1);

            Point topLeft = new(250, 55);
            Point topRight = new(730, 55);
            Point bottomLeft = new(370, 340);
            Point bottomRight = new(730, 340);

            // 外框
            e.Graphics.DrawLine(pen, topLeft, topRight);
            e.Graphics.DrawLine(pen, topLeft, bottomLeft);
            e.Graphics.DrawLine(pen, topRight, bottomRight);
            e.Graphics.DrawLine(pen, bottomLeft, bottomRight);

            // 中央参考线
            e.Graphics.DrawLine(
                pen,
                new Point(490, 55),
                new Point(550, 340));
        };

        // 前、央、后
        AddVowelChartHeader(
            "前  Front",
            new Point(220, 10));

        AddVowelChartHeader(
            "央  Central",
            new Point(450, 10));

        AddVowelChartHeader(
            "后  Back",
            new Point(700, 10));

        // 高度标签
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

        // 根据现有 IPA 元音数据库自动创建对应位置。
        foreach (IpaVowel vowel in IpaVowels.All)
        {
            Point anchor =
                GetVowelChartPosition(
                    vowel.Height,
                    vowel.Backness);

            bool hasRoundedPair =
                IpaVowels.All.Any(x =>
                    x.Height == vowel.Height &&
                    x.Backness == vowel.Backness &&
                    x.Roundedness != vowel.Roundedness);

            int x;

            if (!hasRoundedPair)
            {
                x = anchor.X - 20;
            }
            else if (vowel.Roundedness.StartsWith("不圆唇"))
            {
                x = anchor.X - 42;
            }
            else
            {
                x = anchor.X + 2;
            }

            Label cell = new()
            {
                Text = "",
                Location = new Point(x, anchor.Y - 14),
                Size = new Size(40, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 14)
            };

            vowelChartCells[
                (vowel.Height,
                 vowel.Backness,
                 vowel.Roundedness)
            ] = cell;

            vowelChart.Controls.Add(cell);
        }

        chartContainer.Controls.Add(vowelChart);

        return chartContainer;
    }
    // 添加元音图顶部的前后度标题。
    private void AddVowelChartHeader(
        string text,
        Point location)
    {
        Label label = new()
        {
            Text = text,
            AutoSize = true,
            Location = location,
            Font = new Font(
                "Microsoft YaHei UI",
                10,
                FontStyle.Bold)
        };

        vowelChart.Controls.Add(label);
    }
    // 添加元音图左侧的高度标题。
    private void AddVowelHeightLabel(
        string text,
        int y)
    {
        Label label = new()
        {
            Text = text,
            Size = new Size(180, 30),
            Location = new Point(10, y - 14),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font(
                "Microsoft YaHei UI",
                9)
        };

        vowelChart.Controls.Add(label);
    }
    // 根据当前项目中的元音重新填充元音图。
    private void RefreshVowelChart()
    {
        foreach (Label cell in vowelChartCells.Values)
        {
            cell.Text = "";
        }

        foreach (VowelPhoneme vowel in project.Phonology.Vowels)
        {
            var key =
                (
                    vowel.Height,
                    vowel.Backness,
                    vowel.Roundedness
                );

            if (!vowelChartCells.TryGetValue(
                key,
                out Label? cell))
            {
                continue;
            }

            if (cell.Text.Length == 0)
            {
                cell.Text = vowel.Symbol;
            }
            else
            {
                cell.Text += $" {vowel.Symbol}";
            }
        }
    }
    // 根据元音高度和前后度计算其在梯形图中的位置。
    private static Point GetVowelChartPosition(
        string height,
        string backness)
    {
        int row = height switch
        {
            "闭  Close" => 0,
            "近闭  Near-close" => 1,
            "半闭  Close-mid" => 2,
            "中  Mid" => 3,
            "半开  Open-mid" => 4,
            "近开  Near-open" => 5,
            "开  Open" => 6,
            _ => 0
        };

        int y = 55 + row * 45;

        // 越靠近开元音，前元音的位置越向右，
        // 从而形成 IPA 元音梯形。
        int frontX = 250 + row * 20;

        int backX = 730;

        int centralX =
            (frontX + backX) / 2;

        int x = backness switch
        {
            "前  Front" => frontX,
            "央  Central" => centralX,
            "后  Back" => backX,
            _ => centralX
        };

        return new Point(x, y);
    }

    // 根据音素类型和选择模式切换可见控件，并同步当前音素。
    private void UpdateSelectionMode()
    {
        bool isConsonant = phonemeType.SelectedIndex == 0;
        bool isVowel = phonemeType.SelectedIndex == 1;
        IpaConsonant? currentConsonant = FindConsonantFromInput();

        bool detailedMode = selectionMode == SelectionMode.Detailed;
        bool guidedMode = selectionMode == SelectionMode.Guided;
        bool listMode = selectionMode == SelectionMode.List;

        ipaSymbolPicker.Visible = listMode;
        consonantPlace.Visible = detailedMode && isConsonant;
        consonantManner.Visible = detailedMode && isConsonant;
        consonantVoicing.Visible = detailedMode && isConsonant;
        vowelHeight.Visible = detailedMode && isVowel;
        vowelBackness.Visible = detailedMode && isVowel;
        vowelRoundedness.Visible = detailedMode && isVowel;
        ipaCategory.Visible = guidedMode;
        ipaChoice.Visible = guidedMode;

        selectionModeButton.Text = selectionMode switch
        {
            SelectionMode.Detailed => "切换到引导模式",
            SelectionMode.Guided => "切换到列表模式",
            SelectionMode.List => "切换到详细模式",
            _ => "切换模式"
        };

        if (selectionMode == SelectionMode.Detailed)
        {
            if (isConsonant)
            {
                if (currentConsonant != null)
                {
                    SelectDetailedConsonant(currentConsonant);
                }
            }
            else
            {
                UpdateVowelSymbolFromFeatures();
            }

            return;
        }

        if (selectionMode == SelectionMode.List)
        {
            if (isConsonant)
            {
                LoadDetailedIpaPicker();

                if (currentConsonant != null)
                {
                    SelectDetailedConsonant(currentConsonant);
                }
            }
            else
            {
                LoadVowelListPicker();

                IpaVowel? currentVowel = FindVowelFromInput();

                if (currentVowel != null)
                {
                    SelectListVowel(currentVowel);
                }
            }

            return;
        }

        if (isConsonant)
        {
            ipaCategory.Enabled = true;
            ipaChoice.Enabled = true;
            EnsureConsonantCategories();

            if (currentConsonant != null)
            {
                SelectGuidedConsonant(currentConsonant);
            }
        }
        else
        {
            ipaCategory.Enabled = true;
            ipaChoice.Enabled = true;
            LoadVowelCategories();

            IpaVowel? currentVowel = FindVowelFromInput();

            if (currentVowel != null)
            {
                SelectGuidedVowel(currentVowel);
            }
            else
            {
                UpdateVowelSymbolFromFeatures();
            }
        }
    }

    // 把全部辅音按发音方法分组加载到列表模式下拉栏。
    private void LoadDetailedIpaPicker()
    {
        ipaSymbolPicker.BeginUpdate();
        ipaSymbolPicker.Items.Clear();

        foreach (IGrouping<string, IpaConsonant> category in
            IpaConsonants.All.GroupBy(x => x.Manner))
        {
            ipaSymbolPicker.Items.Add(
                new IpaDisplayItem(
                    $"── {GetCategoryDisplayName(category.Key)} ──"));

            foreach (IpaConsonant consonant in category)
            {
                ipaSymbolPicker.Items.Add(
                    new IpaDisplayItem(
                        $"{consonant.Symbol}   {GetConsonantDescription(consonant)}",
                        consonant));
            }
        }

        ipaSymbolPicker.SelectedIndex = -1;
        lastIpaSymbolIndex = -1;
        ipaSymbolPicker.EndUpdate();
    }

    // 把辅音的发音方法加载为引导模式的一级类别。
    private void LoadConsonantCategories()
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaCategory.BeginUpdate();
        ipaCategory.Items.Clear();

        foreach (string manner in IpaConsonants.All
            .Select(x => x.Manner)
            .Distinct())
        {
            ipaCategory.Items.Add(
                new IpaCategoryItem(manner, GetCategoryDisplayName(manner)));
        }

        ipaCategory.SelectedIndex = -1;
        ipaCategory.EndUpdate();
        ipaChoice.Items.Clear();

        isSynchronizingSelection = wasSynchronizing;
    }

    // 根据选中的发音方法加载引导模式中的具体辅音。
    private void PopulateGuidedChoices(string manner)
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaChoice.BeginUpdate();
        ipaChoice.Items.Clear();

        foreach (IpaConsonant consonant in
            IpaConsonants.All.Where(x => x.Manner == manner))
        {
            ipaChoice.Items.Add(
                new IpaDisplayItem(
                    $"{consonant.Symbol}   {GetConsonantDescription(consonant)}",
                    consonant));
        }

        ipaChoice.SelectedIndex = -1;
        ipaChoice.EndUpdate();

        isSynchronizingSelection = wasSynchronizing;
    }

    // 从元音类别切回辅音时，确保辅音类别已经重新加载。
    private void EnsureConsonantCategories()
    {
        if (ipaCategory.Items.OfType<IpaCategoryItem>().Any())
        {
            return;
        }

        LoadConsonantCategories();
    }

    // 把一个辅音同步到输入框、属性框和两种 IPA 选择器。
    private void ApplyConsonant(IpaConsonant consonant)
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        phonemeType.SelectedIndex = 0;
        phonemeInput.Text = consonant.Symbol;
        phonemeInput.SelectionStart = phonemeInput.Text.Length;

        consonantPlace.SelectedItem = consonant.Place;
        consonantManner.SelectedItem = consonant.Manner;
        consonantVoicing.SelectedItem = consonant.Voicing;

        SelectDetailedConsonant(consonant);
        SelectGuidedConsonant(consonant);

        isSynchronizingSelection = wasSynchronizing;
    }

    // 在列表模式下拉栏中定位指定辅音。
    private void SelectDetailedConsonant(IpaConsonant consonant)
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        for (int index = 0; index < ipaSymbolPicker.Items.Count; index++)
        {
            if (ipaSymbolPicker.Items[index] is IpaDisplayItem item &&
                item.Consonant?.Symbol == consonant.Symbol)
            {
                ipaSymbolPicker.SelectedIndex = index;
                lastIpaSymbolIndex = index;
                break;
            }
        }

        isSynchronizingSelection = wasSynchronizing;
    }

    // 在引导模式的类别和具体音素下拉栏中定位指定辅音。
    private void SelectGuidedConsonant(IpaConsonant consonant)
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        EnsureConsonantCategories();

        for (int index = 0; index < ipaCategory.Items.Count; index++)
        {
            if (ipaCategory.Items[index] is IpaCategoryItem category &&
                category.Manner == consonant.Manner)
            {
                ipaCategory.SelectedIndex = index;
                break;
            }
        }

        PopulateGuidedChoices(consonant.Manner);

        for (int index = 0; index < ipaChoice.Items.Count; index++)
        {
            if (ipaChoice.Items[index] is IpaDisplayItem item &&
                item.Consonant?.Symbol == consonant.Symbol)
            {
                ipaChoice.SelectedIndex = index;
                break;
            }
        }

        isSynchronizingSelection = wasSynchronizing;
    }

    // 根据输入框中的符号查找对应的辅音数据。
    private IpaConsonant? FindConsonantFromInput()
    {
        string symbol = NormalizeInputSymbol(phonemeInput.Text.Trim());

        return IpaConsonants.All.FirstOrDefault(x => x.Symbol == symbol);
    }

    // 把普通拉丁字母 g 统一为 IPA 使用的单层 ɡ。
    private static string NormalizeInputSymbol(string symbol)
    {
        return symbol == "g" ? "ɡ" : symbol;
    }

    // 根据当前辅音属性查找 IPA 符号，并更新输入与提示状态。
    private void UpdateSymbolFromFeatures()
    {
        if (isSynchronizingSelection || phonemeType.SelectedIndex != 0)
        {
            return;
        }

        IpaConsonant? match = IpaConsonants.All.FirstOrDefault(x =>
            x.Place == consonantPlace.Text &&
            x.Manner == consonantManner.Text &&
            x.Voicing == consonantVoicing.Text);

        if (match == null)
        {
            bool wasSynchronizing = isSynchronizingSelection;
            isSynchronizingSelection = true;

            phonemeInput.Clear();

            isSynchronizingSelection = wasSynchronizing;
            ClearIpaSelections();
            noMatchingPhonemeLabel.Visible = true;
            addPhonemeButton.Enabled = false;
            return;
        }

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;
        ApplyConsonant(match);
    }

    // 把全部元音按高度分组加载到列表模式下拉栏。
    private void LoadVowelListPicker()
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaSymbolPicker.BeginUpdate();
        ipaSymbolPicker.Items.Clear();

        foreach (IGrouping<string, IpaVowel> category in
            IpaVowels.All.GroupBy(x => x.Height))
        {
            ipaSymbolPicker.Items.Add(
                new IpaDisplayItem(
                    $"── {GetVowelCategoryDisplayName(category.Key)} ──"));

            foreach (IpaVowel vowel in category)
            {
                ipaSymbolPicker.Items.Add(
                    new IpaDisplayItem(
                        $"{vowel.Symbol}   {GetVowelDescription(vowel)}",
                        vowel));
            }
        }

        ipaSymbolPicker.SelectedIndex = -1;
        lastIpaSymbolIndex = -1;
        ipaSymbolPicker.EndUpdate();

        isSynchronizingSelection = wasSynchronizing;
    }

    // 把元音高度加载为引导模式的一级类别。
    private void LoadVowelCategories()
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaCategory.BeginUpdate();
        ipaCategory.Items.Clear();

        foreach (string height in IpaVowels.All
            .Select(x => x.Height)
            .Distinct())
        {
            ipaCategory.Items.Add(
                new IpaVowelCategoryItem(
                    height,
                    GetVowelCategoryDisplayName(height)));
        }

        ipaCategory.SelectedIndex = -1;
        ipaCategory.EndUpdate();
        ipaChoice.Items.Clear();

        isSynchronizingSelection = wasSynchronizing;
    }

    // 根据选中的元音高度加载引导模式中的具体元音。
    private void PopulateGuidedVowelChoices(string height)
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaChoice.BeginUpdate();
        ipaChoice.Items.Clear();

        foreach (IpaVowel vowel in
            IpaVowels.All.Where(x => x.Height == height))
        {
            ipaChoice.Items.Add(
                new IpaDisplayItem(
                    $"{vowel.Symbol}   {GetVowelDescription(vowel)}",
                    vowel));
        }

        ipaChoice.SelectedIndex = -1;
        ipaChoice.EndUpdate();

        isSynchronizingSelection = wasSynchronizing;
    }

    // 把一个元音同步到输入框、属性框和两种 IPA 选择器。
    private void ApplyVowel(IpaVowel vowel)
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        phonemeInput.Text = vowel.Symbol;
        phonemeInput.SelectionStart = phonemeInput.Text.Length;

        vowelHeight.SelectedItem = vowel.Height;
        vowelBackness.SelectedItem = vowel.Backness;
        vowelRoundedness.SelectedItem = vowel.Roundedness;

        SelectListVowel(vowel);
        SelectGuidedVowel(vowel);

        isSynchronizingSelection = wasSynchronizing;
    }

    // 在列表模式下拉栏中定位指定元音。
    private void SelectListVowel(IpaVowel vowel)
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        for (int index = 0; index < ipaSymbolPicker.Items.Count; index++)
        {
            if (ipaSymbolPicker.Items[index] is IpaDisplayItem item &&
                item.Vowel?.Symbol == vowel.Symbol)
            {
                ipaSymbolPicker.SelectedIndex = index;
                lastIpaSymbolIndex = index;
                break;
            }
        }

        isSynchronizingSelection = wasSynchronizing;
    }

    // 在引导模式的类别和具体音素下拉栏中定位指定元音。
    private void SelectGuidedVowel(IpaVowel vowel)
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        if (!ipaCategory.Items.OfType<IpaVowelCategoryItem>().Any())
        {
            LoadVowelCategories();
        }

        for (int index = 0; index < ipaCategory.Items.Count; index++)
        {
            if (ipaCategory.Items[index] is IpaVowelCategoryItem category &&
                category.Height == vowel.Height)
            {
                ipaCategory.SelectedIndex = index;
                break;
            }
        }

        PopulateGuidedVowelChoices(vowel.Height);

        for (int index = 0; index < ipaChoice.Items.Count; index++)
        {
            if (ipaChoice.Items[index] is IpaDisplayItem item &&
                item.Vowel?.Symbol == vowel.Symbol)
            {
                ipaChoice.SelectedIndex = index;
                break;
            }
        }

        isSynchronizingSelection = wasSynchronizing;
    }

    // 根据输入框中的符号查找对应的元音数据。
    private IpaVowel? FindVowelFromInput()
    {
        string symbol = phonemeInput.Text.Trim();

        return IpaVowels.All.FirstOrDefault(x => x.Symbol == symbol);
    }

    // 根据当前元音属性查找 IPA 符号，并更新输入与提示状态。
    private void UpdateVowelSymbolFromFeatures()
    {
        if (isSynchronizingSelection || phonemeType.SelectedIndex != 1)
        {
            return;
        }

        IpaVowel? match = IpaVowels.All.FirstOrDefault(x =>
            x.Height == vowelHeight.Text &&
            x.Backness == vowelBackness.Text &&
            x.Roundedness == vowelRoundedness.Text);

        if (match == null)
        {
            bool wasSynchronizing = isSynchronizingSelection;
            isSynchronizingSelection = true;

            phonemeInput.Clear();

            isSynchronizingSelection = wasSynchronizing;
            noMatchingPhonemeLabel.Visible = true;
            addPhonemeButton.Enabled = false;
            return;
        }

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;
        ApplyVowel(match);
    }

    // 根据手动输入的 IPA 符号反向同步辅音或元音属性。
    private void UpdateFeaturesFromSymbol()
    {
        if (isSynchronizingSelection)
        {
            return;
        }

        string input = phonemeInput.Text.Trim();

        // 输入框为空时，不显示错误，
        // 但不能允许添加音素。
        if (input.Length == 0)
        {
            ClearIpaSelections();

            noMatchingPhonemeLabel.Visible = false;
            addPhonemeButton.Enabled = false;

            return;
        }

        // 辅音
        if (phonemeType.SelectedIndex == 0)
        {
            IpaConsonant? consonant =
                FindConsonantFromInput();

            if (consonant == null)
            {
                ClearIpaSelections();

                noMatchingPhonemeLabel.Visible = true;
                addPhonemeButton.Enabled = false;

                return;
            }

            noMatchingPhonemeLabel.Visible = false;
            addPhonemeButton.Enabled = true;

            ApplyConsonant(consonant);

            return;
        }

        // 元音
        IpaVowel? vowel =
            FindVowelFromInput();

        if (vowel == null)
        {
            ClearIpaSelections();

            noMatchingPhonemeLabel.Visible = true;
            addPhonemeButton.Enabled = false;

            return;
        }

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;

        ApplyVowel(vowel);
    }

    // 清除两个 IPA 选择器中的当前选择和历史索引。
    private void ClearIpaSelections()
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaSymbolPicker.SelectedIndex = -1;
        ipaChoice.SelectedIndex = -1;
        lastIpaSymbolIndex = -1;

        isSynchronizingSelection = wasSynchronizing;
    }
    // 把项目中的音系数据加载到页面控件。
    private void LoadProjectPhonology()
    {
        consonantList.Items.Clear();
        vowelList.Items.Clear();

        foreach (ConsonantPhoneme consonant in project.Phonology.Consonants)
        {
            consonantList.Items.Add(
                new ConsonantEntry
                {
                    Symbol = consonant.Symbol,
                    Place = consonant.Place,
                    Manner = consonant.Manner,
                    Voicing = consonant.Voicing
                });
        }

        foreach (VowelPhoneme vowel in project.Phonology.Vowels)
        {
            vowelList.Items.Add(
                new VowelEntry
                {
                    Symbol = vowel.Symbol,
                    Height = vowel.Height,
                    Backness = vowel.Backness,
                    Roundedness = vowel.Roundedness
                });
        }

        RefreshConsonantChart();
        RefreshVowelChart();
    }
    // 把当前输入的音素加入项目。
    // 只有 IPA 参考数据库中存在的音素才能被加入。
    private void AddPhoneme(object? sender, EventArgs e)
    {
        string phoneme =
            phonemeInput.Text.Trim();

        if (phoneme.Length == 0)
        {
            return;
        }

        // 添加辅音
        if (phonemeType.SelectedIndex == 0)
        {
            phoneme =
                NormalizeInputSymbol(phoneme);

            // 必须从 IPA 数据库中找到对应辅音。
            IpaConsonant? ipaConsonant =
                IpaConsonants.All.FirstOrDefault(
                    x => x.Symbol == phoneme);

            if (ipaConsonant == null)
            {
                noMatchingPhonemeLabel.Visible = true;
                addPhonemeButton.Enabled = false;

                return;
            }

            // 防止重复添加。
            if (project.Phonology.Consonants.Any(
                x => x.Symbol == phoneme))
            {
                return;
            }

            // 属性直接来自 IPA 参考数据库，
            // 而不是依赖界面下拉框当前残留的状态。
            ConsonantPhoneme projectConsonant = new()
            {
                Symbol = ipaConsonant.Symbol,
                Place = ipaConsonant.Place,
                Manner = ipaConsonant.Manner,
                Voicing = ipaConsonant.Voicing
            };

            project.Phonology.Consonants.Add(
                projectConsonant);

            ConsonantEntry displayConsonant = new()
            {
                Symbol = projectConsonant.Symbol,
                Place = projectConsonant.Place,
                Manner = projectConsonant.Manner,
                Voicing = projectConsonant.Voicing
            };

            consonantList.Items.Add(
                displayConsonant);

            RefreshConsonantChart();
        }

        // 添加元音
        else
        {
            // 必须从 IPA 数据库中找到对应元音。
            IpaVowel? ipaVowel =
                IpaVowels.All.FirstOrDefault(
                    x => x.Symbol == phoneme);

            if (ipaVowel == null)
            {
                noMatchingPhonemeLabel.Visible = true;
                addPhonemeButton.Enabled = false;

                return;
            }

            // 防止重复添加。
            if (project.Phonology.Vowels.Any(
                x => x.Symbol == phoneme))
            {
                return;
            }

            // 属性直接来自 IPA 参考数据库。
            VowelPhoneme projectVowel = new()
            {
                Symbol = ipaVowel.Symbol,
                Height = ipaVowel.Height,
                Backness = ipaVowel.Backness,
                Roundedness = ipaVowel.Roundedness
            };

            project.Phonology.Vowels.Add(
                projectVowel);

            VowelEntry displayVowel = new()
            {
                Symbol = projectVowel.Symbol,
                Height = projectVowel.Height,
                Backness = projectVowel.Backness,
                Roundedness = projectVowel.Roundedness
            };

            vowelList.Items.Add(
                displayVowel);

            RefreshVowelChart();
        }

        // 通知主窗口：项目已经修改。
        projectModified?.Invoke();

        phonemeInput.Clear();
        phonemeInput.Focus();
    }

    // 删除当前选中的辅音或元音，同时修改项目数据。
    private void RemovePhoneme(object? sender, EventArgs e)
    {
        if (consonantList.SelectedItem is ConsonantEntry consonant)
        {
            project.Phonology.Consonants.RemoveAll(
                x => x.Symbol == consonant.Symbol);

            consonantList.Items.Remove(consonant);

            RefreshConsonantChart();
            projectModified?.Invoke();

            return;
        }

        if (vowelList.SelectedItem is VowelEntry vowel)
        {
            project.Phonology.Vowels.RemoveAll(
                x => x.Symbol == vowel.Symbol);

            vowelList.Items.Remove(vowel);

            RefreshVowelChart();
            projectModified?.Invoke();
        }
    }

    // 按辅音清单内容重新填充音系表中的对应单元格。
    private void RefreshConsonantChart()
    {
        foreach (Label cell in consonantChartCells.Values)
        {
            cell.Text = "";
        }

        foreach (ConsonantEntry consonant in consonantList.Items)
        {
            var key =
                (consonant.Place, consonant.Manner, consonant.Voicing);

            if (!consonantChartCells.TryGetValue(key, out Label? cell))
            {
                continue;
            }

            if (cell.Text.Length == 0)
            {
                cell.Text = consonant.Symbol;
            }
            else
            {
                cell.Text += $" {consonant.Symbol}";
            }

            cell.Font = new Font("Microsoft YaHei UI", 12);
        }
    }

    // 把辅音类别转换成适合下拉栏显示的双语复数名称。
    private static string GetCategoryDisplayName(string manner)
    {
        return manner switch
        {
            "塞音  Plosive" => "塞音  Plosives",
            "塞擦音  Affricate" => "塞擦音  Affricates",
            "鼻音  Nasal" => "鼻音  Nasals",
            "颤音  Trill" => "颤音  Trills",
            "闪音  Tap / Flap" => "闪音  Taps / Flaps",
            "擦音  Fricative" => "擦音  Fricatives",
            "边擦音  Lateral fricative" => "边擦音  Lateral fricatives",
            "近音  Approximant" => "近音  Approximants",
            "边近音  Lateral approximant" => "边近音  Lateral approximants",
            _ => manner
        };
    }

    // 把元音高度转换成适合分组标题显示的双语名称。
    private static string GetVowelCategoryDisplayName(string height)
    {
        return height switch
        {
            "闭  Close" => "闭元音  Close vowels",
            "近闭  Near-close" => "近闭元音  Near-close vowels",
            "半闭  Close-mid" => "半闭元音  Close-mid vowels",
            "中  Mid" => "中元音  Mid vowels",
            "半开  Open-mid" => "半开元音  Open-mid vowels",
            "近开  Near-open" => "近开元音  Near-open vowels",
            "开  Open" => "开元音  Open vowels",
            _ => height
        };
    }

    // 把辅音的清浊、部位和方法组合成中文简短说明。
    private static string GetConsonantDescription(IpaConsonant consonant)
    {
        string voicing = consonant.Voicing.StartsWith("清音") ? "清" : "浊";

        return $"{voicing}{GetChinesePart(consonant.Place)}" +
            $"{GetChinesePart(consonant.Manner)}";
    }

    // 把元音的圆唇、前后度和高度组合成中文简短说明。
    private static string GetVowelDescription(IpaVowel vowel)
    {
        return $"{GetChinesePart(vowel.Roundedness)}" +
            $"{GetChinesePart(vowel.Backness)}" +
            $"{GetChinesePart(vowel.Height)}元音";
    }

    // 从双语属性文字中取出两个空格之前的中文部分。
    private static string GetChinesePart(string bilingualText)
    {
        int separatorIndex = bilingualText.IndexOf("  ", StringComparison.Ordinal);

        return separatorIndex >= 0
            ? bilingualText[..separatorIndex]
            : bilingualText;
    }
}
