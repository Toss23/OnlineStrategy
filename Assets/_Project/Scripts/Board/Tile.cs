using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Tile : NetworkBehaviour
{
    public event Action OnStateChanged;

    [SyncVar(hook = nameof(OnResourceChanged))]
    private Resource _resource = Resource.None;
    
    [SyncVar(hook = nameof(OnResourceLevelChanged))] 
    private int _resourceLevel = 0;
    
    [SyncVar(hook = nameof(OnBuildingChanged))] 
    private Building _building = Building.None;
    
    [SyncVar(hook = nameof(OnBuildingProductionChanged))] 
    private int _buildingProduction = -1; // -1 = нет производства
    
    [SyncVar(hook = nameof(OnBuildingProgressChanged))] 
    private int _buildingProgress = 0;

    [SyncVar(hook = nameof(OnSideChanged))]
    private Side _side = Side.None;

    [SyncVar] private int _x;
    [SyncVar] private int _y;
    [SyncVar] private int _width = 1;
    [SyncVar] private int _height = 1;

    [SyncVar(hook = nameof(OnBuildingReadyChanged))]
    private bool _buildingReady = false;

    // Публичные свойства
    public Side Side => _side;
    public Resource Resource => _resource;
    public int ResourceLevel => _resourceLevel;
    public Building Building => _building;
    public int BuildingProduction => _buildingProduction;
    public int BuildingProgress => _buildingProgress;
    public bool BuildingReady => _buildingReady;
    public int X => _x;
    public int Y => _y;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Board board = FindAnyObjectByType<Board>();
        board.RegisterTile(this, _x, _y, _width, _height);
        OnStateChanged?.Invoke();
    }

    // ---- Серверные методы изменения состояния ----

    [Server]
    public void SetSide(Side side)
    {
        if (!isServer) return;
        _side = side;
        OnStateChanged?.Invoke();
    }

    [Server]
    public void SetResourceNode(Resource resource)
    {
        if (!isServer) return;
        _resource = resource;
        OnStateChanged?.Invoke();
    }
    
    [Server]
    public void SetResourceLevel(int level)
    {
        if (!isServer) return;
        _resourceLevel = level;
        RpcLevelChanged();
    }
    
    [Server]
    public void SetBuilding(Building building)
    {
        if (!isServer) return;
        if (_resource != Resource.None) return;
        if (_building != Building.None) return;
        
        _building = building;
        _buildingProgress = 0;
        _buildingReady = false;
        _buildingProduction = -1;
        
        OnStateChanged?.Invoke();
    }
    
    [Server]
    public void SetBuildingProduction(BuildingProduction buildingProduction)
    {
        if (!isServer) return;
        if (_building == Building.None) return;
        if (!_buildingReady) return;

        _buildingProduction = -1;
        _buildingProgress = 0;
        
        if (buildingProduction != null && Buildings.Data.ContainsKey(_building))
        {
            int index = Buildings.Data[_building].Productions.IndexOf(buildingProduction);
            _buildingProduction = index;
        }
        
        OnStateChanged?.Invoke();
    }

    [Server]
    public void AddBuildingProgress(int amount)
    {
        if (!isServer) return;
        if (_building == Building.None) return;
        
        _buildingProgress += amount;
        if (_buildingReady == false)
        {
            if (_buildingProgress >= Buildings.Data[_building].Turns)
            {
                _buildingReady = true;
                _buildingProgress = 0;
            }
        }
        
        OnStateChanged?.Invoke();
    }

    [Server]
    public void ResetBuildingProgress()
    {
        if (!isServer) return;
        _buildingProgress = 0;
        OnStateChanged?.Invoke();
    }

    [Server]
    public void SetCoords(int x, int y)
    {
        _x = x;
        _y = y;
    }

    [Server]
    public void SetCity(int startX, int startY, int width, int height)
    {
        _x = startX;
        _y = startY;
        _width = width;
        _height = height;
    }

    [ClientRpc]
    private void RpcLevelChanged()
    {
        OnStateChanged?.Invoke();
    }

    public bool HasAllyNeighbor(Side side, Board board)
    {
        if (side == Side.None || board == null) return false;
        foreach (var neighbor in board.GetNeighbors(this))
            if (neighbor.Side == side)
                return true;
        return false;
    }

    // ---- Хуки ----

    private void OnResourceChanged(Resource oldValue, Resource newValue) => OnStateChanged?.Invoke();
    private void OnResourceLevelChanged(int oldValue, int newValue) => OnStateChanged?.Invoke();
    private void OnSideChanged(Side oldValue, Side newValue) => OnStateChanged?.Invoke();
    private void OnBuildingChanged(Building oldValue, Building newValue) => OnStateChanged?.Invoke();
    private void OnBuildingProductionChanged(int oldValue, int newValue) => OnStateChanged?.Invoke();
    private void OnBuildingProgressChanged(int oldValue, int newValue) => OnStateChanged?.Invoke();
    private void OnBuildingReadyChanged(bool oldValue, bool newValue) => OnStateChanged?.Invoke();
}