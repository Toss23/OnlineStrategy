using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float speed = 10f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 1.5f;

    private Vector3 targetPosition;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        targetPosition = transform.position;
        enabled = false;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // Движение (ПКМ)
        if (mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue() * (sensitivity * speed * Time.deltaTime);
            float zoom = cam.fieldOfView / maxZoom;
            targetPosition -= new Vector3(delta.x, delta.y, 0) * zoom;

            targetPosition.x = Mathf.Clamp(targetPosition.x, 0, Configs.BOARD_SIZE.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, 0, Configs.BOARD_SIZE.y);
        }

        // Зум
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0 && cam != null)
        {
            float newFov = cam.fieldOfView - scroll * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(newFov, minZoom, maxZoom);
        }

        // Плавное перемещение
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
    }

    /// <summary>
    /// Устанавливает позицию камеры (X, Y) с учётом границ поля.
    /// </summary>
    /// <param name="position">Целевая позиция (X, Y). Z не меняется.</param>
    /// <param name="instant">Если true – сразу переместить, иначе плавно.</param>
    public void SetPosition(Vector2 position, bool instant = false)
    {
        targetPosition.x = Mathf.Clamp(position.x, 0, Configs.BOARD_SIZE.x);
        targetPosition.y = Mathf.Clamp(position.y, 0, Configs.BOARD_SIZE.y);
        if (instant)
            transform.position = targetPosition;
    }
}