using System;
using Mirror;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public event Action<Side> OnTurnChanged;

    [SerializeField] private TurnUI _turnUI;

    [SyncVar(hook = nameof(OnTurnChangedHook))]
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

    private void OnTurnChangedHook(Side oldValue, Side newValue)
    {
        OnTurnChanged?.Invoke(newValue);
        _turnUI.UpdateUI(_currentTurn);
    }
}