using CBT.Models;
using CBT.Pages;
using CBT.Services;

namespace CBT;

public partial class MainForm : Form
{
    private readonly MenuStrip mainMenu = new();
    private readonly SplitContainer mainSplitContainer = new();
    private readonly FlowLayoutPanel navigationPanel = new();
    private readonly Panel workspacePanel = new();

    private string? currentFilePath;
    private ConlangProject currentProject = new();
    private bool isModified;

    public MainForm()
    {
        InitializeComponent();

        Text = "Conlang Builder Tool";
        UpdateWindowTitle();
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1980, 1080);
        MinimumSize = new Size(1000, 700);

        BuildLayout();
        BuildFileMenu();
        BuildNavigation();
        ShowControl(new OverviewPage(currentProject, MarkProjectModified));
    }

    private void MarkProjectModified()
    {
        isModified = true;
        UpdateWindowTitle();
    }

    private void UpdateWindowTitle()
    {
        string projectName = string.IsNullOrWhiteSpace(currentFilePath)
            ? "Untitled"
            : Path.GetFileName(currentFilePath);

        var modifiedMark = isModified ? " *" : "";
        Text = $"Conlang Builder Tool - {projectName}{modifiedMark}";
    }

    private void BuildNavigation()
    {
        var overviewButton = CreateNavigationButton("概览  Overview");
        var phonologyButton = CreateNavigationButton("音系  Phonology");
        var grammarButton = CreateNavigationButton("语法  Grammar");
        var lexiconButton = CreateNavigationButton("词汇  Lexicon");

        navigationPanel.Controls.Add(overviewButton);
        navigationPanel.Controls.Add(phonologyButton);
        navigationPanel.Controls.Add(grammarButton);
        navigationPanel.Controls.Add(lexiconButton);

        overviewButton.Click += (sender, e) =>
            ShowControl(new OverviewPage(currentProject, MarkProjectModified));

        phonologyButton.Click += (sender, e) =>
            ShowControl(new PhonologyPage(currentProject, MarkProjectModified));

        grammarButton.Click += (sender, e) => ShowPage("语法  Grammar");
        lexiconButton.Click += (sender, e) => ShowPage("词汇  Lexicon");

        FormClosing += MainForm_FormClosing;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!ConfirmUnsavedChanges()) e.Cancel = true;
    }

    //存在未保存修改时询问用户是否保存。
    private bool ConfirmUnsavedChanges()
    {
        if (!isModified) return true;

        var result = MessageBox.Show(
            this,
            "当前项目有尚未保存的修改。\n" +
            "是否在继续之前保存？\n\n" +
            "The current project has unsaved changes.\n" +
            "Save before continuing?",
            "Conlang Builder Tool",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Cancel) return false;
        if (result == DialogResult.No) return true;

        SaveProject();

        //如果用户在Save As窗口中取消，项目仍然保持修改状态。
        return !isModified;
    }

    private void ShowPage(string pageName)
    {
        workspacePanel.Controls.Clear();

        Label pageTitle = new()
        {
            Text = pageName,
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 18)
        };

        workspacePanel.Controls.Add(pageTitle);
    }

    private void ShowControl(Control page)
    {
        workspacePanel.Controls.Clear();
        page.Dock = DockStyle.Fill;
        workspacePanel.Controls.Add(page);
    }

    private Button CreateNavigationButton(string text)
    {
        Button button = new()
        {
            Text = text,
            Size = new Size(240, 50),
            Margin = new Padding(0, 0, 0, 10),
            Font = new Font("Microsoft YaHei UI", 12, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleCenter
        };

        return button;
    }

    private void BuildLayout()
    {
        mainMenu.Dock = DockStyle.Top;

        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.Orientation = Orientation.Vertical;
        mainSplitContainer.FixedPanel = FixedPanel.Panel1;
        mainSplitContainer.SplitterWidth = 4;

        navigationPanel.Dock = DockStyle.Fill;
        navigationPanel.FlowDirection = FlowDirection.TopDown;
        navigationPanel.WrapContents = false;
        navigationPanel.AutoScroll = true;
        navigationPanel.Padding = new Padding(10);

        workspacePanel.Dock = DockStyle.Fill;
        workspacePanel.Padding = new Padding(30);

        mainSplitContainer.Panel1.Controls.Add(navigationPanel);
        mainSplitContainer.Panel2.Controls.Add(workspacePanel);

        Controls.Add(mainSplitContainer);
        Controls.Add(mainMenu);

        MainMenuStrip = mainMenu;
        Load += (sender, e) =>
        {
            mainSplitContainer.Panel1MinSize = 280;
            mainSplitContainer.SplitterDistance = 280;
        };
    }

    private void BuildFileMenu()
    {
        ToolStripMenuItem fileMenu = new("文件  File");
        ToolStripMenuItem newProjectItem = new("新建项目  New Project");
        ToolStripMenuItem openProjectItem = new("打开项目  Open Project");
        ToolStripMenuItem saveProjectItem = new("保存  Save");
        ToolStripMenuItem saveProjectAsItem = new("另存为  Save As");

        newProjectItem.ShortcutKeys = Keys.Control | Keys.N;
        openProjectItem.ShortcutKeys = Keys.Control | Keys.O;
        saveProjectItem.ShortcutKeys = Keys.Control | Keys.S;
        saveProjectAsItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;

        newProjectItem.Click += (sender, e) => NewProject();
        openProjectItem.Click += (sender, e) => OpenProject();
        saveProjectItem.Click += (sender, e) => SaveProject();
        saveProjectAsItem.Click += (sender, e) => SaveProjectAs();

        fileMenu.DropDownItems.Add(newProjectItem);
        fileMenu.DropDownItems.Add(openProjectItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(saveProjectItem);
        fileMenu.DropDownItems.Add(saveProjectAsItem);

        mainMenu.Items.Add(fileMenu);
    }

    private void NewProject()
    {
        if (!ConfirmUnsavedChanges()) return;

        currentProject = new ConlangProject();
        currentFilePath = null;
        isModified = false;

        UpdateWindowTitle();
        ShowControl(new OverviewPage(currentProject, MarkProjectModified));
    }

    private void OpenProject()
    {
        if (!ConfirmUnsavedChanges()) return;

        using OpenFileDialog dialog = new()
        {
            Filter = "CBT Project (*.cbt)|*.cbt|All files (*.*)|*.*",
            DefaultExt = "cbt",
            AddExtension = true,
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            currentProject = ProjectFileService.Load(dialog.FileName);
            currentFilePath = dialog.FileName;
            isModified = false;

            UpdateWindowTitle();
            ShowControl(new OverviewPage(currentProject, MarkProjectModified));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"无法打开项目。\nCould not open the project.\n\n{ex.Message}",
                "Conlang Builder Tool",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void SaveProject()
    {
        if (string.IsNullOrWhiteSpace(currentFilePath))
        {
            SaveProjectAs();
            return;
        }

        SaveProjectTo(currentFilePath);
    }

    private void SaveProjectAs()
    {
        using SaveFileDialog dialog = new()
        {
            Filter = "CBT Project (*.cbt)|*.cbt|All files (*.*)|*.*",
            DefaultExt = "cbt",
            AddExtension = true,
            OverwritePrompt = true
        };

        if (!string.IsNullOrWhiteSpace(currentFilePath))
        {
            dialog.FileName = Path.GetFileName(currentFilePath);
            dialog.InitialDirectory = Path.GetDirectoryName(currentFilePath);
        }
        else if (!string.IsNullOrWhiteSpace(currentProject.Name))
        {
            dialog.FileName = currentProject.Name + ".cbt";
        }

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        if (SaveProjectTo(dialog.FileName))
        {
            currentFilePath = dialog.FileName;
            UpdateWindowTitle();
        }
    }

    private bool SaveProjectTo(string filePath)
    {
        try
        {
            ProjectFileService.Save(filePath, currentProject);

            isModified = false;
            UpdateWindowTitle();

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"无法保存项目。\nCould not save the project.\n\n{ex.Message}",
                "Conlang Builder Tool",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return false;
        }
    }
}