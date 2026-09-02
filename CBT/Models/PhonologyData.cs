using System;
using System.Collections.Generic;
using System.Text;

namespace CBT.Models;

public class PhonologyData
{
    public List<ConsonantPhoneme> Consonants { get; set; } = new();

    public List<VowelPhoneme> Vowels { get; set; } = new();
}

public class ConsonantPhoneme
{
    public string Symbol { get; set; } = "";

    public string Place { get; set; } = "";

    public string Manner { get; set; } = "";

    public string Voicing { get; set; } = "";
}

public class VowelPhoneme
{
    public string Symbol { get; set; } = "";

    public string Height { get; set; } = "";

    public string Backness { get; set; } = "";

    public string Roundedness { get; set; } = "";
}