namespace CBT;

public partial class MainForm : Form
{
    // 菜单栏放在顶部
    private readonly MenuStrip mainMenu = new();

    // 主区域
    private readonly SplitContainer mainSplitContainer = new();

    // 左侧导航区域
    private readonly FlowLayoutPanel navigationPanel = new();


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
        
    }

    private void BuildNavigation()
    {
        Button overviewButton = CreateNavigationButton("Overview");
        Button phonologyButton = CreateNavigationButton("Phonology");
        Button grammarButton = CreateNavigationButton("Grammar");
        Button lexiconButton = CreateNavigationButton("Lexicon");

        navigationPanel.Controls.Add(overviewButton);
        navigationPanel.Controls.Add(phonologyButton);
        navigationPanel.Controls.Add(grammarButton);
        navigationPanel.Controls.Add(lexiconButton);

    }
    private Button CreateNavigationButton(string text)
    {
        //加入按钮
        Button button = new();
        
        //按钮的文本和大小
        button.Text = text;
        button.Size = new Size(100, 45);
        button.Margin = new Padding(0, 0, 0, 10);
        button.TextAlign = ContentAlignment.MiddleLeft;

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

        // 左侧导航栏宽度
        mainSplitContainer.SplitterDistance = 260;
        mainSplitContainer.SplitterWidth = 4;
        mainSplitContainer.Panel1MinSize = 260;

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

        // 把控件组合起来

        // 导航栏放进左侧区域
        mainSplitContainer.Panel1.Controls.Add(navigationPanel);

        // 主区域加入窗口
        Controls.Add(mainSplitContainer);

        // 菜单栏加入窗口
        Controls.Add(mainMenu);

        // 指定主菜单
        MainMenuStrip = mainMenu;
    }
}