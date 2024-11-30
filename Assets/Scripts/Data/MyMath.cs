using UnityEngine;

public static class MyMath
{
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

    public static char DecToHex(int i)
    {
        if(i > 9)
        {
            return (char)(i + 55);
        }
        else
        {
            return (char)(i + 48);
        }
    }

    public static int HexToEnum(char c)
    {
        return HexToDec("" + c) % 3;
    }
}
