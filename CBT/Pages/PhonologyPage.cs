using CBT.Data;
using CBT.Dialogs;
using CBT.Models;
using CBT.Services;

namespace CBT.Pages;

public class PhonologyPage : UserControl
{
    private readonly Button addPhonemeButton = new();

    //普通辅音表
    private readonly TableLayoutPanel consonantChart = new();
    private readonly Dictionary<(string Place, string Manner, string Voicing), Label> consonantChartCells = new();
    private readonly ListBox consonantList = new();

    //辅音控件
    private readonly ComboBox consonantPlace = new();
    private readonly ComboBox consonantManner = new();
    private readonly ComboBox consonantVoicing = new();

    //IPA附加符号组合
    private readonly Button diacriticButton = new();

    //IPA选择控件
    private readonly ComboBox ipaCategory = new();
    private readonly ComboBox ipaChoice = new();
    private readonly ComboBox ipaSymbolPicker = new();
    private readonly Button selectionModeButton = new();

    private readonly Label noMatchingPhonemeLabel = new();

    //非肺部气流辅音表
    private readonly TableLayoutPanel nonPulmonicChart = new();
    private readonly Dictionary<string, Label> nonPulmonicChartCells = new();

    //Other IPA Symbols显示区域
    private readonly Label otherSymbolsContent = new();

    //音素输入与操作
    private readonly TextBox phonemeInput = new();
    private readonly ComboBox phonemeType = new();
    private readonly Button removePhonemeButton = new();

    private readonly ConlangProject project;
    private readonly Action? projectModified;

    //元音图
    private readonly Panel vowelChart = new();
    private readonly Dictionary<(string Height, string Backness, string Roundedness), Label> vowelChartCells = new();

    //元音控件
    private readonly ComboBox vowelHeight = new();
    private readonly ComboBox vowelBackness = new();
    private readonly ComboBox vowelRoundedness = new();
    private readonly ComboBox vowelLength = new();
    private readonly ListBox vowelList = new();

    private bool isSynchronizingSelection;
    private int lastIpaSymbolIndex = -1;

    //当前等待添加的基础音素和Diacritics
    private string pendingBaseSymbol = "";
    private List<string> pendingDiacritics = new();
    private string pendingLengthMark = "";

    private SelectionMode selectionMode = SelectionMode.Detailed;

    public PhonologyPage() : this(new ConlangProject(), null)
    {
    }

    public PhonologyPage(ConlangProject project, Action? projectModified)
    {
        this.project = project;
        this.projectModified = projectModified;

        Dock = DockStyle.Fill;
        Padding = new Padding(0);

        //构建期间暂停布局，避免中间态被绘制出来。
        SuspendLayout();
        try
        {
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
        finally
        {
            ResumeLayout(true);
        }
    }

    //创建整个Phonology页面
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

        Label consonantTitle = new() { Text = "辅音  Consonants", AutoSize = true };
        Label vowelTitle = new() { Text = "元音  Vowels", AutoSize = true };

        TableLayoutPanel phonemeLists = new()
        {
            ColumnCount = 3,
            RowCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Margin = new Padding(0)
        };

        phonemeLists.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 725));
        phonemeLists.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
        phonemeLists.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 725));
        phonemeLists.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        phonemeLists.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));

        Panel bottomSpacer = new() { Size = new Size(1, 60), Margin = new Padding(0) };

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
        inputRow.Controls.Add(vowelLength);
        inputRow.Controls.Add(diacriticButton);
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
        section.Controls.Add(BuildLowerChartsRow());
        section.Controls.Add(bottomSpacer);

        return section;
    }

    //切换Detailed/Guided/List
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

    //List mode下拉栏
    private void ConfigureIpaSymbolPicker()
    {
        ipaSymbolPicker.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaSymbolPicker.Width = 250;
        ipaSymbolPicker.DropDownWidth = 560;
        ipaSymbolPicker.Font = new Font("Microsoft YaHei UI", 10);
        ipaSymbolPicker.Margin = new Padding(0, 3, 6, 0);

        LoadDetailedIpaPicker();

        ipaSymbolPicker.SelectedIndexChanged += (sender, e) =>
        {
            if (isSynchronizingSelection || ipaSymbolPicker.SelectedItem is not IpaDisplayItem selectedItem)
                return;

            //分组标题不能作为音素选择
            if (selectedItem.Consonant == null &&
                selectedItem.NonPulmonicConsonant == null &&
                selectedItem.OtherSymbol == null &&
                selectedItem.Vowel == null)
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

            if (selectedItem.NonPulmonicConsonant != null)
            {
                ApplyNonPulmonicConsonant(selectedItem.NonPulmonicConsonant);
                return;
            }

            if (selectedItem.OtherSymbol != null)
            {
                ApplyOtherSymbol(selectedItem.OtherSymbol);
                return;
            }

            if (selectedItem.Vowel != null)
                ApplyVowel(selectedItem.Vowel);
        };
    }

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

    private void ConfigureGuidedIpaPickers()
    {
        ipaCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaCategory.Width = 220;
        ipaCategory.Font = new Font("Microsoft YaHei UI", 10);
        ipaCategory.Margin = new Padding(0, 3, 6, 0);

        ipaChoice.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaChoice.Width = 240;
        ipaChoice.DropDownWidth = 560;
        ipaChoice.Font = new Font("Microsoft YaHei UI", 10);
        ipaChoice.Margin = new Padding(0, 3, 6, 0);

        LoadConsonantCategories();

        ipaCategory.SelectedIndexChanged += (sender, e) =>
        {
            if (isSynchronizingSelection) return;

            if (ipaCategory.SelectedItem is IpaCategoryItem consonantCategory)
            {
                PopulateGuidedChoices(consonantCategory.Manner);
                return;
            }

            if (ipaCategory.SelectedItem is IpaVowelCategoryItem vowelCategory)
                PopulateGuidedVowelChoices(vowelCategory.Height);
        };

        ipaChoice.SelectedIndexChanged += (sender, e) =>
        {
            if (isSynchronizingSelection || ipaChoice.SelectedItem is not IpaDisplayItem selectedItem)
                return;

            if (selectedItem.Consonant != null)
            {
                ApplyConsonant(selectedItem.Consonant);
                return;
            }

            if (selectedItem.Vowel != null)
                ApplyVowel(selectedItem.Vowel);
        };
    }

    private void ConfigureConsonantFeatureSelectors()
    {
        consonantPlace.Items.AddRange(
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
            "唇软腭  Labial-velar");

        consonantPlace.DropDownStyle = ComboBoxStyle.DropDownList;
        consonantPlace.Width = 180;
        consonantPlace.SelectedIndex = 0;
        consonantPlace.Font = new Font("Microsoft YaHei UI", 10);
        consonantPlace.Margin = new Padding(0, 3, 6, 0);

        consonantManner.Items.AddRange(
            "塞音  Plosive",
            "鼻音  Nasal",
            "颤音  Trill",
            "闪音  Tap / Flap",
            "边闪音  Lateral flap",
            "擦音  Fricative",
            "边擦音  Lateral fricative",
            "近音  Approximant",
            "边近音  Lateral approximant",
            "塞擦音  Affricate");

        consonantManner.DropDownStyle = ComboBoxStyle.DropDownList;
        consonantManner.Width = 190;
        consonantManner.SelectedIndex = 0;
        consonantManner.Font = new Font("Microsoft YaHei UI", 10);
        consonantManner.Margin = new Padding(0, 3, 6, 0);

        consonantVoicing.Items.AddRange("清音  Voiceless", "浊音  Voiced");
        consonantVoicing.DropDownStyle = ComboBoxStyle.DropDownList;
        consonantVoicing.Width = 150;
        consonantVoicing.SelectedIndex = 0;
        consonantVoicing.Font = new Font("Microsoft YaHei UI", 10);
        consonantVoicing.Margin = new Padding(0, 3, 6, 0);

        consonantPlace.SelectedIndexChanged += (sender, e) => UpdateSymbolFromFeatures();
        consonantManner.SelectedIndexChanged += (sender, e) => UpdateSymbolFromFeatures();
        consonantVoicing.SelectedIndexChanged += (sender, e) => UpdateSymbolFromFeatures();
    }

    private void ConfigureVowelFeatureSelectors()
    {
        vowelHeight.Items.AddRange(
            "闭  Close",
            "近闭  Near-close",
            "半闭  Close-mid",
            "中  Mid",
            "半开  Open-mid",
            "近开  Near-open",
            "开  Open");

        vowelHeight.DropDownStyle = ComboBoxStyle.DropDownList;
        vowelHeight.Width = 170;
        vowelHeight.SelectedIndex = 0;
        vowelHeight.Font = new Font("Microsoft YaHei UI", 10);
        vowelHeight.Margin = new Padding(0, 3, 6, 0);

        vowelBackness.Items.AddRange("前  Front", "央  Central", "后  Back");
        vowelBackness.DropDownStyle = ComboBoxStyle.DropDownList;
        vowelBackness.Width = 150;
        vowelBackness.SelectedIndex = 0;
        vowelBackness.Font = new Font("Microsoft YaHei UI", 10);
        vowelBackness.Margin = new Padding(0, 3, 6, 0);

        vowelRoundedness.Items.AddRange("不圆唇  Unrounded", "圆唇  Rounded");
        vowelRoundedness.DropDownStyle = ComboBoxStyle.DropDownList;
        vowelRoundedness.Width = 170;
        vowelRoundedness.SelectedIndex = 0;
        vowelRoundedness.Font = new Font("Microsoft YaHei UI", 10);
        vowelRoundedness.Margin = new Padding(0, 3, 6, 0);

        vowelLength.DropDownStyle = ComboBoxStyle.DropDownList;
        vowelLength.Width = 190;
        vowelLength.Font = new Font("Microsoft YaHei UI", 10);
        vowelLength.Margin = new Padding(0, 3, 6, 0);
        vowelLength.DisplayMember = nameof(IpaVowelLength.DisplayText);

        foreach (var length in IpaVowelLengths.All)
            vowelLength.Items.Add(length);

        vowelLength.SelectedIndex = 0;

        vowelHeight.SelectedIndexChanged += (sender, e) => UpdateVowelSymbolFromFeatures();
        vowelBackness.SelectedIndexChanged += (sender, e) => UpdateVowelSymbolFromFeatures();
        vowelRoundedness.SelectedIndexChanged += (sender, e) => UpdateVowelSymbolFromFeatures();
        vowelLength.SelectedIndexChanged += (sender, e) => UpdateVowelLength();
    }

    private void ConfigurePhonemeInput()
    {
        phonemeInput.Width = 180;
        phonemeInput.Font = new Font("Microsoft YaHei UI", 12);
        phonemeInput.Margin = new Padding(0, 3, 6, 0);

        phonemeInput.TextChanged += (sender, e) =>
        {
            UpdateFeaturesFromSymbol();
            UpdateDiacriticButtonState();
        };

        //在音素输入框按Enter时直接执行添加。
        phonemeInput.KeyDown += (sender, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;
            AddPhoneme(sender, EventArgs.Empty);
        };
    }

    private void ConfigureActionButtons()
    {
        diacriticButton.Text = "附加符号  Diacritics…";
        diacriticButton.Width = 160;
        diacriticButton.Height = phonemeType.PreferredHeight;
        diacriticButton.Font = new Font("Microsoft YaHei UI", 9);
        diacriticButton.Margin = new Padding(0, 3, 6, 0);
        diacriticButton.Enabled = false;
        diacriticButton.Click += OpenDiacriticComposer;

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

    //打开IPADiacritic Composer。选中已有音素时编辑，否则创建新组合
    private void OpenDiacriticComposer(object? sender, EventArgs e)
    {
        if (TryEditSelectedInventoryPhoneme()) return;

        var pendingVowel = phonemeType.SelectedIndex == 1 &&
                           pendingBaseSymbol.Length > 0 &&
                           HasPendingComposition(phonemeInput.Text);

        var baseSymbol = pendingVowel
            ? pendingBaseSymbol
            : NormalizeInputSymbol(phonemeInput.Text);

        if (baseSymbol.Length == 0) return;

        var validBase = phonemeType.SelectedIndex == 0
            ? IpaConsonants.All.Any(x => IpaComposer.AreEquivalent(x.Symbol, baseSymbol))
            : IpaVowels.All.Any(x => IpaComposer.AreEquivalent(x.Symbol, baseSymbol));

        if (!validBase) return;

        using DiacriticComposerDialog dialog = pendingVowel
            ? new DiacriticComposerDialog(baseSymbol, pendingDiacritics)
            : new DiacriticComposerDialog(baseSymbol);

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        if (dialog.SelectedDiacritics.Count == 0 &&
            (phonemeType.SelectedIndex == 0 || pendingLengthMark.Length == 0))
            return;

        pendingBaseSymbol = baseSymbol;
        pendingDiacritics = new List<string>(dialog.SelectedDiacritics);

        var result = phonemeType.SelectedIndex == 1
            ? IpaComposer.ComposeVowel(baseSymbol, pendingDiacritics, pendingLengthMark)
            : IpaComposer.Compose(baseSymbol, pendingDiacritics);

        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        phonemeInput.Text = result;
        phonemeInput.SelectionStart = phonemeInput.Text.Length;

        isSynchronizingSelection = wasSynchronizing;

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;

        //元音可以继续修改Diacritics和Length
        diacriticButton.Enabled = phonemeType.SelectedIndex == 1;
    }

    private bool HasPendingComposition(string symbol)
    {
        if (pendingBaseSymbol.Length == 0) return false;

        if (phonemeType.SelectedIndex == 1)
        {
            if (pendingDiacritics.Count == 0 && pendingLengthMark.Length == 0) return false;

            var composed = IpaComposer.ComposeVowel(
                pendingBaseSymbol,
                pendingDiacritics,
                pendingLengthMark);

            return IpaComposer.AreEquivalent(symbol, composed);
        }

        if (pendingDiacritics.Count == 0) return false;

        var consonant = IpaComposer.Compose(pendingBaseSymbol, pendingDiacritics);
        return IpaComposer.AreEquivalent(symbol, consonant);
    }

    private void ClearPendingComposition()
    {
        pendingBaseSymbol = "";
        pendingDiacritics.Clear();
        pendingLengthMark = "";
    }

    private void UpdateVowelLength()
    {
        if (isSynchronizingSelection || phonemeType.SelectedIndex != 1) return;
        if (vowelLength.SelectedItem is not IpaVowelLength length) return;

        //清单中选中了已有元音时，直接编辑它
        if (vowelList.SelectedItem is VowelEntry entry)
        {
            EditSelectedVowelLength(entry, length);
            return;
        }

        var baseSymbol = pendingBaseSymbol;
        var diacritics = new List<string>(pendingDiacritics);

        if (baseSymbol.Length == 0)
        {
            var vowel = FindVowelFromInput();
            if (vowel == null) return;

            baseSymbol = vowel.Symbol;
        }

        var result = IpaComposer.ComposeVowel(baseSymbol, diacritics, length.Mark);

        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        phonemeInput.Text = result;
        phonemeInput.SelectionStart = phonemeInput.Text.Length;

        isSynchronizingSelection = wasSynchronizing;

        if (diacritics.Count == 0 && length.Mark.Length == 0)
        {
            ClearPendingComposition();
        }
        else
        {
            pendingBaseSymbol = baseSymbol;
            pendingDiacritics = diacritics;
            pendingLengthMark = length.Mark;
        }

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;
        diacriticButton.Enabled = true;
    }

    private void EditSelectedVowelLength(VowelEntry entry, IpaVowelLength length)
    {
        var vowel = project.Phonology.Vowels.FirstOrDefault(x =>
            IpaComposer.AreEquivalent(x.Symbol, entry.Symbol));

        if (vowel == null) return;

        var oldLengthMark = vowel.LengthMark;
        var baseSymbol = string.IsNullOrWhiteSpace(vowel.BaseSymbol)
            ? vowel.Symbol
            : vowel.BaseSymbol;

        var newSymbol = IpaComposer.ComposeVowel(baseSymbol, vowel.Diacritics, length.Mark);

        var duplicate = project.Phonology.Vowels.Any(existing =>
            !ReferenceEquals(existing, vowel) &&
            IpaComposer.AreEquivalent(existing.Symbol, newSymbol));

        if (duplicate)
        {
            MessageBox.Show(
                this,
                "该音素已经存在。\n\nThis phoneme already exists.",
                "重复音素  Duplicate phoneme",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            SyncVowelLength(oldLengthMark);
            return;
        }

        vowel.Symbol = newSymbol;
        vowel.BaseSymbol = baseSymbol;
        vowel.LengthMark = length.Mark;
        entry.Symbol = newSymbol;

        var index = vowelList.Items.IndexOf(entry);

        if (index >= 0)
        {
            vowelList.Items[index] = entry;
            vowelList.SelectedIndex = index;
        }

        RefreshVowelChart();
        projectModified?.Invoke();
    }

    private void SyncVowelLength(string lengthMark)
    {
        var index = IpaVowelLengths.All.FindIndex(x => x.Mark == lengthMark);
        if (index < 0) index = 0;

        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;
        vowelLength.SelectedIndex = index;
        isSynchronizingSelection = wasSynchronizing;
    }

    //检查清单中是否有可以编辑Diacritics的音素
    private bool TryEditSelectedInventoryPhoneme()
    {
        if (consonantList.SelectedItem is ConsonantEntry consonantEntry)
        {
            var consonant = project.Phonology.Consonants.FirstOrDefault(x =>
                IpaComposer.AreEquivalent(x.Symbol, consonantEntry.Symbol));

            if (consonant == null) return false;

            var baseSymbol = string.IsNullOrWhiteSpace(consonant.BaseSymbol)
                ? consonant.Symbol
                : consonant.BaseSymbol;

            //目前只允许普通pulmonic consonant进入Composer
            var validBase = IpaConsonants.All.Any(x => IpaComposer.AreEquivalent(x.Symbol, baseSymbol));
            if (!validBase) return false;

            EditConsonantDiacritics(consonant, consonantEntry, baseSymbol);
            return true;
        }

        if (vowelList.SelectedItem is VowelEntry vowelEntry)
        {
            var vowel = project.Phonology.Vowels.FirstOrDefault(x =>
                IpaComposer.AreEquivalent(x.Symbol, vowelEntry.Symbol));

            if (vowel == null) return false;

            var baseSymbol = string.IsNullOrWhiteSpace(vowel.BaseSymbol)
                ? vowel.Symbol
                : vowel.BaseSymbol;

            var validBase = IpaVowels.All.Any(x => IpaComposer.AreEquivalent(x.Symbol, baseSymbol));
            if (!validBase) return false;

            EditVowelDiacritics(vowel, vowelEntry, baseSymbol);
            return true;
        }

        return false;
    }

    private void EditConsonantDiacritics(ConsonantPhoneme consonant, ConsonantEntry entry, string baseSymbol)
    {
        using DiacriticComposerDialog dialog = new(baseSymbol, consonant.Diacritics);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var newSymbol = NormalizeInputSymbol(dialog.ResultSymbol);

        var duplicate = project.Phonology.Consonants.Any(existing =>
            !ReferenceEquals(existing, consonant) &&
            IpaComposer.AreEquivalent(existing.Symbol, newSymbol));

        if (duplicate)
        {
            MessageBox.Show(
                this,
                "该音素已经存在。\n\nThis phoneme already exists.",
                "重复音素  Duplicate phoneme",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        consonant.Symbol = newSymbol;
        consonant.BaseSymbol = baseSymbol;
        consonant.Diacritics = new List<string>(dialog.SelectedDiacritics);
        entry.Symbol = newSymbol;

        var index = consonantList.Items.IndexOf(entry);

        if (index >= 0)
        {
            //重新赋值，让ListBox立即重新调用ToString()
            consonantList.Items[index] = entry;
            consonantList.SelectedIndex = index;
        }

        RefreshConsonantChart();
        projectModified?.Invoke();
        UpdateDiacriticButtonState();
    }

    private void EditVowelDiacritics(VowelPhoneme vowel, VowelEntry entry, string baseSymbol)
    {
        using DiacriticComposerDialog dialog = new(baseSymbol, vowel.Diacritics);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var newSymbol = NormalizeInputSymbol(
            IpaComposer.ComposeVowel(
                baseSymbol,
                dialog.SelectedDiacritics,
                vowel.LengthMark));

        var duplicate = project.Phonology.Vowels.Any(existing =>
            !ReferenceEquals(existing, vowel) &&
            IpaComposer.AreEquivalent(existing.Symbol, newSymbol));

        if (duplicate)
        {
            MessageBox.Show(
                this,
                "该音素已经存在。\n\nThis phoneme already exists.",
                "重复音素  Duplicate phoneme",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        vowel.Symbol = newSymbol;
        vowel.BaseSymbol = baseSymbol;
        vowel.Diacritics = new List<string>(dialog.SelectedDiacritics);
        entry.Symbol = newSymbol;

        var index = vowelList.Items.IndexOf(entry);

        if (index >= 0)
        {
            vowelList.Items[index] = entry;
            vowelList.SelectedIndex = index;
        }

        RefreshVowelChart();
        projectModified?.Invoke();
        UpdateDiacriticButtonState();
    }

    //根据清单选择和输入框状态决定Diacritics按钮是创建还是编辑
    private void UpdateDiacriticButtonState()
    {
        if (consonantList.SelectedItem is ConsonantEntry consonantEntry)
        {
            var consonant = project.Phonology.Consonants.FirstOrDefault(x =>
                IpaComposer.AreEquivalent(x.Symbol, consonantEntry.Symbol));

            if (consonant != null)
            {
                var baseSymbol = string.IsNullOrWhiteSpace(consonant.BaseSymbol)
                    ? consonant.Symbol
                    : consonant.BaseSymbol;

                var editable = IpaConsonants.All.Any(x =>
                    IpaComposer.AreEquivalent(x.Symbol, baseSymbol));

                if (editable)
                {
                    diacriticButton.Text = "编辑附加符号  Edit…";
                    diacriticButton.Enabled = true;
                    return;
                }
            }

            //NP和Other Symbols暂时不可编辑
            diacriticButton.Text = "附加符号  Diacritics…";
            diacriticButton.Enabled = false;
            return;
        }

        if (vowelList.SelectedItem is VowelEntry vowelEntry)
        {
            var vowel = project.Phonology.Vowels.FirstOrDefault(x =>
                IpaComposer.AreEquivalent(x.Symbol, vowelEntry.Symbol));

            if (vowel != null)
            {
                var baseSymbol = string.IsNullOrWhiteSpace(vowel.BaseSymbol)
                    ? vowel.Symbol
                    : vowel.BaseSymbol;

                var editable = IpaVowels.All.Any(x =>
                    IpaComposer.AreEquivalent(x.Symbol, baseSymbol));

                if (editable)
                {
                    diacriticButton.Text = "编辑附加符号  Edit…";
                    diacriticButton.Enabled = true;
                    return;
                }
            }

            diacriticButton.Text = "附加符号  Diacritics…";
            diacriticButton.Enabled = false;
            return;
        }

        diacriticButton.Text = "附加符号  Diacritics…";

        var input = NormalizeInputSymbol(phonemeInput.Text);

        if (input.Length == 0)
        {
            diacriticButton.Enabled = false;
            return;
        }

        if (HasPendingComposition(input))
        {
            diacriticButton.Enabled =
                phonemeType.SelectedIndex == 1 &&
                pendingBaseSymbol.Length > 0;

            return;
        }

        if (phonemeType.SelectedIndex == 0)
        {
            diacriticButton.Enabled = IpaConsonants.All.Any(x =>
                IpaComposer.AreEquivalent(x.Symbol, input));
            return;
        }

        diacriticButton.Enabled = IpaVowels.All.Any(x =>
            IpaComposer.AreEquivalent(x.Symbol, input));
    }

    private void ConfigurePhonemeLists()
    {
        consonantList.Size = new Size(725, 180);
        consonantList.Font = new Font("Microsoft YaHei UI", 12);

        vowelList.Size = new Size(725, 180);
        vowelList.Font = new Font("Microsoft YaHei UI", 12);

        var lastConsonantClickedIndex = -1;
        var lastVowelClickedIndex = -1;

        consonantList.SelectedIndexChanged += (sender, e) =>
        {
            if (consonantList.SelectedIndex != lastConsonantClickedIndex)
                lastConsonantClickedIndex = -1;

            UpdateDiacriticButtonState();
        };

        vowelList.SelectedIndexChanged += (sender, e) =>
        {
            if (vowelList.SelectedIndex != lastVowelClickedIndex)
                lastVowelClickedIndex = -1;

            if (vowelList.SelectedItem is VowelEntry entry)
            {
                var vowel = project.Phonology.Vowels.FirstOrDefault(x =>
                    IpaComposer.AreEquivalent(x.Symbol, entry.Symbol));

                if (vowel != null)
                    SyncVowelLength(vowel.LengthMark);
            }

            UpdateDiacriticButtonState();
        };

        consonantList.MouseClick += (sender, e) =>
        {
            if (e.Button != MouseButtons.Left) return;

            var clickedIndex = consonantList.IndexFromPoint(e.Location);

            //点击空白区域取消选择
            if (clickedIndex == ListBox.NoMatches)
            {
                consonantList.ClearSelected();
                lastConsonantClickedIndex = -1;
                UpdateDiacriticButtonState();
                return;
            }

            //再次点击同一个辅音取消选择
            if (clickedIndex == lastConsonantClickedIndex &&
                consonantList.SelectedIndex == clickedIndex)
            {
                consonantList.ClearSelected();
                lastConsonantClickedIndex = -1;
                UpdateDiacriticButtonState();
                return;
            }

            consonantList.SelectedIndex = clickedIndex;
            lastConsonantClickedIndex = clickedIndex;

            //两边不能同时选中
            vowelList.ClearSelected();
            lastVowelClickedIndex = -1;

            UpdateDiacriticButtonState();
        };

        vowelList.MouseClick += (sender, e) =>
        {
            if (e.Button != MouseButtons.Left) return;

            var clickedIndex = vowelList.IndexFromPoint(e.Location);

            //点击空白区域取消选择
            if (clickedIndex == ListBox.NoMatches)
            {
                vowelList.ClearSelected();
                lastVowelClickedIndex = -1;
                UpdateDiacriticButtonState();
                return;
            }

            //再次点击同一个元音取消选择
            if (clickedIndex == lastVowelClickedIndex &&
                vowelList.SelectedIndex == clickedIndex)
            {
                vowelList.ClearSelected();
                lastVowelClickedIndex = -1;
                UpdateDiacriticButtonState();
                return;
            }

            vowelList.SelectedIndex = clickedIndex;
            lastVowelClickedIndex = clickedIndex;

            consonantList.ClearSelected();
            lastConsonantClickedIndex = -1;

            UpdateDiacriticButtonState();
        };
    }

    private void ConfigureNoMatchingPhonemeLabel()
    {
        noMatchingPhonemeLabel.Text = "无此音素  No corresponding phoneme";
        noMatchingPhonemeLabel.AutoSize = true;
        noMatchingPhonemeLabel.Font = new Font("Microsoft YaHei UI", 9);
        noMatchingPhonemeLabel.Margin = new Padding(8, 7, 0, 0);
        noMatchingPhonemeLabel.Visible = false;
    }

    //普通肺部辅音表
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

        consonantChart.RowCount = manners.Length + 1;
        consonantChart.ColumnCount = places.Length + 1;
        consonantChart.AutoSize = true;
        consonantChart.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
        consonantChart.Location = new Point(0, 0);

        consonantChart.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));

        foreach (var place in places)
            consonantChart.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

        consonantChart.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        for (var column = 0; column < places.Length; column++)
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

        for (var row = 0; row < manners.Length; row++)
        {
            consonantChart.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            Label mannerLabel = new()
            {
                Text = manners[row],
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 9)
            };

            consonantChart.Controls.Add(mannerLabel, 0, row + 1);

            for (var column = 0; column < places.Length; column++)
            {
                TableLayoutPanel cell = new()
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Margin = new Padding(0)
                };

                cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

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

                consonantChartCells[(placeKeys[column], mannerKeys[row], "清音  Voiceless")] = voiceless;
                consonantChartCells[(placeKeys[column], mannerKeys[row], "浊音  Voiced")] = voiced;

                cell.Controls.Add(voiceless, 0, 0);
                cell.Controls.Add(voiced, 1, 0);
                consonantChart.Controls.Add(cell, column + 1, row + 1);
            }
        }

        chartContainer.Controls.Add(consonantChart);
        chartContainer.Height = consonantChart.PreferredSize.Height + SystemInformation.HorizontalScrollBarHeight + 5;

        return chartContainer;
    }

    //页面底部左侧元音图，右侧Non-pulmonic
    private Control BuildLowerChartsRow()
    {
        TableLayoutPanel row = new()
        {
            Width = 1480,
            Height = 430,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 20, 0, 25),
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };

        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 800));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 560));
        row.RowStyles.Add(new RowStyle(SizeType.Absolute, 430));

        var vowelArea = BuildVowelChart();
        vowelArea.Margin = new Padding(0);

        var nonPulmonicArea = BuildNonPulmonicChart();
        nonPulmonicArea.Margin = new Padding(20, 0, 0, 0);

        row.Controls.Add(vowelArea, 0, 0);
        row.Controls.Add(nonPulmonicArea, 1, 0);

        return row;
    }

    //Non-pulmonic和Other IPA Symbols
    private Control BuildNonPulmonicChart()
    {
        Panel container = new()
        {
            Width = 650,
            Height = 400,
            Margin = new Padding(0)
        };

        Label title = new()
        {
            Text = "非肺部气流辅音  Non-pulmonic consonants",
            Location = new Point(0, 10),
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 11, FontStyle.Bold)
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

        nonPulmonicChart.Location = new Point(0, 45);
        nonPulmonicChart.Size = new Size(650, 300);
        nonPulmonicChart.ColumnCount = 3;
        nonPulmonicChart.RowCount = 2;
        nonPulmonicChart.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        nonPulmonicChart.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

        nonPulmonicChart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        nonPulmonicChart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        nonPulmonicChart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334f));
        nonPulmonicChart.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        nonPulmonicChart.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        for (var column = 0; column < categoryKeys.Length; column++)
        {
            var categoryKey = categoryKeys[column];

            Label header = new()
            {
                Text = categoryTitles[column],
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold)
            };

            Label content = new()
            {
                Text = "",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Font = new Font("Microsoft YaHei UI", 11),
                Padding = new Padding(10, 10, 6, 6)
            };

            nonPulmonicChart.Controls.Add(header, column, 0);
            nonPulmonicChart.Controls.Add(content, column, 1);
            nonPulmonicChartCells[categoryKey] = content;
        }

        Label otherSymbolsTitle = new()
        {
            Text = "其他 IPA 符号  Other IPA symbols",
            Location = new Point(0, 350),
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold)
        };

        otherSymbolsContent.Location = new Point(0, 375);
        otherSymbolsContent.Size = new Size(650, 25);
        otherSymbolsContent.Font = new Font("Microsoft YaHei UI", 10);
        otherSymbolsContent.Text = "";

        container.Controls.Add(title);
        container.Controls.Add(nonPulmonicChart);
        container.Controls.Add(otherSymbolsTitle);
        container.Controls.Add(otherSymbolsContent);

        return container;
    }

    //IPA元音梯形图
    private Control BuildVowelChart()
    {
        Panel chartContainer = new()
        {
            Width = 800,
            Height = 400,
            Margin = new Padding(0)
        };

        vowelChart.Size = new Size(790, 390);
        vowelChart.Location = new Point(0, 0);
        vowelChartCells.Clear();

        vowelChart.Paint += (sender, e) =>
        {
            using Pen pen = new(Color.Gray, 1);

            Point topLeft = new(250, 55);
            Point topRight = new(730, 55);
            Point bottomLeft = new(370, 340);
            Point bottomRight = new(730, 340);

            e.Graphics.DrawLine(pen, topLeft, topRight);
            e.Graphics.DrawLine(pen, topLeft, bottomLeft);
            e.Graphics.DrawLine(pen, topRight, bottomRight);
            e.Graphics.DrawLine(pen, bottomLeft, bottomRight);
            e.Graphics.DrawLine(pen, new Point(490, 55), new Point(550, 340));
        };

        AddVowelChartHeader("前  Front", 250);
        AddVowelChartHeader("央  Central", 490);
        AddVowelChartHeader("后  Back", 730);

        AddVowelHeightLabel("闭  Close", 55);
        AddVowelHeightLabel("近闭  Near-close", 100);
        AddVowelHeightLabel("半闭  Close-mid", 145);
        AddVowelHeightLabel("中  Mid", 190);
        AddVowelHeightLabel("半开  Open-mid", 235);
        AddVowelHeightLabel("近开  Near-open", 280);
        AddVowelHeightLabel("开  Open", 325);

        foreach (var vowel in IpaVowels.All)
        {
            var anchor = GetVowelChartPosition(vowel.Height, vowel.Backness);
            var hasRoundedPair = IpaVowels.All.Any(x =>
                x.Height == vowel.Height &&
                x.Backness == vowel.Backness &&
                x.Roundedness != vowel.Roundedness);

            int x;

            if (!hasRoundedPair)
                x = anchor.X - 20;
            else if (vowel.Roundedness.StartsWith("不圆唇"))
                x = anchor.X - 42;
            else
                x = anchor.X + 2;

            Label cell = new()
            {
                Text = "",
                Location = new Point(x, anchor.Y - 10),
                Size = new Size(40, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 14)
            };

            vowelChartCells[(vowel.Height, vowel.Backness, vowel.Roundedness)] = cell;
            vowelChart.Controls.Add(cell);
        }

        chartContainer.Controls.Add(vowelChart);
        return chartContainer;
    }

    private void AddVowelChartHeader(string text, int centerX)
    {
        Label label = new()
        {
            Text = text,
            Size = new Size(160, 30),
            Location = new Point(centerX - 80, 8),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold)
        };

        vowelChart.Controls.Add(label);
    }

    private void AddVowelHeightLabel(string text, int y)
    {
        Label label = new()
        {
            Text = text,
            Size = new Size(180, 30),
            Location = new Point(10, y - 10),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Microsoft YaHei UI", 9)
        };

        vowelChart.Controls.Add(label);
    }

    private void RefreshVowelChart()
    {
        foreach (var cell in vowelChartCells.Values)
            cell.Text = "";

        foreach (var vowel in project.Phonology.Vowels)
        {
            var key = (vowel.Height, vowel.Backness, vowel.Roundedness);

            if (!vowelChartCells.TryGetValue(key, out var cell))
                continue;

            if (cell.Text.Length == 0)
                cell.Text = vowel.Symbol;
            else
                cell.Text += $" {vowel.Symbol}";
        }
    }

    private static Point GetVowelChartPosition(string height, string backness)
    {
        var row = height switch
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

        var y = 55 + row * 45;
        var frontX = 250 + row * 20;
        var backX = 730;
        var centralX = (frontX + backX) / 2;

        var x = backness switch
        {
            "前  Front" => frontX,
            "央  Central" => centralX,
            "后  Back" => backX,
            _ => centralX
        };

        return new Point(x, y);
    }

    //根据音素类型和选择模式更新UI
    private void UpdateSelectionMode()
    {
        //批量切换Visible会引发多次重排，暂停布局直到全部设置完成。
        SuspendLayout();
        try
        {
            UpdateSelectionModeCore();
        }
        finally
        {
            ResumeLayout(true);
        }
    }

    private void UpdateSelectionModeCore()
    {
        var isConsonant = phonemeType.SelectedIndex == 0;
        var isVowel = phonemeType.SelectedIndex == 1;
        var currentConsonant = FindConsonantFromInput();

        var detailedMode = selectionMode == SelectionMode.Detailed;
        var guidedMode = selectionMode == SelectionMode.Guided;
        var listMode = selectionMode == SelectionMode.List;

        ipaSymbolPicker.Visible = listMode;

        consonantPlace.Visible = detailedMode && isConsonant;
        consonantManner.Visible = detailedMode && isConsonant;
        consonantVoicing.Visible = detailedMode && isConsonant;

        vowelHeight.Visible = detailedMode && isVowel;
        vowelBackness.Visible = detailedMode && isVowel;
        vowelRoundedness.Visible = detailedMode && isVowel;

        //元音长度选择在所有元音选择模式中都可使用
        vowelLength.Visible = isVowel;
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
                    SelectDetailedConsonant(currentConsonant);
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
                    SelectDetailedConsonant(currentConsonant);
            }
            else
            {
                LoadVowelListPicker();

                var currentVowel = FindVowelFromInput();

                if (currentVowel != null)
                    SelectListVowel(currentVowel);
            }

            return;
        }

        if (isConsonant)
        {
            ipaCategory.Enabled = true;
            ipaChoice.Enabled = true;

            EnsureConsonantCategories();

            if (currentConsonant != null)
                SelectGuidedConsonant(currentConsonant);
        }
        else
        {
            ipaCategory.Enabled = true;
            ipaChoice.Enabled = true;

            LoadVowelCategories();

            var currentVowel = FindVowelFromInput();

            if (currentVowel != null)
                SelectGuidedVowel(currentVowel);
            else
                UpdateVowelSymbolFromFeatures();
        }
    }

    //List mode中的普通辅音、NP和Other Symbols
    private void LoadDetailedIpaPicker()
    {
        ipaSymbolPicker.BeginUpdate();
        ipaSymbolPicker.Items.Clear();

        foreach (var category in IpaConsonants.All.GroupBy(x => x.Manner))
        {
            ipaSymbolPicker.Items.Add(
                new IpaDisplayItem($"── {GetCategoryDisplayName(category.Key)} ──"));

            foreach (var consonant in category)
            {
                ipaSymbolPicker.Items.Add(
                    new IpaDisplayItem(
                        $"{consonant.Symbol}   {GetConsonantDescription(consonant)}",
                        consonant));
            }
        }

        foreach (var category in IpaNonPulmonicConsonants.All.GroupBy(x => x.Category))
        {
            ipaSymbolPicker.Items.Add(new IpaDisplayItem($"── {category.Key} ──"));

            foreach (var consonant in category)
            {
                ipaSymbolPicker.Items.Add(
                    new IpaDisplayItem(
                        $"{consonant.Symbol}   {consonant.Description}",
                        consonant));
            }
        }

        if (IpaOtherSymbols.All.Count > 0)
        {
            ipaSymbolPicker.Items.Add(new IpaDisplayItem("── 其他 IPA 符号  Other IPA symbols ──"));

            foreach (var symbol in IpaOtherSymbols.All)
                ipaSymbolPicker.Items.Add(new IpaDisplayItem($"{symbol.Symbol}   {symbol.Name}", symbol));
        }

        ipaSymbolPicker.SelectedIndex = -1;
        lastIpaSymbolIndex = -1;

        ipaSymbolPicker.EndUpdate();
    }

    private void LoadConsonantCategories()
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaCategory.BeginUpdate();
        ipaCategory.Items.Clear();

        foreach (var manner in IpaConsonants.All.Select(x => x.Manner).Distinct())
            ipaCategory.Items.Add(new IpaCategoryItem(manner, GetCategoryDisplayName(manner)));

        ipaCategory.SelectedIndex = -1;
        ipaCategory.EndUpdate();

        ipaChoice.Items.Clear();

        isSynchronizingSelection = wasSynchronizing;
    }

    private void PopulateGuidedChoices(string manner)
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaChoice.BeginUpdate();
        ipaChoice.Items.Clear();

        foreach (var consonant in IpaConsonants.All.Where(x => x.Manner == manner))
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

    private void EnsureConsonantCategories()
    {
        if (ipaCategory.Items.OfType<IpaCategoryItem>().Any()) return;
        LoadConsonantCategories();
    }

    //把普通辅音同步到输入框和选择器
    private void ApplyConsonant(IpaConsonant consonant)
    {
        ClearPendingComposition();

        var wasSynchronizing = isSynchronizingSelection;
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

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;
        diacriticButton.Enabled = true;
    }

    private void ApplyNonPulmonicConsonant(IpaNonPulmonicConsonant consonant)
    {
        ClearPendingComposition();

        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        phonemeType.SelectedIndex = 0;
        phonemeInput.Text = consonant.Symbol;
        phonemeInput.SelectionStart = phonemeInput.Text.Length;

        isSynchronizingSelection = wasSynchronizing;

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;

        //NP的Diacritics以后再接
        diacriticButton.Enabled = false;
    }

    private void ApplyOtherSymbol(IpaOtherSymbol symbol)
    {
        ClearPendingComposition();

        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        phonemeType.SelectedIndex = 0;
        phonemeInput.Text = symbol.Symbol;
        phonemeInput.SelectionStart = phonemeInput.Text.Length;

        isSynchronizingSelection = wasSynchronizing;

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;

        //Other Symbols暂时不进入Composer
        diacriticButton.Enabled = false;
    }

    private void SelectDetailedConsonant(IpaConsonant consonant)
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        for (var index = 0; index < ipaSymbolPicker.Items.Count; index++)
        {
            if (ipaSymbolPicker.Items[index] is not IpaDisplayItem item ||
                item.Consonant?.Symbol != consonant.Symbol)
                continue;

            ipaSymbolPicker.SelectedIndex = index;
            lastIpaSymbolIndex = index;
            break;
        }

        isSynchronizingSelection = wasSynchronizing;
    }

    private void SelectGuidedConsonant(IpaConsonant consonant)
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        EnsureConsonantCategories();

        for (var index = 0; index < ipaCategory.Items.Count; index++)
        {
            if (ipaCategory.Items[index] is not IpaCategoryItem category ||
                category.Manner != consonant.Manner)
                continue;

            ipaCategory.SelectedIndex = index;
            break;
        }

        PopulateGuidedChoices(consonant.Manner);

        for (var index = 0; index < ipaChoice.Items.Count; index++)
        {
            if (ipaChoice.Items[index] is not IpaDisplayItem item ||
                item.Consonant?.Symbol != consonant.Symbol)
                continue;

            ipaChoice.SelectedIndex = index;
            break;
        }

        isSynchronizingSelection = wasSynchronizing;
    }

    private IpaConsonant? FindConsonantFromInput()
    {
        var symbol = NormalizeInputSymbol(phonemeInput.Text.Trim());
        return IpaConsonants.All.FirstOrDefault(x => x.Symbol == symbol);
    }

    private IpaNonPulmonicConsonant? FindNonPulmonicConsonantFromInput()
    {
        var symbol = phonemeInput.Text.Trim();
        return IpaNonPulmonicConsonants.All.FirstOrDefault(x => x.Symbol == symbol);
    }

    private IpaOtherSymbol? FindOtherSymbolFromInput()
    {
        var symbol = phonemeInput.Text.Trim();
        return IpaOtherSymbols.All.FirstOrDefault(x => x.Symbol == symbol);
    }

    private static string NormalizeInputSymbol(string symbol)
    {
        symbol = symbol.Trim();

        //普通键盘g转成IPAɡ
        if (symbol == "g") symbol = "ɡ";

        return IpaComposer.NormalizeSymbol(symbol);
    }

    private void SetPhonemeInput(string symbol)
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        phonemeInput.Text = symbol;
        phonemeInput.SelectionStart = phonemeInput.Text.Length;

        isSynchronizingSelection = wasSynchronizing;
    }

    //识别塞擦音和双重调音的快捷输入。
    private bool TryApplyTiedConsonantInput(string input)
    {
        if (!IpaTieBarComposer.TryParse(input, out var tied) || tied == null)
            return false;

        SetPhonemeInput(tied.Symbol);

        //Kiel表中已有的塞擦音继续使用标准参考属性。
        var reference = IpaConsonants.All.FirstOrDefault(x =>
            IpaComposer.AreEquivalent(x.Symbol, tied.Symbol));

        if (reference != null)
        {
            ApplyConsonant(reference);
            return true;
        }

        //自定义双重调音允许加入Inventory，但不强塞进错误的单一调音部位格子。
        ClearIpaSelections();

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;
        diacriticButton.Enabled = false;

        return true;
    }

    private void UpdateSymbolFromFeatures()
    {
        if (isSynchronizingSelection || phonemeType.SelectedIndex != 0) return;

        var match = IpaConsonants.All.FirstOrDefault(x =>
            x.Place == consonantPlace.Text &&
            x.Manner == consonantManner.Text &&
            x.Voicing == consonantVoicing.Text);

        if (match == null)
        {
            var wasSynchronizing = isSynchronizingSelection;
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

    private void LoadVowelListPicker()
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaSymbolPicker.BeginUpdate();
        ipaSymbolPicker.Items.Clear();

        foreach (var category in IpaVowels.All.GroupBy(x => x.Height))
        {
            ipaSymbolPicker.Items.Add(
                new IpaDisplayItem($"── {GetVowelCategoryDisplayName(category.Key)} ──"));

            foreach (var vowel in category)
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

    private void LoadVowelCategories()
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaCategory.BeginUpdate();
        ipaCategory.Items.Clear();

        foreach (var height in IpaVowels.All.Select(x => x.Height).Distinct())
            ipaCategory.Items.Add(new IpaVowelCategoryItem(height, GetVowelCategoryDisplayName(height)));

        ipaCategory.SelectedIndex = -1;
        ipaCategory.EndUpdate();

        ipaChoice.Items.Clear();

        isSynchronizingSelection = wasSynchronizing;
    }

    private void PopulateGuidedVowelChoices(string height)
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaChoice.BeginUpdate();
        ipaChoice.Items.Clear();

        foreach (var vowel in IpaVowels.All.Where(x => x.Height == height))
            ipaChoice.Items.Add(new IpaDisplayItem($"{vowel.Symbol}   {GetVowelDescription(vowel)}", vowel));

        ipaChoice.SelectedIndex = -1;
        ipaChoice.EndUpdate();

        isSynchronizingSelection = wasSynchronizing;
    }

    //把元音同步到输入框和选择器
    private void ApplyVowel(IpaVowel vowel)
    {
        ClearPendingComposition();

        var lengthMark = vowelLength.SelectedItem is IpaVowelLength length
            ? length.Mark
            : "";

        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        phonemeType.SelectedIndex = 1;

        phonemeInput.Text = IpaComposer.ComposeVowel(
            vowel.Symbol,
            Array.Empty<string>(),
            lengthMark);

        phonemeInput.SelectionStart = phonemeInput.Text.Length;

        vowelHeight.SelectedItem = vowel.Height;
        vowelBackness.SelectedItem = vowel.Backness;
        vowelRoundedness.SelectedItem = vowel.Roundedness;

        SelectListVowel(vowel);
        SelectGuidedVowel(vowel);

        isSynchronizingSelection = wasSynchronizing;

        if (lengthMark.Length > 0)
        {
            pendingBaseSymbol = vowel.Symbol;
            pendingLengthMark = lengthMark;
        }

        noMatchingPhonemeLabel.Visible = false;
        addPhonemeButton.Enabled = true;
        diacriticButton.Enabled = true;
    }

    private void SelectListVowel(IpaVowel vowel)
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        for (var index = 0; index < ipaSymbolPicker.Items.Count; index++)
        {
            if (ipaSymbolPicker.Items[index] is not IpaDisplayItem item ||
                item.Vowel?.Symbol != vowel.Symbol)
                continue;

            ipaSymbolPicker.SelectedIndex = index;
            lastIpaSymbolIndex = index;
            break;
        }

        isSynchronizingSelection = wasSynchronizing;
    }

    private void SelectGuidedVowel(IpaVowel vowel)
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        if (!ipaCategory.Items.OfType<IpaVowelCategoryItem>().Any())
            LoadVowelCategories();

        for (var index = 0; index < ipaCategory.Items.Count; index++)
        {
            if (ipaCategory.Items[index] is not IpaVowelCategoryItem category ||
                category.Height != vowel.Height)
                continue;

            ipaCategory.SelectedIndex = index;
            break;
        }

        PopulateGuidedVowelChoices(vowel.Height);

        for (var index = 0; index < ipaChoice.Items.Count; index++)
        {
            if (ipaChoice.Items[index] is not IpaDisplayItem item ||
                item.Vowel?.Symbol != vowel.Symbol)
                continue;

            ipaChoice.SelectedIndex = index;
            break;
        }

        isSynchronizingSelection = wasSynchronizing;
    }

    private IpaVowel? FindVowelFromInput()
    {
        var symbol = phonemeInput.Text.Trim();
        return IpaVowels.All.FirstOrDefault(x => x.Symbol == symbol);
    }

    //识别带长度标记的基础元音
    private bool TryParseLengthMarkedVowel(
        string input,
        out IpaVowel? vowel,
        out IpaVowelLength? length)
    {
        vowel = null;
        length = null;

        foreach (var candidateVowel in IpaVowels.All)
        {
            foreach (var candidateLength in IpaVowelLengths.All)
            {
                if (candidateLength.Mark.Length == 0) continue;

                var composed = IpaComposer.ComposeVowel(
                    candidateVowel.Symbol,
                    Array.Empty<string>(),
                    candidateLength.Mark);

                if (!IpaComposer.AreEquivalent(input, composed)) continue;

                vowel = candidateVowel;
                length = candidateLength;
                return true;
            }
        }

        return false;
    }

    private void UpdateVowelSymbolFromFeatures()
    {
        if (isSynchronizingSelection || phonemeType.SelectedIndex != 1) return;

        var match = IpaVowels.All.FirstOrDefault(x =>
            x.Height == vowelHeight.Text &&
            x.Backness == vowelBackness.Text &&
            x.Roundedness == vowelRoundedness.Text);

        if (match == null)
        {
            var wasSynchronizing = isSynchronizingSelection;
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

    //手动输入IPA后的反向识别
    //手动输入IPA后的反向识别
    private void UpdateFeaturesFromSymbol()
    {
        if (isSynchronizingSelection) return;

        var input = NormalizeInputSymbol(phonemeInput.Text);

        //输入冒号时自动替换为长音符号，半角和全角冒号都接受
        if (phonemeType.SelectedIndex == 1 &&
            (input.EndsWith(":") || input.EndsWith("：")))
        {
            input = input[..^1] + "ː";
            SetPhonemeInput(input);
        }

        if (input.Length == 0)
        {
            ClearPendingComposition();
            ClearIpaSelections();

            noMatchingPhonemeLabel.Visible = false;
            addPhonemeButton.Enabled = false;
            diacriticButton.Enabled = false;
            return;
        }

        //Composer刚创建的音素不需要存在于基础IPA数据库
        if (HasPendingComposition(input))
        {
            noMatchingPhonemeLabel.Visible = false;
            addPhonemeButton.Enabled = true;
            diacriticButton.Enabled = phonemeType.SelectedIndex == 1;
            return;
        }

        //用户手动修改组合内容后，之前的Composer状态失效
        ClearPendingComposition();

        if (phonemeType.SelectedIndex == 0)
        {
            var consonant = FindConsonantFromInput();

            if (consonant != null)
            {
                ApplyConsonant(consonant);
                return;
            }

            //dz、d-z、nm、n-m等输入在这里转成规范tie bar形式。
            if (TryApplyTiedConsonantInput(input))
                return;

            var nonPulmonic = FindNonPulmonicConsonantFromInput();

            if (nonPulmonic != null)
            {
                ClearIpaSelections();
                noMatchingPhonemeLabel.Visible = false;
                addPhonemeButton.Enabled = true;
                diacriticButton.Enabled = false;
                return;
            }

            var otherSymbol = FindOtherSymbolFromInput();

            if (otherSymbol != null)
            {
                ClearIpaSelections();
                noMatchingPhonemeLabel.Visible = false;
                addPhonemeButton.Enabled = true;
                diacriticButton.Enabled = false;
                return;
            }

            ClearIpaSelections();
            noMatchingPhonemeLabel.Visible = true;
            addPhonemeButton.Enabled = false;
            diacriticButton.Enabled = false;
            return;
        }

        //先识别带长度标记的基础元音
        if (TryParseLengthMarkedVowel(input, out var lengthVowel, out var length) &&
            lengthVowel != null && length != null)
        {
            SyncVowelLength(length.Mark);
            ApplyVowel(lengthVowel);
            return;
        }

        var vowel = FindVowelFromInput();

        if (vowel == null)
        {
            ClearIpaSelections();
            noMatchingPhonemeLabel.Visible = true;
            addPhonemeButton.Enabled = false;
            diacriticButton.Enabled = false;
            return;
        }

        //手动输入普通元音时返回普通长度
        SyncVowelLength("");
        ApplyVowel(vowel);
    }

    private void ClearIpaSelections()
    {
        var wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaSymbolPicker.SelectedIndex = -1;
        ipaChoice.SelectedIndex = -1;
        lastIpaSymbolIndex = -1;

        isSynchronizingSelection = wasSynchronizing;
    }

    //从项目模型加载音系
    private void LoadProjectPhonology()
    {
        consonantList.Items.Clear();
        vowelList.Items.Clear();

        foreach (var consonant in project.Phonology.Consonants)
        {
            consonantList.Items.Add(new ConsonantEntry
            {
                Symbol = consonant.Symbol,
                Place = consonant.Place,
                Manner = consonant.Manner,
                Voicing = consonant.Voicing,
                Category = consonant.Category,
                Description = consonant.Description
            });
        }

        foreach (var vowel in project.Phonology.Vowels)
        {
            vowelList.Items.Add(new VowelEntry
            {
                Symbol = vowel.Symbol,
                Height = vowel.Height,
                Backness = vowel.Backness,
                Roundedness = vowel.Roundedness
            });
        }

        RefreshConsonantChart();
        RefreshNonPulmonicChart();
        RefreshOtherSymbols();
        RefreshVowelChart();
    }

    //添加由基础音素和Diacritics组成的音素
    private bool AddPendingComposedPhoneme(string phoneme)
    {
        if (!HasPendingComposition(phoneme)) return false;

        if (phonemeType.SelectedIndex == 0)
        {
            //防止Unicode等价的重复项
            if (project.Phonology.Consonants.Any(x =>
                    IpaComposer.AreEquivalent(x.Symbol, phoneme)))
                return false;

            var baseConsonant = IpaConsonants.All.FirstOrDefault(x =>
                IpaComposer.AreEquivalent(x.Symbol, pendingBaseSymbol));

            if (baseConsonant == null) return false;

            ConsonantPhoneme projectConsonant = new()
            {
                Symbol = phoneme,
                BaseSymbol = baseConsonant.Symbol,
                Diacritics = new List<string>(pendingDiacritics),
                Place = baseConsonant.Place,
                Manner = baseConsonant.Manner,
                Voicing = baseConsonant.Voicing
            };

            project.Phonology.Consonants.Add(projectConsonant);

            consonantList.Items.Add(new ConsonantEntry
            {
                Symbol = projectConsonant.Symbol,
                Place = projectConsonant.Place,
                Manner = projectConsonant.Manner,
                Voicing = projectConsonant.Voicing
            });

            RefreshConsonantChart();
            return true;
        }

        if (project.Phonology.Vowels.Any(x =>
                IpaComposer.AreEquivalent(x.Symbol, phoneme)))
            return false;

        var baseVowel = IpaVowels.All.FirstOrDefault(x =>
            IpaComposer.AreEquivalent(x.Symbol, pendingBaseSymbol));

        if (baseVowel == null) return false;

        VowelPhoneme projectVowel = new()
        {
            Symbol = phoneme,
            BaseSymbol = baseVowel.Symbol,
            Diacritics = new List<string>(pendingDiacritics),
            LengthMark = pendingLengthMark,
            Height = baseVowel.Height,
            Backness = baseVowel.Backness,
            Roundedness = baseVowel.Roundedness
        };

        project.Phonology.Vowels.Add(projectVowel);

        vowelList.Items.Add(new VowelEntry
        {
            Symbol = projectVowel.Symbol,
            Height = projectVowel.Height,
            Backness = projectVowel.Backness,
            Roundedness = projectVowel.Roundedness
        });

        RefreshVowelChart();
        return true;
    }

    //将当前音素加入项目
    //将当前音素加入项目
    private void AddPhoneme(object? sender, EventArgs e)
    {
        var phoneme = NormalizeInputSymbol(phonemeInput.Text);
        if (phoneme.Length == 0) return;

        //Composer创建的音素优先处理，例如pʰ、ã、n̥
        if (HasPendingComposition(phoneme))
        {
            if (!AddPendingComposedPhoneme(phoneme)) return;

            projectModified?.Invoke();

            ClearPendingComposition();
            phonemeInput.Clear();
            phonemeInput.Focus();
            return;
        }

        if (phonemeType.SelectedIndex == 0)
        {
            IpaTiedConsonant? tiedConsonant = null;

            //输入框此时可能已经从x-y转换成x^y，因此同时识别内部规范形式。
            if ((IpaTieBarComposer.TryParse(phoneme, out var parsedTied) ||
                 IpaTieBarComposer.TryParseComposed(phoneme, out parsedTied)) &&
                parsedTied != null)
            {
                tiedConsonant = parsedTied;
                phoneme = tiedConsonant.Symbol;
            }

            if (project.Phonology.Consonants.Any(x =>
                    IpaComposer.AreEquivalent(x.Symbol, phoneme)))
                return;

            //Kiel表中已有的普通辅音和塞擦音
            var ipaConsonant = IpaConsonants.All.FirstOrDefault(x =>
                IpaComposer.AreEquivalent(x.Symbol, phoneme));

            if (ipaConsonant != null)
            {
                ConsonantPhoneme projectConsonant = new()
                {
                    Symbol = ipaConsonant.Symbol,
                    BaseSymbol = ipaConsonant.Symbol,
                    Components = tiedConsonant?.Components.ToList() ?? new List<string>(),
                    Place = ipaConsonant.Place,
                    Manner = ipaConsonant.Manner,
                    Voicing = ipaConsonant.Voicing
                };

                project.Phonology.Consonants.Add(projectConsonant);

                consonantList.Items.Add(new ConsonantEntry
                {
                    Symbol = projectConsonant.Symbol,
                    Place = projectConsonant.Place,
                    Manner = projectConsonant.Manner,
                    Voicing = projectConsonant.Voicing
                });

                RefreshConsonantChart();
            }
            else if (tiedConsonant != null)
            {
                //没有单独表格位置的双重调音仍然作为一个完整音素保存。
                ConsonantPhoneme projectConsonant = new()
                {
                    Symbol = tiedConsonant.Symbol,
                    BaseSymbol = tiedConsonant.Symbol,
                    Components = tiedConsonant.Components.ToList(),
                    Place = tiedConsonant.Place,
                    Manner = tiedConsonant.Manner,
                    Voicing = tiedConsonant.Voicing,
                    Category = "复合调音  Tied articulation",
                    Description = tiedConsonant.Description
                };

                project.Phonology.Consonants.Add(projectConsonant);

                consonantList.Items.Add(new ConsonantEntry
                {
                    Symbol = projectConsonant.Symbol,
                    Place = projectConsonant.Place,
                    Manner = projectConsonant.Manner,
                    Voicing = projectConsonant.Voicing,
                    Category = projectConsonant.Category,
                    Description = projectConsonant.Description
                });

                RefreshConsonantChart();
            }
            else
            {
                var nonPulmonic = IpaNonPulmonicConsonants.All.FirstOrDefault(x =>
                    x.Symbol == phoneme);

                if (nonPulmonic != null)
                {
                    ConsonantPhoneme projectConsonant = new()
                    {
                        Symbol = nonPulmonic.Symbol,
                        Category = nonPulmonic.Category,
                        Description = nonPulmonic.Description
                    };

                    project.Phonology.Consonants.Add(projectConsonant);

                    consonantList.Items.Add(new ConsonantEntry
                    {
                        Symbol = projectConsonant.Symbol,
                        Category = projectConsonant.Category,
                        Description = projectConsonant.Description
                    });

                    RefreshNonPulmonicChart();
                }
                else
                {
                    var otherSymbol = IpaOtherSymbols.All.FirstOrDefault(x =>
                        x.Symbol == phoneme);

                    if (otherSymbol == null)
                    {
                        noMatchingPhonemeLabel.Visible = true;
                        addPhonemeButton.Enabled = false;
                        return;
                    }

                    ConsonantPhoneme projectConsonant = new()
                    {
                        Symbol = otherSymbol.Symbol,
                        Category = "其他 IPA 符号  Other IPA symbol",
                        Description = otherSymbol.Name
                    };

                    project.Phonology.Consonants.Add(projectConsonant);

                    consonantList.Items.Add(new ConsonantEntry
                    {
                        Symbol = projectConsonant.Symbol,
                        Category = projectConsonant.Category,
                        Description = projectConsonant.Description
                    });

                    RefreshOtherSymbols();
                }
            }
        }
        else
        {
            var ipaVowel = IpaVowels.All.FirstOrDefault(x => x.Symbol == phoneme);

            if (ipaVowel == null)
            {
                noMatchingPhonemeLabel.Visible = true;
                addPhonemeButton.Enabled = false;
                return;
            }

            if (project.Phonology.Vowels.Any(x => x.Symbol == phoneme))
                return;

            VowelPhoneme projectVowel = new()
            {
                Symbol = ipaVowel.Symbol,
                BaseSymbol = ipaVowel.Symbol,
                Height = ipaVowel.Height,
                Backness = ipaVowel.Backness,
                Roundedness = ipaVowel.Roundedness
            };

            project.Phonology.Vowels.Add(projectVowel);

            vowelList.Items.Add(new VowelEntry
            {
                Symbol = projectVowel.Symbol,
                Height = projectVowel.Height,
                Backness = projectVowel.Backness,
                Roundedness = projectVowel.Roundedness
            });

            RefreshVowelChart();
        }

        projectModified?.Invoke();

        phonemeInput.Clear();
        phonemeInput.Focus();
    }

    private void RemovePhoneme(object? sender, EventArgs e)
    {
        if (consonantList.SelectedItem is ConsonantEntry consonant)
        {
            if (!ConfirmReferencedPhonemeRemoval(consonant.Symbol))
                return;

            PhonotacticsReferenceService.RemoveReferences(
                project.Phonotactics,
                consonant.Symbol);

            project.Phonology.Consonants.RemoveAll(x =>
                IpaComposer.AreEquivalent(x.Symbol, consonant.Symbol));

            consonantList.Items.Remove(consonant);

            RefreshConsonantChart();
            RefreshNonPulmonicChart();
            RefreshOtherSymbols();

            projectModified?.Invoke();
            return;
        }

        if (vowelList.SelectedItem is VowelEntry vowel)
        {
            if (!ConfirmReferencedPhonemeRemoval(vowel.Symbol))
                return;

            PhonotacticsReferenceService.RemoveReferences(
                project.Phonotactics,
                vowel.Symbol);

            project.Phonology.Vowels.RemoveAll(x =>
                IpaComposer.AreEquivalent(x.Symbol, vowel.Symbol));

            vowelList.Items.Remove(vowel);

            RefreshVowelChart();
            projectModified?.Invoke();
        }
    }

    //删除音素前检查Phonotactics中的引用。
    private bool ConfirmReferencedPhonemeRemoval(string phoneme)
    {
        var references = PhonotacticsReferenceService.FindReferences(
            project.Phonotactics,
            phoneme);

        if (!references.HasReferences)
            return true;

        List<string> affected = new();

        if (references.AllowedOnsets > 0)
            affected.Add($"Allowed Onsets: {references.AllowedOnsets}");

        if (references.AllowedNuclei > 0)
            affected.Add($"Allowed Nuclei: {references.AllowedNuclei}");

        if (references.AllowedCodas > 0)
            affected.Add($"Allowed Codas: {references.AllowedCodas}");

        if (references.ForbiddenSequences > 0)
            affected.Add($"Forbidden Sequences: {references.ForbiddenSequences}");

        var details = string.Join(Environment.NewLine, affected);

        var result = MessageBox.Show(
            this,
            $"音素 {phoneme} 正在被音系配列规则使用。\n" +
            $"删除该音素也会删除包含它的完整规则。\n\n" +
            $"{details}\n\n" +
            $"Phoneme {phoneme} is currently used by phonotactic rules.\n" +
            $"Deleting it will also remove the complete rules containing it.\n\n" +
            $"是否继续？\nContinue?",
            "Conlang Builder Tool",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        return result == DialogResult.Yes;
    }

    private void RefreshConsonantChart()
    {
        foreach (var cell in consonantChartCells.Values)
            cell.Text = "";

        foreach (ConsonantEntry consonant in consonantList.Items)
        {
            var key = (consonant.Place, consonant.Manner, consonant.Voicing);

            if (!consonantChartCells.TryGetValue(key, out var cell))
                continue;

            if (cell.Text.Length == 0)
                cell.Text = consonant.Symbol;
            else
                cell.Text += $" {consonant.Symbol}";

            cell.Font = new Font("Microsoft YaHei UI", 12);
        }
    }

    private void RefreshNonPulmonicChart()
    {
        foreach (var cell in nonPulmonicChartCells.Values)
            cell.Text = "";

        foreach (var consonant in project.Phonology.Consonants)
        {
            if (string.IsNullOrWhiteSpace(consonant.Category))
                continue;

            if (!nonPulmonicChartCells.TryGetValue(consonant.Category, out var cell))
                continue;

            var description = string.IsNullOrWhiteSpace(consonant.Description)
                ? ""
                : GetChinesePart(consonant.Description);

            var line = description.Length == 0
                ? consonant.Symbol
                : $"{consonant.Symbol}    {description}";

            if (cell.Text.Length == 0)
                cell.Text = line;
            else
                cell.Text += Environment.NewLine + line;
        }
    }

    private void RefreshOtherSymbols()
    {
        otherSymbolsContent.Text = "";

        foreach (var consonant in project.Phonology.Consonants)
        {
            var reference = IpaOtherSymbols.All.FirstOrDefault(x => x.Symbol == consonant.Symbol);
            if (reference == null) continue;

            var line = $"{reference.Symbol}    {reference.Name}";

            if (otherSymbolsContent.Text.Length == 0)
                otherSymbolsContent.Text = line;
            else
                otherSymbolsContent.Text += "    " + line;
        }
    }

    private static string GetCategoryDisplayName(string manner)
    {
        return manner switch
        {
            "塞音  Plosive" => "塞音  Plosives",
            "塞擦音  Affricate" => "塞擦音  Affricates",
            "鼻音  Nasal" => "鼻音  Nasals",
            "颤音  Trill" => "颤音  Trills",
            "闪音  Tap / Flap" => "闪音  Taps / Flaps",
            "边闪音  Lateral flap" => "边闪音  Lateral flaps",
            "擦音  Fricative" => "擦音  Fricatives",
            "边擦音  Lateral fricative" => "边擦音  Lateral fricatives",
            "近音  Approximant" => "近音  Approximants",
            "边近音  Lateral approximant" => "边近音  Lateral approximants",
            _ => manner
        };
    }

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

    private static string GetConsonantDescription(IpaConsonant consonant)
    {
        var voicing = consonant.Voicing.StartsWith("清音") ? "清" : "浊";
        return $"{voicing}{GetChinesePart(consonant.Place)}{GetChinesePart(consonant.Manner)}";
    }

    private static string GetVowelDescription(IpaVowel vowel)
    {
        return $"{GetChinesePart(vowel.Roundedness)}{GetChinesePart(vowel.Backness)}{GetChinesePart(vowel.Height)}元音";
    }

    private static string GetChinesePart(string bilingualText)
    {
        var separatorIndex = bilingualText.IndexOf("  ", StringComparison.Ordinal);
        return separatorIndex >= 0 ? bilingualText[..separatorIndex] : bilingualText;
    }

    private enum SelectionMode
    {
        Detailed,
        Guided,
        List
    }

    //下拉栏显示项目
    private class IpaDisplayItem
    {
        public IpaDisplayItem(string text)
        {
            Text = text;
        }

        public IpaDisplayItem(string text, IpaConsonant consonant)
        {
            Text = text;
            Consonant = consonant;
        }

        public IpaDisplayItem(string text, IpaNonPulmonicConsonant consonant)
        {
            Text = text;
            NonPulmonicConsonant = consonant;
        }

        public IpaDisplayItem(string text, IpaOtherSymbol symbol)
        {
            Text = text;
            OtherSymbol = symbol;
        }

        public IpaDisplayItem(string text, IpaVowel vowel)
        {
            Text = text;
            Vowel = vowel;
        }

        public string Text { get; }
        public IpaConsonant? Consonant { get; }
        public IpaNonPulmonicConsonant? NonPulmonicConsonant { get; }
        public IpaOtherSymbol? OtherSymbol { get; }
        public IpaVowel? Vowel { get; }

        public override string ToString()
        {
            return Text;
        }
    }

    private class IpaCategoryItem
    {
        public IpaCategoryItem(string manner, string text)
        {
            Manner = manner;
            Text = text;
        }

        public string Manner { get; }
        public string Text { get; }

        public override string ToString()
        {
            return Text;
        }
    }

    private class IpaVowelCategoryItem
    {
        public IpaVowelCategoryItem(string height, string text)
        {
            Height = height;
            Text = text;
        }

        public string Height { get; }
        public string Text { get; }

        public override string ToString()
        {
            return Text;
        }
    }

    //项目辅音在清单中的显示形式
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
            //非肺部辅音和Other Symbols使用Description显示
            if (!string.IsNullOrWhiteSpace(Description))
                return $"{Symbol}    {GetChinesePart(Description)}";

            var place = Place.Split("  ")[0];
            var manner = Manner.Split("  ")[0];
            var voicing = Voicing.StartsWith("清音") ? "清" : "浊";

            return $"{Symbol}    {place}{voicing}{manner}";
        }
    }

    //项目元音在清单中的显示形式
    private class VowelEntry
    {
        public string Symbol { get; set; } = "";
        public string Height { get; set; } = "";
        public string Backness { get; set; } = "";
        public string Roundedness { get; set; } = "";

        public override string ToString()
        {
            var height = GetChinesePart(Height);
            var backness = GetChinesePart(Backness);
            var roundedness = GetChinesePart(Roundedness);

            return $"{Symbol}    {roundedness}{backness}{height}";
        }
    }
}