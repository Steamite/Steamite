using UnityEngine;

/// <summary>Util class for converting hex to dec and back.</summary>
public static class MyMath
{
    /// <summary>
    /// Converts a hex string into a dec string.
    /// </summary>
    /// <param name="s">Hexadecimal number.</param>
    /// <returns>Converted decimal number.</returns>
    public static int HexToDec(string s)
    {
        int length = s.Length;
        int result = 0;
        for (int i = 0; i < length; i++)
        {
            int cInt = s[i];
            if (cInt >= 48 && cInt <= 57)
            {
                result += (cInt - 48) * (int)Mathf.Pow(16, length - i - 1);
            }
            else if (cInt >= 65 && cInt <= 70)
            {
                result += (cInt - 55) * (int)Mathf.Pow(16, length - i - 1);
            }
            else
            {
                // if incorrect input fix it to a middle value
                result += 8 * (int)Mathf.Pow(16, length - i - 1);
            }
        }
        return result;
    }
}
