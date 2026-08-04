using System.Collections.Generic;

public enum Building
{
    None,
    Factory,
    LaunchMissle,
    AirDefence,
    NuclearReactor
}

public static class Buildings
{
    public static readonly Dictionary<Building, int> Icon = new Dictionary<Building, int>()
    {
        [Building.Factory] = 8,
        [Building.LaunchMissle] = 9,
        [Building.AirDefence] = 7,
        [Building.NuclearReactor] = 10,
    };

    public static readonly Dictionary<Building, int> Maximum = new Dictionary<Building, int>()
    {
        [Building.NuclearReactor] = 5,
    };
    
    public static readonly Dictionary<Building, BuildingData> Data = new Dictionary<Building, BuildingData>()
    {
        [Building.Factory] = new BuildingData()
        {
            Name = "Завод",
            Description = 
                $"Производит " +
                $"<sprite={(Resources.Icon.ContainsKey(Resource.Chip) ? Resources.Icon[Resource.Chip] : Resources.TryGetRuName(Resource.Chip))}> и " +
                $"<sprite={(Resources.Icon.ContainsKey(Resource.Rocket) ? Resources.Icon[Resource.Rocket] : Resources.TryGetRuName(Resource.Rocket))}>",
            
            Turns = 3,
            
            Cost = new Dictionary<Resource, float>()
            {
                [Resource.Iron] = 8,
                [Resource.Energy] = 8
            },
            
            Productions = new List<BuildingProduction>()
            {
                new BuildingProduction()
                {
                    Resource = Resource.Chip,
                    Count = 2,
                    Cost =  new Dictionary<Resource, float>()
                    {
                        [Resource.Gold] = 2,
                        [Resource.Oil] = 2,
                    },
                    Turns = 2,
                },
                new BuildingProduction()
                {
                    Resource = Resource.Rocket,
                    Count = 1,
                    Cost =  new Dictionary<Resource, float>()
                    {
                        [Resource.Iron] = 8,
                        [Resource.Oil] = 4,
                        [Resource.Chip] = 2,
                    },
                    Turns = 3,
                },
            }
        },
        
        [Building.LaunchMissle] = new BuildingData()
        {
            Name = "Ракетная шахта",
            Description = 
                $"Запускает ракеты " +
                $"<sprite={(Resources.Icon.ContainsKey(Resource.Rocket) ? Resources.Icon[Resource.Rocket] : Resources.TryGetRuName(Resource.Rocket))}> " +
                $"на расcтоянии ({Configs.ROCKET_RANGE}) клеток",
        
            Cost = new Dictionary<Resource, float>()
            {
                [Resource.Iron] = 12,
                [Resource.Chip] = 3,
                [Resource.Energy] = 10,
            },
        },
        
        [Building.AirDefence] = new BuildingData()
        {
            Name = "ПВО",
            Description = 
                $"Защищает клетки от ракет " +
                $"<sprite={(Resources.Icon.ContainsKey(Resource.Rocket) ? Resources.Icon[Resource.Rocket] : Resources.TryGetRuName(Resource.Rocket))}> " +
                $"на расcтоянии ({Configs.AIR_DEFENCE_RANGE}) клеток",
        
            Cost = new Dictionary<Resource, float>()
            {
                [Resource.Iron] = 15,
                [Resource.Chip] = 2,
                [Resource.Energy] = 12,
            },
        },
        
        [Building.NuclearReactor] = new BuildingData()
        {
            Name = "Ядерный реактор",
            Description = 
                $"Производит 3 " +
                $"<sprite={(Resources.Icon.ContainsKey(Resource.Energy) ? Resources.Icon[Resource.Energy] : Resources.TryGetRuName(Resource.Energy))}>" +
                $" за ход\n" +
                $"Максимум: {Maximum[Building.NuclearReactor]}",
            
            Cost = new Dictionary<Resource, float>()
            {
                [Resource.Iron] = 20,
                [Resource.Uranium] = 1,
                [Resource.Chip] = 6,
                [Resource.Energy] = 18,
            },
            
            Income = new Dictionary<Resource, float>()
            {
                [Resource.Energy] = 3
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