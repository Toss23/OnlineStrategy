using System.Collections.Generic;
using UnityEngine;

public static class Configs
{
    public static readonly Vector2Int BOARD_SIZE = new Vector2Int(20, 20);
    public static readonly int CITY_SIZE = 2;
    
    public static readonly Dictionary<Resource, float> RESOURCES_AT_START = new Dictionary<Resource, float>()
    {
        [Resource.Energy] = 20,
        [Resource.Iron] = 100
    };
    
    public static readonly Dictionary<Resource, float> RESOURCES_PASSIVE_INCOME = new Dictionary<Resource, float>()
    {
        [Resource.Energy] = 3,
    };
}