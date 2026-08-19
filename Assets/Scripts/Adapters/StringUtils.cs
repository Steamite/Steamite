using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public static class StringUtils
{
    public static string LowerCamelCase(this string text)
    {
        string firstLetter = text[0].ToString().ToLower();
        text = firstLetter + text[1..];
        return text;
    }
}