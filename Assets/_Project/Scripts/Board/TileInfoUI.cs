using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject _content;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _infoText;

    private InputAction _escapeAction;
    private CameraController _cameraController;

    public bool Active => _content.activeSelf;

    private void Awake() => _content.SetActive(false);

    public void Init()
    {
        _cameraController = FindAnyObjectByType<CameraController>();
        Close();

        _escapeAction = new InputAction(binding: "<Keyboard>/escape");
        _escapeAction.performed += OnEscape;
        _escapeAction.Enable();
    }

    private void OnEscape(InputAction.CallbackContext ctx) => Close();

    public void Open(Tile tile)
    {
        if (_content.activeSelf) return;

        _titleText.text = $"Клетка [{tile.X}, {tile.Y}]";
        _infoText.text =
            $"Владелец: {Sides.TryGetRuName(tile.Side)}\n" +
            $"Ресурс: {Resources.TryGetRuName(tile.Resource)}";

        if (Resources.Icon.ContainsKey(tile.Resource))
            _infoText.text += $" <sprite={Resources.Icon[tile.Resource]}>";

        if (tile.Resource != Resource.None)
            _infoText.text += GetResourceLevelsText(tile.Resource);

        _cameraController.enabled = false;
        _content.SetActive(true);
    }

    private string GetResourceLevelsText(Resource resource)
    {
        if (resource == Resource.None || !Resources.Data.ContainsKey(resource))
            return "";

        var data = Resources.Data[resource];
        int maxLevel = data.MaxLevel();
        if (maxLevel == 0) return "";

        var sb = new StringBuilder();
        sb.Append("\n\nУровни ресурса:");

        for (int level = 0; level < maxLevel; level++)
        {
            float production = data.Production[level];
            var priceResources = data.PriceResource;
            var priceCounts = data.PriceCount[level];

            var costParts = new List<string>();
            for (int i = 0; i < priceResources.Count; i++)
            {
                float count = priceCounts[i];
                if (count > 0)
                {
                    if (Resources.Icon.ContainsKey(priceResources[i]))
                        costParts.Add($"{count} <sprite={Resources.Icon[priceResources[i]]}>");
                    else
                        costParts.Add($"{count} {Resources.TryGetRuName(priceResources[i])}");
                }
            }

            string costStr = costParts.Count > 0 ? $"{string.Join(", ", costParts)}" : "";
            sb.Append($"\n[{level + 1} ур.] {production} за ход ({costStr})");
        }

        return sb.ToString();
    }

    public void Close()
    {
        if (!_content.activeSelf) return;
        _cameraController.enabled = true;
        _content.SetActive(false);
    }
}