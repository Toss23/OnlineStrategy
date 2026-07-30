using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUIPrefab : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _outlineImage;
    [SerializeField] private TMP_Text _iconText;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _infoText;
    
    public Button Button => _button;
    public Image OutlineImage => _outlineImage;
    
    public void Init(Building building)
    {
        int index = Buildings.Icon.GetValueOrDefault(building, -1);
        _iconText.text = index != -1 ? $"<sprite={index}>" : building.ToString();

        BuildingData data = Buildings.Data[building];
        
        _titleText.text = data.Name;
        _infoText.text = data.Description;

        if (data.Cost.Count > 0)
        {
            _infoText.text += "\n" +
                              "Цена:\n";
            
            List<string> costParts = new List<string>();
            foreach (var res in data.Cost)
            {
                if (Resources.Icon.ContainsKey(res.Key))
                    costParts.Add($"{res.Value} <sprite={Resources.Icon[res.Key]}>");
                else
                    costParts.Add($"{res.Value} {Resources.TryGetRuName(res.Key)}");
            }

            string costStr = costParts.Count > 0 ? $"{string.Join(", ", costParts)}" : "";
            _infoText.text += costStr;
        }
    }
}