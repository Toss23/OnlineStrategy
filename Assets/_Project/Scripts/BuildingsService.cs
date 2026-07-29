using System.Collections.Generic;
using Mirror;

public class BuildingsService : NetworkBehaviour
{
    private TurnManager _turnManager;
    private Board _board;
    private Network _network;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        _turnManager = FindAnyObjectByType<TurnManager>();
        _board = FindAnyObjectByType<Board>();
        _network = FindAnyObjectByType<Network>();
        
        _turnManager.OnTurnChanged += ProcessBuildings;
    }
    
    private void OnDestroy()
    {
        _turnManager.OnTurnChanged -= ProcessBuildings;
    }

    [Server]
    private void ProcessBuildings(Side currentTurn)
    {
        if (!isServer) return;
        
        Player player = _network.GetPlayerBySide(currentTurn);
        Player enemyPlayer = _network.GetPlayerBySide(currentTurn == Side.Red ? Side.Purple : Side.Red);

        foreach (Tile tile in _board.GetAllTiles())
        {
            if (tile.Building == Building.None) continue;
            if (Buildings.Data.ContainsKey(tile.Building)) continue;
            
            if (tile.Side == currentTurn)
            {
                if (tile.BuildingProduction == -1)
                {
                    if (tile.BuildingProgress < Buildings.Data[tile.Building].Turns)
                    {
                        tile.SetBuildingProgress(tile.BuildingProgress + 1);
                    }
                }
                else if (Buildings.Data[tile.Building].Productions.Count > tile.BuildingProduction)
                {
                    BuildingProduction production = Buildings.Data[tile.Building].Productions[tile.BuildingProduction];
                    tile.SetBuildingProgress(tile.BuildingProgress + 1);
                    if (tile.BuildingProgress >= production.Turns)
                    {
                        tile.SetBuildingProgress(0);
                        player.PlayerResources[production.Resource] += production.Count;
                    }
                }
            }
            else
            {
                if (tile.BuildingProduction != -1 && Buildings.Data[tile.Building].Productions.Count > tile.BuildingProduction)
                {
                    if (tile.BuildingProgress == 0)
                    {
                        BuildingProduction production = Buildings.Data[tile.Building].Productions[tile.BuildingProduction];
                        if (enemyPlayer.HaveResources(production.Cost))
                        {
                            enemyPlayer.SubstractResources(production.Cost);
                        }
                        else
                        {
                            tile.SetBuildingProduction(null);
                        }
                    }
                }
            }
        }
    }
}