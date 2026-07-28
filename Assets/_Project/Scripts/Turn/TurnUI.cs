using TMPro;
using UnityEngine;

public class TurnUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _turnText;
    
    public void UpdateUI(Side currentTurn)
    {
        _turnText.text = $"Ход <{Sides.TryGetRuName(currentTurn)}>";
    }
}