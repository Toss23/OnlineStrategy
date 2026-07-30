using Mirror;
using UnityEngine;

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
            if (!Buildings.Data.ContainsKey(tile.Building)) continue;
            BuildingData data = Buildings.Data[tile.Building];

            if (tile.Side == currentTurn)
            {
                foreach (var income in data.Income)
                {
                    if (player.PlayerResources.ContainsKey(income.Key))
                        player.PlayerResources[income.Key] += income.Value;
                }
            }

            if (tile.Side == currentTurn)
            {
                tile.AddBuildingProgress(1);

                if (tile.BuildingReady && tile.BuildingProduction != -1)
                {
                    BuildingProduction production = data.Productions[tile.BuildingProduction];
                    if (tile.BuildingProgress >= production.Turns)
                    {
                        player.PlayerResources[production.Resource] += production.Count;
                        tile.ResetBuildingProgress();
                    }
                }
            }
            else
            {
                if (tile.BuildingReady && tile.BuildingProduction != -1)
                {
                    BuildingProduction production = data.Productions[tile.BuildingProduction];
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