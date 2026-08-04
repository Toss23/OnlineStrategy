using System.Collections.Generic;
using UnityEngine;

public enum Resource
{
    None,
    Iron,
    Gold,
    Oil,
    Uranium,
    Chip,
    Rocket,
    Energy
}

public static class Resources
{
    public static readonly Dictionary<Resource, int> NodeCount = new Dictionary<Resource, int>()
    {
        [Resource.Iron] = 20,
        [Resource.Gold] = 12,
        [Resource.Oil] = 7,
        [Resource.Uranium] = 5,
    };
    
    public static readonly Dictionary<Resource, int> Icon = new Dictionary<Resource, int>()
    {
        [Resource.Iron] = 0,
        [Resource.Gold] = 1,
        [Resource.Energy] = 2,
        [Resource.Oil] = 3,
        [Resource.Uranium] = 4,
        [Resource.Chip] = 5,
        [Resource.Rocket] = 6,
    };
    
    public static readonly Dictionary<Resource, int> HaveMaximum = new Dictionary<Resource, int>()
    {
        [Resource.Energy] = 100
    };

    public static readonly Dictionary<Resource, ResourceData> Data = new Dictionary<Resource, ResourceData>()
    {
        [Resource.Iron] = new ResourceData(){
            PriceResource = { Resource.Iron, Resource.Energy },
            PriceCount = 
            {
                new[] { 0f, 5f },
                new[] { 3f, 8f },
                new[] { 6f, 12f },
                new[] { 12f, 18f },
                new[] { 20f, 25f }
            },
            Production = { 1f, 2f, 3.5f, 5f, 7f }
        },
        
        [Resource.Gold] = new ResourceData(){
            PriceResource = { Resource.Iron, Resource.Gold, Resource.Energy },
            PriceCount = 
            {
                new[] { 3f, 0f, 8f },
                new[] { 5f, 1f, 10f },
                new[] { 10f, 3f, 18f },
                new[] { 18f, 6f, 28f },
                new[] { 28f, 10f, 40f }
            },
            Production = { 0.5f, 1f, 1.8f, 2.8f, 4f }
        },
        
        [Resource.Oil] = new ResourceData(){
            PriceResource = { Resource.Iron, Resource.Chip, Resource.Energy },
            PriceCount = 
            {
                new[] { 5f, 0f, 10f },
                new[] { 8f, 2f, 14f },
                new[] { 14f, 4f, 22f },
                new[] { 25f, 8f, 35f },
                new[] { 38f, 14f, 50f }
            },
            Production = { 0.5f, 1f, 1.8f, 2.8f, 4f }
        },
        
        [Resource.Uranium] = new ResourceData(){
            PriceResource = { Resource.Iron, Resource.Chip, Resource.Energy },
            PriceCount = 
            {
                new[] { 10f, 3f, 18f },
                new[] { 15f, 5f, 25f },
                new[] { 25f, 10f, 40f },
                new[] { 40f, 18f, 60f },
                new[] { 60f, 28f, 85f }
            },
            Production = { 0.1f, 0.25f, 0.5f, 0.8f, 1.2f }
        },
    };

    private static readonly Dictionary<Resource, string> Ru = new Dictionary<Resource, string>()
    {
        [Resource.None] = "Ничего",
        [Resource.Iron] = "Железо",
        [Resource.Gold] = "Золото",
        [Resource.Oil] = "Нефть",
        [Resource.Uranium] = "Уран",
        [Resource.Chip] = "Микросхема",
        [Resource.Rocket] = "Ракета",
        [Resource.Energy] = "Энергия"
    };

    public static string TryGetRuName(Resource resource)
    {
        return Ru.TryGetValue(resource, out string ruName) ? ruName : resource.ToString();
    }
}

public class ResourceData
{
    public List<Resource> PriceResource = new List<Resource>();
    public List<float[]> PriceCount = new List<float[]>();
    public List<float> Production = new List<float>();

    public int MaxLevel()
    {
        return Mathf.Min(PriceCount.Count, Production.Count);
    }
}