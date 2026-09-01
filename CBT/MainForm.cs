using CBT.Pages;
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

    public MainForm()
    {
        InitializeComponent();

        //这部分我故意没有留comment:) 养成良好的编程习惯！
        Text = "Conlang Builder Tool";
        StartPosition = FormStartPosition.CenterScreen;

        ClientSize = new Size(1980, 1080);

        MinimumSize = new Size(1000, 700);

        BuildLayout();
        BuildNavigation();

        ShowControl(new OverviewPage());
        //把点击按钮返回的内容都来自overviewpage

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
        //显示overview页
        overviewButton.Click += (sender, e) => ShowControl(new OverviewPage());
        phonologyButton.Click += (sender, e) => ShowPage("音系  Phonology");
        grammarButton.Click += (sender, e) => ShowPage("语法  Grammar");
        lexiconButton.Click += (sender, e) => ShowPage("词汇  Lexicon");

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
}