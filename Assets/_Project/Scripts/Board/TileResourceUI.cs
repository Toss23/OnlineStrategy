using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TileResourceUI : MonoBehaviour
{
    public event Action<Tile> OnClickUpgrade;
    
    [SerializeField] private GameObject _content;
    
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _infoText;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private TMP_Text _upgradeButtonText;

    private InputAction _escapeAction;
    private CameraController _cameraController;
    private Tile _tile;
    
    public bool Active => _content.activeSelf;

    private void Awake()
    {
        _content.SetActive(false);
        _upgradeButton.onClick.AddListener(OnUpgrade);
    }

    public void Init()
    {
        _cameraController = FindAnyObjectByType<CameraController>();
        
        Close();
        
        _escapeAction = new InputAction(binding: "<Keyboard>/escape");
        _escapeAction.performed += OnEscape;
        _escapeAction.Enable();
    }
    
    private void OnEscape(InputAction.CallbackContext ctx)
    {
        Close();
    }

    private void OnUpgrade()
    {
        if (_tile != null)
        {
            OnClickUpgrade?.Invoke(_tile);
        }
    }

    public void Open(Tile tile, Player player)
    {
        if (_content.activeSelf == true) return;
        
        _tile = tile;
        _titleText.text = $"Клетка [{tile.X}, {tile.Y}]";
        _infoText.text =
            $"Владелец: {Sides.TryGetRuName(tile.Side)}\n" +
            $"Ресурс: {Resources.TryGetRuName(tile.Resource)}";

        if (Resources.Icon.ContainsKey(tile.Resource))
        {
            _infoText.text += $" <sprite={Resources.Icon[tile.Resource]}>";
        }

        if (tile.ResourceLevel > 0)
        {
            _infoText.text += $"\nДобыча: {Resources.Data[tile.Resource].Production[tile.ResourceLevel - 1]} за ход";
        }
        else
        {
            _infoText.text += $"\nДобыча: 0 за ход";
        }

        if (tile.ResourceLevel != Resources.Data[tile.Resource].MaxLevel())
        {
            _infoText.text += $"\nСледующий уровень: {Resources.Data[tile.Resource].Production[tile.ResourceLevel + 1]} за ход";
            
            _upgradeButton.interactable = player.HaveResources(Resources.Data[tile.Resource], tile.ResourceLevel);
            
            ResourceData data = Resources.Data[tile.Resource];
            List<string> costParts = new List<string>();
            for (int resIndex = 0; resIndex < data.PriceResource.Count; resIndex++)
            {
                float count = data.PriceCount[tile.ResourceLevel][resIndex];
                if (count > 0)
                {
                    if (Resources.Icon.ContainsKey(data.PriceResource[resIndex]))
                    {
                        costParts.Add($"{count} <sprite={Resources.Icon[data.PriceResource[resIndex]]}>");
                    }
                    else
                    {
                        string resName = Resources.TryGetRuName(data.PriceResource[resIndex]);
                        costParts.Add($"{count} {resName}");
                    }
                }
            }

            string costStr = costParts.Count > 0 ? $"({string.Join(", ", costParts)})" : "";
            _upgradeButtonText.text = $"Улучшить {costStr}";
        }
        else
        {
            _upgradeButton.interactable = false;
            _upgradeButtonText.text = "Максимальный уровень";
        }

        _cameraController.enabled = false;
        _content.SetActive(true);
    }

    public void Close()
    {
        if (_content.activeSelf == false) return;
        
        _tile = null;
        _cameraController.enabled = true;
        _content.SetActive(false);
    }
}