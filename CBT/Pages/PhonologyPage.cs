using System;
using System.Collections.Generic;
using System.Text;
using CBT.Data;

namespace CBT.Pages;

public class PhonologyPage : UserControl
{
    //音素清单控件
    private readonly TextBox phonemeInput = new();
    //删除音素的按钮
    private readonly Button removePhonemeButton = new();
    //添加音素的按钮
    private readonly Button addPhonemeButton = new();
    // 音素分类
    private readonly ComboBox phonemeType = new();
    //IPA符号选择器，把无法正常从输入法输入的内容用热键转换成IPA
    private readonly ComboBox ipaSymbolPicker = new();
    //音素选择模式
    private readonly Button selectionModeButton = new();
    //引导模式使用的分类和具体IPA选择器
    private readonly ComboBox ipaCategory = new();
    private readonly ComboBox ipaChoice = new();
    private bool isDetailedMode = true;
    private bool isSynchronizingSelection;
    private int lastIpaSymbolIndex = -1;
    //添加/选择辅音属性
    private readonly ComboBox consonantPlace = new();
    private readonly ComboBox consonantManner = new();
    private readonly ComboBox consonantVoicing = new();

    //为什么自动生成这么多空行？VS？
    private readonly ListBox consonantList = new();
    private readonly ListBox vowelList = new();
    //下拉栏里的分组标题和IPA项目共用这个显示类型
    private class IpaDisplayItem
    {
        public string Text { get; }
        public IpaConsonant? Consonant { get; }

        public IpaDisplayItem(string text, IpaConsonant? consonant = null)
        {
            Text = text;
            Consonant = consonant;
        }

        public override string ToString()
        {
            return Text;
        }
    }
    //引导模式的辅音类别
    private class IpaCategoryItem
    {
        public string Manner { get; }
        public string Text { get; }

        public IpaCategoryItem(string manner, string text)
        {
            Manner = manner;
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
    //辅音和它的语言学属性
    private class ConsonantEntry
    {
        public string Symbol { get; set; } = "";
        public string Place { get; set; } = "";
        public string Manner { get; set; } = "";
        public string Voicing { get; set; } = "";

        public override string ToString()
        {
            return $"{Symbol}    {Place}    {Manner}    {Voicing}";
        }
    }
    public PhonologyPage()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(30);

        Label title = new();

        title.Text = "音系  Phonology";
        title.AutoSize = true;
        title.Font = new Font("Microsoft YaHei UI", 18);

        FlowLayoutPanel contentPanel = new();

        contentPanel.Dock = DockStyle.Fill;
        contentPanel.FlowDirection = FlowDirection.TopDown;
        contentPanel.WrapContents = false;
        contentPanel.AutoScroll = true;
        contentPanel.Padding = new Padding(0, 20, 0, 0);
        //把标题加入内容区域
        contentPanel.Controls.Add(title);
        contentPanel.Controls.Add(BuildPhonemeInventory());
        //内容加入页面
        Controls.Add(contentPanel);
    }

    private Control BuildPhonemeInventory()
    {
        // 音素清单区域
        FlowLayoutPanel section = new();

        section.FlowDirection = FlowDirection.TopDown;
        section.WrapContents = false;
        section.AutoSize = true;
        section.Margin = new Padding(0, 25, 0, 0);


        // 区域标题
        Label sectionTitle = new();

        sectionTitle.Text = "音素清单  Phoneme Inventory";
        sectionTitle.AutoSize = true;
        sectionTitle.Font = new Font("Microsoft YaHei UI", 14);

        //标题和模式切换按钮
        FlowLayoutPanel sectionHeader = new();

        sectionHeader.FlowDirection = FlowDirection.LeftToRight;
        sectionHeader.AutoSize = true;

        selectionModeButton.Text = "切换到引导模式";
        selectionModeButton.AutoSize = true;
        selectionModeButton.Font = new Font("Microsoft YaHei UI", 9);
        selectionModeButton.Margin = new Padding(20, 0, 0, 0);
        selectionModeButton.Click += (sender, e) =>
        {
            isDetailedMode = !isDetailedMode;
            UpdateSelectionMode();
        };

        sectionHeader.Controls.Add(sectionTitle);
        sectionHeader.Controls.Add(selectionModeButton);


        // 输入区域
        FlowLayoutPanel inputRow = new();

        inputRow.FlowDirection = FlowDirection.LeftToRight;
        inputRow.AutoSize = true;
        inputRow.Margin = new Padding(0, 10, 0, 10);


        phonemeInput.Width = 180;
        phonemeInput.Font = new Font("Microsoft YaHei UI", 12);
        //IPA符号选择器
        ipaSymbolPicker.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaSymbolPicker.Width = 250;
        ipaSymbolPicker.Font = new Font("Microsoft YaHei UI", 10);

        //从IPA数据库读取按类别分组的符号和中文说明
        LoadDetailedIpaPicker();

        //选择符号后自动填入输入框
        ipaSymbolPicker.SelectedIndexChanged += (sender, e) =>
        {
            if (isSynchronizingSelection ||
                ipaSymbolPicker.SelectedItem is not IpaDisplayItem selectedItem)
                return;

            //分组标题只负责显示，不能成为实际音素
            if (selectedItem.Consonant == null)
            {
                isSynchronizingSelection = true;
                ipaSymbolPicker.SelectedIndex = lastIpaSymbolIndex;
                isSynchronizingSelection = false;
                return;
            }

            lastIpaSymbolIndex = ipaSymbolPicker.SelectedIndex;
            ApplyConsonant(selectedItem.Consonant);
        };

        //选择音素类型
        phonemeType.Items.Add("辅音  Consonant");
        phonemeType.Items.Add("元音  Vowel");
        phonemeType.SelectedIndex = 0;
        phonemeType.DropDownStyle = ComboBoxStyle.DropDownList;
        phonemeType.Width = 160;

        //切换辅音/元音时，更新当前模式使用的选择器
        phonemeType.SelectedIndexChanged += (sender, e) => UpdateSelectionMode();

        //引导模式：IPA分类
        ipaCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaCategory.Width = 220;
        ipaCategory.Font = new Font("Microsoft YaHei UI", 10);

        //引导模式：具体IPA
        ipaChoice.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaChoice.Width = 240;
        ipaChoice.Font = new Font("Microsoft YaHei UI", 10);

        LoadConsonantCategories();

        ipaCategory.SelectedIndexChanged += (sender, e) =>
        {
            if (isSynchronizingSelection ||
                ipaCategory.SelectedItem is not IpaCategoryItem category)
                return;

            PopulateGuidedChoices(category.Manner);
        };

        ipaChoice.SelectedIndexChanged += (sender, e) =>
        {
            if (isSynchronizingSelection ||
                ipaChoice.SelectedItem is not IpaDisplayItem selectedItem ||
                selectedItem.Consonant == null)
                return;

            ApplyConsonant(selectedItem.Consonant);
        };

        //选择发音部位
        consonantPlace.Items.AddRange(new object[]
        {
            "双唇  Bilabial",
            "唇齿  Labiodental",
            "齿  Dental",
            "齿龈  Alveolar",
            "龈后  Postalveolar",
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

        //选择发音方法
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

        //选择清浊
        consonantVoicing.Items.AddRange(new object[]
        {
            "清音  Voiceless",
            "浊音  Voiced"
        });
        //修改属性时自动更新 IPA 符号
        consonantPlace.SelectedIndexChanged +=
            (sender, e) => UpdateSymbolFromFeatures();

        consonantManner.SelectedIndexChanged +=
            (sender, e) => UpdateSymbolFromFeatures();

        consonantVoicing.SelectedIndexChanged +=
            (sender, e) => UpdateSymbolFromFeatures();

        consonantVoicing.DropDownStyle = ComboBoxStyle.DropDownList;
        consonantVoicing.Width = 150;
        consonantVoicing.SelectedIndex = 0;

        //手动输入 IPA 时自动识别属性
        phonemeInput.TextChanged +=
            (sender, e) => UpdateFeaturesFromSymbol();
        //字体
        phonemeType.Font = new Font("Microsoft YaHei UI", 10);
        consonantPlace.Font = new Font("Microsoft YaHei UI", 10);
        consonantManner.Font = new Font("Microsoft YaHei UI", 10);
        consonantVoicing.Font = new Font("Microsoft YaHei UI", 10);

        //统一间距
        phonemeInput.Margin = new Padding(0, 3, 6, 0);
        phonemeType.Margin = new Padding(0, 3, 6, 0);
        consonantPlace.Margin = new Padding(0, 3, 6, 0);
        consonantManner.Margin = new Padding(0, 3, 6, 0);
        consonantVoicing.Margin = new Padding(0, 3, 6, 0);
        addPhonemeButton.Margin = new Padding(0, 3, 6, 0);
        removePhonemeButton.Margin = new Padding(0, 3, 0, 0);
        ipaSymbolPicker.Margin = new Padding(0, 3, 6, 0);
        ipaCategory.Margin = new Padding(0, 3, 6, 0);
        ipaChoice.Margin = new Padding(0, 3, 6, 0);

        //添加按钮
        addPhonemeButton.Text = "添加";
        addPhonemeButton.Width = 100;
        addPhonemeButton.Height = phonemeType.PreferredHeight;
        addPhonemeButton.Font = new Font("Microsoft YaHei UI", 10);

        addPhonemeButton.Click += AddPhoneme;

        //删除按钮
        removePhonemeButton.Text = "删除";
        removePhonemeButton.Width = 100;
        removePhonemeButton.Height = phonemeType.PreferredHeight;
        removePhonemeButton.Click += RemovePhoneme;
        removePhonemeButton.Font = new Font("Microsoft YaHei UI", 10);

        //列出音素列表:
        //辅音列表
        Label consonantTitle = new();
        consonantTitle.Text = "辅音  Consonants";
        consonantTitle.AutoSize = true;

        consonantList.Size = new Size(500, 180);
        consonantList.Font = new Font("Microsoft YaHei UI", 12);

        // 元音列表
        Label vowelTitle = new();
        vowelTitle.Text = "元音  Vowels";
        vowelTitle.AutoSize = true;

        vowelList.Size = new Size(500, 180);
        vowelList.Font = new Font("Microsoft YaHei UI", 12);

        //输入 添加 删除 所有其他功能
        inputRow.Controls.Add(phonemeInput);
        inputRow.Controls.Add(phonemeType);
        inputRow.Controls.Add(ipaSymbolPicker);
        inputRow.Controls.Add(consonantPlace);
        inputRow.Controls.Add(consonantManner);
        inputRow.Controls.Add(consonantVoicing);
        inputRow.Controls.Add(ipaCategory);
        inputRow.Controls.Add(ipaChoice);
        inputRow.Controls.Add(addPhonemeButton);
        inputRow.Controls.Add(removePhonemeButton);

        UpdateConsonantFields();
        UpdateSymbolFromFeatures();

        section.Controls.Add(sectionHeader);
        section.Controls.Add(inputRow);
        section.Controls.Add(consonantTitle);
        section.Controls.Add(consonantList);

        section.Controls.Add(vowelTitle);
        section.Controls.Add(vowelList);

        return section;
    }
    //加载详细模式中的分组IPA清单
    private void LoadDetailedIpaPicker()
    {
        ipaSymbolPicker.BeginUpdate();
        ipaSymbolPicker.Items.Clear();

        foreach (IGrouping<string, IpaConsonant> category in
            IpaConsonants.All.GroupBy(x => x.Manner))
        {
            ipaSymbolPicker.Items.Add(
                new IpaDisplayItem($"── {GetCategoryDisplayName(category.Key)} ──"));

            foreach (IpaConsonant consonant in category)
            {
                ipaSymbolPicker.Items.Add(
                    new IpaDisplayItem(
                        $"{consonant.Symbol}   {GetConsonantDescription(consonant)}",
                        consonant));
            }
        }

        ipaSymbolPicker.SelectedIndex = -1;
        ipaSymbolPicker.EndUpdate();
    }
    //加载引导模式中的辅音类别
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
    //根据类别加载引导模式中的具体IPA
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
    //确保从元音占位框切回辅音时恢复辅音类别
    private void EnsureConsonantCategories()
    {
        if (ipaCategory.Items.OfType<IpaCategoryItem>().Any())
            return;

        LoadConsonantCategories();
    }
    private void UpdateConsonantFields()
    {
        UpdateSelectionMode();
    }
    //在详细模式和引导模式之间切换
    private void UpdateSelectionMode()
    {
        bool isConsonant = phonemeType.SelectedIndex == 0;
        IpaConsonant? currentConsonant = FindConsonantFromInput();

        ipaSymbolPicker.Visible = isDetailedMode && isConsonant;
        consonantPlace.Visible = isDetailedMode && isConsonant;
        consonantManner.Visible = isDetailedMode && isConsonant;
        consonantVoicing.Visible = isDetailedMode && isConsonant;

        ipaCategory.Visible = !isDetailedMode;
        ipaChoice.Visible = !isDetailedMode;

        selectionModeButton.Text = isDetailedMode
            ? "切换到引导模式"
            : "切换到详细模式";

        if (isDetailedMode)
        {
            if (currentConsonant != null)
                SelectDetailedConsonant(currentConsonant);

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
            //元音暂时只保留模式框架，之后接入元音数据库
            bool wasSynchronizing = isSynchronizingSelection;
            isSynchronizingSelection = true;

            ipaCategory.Items.Clear();
            ipaCategory.Items.Add("元音分类（待扩展）");
            ipaCategory.SelectedIndex = 0;
            ipaCategory.Enabled = false;

            ipaChoice.Items.Clear();
            ipaChoice.Items.Add("具体 IPA（待扩展）");
            ipaChoice.SelectedIndex = 0;
            ipaChoice.Enabled = false;

            isSynchronizingSelection = wasSynchronizing;
        }
    }
    //把一个辅音同步到输入框、属性框和两种IPA选择器
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
    private IpaConsonant? FindConsonantFromInput()
    {
        string symbol = NormalizeInputSymbol(phonemeInput.Text.Trim());

        return IpaConsonants.All.FirstOrDefault(x => x.Symbol == symbol);
    }
    private static string NormalizeInputSymbol(string symbol)
    {
        return symbol == "g" ? "ɡ" : symbol;
    }
    private void ClearIpaSelections()
    {
        bool wasSynchronizing = isSynchronizingSelection;
        isSynchronizingSelection = true;

        ipaSymbolPicker.SelectedIndex = -1;
        ipaChoice.SelectedIndex = -1;
        lastIpaSymbolIndex = -1;

        isSynchronizingSelection = wasSynchronizing;
    }
    //根据辅音属性自动寻找 IPA 符号
    private void UpdateSymbolFromFeatures()
    {
        if (isSynchronizingSelection || phonemeType.SelectedIndex != 0)
            return;

        IpaConsonant? match = IpaConsonants.All.FirstOrDefault(x =>
            x.Place == consonantPlace.Text &&
            x.Manner == consonantManner.Text &&
            x.Voicing == consonantVoicing.Text
        );

        if (match == null)
        {
            bool wasSynchronizing = isSynchronizingSelection;
            isSynchronizingSelection = true;
            phonemeInput.Clear();
            isSynchronizingSelection = wasSynchronizing;
            ClearIpaSelections();
            return;
        }

        ApplyConsonant(match);
    }
    //根据手动输入的 IPA 符号更新辅音属性
    private void UpdateFeaturesFromSymbol()
    {
        if (isSynchronizingSelection)
            return;

        IpaConsonant? match = FindConsonantFromInput();

        if (match == null)
        {
            ClearIpaSelections();
            return;
        }

        //普通键盘输入g时，在这里同步转换成标准IPA字符ɡ
        ApplyConsonant(match);
    }
    //把数据库里的辅音类别改成适合下拉栏显示的双语复数形式
    private static string GetCategoryDisplayName(string manner)
    {
        return manner switch
        {
            "塞音  Plosive" => "塞音  Plosives",
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
    //把辅音属性组合成下拉栏中的中文说明
    private static string GetConsonantDescription(IpaConsonant consonant)
    {
        string voicing = consonant.Voicing.StartsWith("清音") ? "清" : "浊";

        return $"{voicing}{GetChinesePart(consonant.Place)}" +
            $"{GetChinesePart(consonant.Manner)}";
    }
    private static string GetChinesePart(string bilingualText)
    {
        int separatorIndex = bilingualText.IndexOf("  ", StringComparison.Ordinal);

        return separatorIndex >= 0
            ? bilingualText[..separatorIndex]
            : bilingualText;
    }
    //添加音素，输入以后按按钮来添加
    private void AddPhoneme(object? sender, EventArgs e)
    {
        string phoneme = phonemeInput.Text.Trim();

        //不样添加空内容
        if (phoneme.Length == 0)
            return;

        //添加辅音
        if (phonemeType.SelectedIndex == 0)
        {
            //避免重复音素
            foreach (ConsonantEntry item in consonantList.Items)
            {
                if (item.Symbol == phoneme)
                    return;
            }

            ConsonantEntry consonant = new();

            consonant.Symbol = phoneme;
            consonant.Place = consonantPlace.Text;
            consonant.Manner = consonantManner.Text;
            consonant.Voicing = consonantVoicing.Text;

            consonantList.Items.Add(consonant);
        }

        //添加元音
        else
        {
            if (!vowelList.Items.Contains(phoneme))
                vowelList.Items.Add(phoneme);
        }

        phonemeInput.Clear();
        phonemeInput.Focus();
    }
    //删除音素的方法
    private void RemovePhoneme(object? sender, EventArgs e)
    {
        //优先删除辅音列表中的选中项
        if (consonantList.SelectedItem != null)
        {
            consonantList.Items.Remove(consonantList.SelectedItem);
            return;
        }

        //否则删除元音列表中的选中项
        if (vowelList.SelectedItem != null)
        {
            vowelList.Items.Remove(vowelList.SelectedItem);
        }
    }
}
