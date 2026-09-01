using System;
using System.Collections.Generic;
using System.Text;

//overview页面
//这个页面负责自己的内容
//多分页，不要把程序变成石山代码！
namespace CBT.Pages;

public class OverviewPage : UserControl
{
    public OverviewPage()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(30);

        Label title = new();
        //标题字体
        title.Text = "概览  Overview";
        title.AutoSize = true;
        title.Font = new Font("Microsoft YaHei UI", 18);

        Controls.Add(title);
    }
}