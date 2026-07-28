using System;
using Mirror;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public event Action<Side> OnTurnUpdated;
    
    [SerializeField] private TurnUI _turnUI;
    
    [SyncVar(hook = nameof(OnTurnChanged))]
    private Side _currentTurn = Side.None;
    
    public Side CurrentTurn => _currentTurn;

    [Server]
    public void StartGame()
    {
        if (!isServer) return;
        _currentTurn = Side.Red;
    }

    [Server]
    public void NextTurn()
    {
        if (isServer == false) return;
        _currentTurn = (_currentTurn == Side.Red) ? Side.Purple : Side.Red;
    }

    private void OnTurnChanged(Side oldValue, Side newValue)
    {
        OnTurnUpdated?.Invoke(newValue);
        _turnUI.UpdateUI(_currentTurn);
    }
}