using System.Collections.Generic;
using UnityEngine;

public enum Side
{
    None, Red, Purple
}

public static class Sides
{
    public static readonly Dictionary<Side, Color> Colors = new Dictionary<Side, Color>()
    {
        [Side.None] = new Color32(0x2E, 0x2E, 0x2E, 255),
        [Side.Red] = Color.brown,
        [Side.Purple] = new Color32(0x7E, 0x56, 0xB7, 255),
    };
    
    private static readonly Dictionary<Side, string> Ru = new Dictionary<Side, string>()
    {
        [Side.None] = "Свободно",
        [Side.Red] = "<color=#F74040>Красный</color>",
        [Side.Purple] = "<color=#7E56B7>Фиолетовый</color>"
    };

    public static string TryGetRuName(Side side)
    {
        return Ru.TryGetValue(side, out string ruName) ? ruName : side.ToString();
    }
}