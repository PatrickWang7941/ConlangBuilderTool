using System;
using System.Collections.Generic;
using System.Text;

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
    //添加/选择辅音属性
    private readonly ComboBox consonantPlace = new();
    private readonly ComboBox consonantManner = new();
    private readonly ComboBox consonantVoicing = new();

    //为什么自动生成这么多空行？VS？
    private readonly ListBox consonantList = new();
    private readonly ListBox vowelList = new();
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

        consonantVoicing.DropDownStyle = ComboBoxStyle.DropDownList;
        consonantVoicing.Width = 150;
        consonantVoicing.SelectedIndex = 0;
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

        //输入 添加 删除
        inputRow.Controls.Add(phonemeInput);
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
        // 只有选择“辅音”时才显示这些属性
        bool isConsonant = phonemeType.SelectedIndex == 0;

        consonantPlace.Visible = isConsonant;
        consonantManner.Visible = isConsonant;
        consonantVoicing.Visible = isConsonant;
    }
    //添加音素，输入以后按按钮来添加
    private void AddPhoneme(object? sender, EventArgs e)
    {
        string phoneme = phonemeInput.Text.Trim();

        //不样添加空内容
        if (phoneme.Length == 0)
            return;

        ListBox targetList;

        if (phonemeType.SelectedIndex == 0)
            targetList = consonantList;
        else
            targetList = vowelList;

        //避免重复添加
        if (!targetList.Items.Contains(phoneme))
            targetList.Items.Add(phoneme);

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