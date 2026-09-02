using CBT.Models;

namespace CBT.Pages;

public class OverviewPage : UserControl
{
    // 项目描述输入框。
    private readonly TextBox descriptionTextBox = new();

    // 项目名称输入框。
    private readonly TextBox nameTextBox = new();

    // 当前正在编辑的语言项目。
    private readonly ConlangProject project;

    // 当项目内容发生变化时，用来通知主窗口。
    private readonly Action? projectModified;

    public OverviewPage(
        ConlangProject project,
        Action? projectModified)
    {
        this.project = project;
        this.projectModified = projectModified;

        Dock = DockStyle.Fill;
        Padding = new Padding(30);

        BuildLayout();

        // 先读取项目已有内容。
        LoadProjectData();

        // 读取完成以后再监听修改事件，
        // 防止打开项目时被错误标记为“未保存”。
        ConnectEvents();
    }

    // 创建 Overview 页面的界面。
    private void BuildLayout()
    {
        FlowLayoutPanel contentPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        Label title = new()
        {
            Text = "概览  Overview",
            AutoSize = true,
            Font = new Font(
                "Microsoft YaHei UI",
                18),
            Margin = new Padding(0, 0, 0, 30)
        };

        Label nameLabel = new()
        {
            Text = "项目名称  Project Name",
            AutoSize = true,
            Font = new Font(
                "Microsoft YaHei UI",
                11),
            Margin = new Padding(0, 0, 0, 5)
        };

        nameTextBox.Width = 700;
        nameTextBox.Font = new Font(
            "Microsoft YaHei UI",
            11);
        nameTextBox.Margin =
            new Padding(0, 0, 0, 25);

        Label descriptionLabel = new()
        {
            Text = "描述  Description",
            AutoSize = true,
            Font = new Font(
                "Microsoft YaHei UI",
                11),
            Margin = new Padding(0, 0, 0, 5)
        };

        descriptionTextBox.Width = 700;
        descriptionTextBox.Height = 220;
        descriptionTextBox.Multiline = true;
        descriptionTextBox.ScrollBars =
            ScrollBars.Vertical;
        descriptionTextBox.Font = new Font(
            "Microsoft YaHei UI",
            11);
        descriptionTextBox.Margin =
            new Padding(0);

        contentPanel.Controls.Add(title);
        contentPanel.Controls.Add(nameLabel);
        contentPanel.Controls.Add(nameTextBox);
        contentPanel.Controls.Add(descriptionLabel);
        contentPanel.Controls.Add(descriptionTextBox);

        Controls.Add(contentPanel);
    }

    // 把当前项目中的数据放入输入框。
    private void LoadProjectData()
    {
        nameTextBox.Text =
            project.Name;

        descriptionTextBox.Text =
            project.Description;
    }

    // 监听用户对项目资料的修改。
    private void ConnectEvents()
    {
        nameTextBox.TextChanged +=
            (sender, e) =>
            {
                project.Name =
                    nameTextBox.Text;

                projectModified?.Invoke();
            };

        descriptionTextBox.TextChanged +=
            (sender, e) =>
            {
                project.Description =
                    descriptionTextBox.Text;

                projectModified?.Invoke();
            };
    }
}