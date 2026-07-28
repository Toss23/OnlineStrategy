using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerSideText;
    private Board _board;

    public void UpdateUI(Player player)
    {
        Debug.Log("Updating UI");
        
        _playerSideText.text = $"Сторона: {Sides.TryGetRuName(player.Side)}\n\n";

        List<string> resTexts = new List<string>();
        int count = Enum.GetNames(typeof(Resource)).Length;
        for (int resourceIndex = 1; resourceIndex < count; resourceIndex++)
        {
            Resource resource = (Resource)resourceIndex;
            float value = 0;
            var playerResources = player.PlayerResources;
            
            if (playerResources != null && playerResources.TryGetValue(resource, out float val))
                value = val;

            Debug.Log("1");
            
            string text = Resources.Icon.TryGetValue(resource, out int spriteIndex)
                ? $"<sprite={spriteIndex}>: {value}"
                : $"{Resources.TryGetRuName(resource)}: {value}";

            if (Resources.HaveMaximum.TryGetValue(resource, out int max))
                text += $" / {max}";

            var income = player.GetResourceIncome();
            if (income != null && income.TryGetValue(resource, out float incomeValue))
            {
                Debug.Log("2");
                
                if (incomeValue != 0)
                {
                    string sign = incomeValue > 0 ? "+" : "";
                    string color = incomeValue > 0 ? "#00ff00" : "#ff0000";
                    text += $" <color={color}>[{sign}{incomeValue:F1}]</color>";
                }
            }

            resTexts.Add(text);
        }

        _playerSideText.text += resTexts.Count > 0 ? string.Join("\n", resTexts) : "";
    }
}