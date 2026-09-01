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
    //添加/选择辅音属性
    private readonly ComboBox consonantPlace = new();
    private readonly ComboBox consonantManner = new();
    private readonly ComboBox consonantVoicing = new();

    //为什么自动生成这么多空行？VS？
    private readonly ListBox consonantList = new();
    private readonly ListBox vowelList = new();
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


        // 输入区域
        FlowLayoutPanel inputRow = new();

        inputRow.FlowDirection = FlowDirection.LeftToRight;
        inputRow.AutoSize = true;
        inputRow.Margin = new Padding(0, 10, 0, 10);


        phonemeInput.Width = 180;
        phonemeInput.Font = new Font("Microsoft YaHei UI", 12);
        //IPA符号选择器
        ipaSymbolPicker.DropDownStyle = ComboBoxStyle.DropDownList;
        ipaSymbolPicker.Width = 90;
        ipaSymbolPicker.Font = new Font("Microsoft YaHei UI", 10);

        //从IPA数据库读取符号
        foreach (IpaConsonant consonant in IpaConsonants.All)
        {
            ipaSymbolPicker.Items.Add(consonant.Symbol);
        }

        ipaSymbolPicker.SelectedIndex = -1;

        //选择符号后自动填入输入框
        ipaSymbolPicker.SelectedIndexChanged += (sender, e) =>
        {
            if (ipaSymbolPicker.SelectedItem != null)
                phonemeInput.Text = ipaSymbolPicker.SelectedItem.ToString();
        };

        //选择音素类型
        phonemeType.Items.Add("辅音  Consonant");
        phonemeType.Items.Add("元音  Vowel");
        phonemeType.SelectedIndex = 0;
        phonemeType.DropDownStyle = ComboBoxStyle.DropDownList;
        phonemeType.Width = 160;

        // 切换辅音/元音时，显示或隐藏辅音属性
        phonemeType.SelectedIndexChanged += (sender, e) => UpdateConsonantFields();

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
        inputRow.Controls.Add(ipaSymbolPicker);
        inputRow.Controls.Add(phonemeType);
        inputRow.Controls.Add(consonantPlace);
        inputRow.Controls.Add(consonantManner);
        inputRow.Controls.Add(consonantVoicing);
        inputRow.Controls.Add(addPhonemeButton);
        inputRow.Controls.Add(removePhonemeButton);

        UpdateConsonantFields();

        section.Controls.Add(sectionTitle);
        section.Controls.Add(inputRow);
        section.Controls.Add(consonantTitle);
        section.Controls.Add(consonantList);

        section.Controls.Add(vowelTitle);
        section.Controls.Add(vowelList);

        return section;
    }
    private void UpdateConsonantFields()
    {
        //只有选择“辅音”时才显示这些属性
        bool isConsonant = phonemeType.SelectedIndex == 0;

        consonantPlace.Visible = isConsonant;
        consonantManner.Visible = isConsonant;
        consonantVoicing.Visible = isConsonant;
    }
    //根据辅音属性自动寻找 IPA 符号
    private void UpdateSymbolFromFeatures()
    {
        if (phonemeType.SelectedIndex != 0)
            return;

        IpaConsonant? match = IpaConsonants.All.FirstOrDefault(x =>
            x.Place == consonantPlace.Text &&
            x.Manner == consonantManner.Text &&
            x.Voicing == consonantVoicing.Text
        );

        phonemeInput.Text = match?.Symbol ?? "";
    }
    //根据手动输入的 IPA 符号更新辅音属性
    private void UpdateFeaturesFromSymbol()
    {
        string symbol = phonemeInput.Text.Trim();

        //普通键盘打不了ɡ，只能输入g然后转换，后面其他的也会一样。
        if (symbol == "g")
            symbol = "ɡ";

        IpaConsonant? match =
            IpaConsonants.All.FirstOrDefault(x => x.Symbol == symbol);

        if (match == null)
        {
            ipaSymbolPicker.SelectedIndex = -1;
            return;
        }
        //手动输入符号时，同步IPA选择器
        if (ipaSymbolPicker.SelectedItem?.ToString() != match.Symbol)
            ipaSymbolPicker.SelectedItem = match.Symbol;
        //如果键盘输入不了IPA字符，自动转换。
        phonemeInput.Text = match.Symbol;
        phonemeInput.SelectionStart = phonemeInput.Text.Length;

        phonemeType.SelectedIndex = 0;

        consonantPlace.SelectedItem = match.Place;
        consonantManner.SelectedItem = match.Manner;
        consonantVoicing.SelectedItem = match.Voicing;
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