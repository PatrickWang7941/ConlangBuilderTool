using CBT.Models;

namespace CBT.Pages;

public class OverviewPage : UserControl
{
    private readonly TextBox descriptionTextBox = new();
    private readonly TextBox nameTextBox = new();
    private readonly ConlangProject project;
    private readonly Action? projectModified;

    public OverviewPage(ConlangProject project, Action? projectModified)
    {
        this.project = project;
        this.projectModified = projectModified;

        Dock = DockStyle.Fill;
        Padding = new Padding(0);

        BuildLayout();
        LoadProjectData();
        ConnectEvents();
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

        Label nameLabel = new()
        {
            Text = "项目名称  Project Name",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 11),
            Margin = new Padding(0, 0, 0, 5)
        };

        nameTextBox.Width = 700;
        nameTextBox.Font = new Font("Microsoft YaHei UI", 11);
        nameTextBox.Margin = new Padding(0, 0, 0, 25);

        Label descriptionLabel = new()
        {
            Text = "描述  Description",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 11),
            Margin = new Padding(0, 0, 0, 5)
        };

        descriptionTextBox.Width = 700;
        descriptionTextBox.Height = 220;
        descriptionTextBox.Multiline = true;
        descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        descriptionTextBox.Font = new Font("Microsoft YaHei UI", 11);
        descriptionTextBox.Margin = new Padding(0);

        contentPanel.Controls.Add(nameLabel);
        contentPanel.Controls.Add(nameTextBox);
        contentPanel.Controls.Add(descriptionLabel);
        contentPanel.Controls.Add(descriptionTextBox);

        Controls.Add(contentPanel);
    }

    private void LoadProjectData()
    {
        nameTextBox.Text = project.Name;
        descriptionTextBox.Text = project.Description;
    }

    private void ConnectEvents()
    {
        nameTextBox.TextChanged += (sender, e) =>
        {
            project.Name = nameTextBox.Text;
            projectModified?.Invoke();
        };

        descriptionTextBox.TextChanged += (sender, e) =>
        {
            project.Description = descriptionTextBox.Text;
            projectModified?.Invoke();
        };
    }
}