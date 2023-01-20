using UnityEngine;


public class bl_ColorHelper
{
    public static Color CoopColor = new Color(0.145f, 0.43857f, 0.9325f, 0.91f);
    public static Color32 CoopColor32 = new Color32(44,134,238,240);
    public static string HexColor = "#1D99F2";

    public static Color MFPColorWihtAlpha(float alpha)
    {
        Color color = new Color(CoopColor.r, CoopColor.g, CoopColor.b, alpha);
        return color;
    }

    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }
}