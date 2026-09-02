using CBT.Models;
using CBT.Pages;
using CBT.Services;
namespace CBT;

public partial class MainForm : Form
{
    // 菜单栏放在顶部
    private readonly MenuStrip mainMenu = new();

    // 主区域
    private readonly SplitContainer mainSplitContainer = new();

    // 左侧导航区域
    private readonly FlowLayoutPanel navigationPanel = new();

    // 右侧工作区域
    private readonly Panel workspacePanel = new();
    // 当前工作项目
    private ConlangProject currentProject = new();
    // 当前项目对应的文件路径。
    // 新建但尚未保存的项目没有路径。
    private string? currentFilePath;

    // 当前项目是否存在尚未保存的修改。
    private bool isModified;
    public MainForm()
    {
        InitializeComponent();

        //这部分我故意没有留comment:) 养成良好的编程习惯！
        Text = "Conlang Builder Tool";
        UpdateWindowTitle();
        StartPosition = FormStartPosition.CenterScreen;

        ClientSize = new Size(1980, 1080);

        MinimumSize = new Size(1000, 700);

        BuildLayout();
        BuildFileMenu();
        BuildNavigation();

        ShowControl(new OverviewPage());
        //把点击按钮返回的内容都来自overviewpage

    }
    // 标记当前项目已经发生修改，并刷新窗口标题。
    private void MarkProjectModified()
    {
        isModified = true;
        UpdateWindowTitle();
    }

    // 根据当前项目文件和修改状态刷新窗口标题。
    private void UpdateWindowTitle()
    {
        string projectName;

        if (string.IsNullOrWhiteSpace(currentFilePath))
        {
            projectName = "Untitled";
        }
        else
        {
            projectName = Path.GetFileName(currentFilePath);
        }

        string modifiedMark =
            isModified ? " *" : "";

        Text =
            $"Conlang Builder Tool - " +
            $"{projectName}" +
            $"{modifiedMark}";
    }
    private void BuildNavigation()
    {
        //按钮输出
        Button overviewButton = CreateNavigationButton("概览  Overview");
        Button phonologyButton = CreateNavigationButton("音系  Phonology");
        Button grammarButton = CreateNavigationButton("语法  Grammar");
        Button lexiconButton = CreateNavigationButton("词汇  Lexicon");

        //增加按钮
        navigationPanel.Controls.Add(overviewButton);
        navigationPanel.Controls.Add(phonologyButton);
        navigationPanel.Controls.Add(grammarButton);
        navigationPanel.Controls.Add(lexiconButton);
        //点击按钮来展示页面
        //显示overview页，后面同理，内容直接来自对应的.cs
        overviewButton.Click += (sender, e) => ShowControl(new OverviewPage());
        phonologyButton.Click += (sender, e) => ShowControl(new PhonologyPage(currentProject,MarkProjectModified));
        grammarButton.Click += (sender, e) => ShowPage("语法  Grammar");
        lexiconButton.Click += (sender, e) => ShowPage("词汇  Lexicon");
        FormClosing += MainForm_FormClosing;

    }
    // 关闭程序前检查是否存在尚未保存的修改。
    private void MainForm_FormClosing(
        object? sender,
        FormClosingEventArgs e)
    {
        if (!ConfirmUnsavedChanges())
        {
            e.Cancel = true;
        }
    }
    // 如果当前项目有未保存修改，询问用户是否保存。
    // 返回 false 表示当前操作应该取消。
    private bool ConfirmUnsavedChanges()
    {
        if (!isModified)
        {
            return true;
        }

        DialogResult result =
            MessageBox.Show(
                this,
                "当前项目有尚未保存的修改。\n" +
                "是否在继续之前保存？\n\n" +
                "The current project has unsaved changes.\n" +
                "Save before continuing?",
                "Conlang Builder Tool",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

        if (result == DialogResult.Cancel)
        {
            return false;
        }

        if (result == DialogResult.No)
        {
            return true;
        }

        SaveProject();

        // 如果用户在 Save As 窗口里又点了取消，
        // isModified 仍然会是 true，因此原操作也取消。
        return !isModified;
    }
    private void ShowPage(string pageName)
    {
        workspacePanel.Controls.Clear();

        Label pageTitle = new();

        pageTitle.Text = pageName;
        pageTitle.AutoSize = true;
        pageTitle.Font = new Font("Microsoft YaHei UI", 18);

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
        //加入按钮
        Button button = new();

        //按钮的文本和大小
        button.Text = text;
        button.Size = new Size(200, 50);
        button.Margin = new Padding(0, 0, 0, 10);

        //文字变大一点，更换字体。
        button.Font = new Font(
            "Microsoft YaHei UI",
            12,
            FontStyle.Regular
        );

        //对齐。
        button.TextAlign = ContentAlignment.MiddleCenter;

        return button;
    }
    //构建导航窗口按钮

    private void BuildLayout()
    {

        // 顶部菜单栏
        mainMenu.Dock = DockStyle.Top;

        // 主区域
        mainSplitContainer.Dock = DockStyle.Fill;

        // 左右分栏
        mainSplitContainer.Orientation = Orientation.Vertical;

        // 左侧宽度固定
        mainSplitContainer.FixedPanel = FixedPanel.Panel1;

        // 左侧导航区域
        navigationPanel.Dock = DockStyle.Fill;

        // 控件从上到下排列
        navigationPanel.FlowDirection = FlowDirection.TopDown;

        // 不自动换列
        navigationPanel.WrapContents = false;

        // 内容太多时出现滚动条
        navigationPanel.AutoScroll = true;

        // 内边距
        navigationPanel.Padding = new Padding(10);
        // 右侧工作区域
        workspacePanel.Dock = DockStyle.Fill;
        workspacePanel.Padding = new Padding(30);

        // 把控件组合起来

        // 导航栏放进左侧区域
        mainSplitContainer.Panel1.Controls.Add(navigationPanel);
        //右侧
        mainSplitContainer.Panel2.Controls.Add(workspacePanel);

        // 主区域加入窗口
        Controls.Add(mainSplitContainer);

        // 左侧导航栏宽度
        mainSplitContainer.SplitterDistance = 260;
        mainSplitContainer.SplitterWidth = 4;
        mainSplitContainer.Panel1MinSize = 260;

        // 菜单栏加入窗口
        Controls.Add(mainMenu);

        // 指定主菜单
        MainMenuStrip = mainMenu;
    }
    // 构建顶部文件菜单，并连接新建、打开、保存和另存为功能。
    private void BuildFileMenu()
    {
        ToolStripMenuItem fileMenu = new("文件  File");

        ToolStripMenuItem newProjectItem =
            new("新建项目  New Project");

        ToolStripMenuItem openProjectItem =
            new("打开项目  Open Project");

        ToolStripMenuItem saveProjectItem =
            new("保存  Save");

        ToolStripMenuItem saveProjectAsItem =
            new("另存为  Save As");

        // 设置常用文件操作快捷键。
        newProjectItem.ShortcutKeys =
            Keys.Control | Keys.N;

        openProjectItem.ShortcutKeys =
            Keys.Control | Keys.O;

        saveProjectItem.ShortcutKeys =
            Keys.Control | Keys.S;

        saveProjectAsItem.ShortcutKeys =
            Keys.Control | Keys.Shift | Keys.S;

        // 点击菜单项时调用对应的项目操作。
        newProjectItem.Click +=
            (sender, e) => NewProject();

        openProjectItem.Click +=
            (sender, e) => OpenProject();

        saveProjectItem.Click +=
            (sender, e) => SaveProject();

        saveProjectAsItem.Click +=
            (sender, e) => SaveProjectAs();

        // 将菜单项按顺序加入文件菜单。
        fileMenu.DropDownItems.Add(newProjectItem);
        fileMenu.DropDownItems.Add(openProjectItem);

        fileMenu.DropDownItems.Add(
            new ToolStripSeparator());

        fileMenu.DropDownItems.Add(saveProjectItem);
        fileMenu.DropDownItems.Add(saveProjectAsItem);

        mainMenu.Items.Add(fileMenu);
    }


    // 创建一个新的空项目，并清除当前项目对应的文件路径。


    // 让用户选择一个 .cbt 文件，并将其中的数据读取为当前项目。
    private void OpenProject()
    {
        if (!ConfirmUnsavedChanges())
        {
            return;
        }
        using OpenFileDialog dialog = new()
        {
            Filter =
                "CBT Project (*.cbt)|*.cbt|" +
                "All files (*.*)|*.*",

            DefaultExt = "cbt",
            AddExtension = true,
            CheckFileExists = true,
            Multiselect = false 
        };

        // 用户取消选择文件时不进行任何操作。
        if (dialog.ShowDialog(this) !=
            DialogResult.OK)
        {
            return;
        }

        try
        {
            // 从文件中读取项目数据。
            currentProject =
                ProjectFileService.Load(
                    dialog.FileName);

            // 保存当前项目的文件路径，
            // 之后使用“保存”时可以直接覆盖这个文件。
            currentFilePath =
                dialog.FileName;

            isModified = false;
            UpdateWindowTitle();

            // 打开新项目后返回概览页。
            // 用户再次进入音系页面时，会读取新的 currentProject。
            ShowControl(new OverviewPage());
        }
        catch (Exception ex)
        {
            // 文件损坏、格式错误或读取失败时显示错误信息。
            MessageBox.Show(
                this,
                $"无法打开项目。\n" +
                $"Could not open the project.\n\n" +
                ex.Message,
                "Conlang Builder Tool",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }


    // 保存当前项目。
    // 如果项目还没有文件路径，则自动进入“另存为”流程。
    private void SaveProject()
    {
        if (string.IsNullOrWhiteSpace(
            currentFilePath))
        {
            SaveProjectAs();
            return;
        }

        // 已经保存过的项目直接覆盖原文件。
        SaveProjectTo(currentFilePath);
    }


    // 让用户选择新的文件路径，并把当前项目保存为 .cbt 文件。
    private void SaveProjectAs()
    {
        using SaveFileDialog dialog = new()
        {
            Filter =
                "CBT Project (*.cbt)|*.cbt|" +
                "All files (*.*)|*.*",

            DefaultExt = "cbt",
            AddExtension = true,
            OverwritePrompt = true
        };

        // 如果项目已经有文件路径，
        // 另存为时默认使用原文件名和所在目录。
        if (!string.IsNullOrWhiteSpace(
            currentFilePath))
        {
            dialog.FileName =
                Path.GetFileName(
                    currentFilePath);

            dialog.InitialDirectory =
                Path.GetDirectoryName(
                    currentFilePath);
        }

        // 如果是从未保存过的新项目，
        // 并且已经设置项目名称，则使用项目名称作为默认文件名。
        else if (!string.IsNullOrWhiteSpace(
            currentProject.Name))
        {
            dialog.FileName =
                currentProject.Name + ".cbt";
        }

        // 用户取消保存时不进行任何操作。
        if (dialog.ShowDialog(this) !=
            DialogResult.OK)
        {
            return;
        }

        // 保存成功后记录新的文件路径。
        if (SaveProjectTo(dialog.FileName))
        {
            currentFilePath =
                dialog.FileName;

            UpdateWindowTitle();
        }
    }
    private void NewProject()
    {
        if (!ConfirmUnsavedChanges())
        {
            return;
        }
        currentProject =
            new ConlangProject();

        currentFilePath = null;

        isModified = false;
        UpdateWindowTitle();

        ShowControl(
            new OverviewPage());
    }

    // 将当前项目保存到指定路径。
    // 保存成功返回 true，失败返回 false。
    private bool SaveProjectTo(
        string filePath)
    {
        try
        {
            // 调用项目文件服务，将 currentProject 序列化并写入文件。
            ProjectFileService.Save(
                filePath,
                currentProject);

            isModified = false;
            UpdateWindowTitle();

            return true;
        }
        catch (Exception ex)
        {
            // 文件无法写入或保存失败时显示错误信息。
            MessageBox.Show(
                this,
                $"无法保存项目。\n" +
                $"Could not save the project.\n\n" +
                ex.Message,
                "Conlang Builder Tool",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return false;
        }
    }
}