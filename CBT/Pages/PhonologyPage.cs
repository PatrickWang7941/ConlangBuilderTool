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

        addPhonemeButton.Text = "添加  Add";
        addPhonemeButton.Size = new Size(110, 35);

        //点击按钮时添加音素
        addPhonemeButton.Click += AddPhoneme;

        //点击按钮时删除选中的音素
        removePhonemeButton.Text = "删除  Remove";
        removePhonemeButton.Size = new Size(110, 35);
        removePhonemeButton.Click += RemovePhoneme;

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

        //输入 添加 和删除
        inputRow.Controls.Add(phonemeInput);
        inputRow.Controls.Add(phonemeType);
        inputRow.Controls.Add(addPhonemeButton);
        inputRow.Controls.Add(removePhonemeButton);

        section.Controls.Add(sectionTitle);
        section.Controls.Add(inputRow);
        section.Controls.Add(consonantTitle);
        section.Controls.Add(consonantList);

        section.Controls.Add(vowelTitle);
        section.Controls.Add(vowelList);

        return section;
    }
    //添加音素，输入以后按按钮来添加
    private void AddPhoneme(object? sender, EventArgs e)
    {
        string phoneme = phonemeInput.Text.Trim();

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