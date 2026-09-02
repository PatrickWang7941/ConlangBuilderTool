using System;
using System.Collections.Generic;
using System.Text;

namespace CBT.Models;

public class ConlangProject
{
    // 项目文件格式版本。
    // 以后修改保存格式时，可以根据这个数字进行兼容处理。
    public int FormatVersion { get; set; } = 1;

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public PhonologyData Phonology { get; set; } = new();

    public GrammarData Grammar { get; set; } = new();

    public LexiconData Lexicon { get; set; } = new();
}