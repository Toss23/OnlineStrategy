using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public event Action RequireUpdateUI;

    [SyncVar(hook = nameof(OnSideChanged))]
    private Side _side = Side.None;

    private readonly SyncDictionary<Resource, float> _resources = new();
    private readonly SyncList<Tile> _capturedTiles = new();

    private Dictionary<Tile, Action> _tileUpgradeDelegates = new Dictionary<Tile, Action>();
    
    private PlayerInput _playerInput;
    private TurnManager _turnManager;
    private Board _board;
    
    private TileInfoUI _tileInfoUI;
    private TileResourceUI _tileResourceUI;
    private TileBuildingUI _tileBuildingUI;

    public Side Side => _side;
    public SyncDictionary<Resource, float> PlayerResources => _resources;

    [Server]
    public void SetSide(Side side)
    {
        if (!isServer) return;
        _side = side;
    }

    private void OnSideChanged(Side oldValue, Side newValue)
    {
        if (isLocalPlayer)
        {
            var camController = FindAnyObjectByType<CameraController>();
            Vector2 target = _side == Side.Purple ? Configs.BOARD_SIZE : Vector2.zero;
            camController.SetPosition(target, true);
        }
        
        RequireUpdateUI?.Invoke();
    }

    private void OnResourcesChanged(SyncDictionary<Resource, float>.Operation op, Resource key, float value)
    {
        if (Resources.HaveMaximum.TryGetValue(key, out int max) && value > max)
            _resources[key] = max;

        RequireUpdateUI?.Invoke();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        _playerInput = FindAnyObjectByType<PlayerInput>();
        _turnManager = FindAnyObjectByType<TurnManager>();
        _board = FindAnyObjectByType<Board>();
        
        _tileInfoUI = FindAnyObjectByType<TileInfoUI>();
        _tileResourceUI = FindAnyObjectByType<TileResourceUI>();
        _tileBuildingUI = FindAnyObjectByType<TileBuildingUI>();

        var playerUI = FindAnyObjectByType<PlayerUI>();
        if (playerUI != null)
            playerUI.Initialize(this);

        _tileInfoUI?.Init();
        _tileResourceUI?.Init();
        _tileBuildingUI?.Init();

        if (_playerInput != null)
        {
            _playerInput.OnTileLeftClick += OnLeftClickTile;
            _playerInput.OnTileRightClick += OnRightClickTile;
        }

        if (_tileResourceUI != null)
        {
            _tileResourceUI.OnClickUpgrade += TryUpgradeResourceTile;
        }

        if (_tileBuildingUI != null)
        {
            _tileBuildingUI.OnClickBuilding += TryBuildBuilding;
        }

        _resources.OnChange += OnResourcesChanged;
        _capturedTiles.OnChange += OnCapturedTilesChanged;

        CmdInitResource();
    }

    private void OnDisable()
    {
        if (_playerInput != null)
        {
            _playerInput.OnTileLeftClick -= OnLeftClickTile;
            _playerInput.OnTileRightClick -= OnRightClickTile;
        }
        
        if (_tileResourceUI != null)
        {
            _tileResourceUI.OnClickUpgrade -= TryUpgradeResourceTile;
        }
        
        if (_tileBuildingUI != null)
        {
            _tileBuildingUI.OnClickBuilding -= TryBuildBuilding;
        }

        _resources.OnChange -= OnResourcesChanged;
        _capturedTiles.OnChange -= OnCapturedTilesChanged;
    }

    private void OnCapturedTilesChanged(SyncList<Tile>.Operation op, int index, Tile tile)
    {
        switch (op)
        {
            case SyncList<Tile>.Operation.OP_ADD:
                _tileUpgradeDelegates[tile] = () => RequireUpdateUI?.Invoke();
                tile.OnStateChanged += _tileUpgradeDelegates[tile];
                break;
            case SyncList<Tile>.Operation.OP_REMOVEAT:
                if (_tileUpgradeDelegates[tile] != null)
                {
                    tile.OnStateChanged -= _tileUpgradeDelegates[tile];
                }
                break;
        }
        
        RequireUpdateUI?.Invoke();
    }

    [Command]
    private void CmdInitResource()
    {
        foreach (string resourceStr in Enum.GetNames(typeof(Resource)))
        {
            Resource resource = Enum.Parse<Resource>(resourceStr);
            if (resource == Resource.None) continue;

            float value = Configs.RESOURCES_AT_START.GetValueOrDefault(resource, 0);
            _resources.Add(resource, value);
        }
    }

    private void OnLeftClickTile(Tile tile)
    {
        if (_turnManager.CurrentTurn == _side && tile.Side == Side.None)
        {
            CmdCaptureTile(_side, tile);
            return;
        }

        if (tile.Side == _side && tile.Resource != Resource.None)
        {
            _tileResourceUI?.Open(tile, this);
            return;
        }

        if (tile.Side == _side && tile.Building == Building.None)
        {
            _tileBuildingUI?.Open(tile, this);
            return;
        }
        
        if (tile.Side == _side && tile.Building == Building.Factory)
        {
            
            return;
        }
    }

    private void OnRightClickTile(Tile tile)
    {
        _tileInfoUI?.Open(tile);
    }

    private void TryUpgradeResourceTile(Tile tile)
    {
        if (_turnManager == null || _turnManager.CurrentTurn != _side) return;
        if (tile.Side != _side) return;
        
        if (HaveResources(Resources.Data[tile.Resource], tile.ResourceLevel))
        {
            _tileResourceUI?.Close();
            CmdUpgradeTile(_side, tile);
        }
    }
    
    private void TryBuildBuilding(Tile tile, Building building)
    {
        if (_turnManager == null || _turnManager.CurrentTurn != _side) return;
        if (tile == null) return;
        if (tile.Side != _side) return;
        if (tile.Building != Building.None) return;
        
        if (Buildings.Data.TryGetValue(building, out BuildingData data))
        {
            if (HaveResources(data.Cost))
            {
                _tileBuildingUI?.Close();
                CmdBuildBuilding(tile, building);
            }
        }
    }

    [Command]
    private void CmdBuildBuilding(Tile tile, Building building)
    {
        if (!isServer) return;
        _turnManager ??= FindAnyObjectByType<TurnManager>();
        
        if (_turnManager == null || _turnManager.CurrentTurn != _side) return;
        if (tile == null) return;
        if (tile.Side != _side) return;
        if (tile.Building != Building.None) return;
        
        if (Buildings.Data.TryGetValue(building, out BuildingData data))
        {
            if (HaveResources(data.Cost) && tile.Building == Building.None && tile.Resource == Resource.None)
            {
                SubstractResources(data.Cost);
                tile.SetBuilding(building);
                _turnManager.NextTurn();
            }
        }
    }

    public bool HaveResources(ResourceData data, int level)
    {
        float[] priceArray = data.PriceCount[level];
        for (int i = 0; i < data.PriceResource.Count; i++)
        {
            Resource res = data.PriceResource[i];
            float required = priceArray[i];
            if (!_resources.TryGetValue(res, out float current) || current < required)
                return false;
        }
        return true;
    }
    
    public bool HaveResources(Dictionary<Resource, float> resources)
    {
        foreach (var kvp in resources)
        {
            if (!_resources.TryGetValue(kvp.Key, out float current) || current < kvp.Value)
                return false;
        }
        return true;
    }
    
    [Server]
    public void SubstractResources(Dictionary<Resource, float> resources)
    {
        if (!isServer) return;
        foreach (var kvp in resources)
        {
            if (_resources.TryGetValue(kvp.Key, out float current))
                _resources[kvp.Key] = current - kvp.Value;
        }
    }


    [Server]
    private void SubstractResources(ResourceData data, int level)
    {
        float[] priceArray = data.PriceCount[level];
        for (int i = 0; i < data.PriceResource.Count; i++)
        {
            Resource res = data.PriceResource[i];
            _resources[res] -= priceArray[i];
        }
    }

    [Command]
    private void CmdCaptureTile(Side side, Tile tile)
    {
        if (!isServer) return;

        _turnManager ??= FindAnyObjectByType<TurnManager>();
        _board ??= FindAnyObjectByType<Board>();

        if (_turnManager == null || _board == null) return;
        if (_turnManager.CurrentTurn != side) return;
        if (tile == null || tile.Side != Side.None) return;
        if (!tile.HasAllyNeighbor(side, _board)) return;

        tile.SetSide(side);
        _capturedTiles.Add(tile);
        _turnManager.NextTurn();
    }

    [Command]
    private void CmdUpgradeTile(Side side, Tile tile)
    {
        if (!isServer) return;

        _turnManager ??= FindAnyObjectByType<TurnManager>();
        if (_turnManager == null) return;
        if (_turnManager.CurrentTurn != side) return;
        if (tile == null || tile.Side != side) return;

        int currentLevel = tile.ResourceLevel;
        var data = Resources.Data[tile.Resource];
        if (currentLevel == data.MaxLevel()) return;

        if (HaveResources(data, currentLevel))
        {
            SubstractResources(data, currentLevel);
            tile.SetResourceLevel(currentLevel + 1);
            _turnManager.NextTurn();
        }
    }

    public Dictionary<Resource, float> GetResourceIncome()
    {
        var income = new Dictionary<Resource, float>();
        foreach (string resourceStr in Enum.GetNames(typeof(Resource)))
        {
            Resource resource = Enum.Parse<Resource>(resourceStr);
            if (resource != Resource.None)
                income.Add(resource, Configs.RESOURCES_PASSIVE_INCOME.GetValueOrDefault(resource, 0));
        }

        foreach (Tile tile in _capturedTiles)
        {
            if (tile.Resource != Resource.None && tile.ResourceLevel > 0)
                income[tile.Resource] += Resources.Data[tile.Resource].Production[tile.ResourceLevel - 1];
        }
        return income;
    }
}