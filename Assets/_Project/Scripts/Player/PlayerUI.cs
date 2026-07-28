using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerSideText;
    private Player _player;

    public void Initialize(Player player)
    {
        if (_player != null) Unsubscribe();
        _player = player;
        if (_player != null) Subscribe();
        UpdateUI();
    }

    private void Subscribe()
    {
        if (_player == null) return;
        _player.RequireUpdateUI += UpdateUI;
    }

    private void Unsubscribe()
    {
        if (_player == null) return;
        _player.RequireUpdateUI -= UpdateUI;
    }

    public void UpdateUI()
    {
        if (_player == null) return;

        _playerSideText.text = $"Сторона: {Sides.TryGetRuName(_player.Side)}\n\n";

        List<string> resTexts = new List<string>();
        int count = Enum.GetNames(typeof(Resource)).Length;
        for (int resourceIndex = 1; resourceIndex < count; resourceIndex++)
        {
            Resource resource = (Resource)resourceIndex;
            float value = 0;
            if (_player.PlayerResources.TryGetValue(resource, out float val))
                value = val;

            string text = Resources.Icon.TryGetValue(resource, out int spriteIndex)
                ? $"<sprite={spriteIndex}>: {value}"
                : $"{Resources.TryGetRuName(resource)}: {value}";

            if (Resources.HaveMaximum.TryGetValue(resource, out int max))
                text += $" / {max}";

            var income = _player.GetResourceIncome();
            if (income != null && income.TryGetValue(resource, out float incomeValue) && incomeValue != 0)
            {
                string sign = incomeValue > 0 ? "+" : "";
                string color = incomeValue > 0 ? "#00ff00" : "#ff0000";
                text += $" <color={color}>[{sign}{incomeValue:F1}]</color>";
            }

            resTexts.Add(text);
        }

        _playerSideText.text += string.Join("\n", resTexts);
    }
}