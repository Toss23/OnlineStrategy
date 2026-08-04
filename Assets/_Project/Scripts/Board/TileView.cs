using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _iconText;
    [SerializeField] private TMP_Text _infoText;

    private Tile _tile;
    private Renderer _renderer;
    private Material _materialInstance;
    private bool _isHighlighted;

    private void Awake()
    {
        _tile = GetComponent<Tile>();
        if (_tile == null)
        {
            Debug.LogError("TileView requires a Tile component.");
            return;
        }

        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _materialInstance = new Material(_renderer.sharedMaterial);
            _renderer.material = _materialInstance;
        }

        _tile.OnStateChanged += UpdateUI;
    }

    private void OnDestroy()
    {
        if (_tile != null)
            _tile.OnStateChanged -= UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_tile == null) return;

        // --- Информация о ресурсе ---
        if (_tile.ResourceLevel == 0)
            _infoText.text = "";
        else
            _infoText.text = $"Ур. {_tile.ResourceLevel}";

        // --- Иконка ---
        if (_tile.Resource != Resource.None)
        {
            if (Resources.Icon.TryGetValue(_tile.Resource, out int resIcon))
                _iconText.text = $"<sprite={resIcon}>";
            else
                _iconText.text = _tile.Resource.ToString();
        }
        else if (_tile.Building != Building.None && Buildings.Icon.TryGetValue(_tile.Building, out int buildIcon))
        {
            _iconText.text = $"<sprite={buildIcon}>";
        }
        else
        {
            _iconText.text = "";
        }

        // --- Прогресс строительства / производства ---
        if (_tile.Building != Building.None)
        {
            int threshold = 0;
            
            if (_tile.BuildingProduction == -1)
            {
                if (_tile.BuildingReady)
                    threshold = -1;
                else if (Buildings.Data.TryGetValue(_tile.Building, out var data))
                    threshold = data.Turns;
            }
            else
            {
                if (Buildings.Data.TryGetValue(_tile.Building, out var data) &&
                    _tile.BuildingProduction < data.Productions.Count)
                {
                    threshold = data.Productions[_tile.BuildingProduction].Turns;
                }
            }
            
            if (threshold > 0)
                _infoText.text = $"{_tile.BuildingProgress} / {threshold}";
            else
                _infoText.text = "";
        }

        // --- Цвет ---
        if (_isHighlighted && _tile.Side == Side.None)
            _materialInstance.SetColor("_BaseColor", Color.yellow);
        else if (Sides.Colors.TryGetValue(_tile.Side, out Color color))
            _materialInstance.SetColor("_BaseColor", color);
    }

    public void SetHighlight(bool highlighted)
    {
        if (_isHighlighted == highlighted) return;
        _isHighlighted = highlighted;
        UpdateUI();
    }
}