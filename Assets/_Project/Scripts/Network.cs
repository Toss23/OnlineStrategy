using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class Network : NetworkManager
{
    [Header("Server References")]
    [SerializeField] private Board _board;
    [SerializeField] private TurnManager _turnManager;
    [SerializeField] private NetworkManagerHUD _networkManagerHUD;

    [Header("Client References")]
    [SerializeField] private CameraController _cameraController;

    // Серверные поля
    private HashSet<Side> _usedSides = new HashSet<Side>();
    private bool _mapGenerated = false;

    // Словарь для хранения игроков по сторонам
    private Dictionary<Side, Player> _players = new Dictionary<Side, Player>();

    public void RegisterPlayer(Side side, Player player) => _players[side] = player;
    public void UnregisterPlayer(Side side) => _players.Remove(side);
    public Player GetPlayerBySide(Side side) => _players.TryGetValue(side, out var p) ? p : null;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        Player player = conn.identity.GetComponent<Player>();
        if (player == null) return;

        // Назначаем сторону
        Side assignedSide = Side.None;
        if (!_usedSides.Contains(Side.Red))
            assignedSide = Side.Red;
        else if (!_usedSides.Contains(Side.Purple))
            assignedSide = Side.Purple;
        else
        {
            Debug.LogWarning("Обе стороны заняты, подключение отклонено");
            conn.Disconnect();
            return;
        }

        player.SetSide(assignedSide);
        _usedSides.Add(assignedSide);
        RegisterPlayer(assignedSide, player);

        // Если теперь два игрока и карта ещё не сгенерирована – генерируем и стартуем игру
        if (numPlayers == 2 && !_mapGenerated)
        {
            _board.GenerateMap();
            _turnManager.StartGame();
            _mapGenerated = true;
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Player player = conn.identity?.GetComponent<Player>();
        if (player != null)
        {
            _usedSides.Remove(player.Side);
            UnregisterPlayer(player.Side);
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        _cameraController.enabled = true;
        _networkManagerHUD.enabled = false;
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        _cameraController.enabled = false;
        _networkManagerHUD.enabled = true;
    }
}