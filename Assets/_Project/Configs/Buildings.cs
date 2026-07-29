using System.Collections.Generic;

public enum Building
{
    None,
    Factory,
    NuclearReactor
}

public static class Buildings
{
    public static readonly Dictionary<Building, int> Icon = new Dictionary<Building, int>()
    {
        [Building.Factory] = 8,
        [Building.NuclearReactor] = 10,
    };
    
    public static readonly Dictionary<Building, BuildingData> Data = new Dictionary<Building, BuildingData>()
    {
        [Building.Factory] = new BuildingData()
        {
            Name = "Завод",
            Description = 
                $"Производит " +
                $"<sprite={(Resources.Icon.ContainsKey(Resource.Chip) ? Resources.Icon[Resource.Chip] : Resources.TryGetRuName(Resource.Chip))} и " +
                $"<sprite={(Resources.Icon.ContainsKey(Resource.Rocket) ? Resources.Icon[Resource.Rocket] : Resources.TryGetRuName(Resource.Rocket))}",
            
            Cost = new Dictionary<Resource, float>()
            {
                [Resource.Iron] = 10,
                [Resource.Energy] = 10
            },
            
            Productions = new List<BuildingProduction>()
            {
                new BuildingProduction()
                {
                    Resource = Resource.Chip,
                    Count = 1,
                    Cost =  new Dictionary<Resource, float>()
                    {
                        [Resource.Iron] = 4,
                        [Resource.Gold] = 4
                    },
                    Turns = 2,
                }
            }
        }
    };
}

public class BuildingData
{
    public string Name = "Building";
    public string Description = "";
    public int Turns = 1;
    public Dictionary<Resource, float> Cost = new();
    public Dictionary<Resource, float> Income = new();
    public List<BuildingProduction> Productions = new();
}

public class BuildingProduction
{
    public Resource Resource;
    public int Count = 1;
    public Dictionary<Resource, float> Cost = new Dictionary<Resource, float>();
    public int Turns = 1;
}