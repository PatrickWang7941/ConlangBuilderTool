using System;
using System.Collections.Generic;
using System.Text;

namespace CBT.Data;

public record IpaVowel(
    string Symbol,
    string Height,
    string Backness,
    string Roundedness
);
