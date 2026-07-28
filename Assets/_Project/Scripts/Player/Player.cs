using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnSideChanged))]
    private Side _side = Side.None;
    
    private readonly SyncDictionary<Resource, float> _resources = new SyncDictionary<Resource, float>();
    private readonly SyncList<Tile> _capturedTiles = new SyncList<Tile>();

    private PlayerInput _playerInput;
    private PlayerUI _playerUI;
    private TurnManager _turnManager;
    private CameraController _cameraController;
    private Board _board;
    private TileInfoUI _tileInfoUI;
    private TileResourceUI _tileResourceUI;
    
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
        if (!isLocalPlayer) return;
        _playerUI?.UpdateUI(this);
        _cameraController?.SetPosition(_side == Side.Purple ? Configs.BOARD_SIZE : Vector2.zero, true);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        _playerInput = FindAnyObjectByType<PlayerInput>();
        _playerUI = FindAnyObjectByType<PlayerUI>();
        _turnManager = FindAnyObjectByType<TurnManager>();
        _cameraController = FindAnyObjectByType<CameraController>();
        _board = FindAnyObjectByType<Board>();
        _tileInfoUI = FindAnyObjectByType<TileInfoUI>();
        _tileResourceUI = FindAnyObjectByType<TileResourceUI>();

        _cameraController.SetPosition(_side == Side.Purple ? Configs.BOARD_SIZE : Vector2.zero, true);
        _tileInfoUI.Init();
        _tileResourceUI.Init();

        _playerInput.OnTileLeftClick += OnLeftClickTile;
        _playerInput.OnTileRightClick += OnRightClickTile;

        _tileResourceUI.OnClickUpgrade += TryUpgradeResourceTile;

        _resources.OnChange += OnResourcesChanged;
        
        CmdInitResource();
        _playerUI.UpdateUI(this);
    }

    [Command]
    private void CmdInitResource()
    {
        foreach (string resourceStr in Enum.GetNames(typeof(Resource)))
        {
            Resource resource = Enum.Parse<Resource>(resourceStr);
            if (resource == Resource.None) continue;

            float value = 0;
            if (Configs.RESOURCES_AT_START.TryGetValue(resource, out float startValue))
            {
                value = startValue;
            }
            _resources.Add(resource, value);
        }
    }

    private void OnResourcesChanged(SyncDictionary<Resource, float>.Operation op, Resource key, float value)
    {
        if (Resources.HaveMaximum.ContainsKey(key))
        {
            if (value > Resources.HaveMaximum[key])
            {
                _resources[key] = Resources.HaveMaximum[key];
            }
        }
        _playerUI.UpdateUI(this);
    }

    private void OnLeftClickTile(Tile tile)
    {
        if (_turnManager.CurrentTurn == _side && tile.Side == Side.None)
        {
            CmdCaptureTile(_side, tile);
        }
    }

    private void OnRightClickTile(Tile tile)
    {
        if (tile.Side != _side)
        {
            _tileInfoUI.Open(tile);
        }
        else if (tile.Resource != Resource.None && tile.Side == _side)
        {
            _tileResourceUI.Open(tile, this);   
        }
    }

    private void TryUpgradeResourceTile(Tile tile)
    {
        if (_turnManager.CurrentTurn == _side && tile.Side == _side)
        {
            if (HaveResources(Resources.Data[tile.Resource], tile.ResourceLevel))
            {
                _tileResourceUI.Close();
                CmdUpgradeTile(_side, tile);
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

    [Server]
    private void AddResources(Dictionary<Resource, float> resources)
    {
        if (isServer == false) return;

        foreach (var res in resources)
        {
            _resources[res.Key] += res.Value;
        }
    }
    
    [Server]
    private void SubstractResources(ResourceData data, int level)
    {
        if (isServer == false) return;
        
        float[] priceArray = data.PriceCount[level];
        for (int i = 0; i < data.PriceResource.Count; i++)
        {
            Resource res = data.PriceResource[i];
            float required = priceArray[i];
            _resources[res] -= required;
        }
    }

    [Command]
    private void CmdCaptureTile(Side side, Tile tile)
    {
        if (!isServer) return;
        _turnManager ??= FindAnyObjectByType<TurnManager>();
        if (_turnManager.CurrentTurn != side) return;
        _board ??= FindAnyObjectByType<Board>();
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
        if (_turnManager.CurrentTurn != side) return;
        _board ??= FindAnyObjectByType<Board>();
        if (tile == null || tile.Side != side) return;

        if (tile.ResourceLevel != Resources.Data[tile.Resource].MaxLevel())
        {
            if (HaveResources(Resources.Data[tile.Resource], tile.ResourceLevel))
            {
                SubstractResources(Resources.Data[tile.Resource], tile.ResourceLevel);
                tile.SetResourceLevel(tile.ResourceLevel + 1);
                _turnManager.NextTurn();
            }
        }
    }

    [ClientRpc]
    private void RequireUpdateUI()
    {
        _playerUI?.UpdateUI(this);
    }

    public Dictionary<Resource, float> GetResourceIncome()
    {
        Dictionary<Resource, float> income = new Dictionary<Resource, float>();
        foreach (string resourceStr in Enum.GetNames(typeof(Resource)))
        {
            Resource resource = Enum.Parse<Resource>(resourceStr);
            if (resource == Resource.None) continue;
            income.Add(resource, 0);
        }

        foreach (Tile tile in _capturedTiles)
        {
            if (tile.Resource != Resource.None && tile.ResourceLevel > 0)
            {
                income[tile.Resource] += Resources.Data[tile.Resource].Production[tile.ResourceLevel - 1];
            }
        }
        
        return income;
    }
}