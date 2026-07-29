using System;
using Mirror;
using TMPro;
using UnityEngine;

public class Tile : NetworkBehaviour
{
    public event Action OnLevelChanged;
    
    [Header("References")]
    [SerializeField] private TMP_Text _iconText;
    [SerializeField] private TMP_Text _infoText;

    [SyncVar(hook = nameof(OnResourceChanged))]
    private Resource _resource = Resource.None;
    
    [SyncVar(hook = nameof(OnResourceLevelChanged))] 
    private int _resourceLevel = 0;
    
    [SyncVar(hook = nameof(OnBuildingChanged))] 
    private Building _building = Building.None;
    
    [SyncVar(hook = nameof(OnBuildingProductionChanged))] 
    private int _buildingProduction;
    
    [SyncVar(hook = nameof(OnBuildingProgressChanged))] 
    private int _buildingProgress = 0;

    [SyncVar(hook = nameof(OnSideChanged))]
    private Side _side = Side.None;

    [SyncVar] private int _x;
    [SyncVar] private int _y;
    [SyncVar] private int _width = 1;
    [SyncVar] private int _height = 1;

    private Renderer _renderer;
    private Material _materialInstance;
    private bool _isHighlighted;

    public Side Side => _side;
    public Resource Resource => _resource;
    public int ResourceLevel => _resourceLevel;
    public Building Building => _building;
    public int BuildingProduction => _buildingProduction;
    public int BuildingProgress => _buildingProgress;
    public int X => _x;
    public int Y => _y;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _materialInstance = new Material(_renderer.sharedMaterial);
            _renderer.material = _materialInstance;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Board board = FindAnyObjectByType<Board>();
        board.RegisterTile(this, _x, _y, _width, _height);
        UpdateTile();
    }

    [Server]
    public void SetSide(Side side)
    {
        if (!isServer) return;
        _side = side;
        UpdateTile();
    }

    [Server]
    public void SetResourceNode(Resource resource)
    {
        if (!isServer) return;
        _resource = resource;
        UpdateTile();
    }
    
    [Server]
    public void SetResourceLevel(int level)
    {
        if (!isServer) return;
        _resourceLevel = level;
        UpdateTile();
        RpcLevelChanged();
    }
    
    [Server]
    public void SetBuilding(Building building)
    {
        if (!isServer) return;
        if (_resource != Resource.None) return;
        _building = building;
        UpdateTile();
    }
    
    [Server]
    public void SetBuildingProduction(BuildingProduction buildingProduction)
    {
        if (!isServer) return;
        if (_building == Building.None) return;

        if (_buildingProduction != -1)
        {
            if (Buildings.Data.TryGetValue(_building, out BuildingData data))
            {
                _buildingProgress = data.Turns;
            }
            else
            {
                _buildingProgress = 0;
            }
        }
        
        if (buildingProduction != null)
        {
            if (Buildings.Data.ContainsKey(_building))
            {
                if (Buildings.Data[_building].Productions.Contains(buildingProduction))
                {
                    int index = Buildings.Data[_building].Productions.IndexOf(buildingProduction);
                    _buildingProduction = index;
                    _buildingProgress = 0;
                    UpdateTile();
                    return;
                }
            }
        }

        _buildingProduction = -1;
        return;
    }
    
    [Server]
    public void SetBuildingProgress(int progress)
    {
        if (!isServer) return;
        _buildingProgress = progress;
        UpdateTile();
    }

    [ClientRpc]
    private void RpcLevelChanged()
    {
        OnLevelChanged?.Invoke();
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

    public void SetHighlight(bool highlighted)
    {
        if (_isHighlighted == highlighted) return;
        _isHighlighted = highlighted;
        UpdateTile();
    }

    private void OnResourceChanged(Resource oldValue, Resource newValue)
    {
        UpdateTile();
    }
    
    private void OnResourceLevelChanged(int oldValue, int newValue)
    {
        UpdateTile();
    }

    private void OnSideChanged(Side oldValue, Side newValue)
    {
        UpdateTile();
    }

    private void OnBuildingChanged(Building oldValue, Building newValue)
    {
        UpdateTile();
    }

    private void OnBuildingProductionChanged(int oldValue, int newValue)
    {
        UpdateTile();
    }

    private void OnBuildingProgressChanged(int oldValue, int newValue)
    {
        UpdateTile();
    }

    private void UpdateTile()
    {
        if (_resourceLevel == 0)
        {
            _infoText.text = "";
        }
        else
        {
            _infoText.text = $"Ур. {_resourceLevel}";
        }

        if (_resource == Resource.None)
            _iconText.text = "";
        else if (Resources.Icon.TryGetValue(_resource, out int resourceIndex))
            _iconText.text = $"<sprite={resourceIndex}>";
        else
            _iconText.text = _resource.ToString();

        if (_resource == Resource.None)
        {
            if (Buildings.Icon.TryGetValue(_building, out int buildingIndex))
                _iconText.text = $"<sprite={buildingIndex}>";
            else if (_building == Building.None)
                _iconText.text = "";
            else
                _iconText.text = _resource.ToString();
            
            if (_building == Building.None)
            {
                _infoText.text = "";
            }
            else if (_buildingProduction == -1)
            {
                int turns = Buildings.Data[_building].Turns;
                _infoText.text = _buildingProgress != turns ? $"{_buildingProgress} / {turns}" : "";
            }
            else
            {
                BuildingProduction buildingProduction = Buildings.Data[_building].Productions[_buildingProduction];
                _infoText.text = $"{_buildingProgress} / {buildingProduction.Turns}";
            }
        }

        if (_isHighlighted && _side == Side.None)
            _materialInstance.SetColor("_BaseColor", Color.yellow);
        else if (Sides.Colors.TryGetValue(_side, out Color color))
            _materialInstance.SetColor("_BaseColor", color);
    }

    public bool HasAllyNeighbor(Side side, Board board)
    {
        if (side == Side.None || board == null) return false;
        foreach (var neighbor in board.GetNeighbors(this))
            if (neighbor.Side == side)
                return true;
        return false;
    }
}