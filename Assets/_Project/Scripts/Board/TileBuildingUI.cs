using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileBuildingUI : MonoBehaviour
{
    public event Action<Tile, Building> OnClickBuilding;
    
    [SerializeField] private GameObject _content;
    [SerializeField] private Transform _listContent;
    [SerializeField] private GameObject _buildingUIPrefab;
    
    private InputAction _escapeAction;
    private CameraController _cameraController;
    private Tile _tile;
    
    private Dictionary<Building, BuildingUIPrefab> _buildingsUI = new Dictionary<Building, BuildingUIPrefab>();
    
    public bool Active => _content.activeSelf;
    
    private void Awake()
    {
        _content.SetActive(false);
    }

    public void Init()
    {
        _cameraController = FindAnyObjectByType<CameraController>();
        Close();

        _escapeAction = new InputAction(binding: "<Keyboard>/escape");
        _escapeAction.performed += OnEscape;
        _escapeAction.Enable();

        foreach (var data in Buildings.Data)
        {
            GameObject buildingUIGameObject = Instantiate(_buildingUIPrefab, _listContent);
            buildingUIGameObject.name = data.Key.ToString();
            
            BuildingUIPrefab buildingUIPrefab = buildingUIGameObject.GetComponent<BuildingUIPrefab>();
            buildingUIPrefab.Init(data.Key);
            buildingUIPrefab.Button.onClick.AddListener(() => OnClickBuildingUI(data.Key));
            _buildingsUI.Add(data.Key, buildingUIPrefab);
        }
    }

    private void OnClickBuildingUI(Building building)
    {
        if (_tile == null) return;
        OnClickBuilding?.Invoke(_tile, building);
    }

    private void OnEscape(InputAction.CallbackContext ctx) => Close();
    
    public void Open(Tile tile, Player player)
    {
        if (_content.activeSelf) return;

        _tile = tile;

        foreach (var buildingUI in _buildingsUI)
        {
            if (Buildings.Data.ContainsKey(buildingUI.Key) == false) continue;
            BuildingData data = Buildings.Data[buildingUI.Key];

            buildingUI.Value.Button.interactable = player.HaveResources(data.Cost);
            buildingUI.Value.OutlineImage.color = 
                player.HaveResources(data.Cost) ? 
                new Color32(255, 255, 255, 67) :
                new Color32(255, 69, 69, 67);
        }
        
        _cameraController.enabled = false;
        _content.SetActive(true);
    }
    
    public void Close()
    {
        if (!_content.activeSelf) return;
        
        _tile = null;
        _cameraController.enabled = true;
        _content.SetActive(false);
    }
}
