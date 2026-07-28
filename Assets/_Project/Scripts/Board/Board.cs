using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Vector2Int _offset;
    [SerializeField] private float _spacing;

    private Dictionary<Vector2Int, Tile> _tiles = new();
    private bool _isGenerated = false;

    [Server]
    public void GenerateMap()
    {
        if (_isGenerated) return;
        _isGenerated = true;

        if (Configs.BOARD_SIZE.x < Configs.CITY_SIZE * 2 || Configs.BOARD_SIZE.y < Configs.CITY_SIZE * 2)
        {
            Debug.LogError("Board слишком мала для двух городов");
            return;
        }

        // ---- Левый город (Red) ----
        Tile leftCity = CreateCity(0, 0, Side.Red);
        // ---- Правый город (Green) ----
        int baseX = Configs.BOARD_SIZE.x - Configs.CITY_SIZE;
        int baseY = Configs.BOARD_SIZE.y - Configs.CITY_SIZE;
        Tile rightCity = CreateCity(baseX, baseY, Side.Purple);

        // ---- Остальные тайлы ----
        List<Tile> neutralTiles = new();
        for (int x = 0; x < Configs.BOARD_SIZE.x; x++)
        {
            for (int y = 0; y < Configs.BOARD_SIZE.y; y++)
            {
                if (IsInsideCity(x, y, 0, 0, Configs.CITY_SIZE) ||
                    IsInsideCity(x, y, baseX, baseY, Configs.CITY_SIZE))
                    continue;

                Tile tile = Instantiate(_tilePrefab, transform);
                tile.name = $"Tile [{x};{y}]";
                tile.SetCoords(x, y);
                tile.transform.localPosition = new Vector2(x, y) * (1 + _spacing) + (Vector2)_offset;
                NetworkServer.Spawn(tile.gameObject);
                neutralTiles.Add(tile);
            }
        }

        // ---- Раздача ресурсов ----
        foreach (var resource in new[] { Resource.Iron, Resource.Gold, Resource.Oil, Resource.Uranium })
        {
            int count = Resources.NodeCount[resource];
            for (int i = 0; i < count && neutralTiles.Count > 0; i++)
            {
                int idx = Random.Range(0, neutralTiles.Count);
                neutralTiles[idx].SetResourceNode(resource);
                neutralTiles.RemoveAt(idx);
            }
        }
    }

    private Tile CreateCity(int startX, int startY, Side side)
    {
        Tile city = Instantiate(_tilePrefab, transform);
        city.SetCity(startX, startY, Configs.CITY_SIZE, Configs.CITY_SIZE);
        city.name = $"City {side}";

        // Позиционирование и масштаб
        float centerX = (startX + (Configs.CITY_SIZE - 1) * 0.5f) * (1 + _spacing) + _offset.x;
        float centerY = (startY + (Configs.CITY_SIZE - 1) * 0.5f) * (1 + _spacing) + _offset.y;
        city.transform.localPosition = new Vector2(centerX, centerY);
        float scaleFactor = Configs.CITY_SIZE + (Configs.CITY_SIZE - 1) * _spacing;
        city.transform.localScale = new Vector3(scaleFactor, scaleFactor, city.transform.localScale.z);

        NetworkServer.Spawn(city.gameObject);
        city.SetSide(side);
        return city;
    }

    private bool IsInsideCity(int x, int y, int cityX, int cityY, int size)
    {
        return x >= cityX && x < cityX + size && y >= cityY && y < cityY + size;
    }

    public void RegisterTile(Tile tile, int x, int y, int width = 1, int height = 1)
    {
        if (tile == null) return;
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                int cx = x + dx;
                int cy = y + dy;
                Vector2Int pos = new Vector2Int(cx, cy);
                if (!_tiles.ContainsKey(pos))
                {
                    _tiles[pos] = tile;
                    tile.transform.SetParent(transform);
                    tile.name = $"Tile [{pos}]";
                }
            }
        }
    }

    public Tile GetTile(int x, int y)
    {
        _tiles.TryGetValue(new Vector2Int(x, y), out Tile tile);
        return tile;
    }

    public List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new();
        int x = tile.X, y = tile.Y;
        TryAddNeighbor(x, y + 1, neighbors);
        TryAddNeighbor(x, y - 1, neighbors);
        TryAddNeighbor(x + 1, y, neighbors);
        TryAddNeighbor(x - 1, y, neighbors);
        return neighbors;
    }

    private void TryAddNeighbor(int x, int y, List<Tile> list)
    {
        if (x >= 0 && x < Configs.BOARD_SIZE.x && y >= 0 && y < Configs.BOARD_SIZE.y)
        {
            if (_tiles.TryGetValue(new Vector2Int(x, y), out Tile tile))
                list.Add(tile);
        }
    }
    
    public IEnumerable<Tile> GetAllTiles()
    {
        return _tiles.Values.Distinct();
    }
}