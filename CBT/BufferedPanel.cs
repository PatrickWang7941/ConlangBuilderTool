namespace CBT;

//带WS_EX_COMPOSITED的工作区面板。
//该样式让整棵子树（包括TextBox、ComboBox、ListBox等原生控件）先离屏合成再一次性呈现，
//从根源上消除页面切换和控件重排时的闪烁。
public class BufferedPanel : Panel
{
    public BufferedPanel()
    {
        DoubleBuffered = true;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var createParams = base.CreateParams;
            createParams.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
            return createParams;
        }
    }
}
