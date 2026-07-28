using System;
using Mirror;
using UnityEngine;

/// <summary>
/// Сервис начисления дохода в начале каждого хода.
/// </summary>
public class IncomeService : NetworkBehaviour
{
    private TurnManager _turnManager;
    private Network _network;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        _turnManager = FindAnyObjectByType<TurnManager>();
        _network = FindAnyObjectByType<Network>();
        
        _turnManager.OnTurnChanged += ApplyIncomeForTurn;
    }

    private void OnDestroy()
    {
        _turnManager.OnTurnChanged -= ApplyIncomeForTurn;
    }

    [Server]
    private void ApplyIncomeForTurn(Side currentTurn)
    {
        if (!isServer) return;

        Player player = _network?.GetPlayerBySide(currentTurn);
        if (player == null) return;

        var income = player.GetResourceIncome();
        foreach (var kvp in income)
        {
            if (Mathf.Approximately(kvp.Value, 0f)) continue;

            float current = player.PlayerResources[kvp.Key];
            float max = Resources.HaveMaximum.TryGetValue(kvp.Key, out int maxVal) ? maxVal : float.MaxValue;
            player.PlayerResources[kvp.Key] = Mathf.Min(current + kvp.Value, max);
        }
    }
}