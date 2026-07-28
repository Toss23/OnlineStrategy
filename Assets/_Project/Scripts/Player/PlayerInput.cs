using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public event Action<Tile> OnTileLeftClick;
    public event Action<Tile> OnTileRightClick;
    public event Action<Tile> OnTileHover;

    private Vector2 _rightClickStartPos;
    private bool _isRightDragging;
    private float _dragThreshold = 5f;
    
    private Camera _camera;
    private Tile _hoveredTile;

    private TileInfoUI _tileInfoUI;
    private TileResourceUI _tileResourceUI;

    private void Awake()
    {
        _camera = Camera.main;
        _tileInfoUI = FindAnyObjectByType<TileInfoUI>();
        _tileResourceUI = FindAnyObjectByType<TileResourceUI>();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;
        if (_tileInfoUI.Active) return;
        if (_tileResourceUI.Active) return;

        Vector2 mousePos = mouse.position.ReadValue();
        Tile currentHover = GetTileUnderMouse(mousePos);

        // --- Обработка наведения (без изменений) ---
        if (currentHover != _hoveredTile)
        {
            _hoveredTile = currentHover;
            OnTileHover?.Invoke(_hoveredTile);
        }

        // --- Обработка правой кнопки (клик / перетаскивание) ---
        if (mouse.rightButton.wasPressedThisFrame)
        {
            _rightClickStartPos = mousePos;
            _isRightDragging = false;
        }

        if (mouse.rightButton.isPressed)
        {
            // Если мышь сдвинулась больше порога – помечаем как перетаскивание
            if (!_isRightDragging && Vector2.Distance(mousePos, _rightClickStartPos) > _dragThreshold)
            {
                _isRightDragging = true;
            }
        }

        if (mouse.rightButton.wasReleasedThisFrame)
        {
            // Если перетаскивания не было – это клик
            if (!_isRightDragging && currentHover != null)
            {
                OnTileRightClick?.Invoke(currentHover);
            }
        }

        // --- Обработка левой кнопки (пока без проверки перетаскивания) ---
        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (currentHover != null)
                OnTileLeftClick?.Invoke(currentHover);
        }
    }

    private Tile GetTileUnderMouse(Vector2 mousePos)
    {
        if (_camera == null) return null;

        Ray ray = _camera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }
}