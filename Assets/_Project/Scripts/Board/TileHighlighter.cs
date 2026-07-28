using UnityEngine;
using Mirror;

public class TileHighlighter : MonoBehaviour
{
    private PlayerInput _playerInput;
    private Board _board;
    private Side _playerSide = Side.None;
    private Tile _highlightedTile;

    private void Awake()
    {
        _playerInput = FindAnyObjectByType<PlayerInput>();
        _board = FindAnyObjectByType<Board>();
    }

    private void OnEnable()
    {
        if (_playerInput != null)
            _playerInput.OnTileHover += OnHoverTile;
    }

    private void OnDisable()
    {
        if (_playerInput != null)
            _playerInput.OnTileHover -= OnHoverTile;
    }

    private void Update()
    {
        // Если ссылки потеряны – переинициализируем
        if (_playerInput == null)
        {
            _playerInput = FindAnyObjectByType<PlayerInput>();
            if (_playerInput != null)
                _playerInput.OnTileHover += OnHoverTile;
        }

        if (_board == null)
            _board = FindAnyObjectByType<Board>();

        if (_playerSide == Side.None)
            UpdatePlayerSide();
    }

    private void UpdatePlayerSide()
    {
        if (NetworkClient.localPlayer != null)
        {
            Player player = NetworkClient.localPlayer.GetComponent<Player>();
            if (player != null)
                _playerSide = player.Side;
        }
    }

    private void OnHoverTile(Tile tile)
    {
        if (_highlightedTile != null && _highlightedTile != tile)
        {
            _highlightedTile.SetHighlight(false);
            _highlightedTile = null;
        }

        if (tile != null && ShouldHighlight(tile))
        {
            tile.SetHighlight(true);
            _highlightedTile = tile;
        }
    }

    private bool ShouldHighlight(Tile tile)
    {
        if (tile.Side != Side.None || _playerSide == Side.None || _board == null)
            return false;

        return tile.HasAllyNeighbor(_playerSide, _board);
    }
}