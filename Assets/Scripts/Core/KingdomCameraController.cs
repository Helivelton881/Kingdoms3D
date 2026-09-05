using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de câmera para o mapa do Reino.
/// Funciona com:
/// - WASD / Setas
/// - Arrastar com botão do meio do mouse
/// - Scroll do mouse
/// - Toque com um dedo
/// - Pinch com dois dedos
/// - Movimento suave
/// - Zoom suave
/// - Limites configuráveis
/// </summary>
public class KingdomCameraController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float smoothMovement = 12f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float zoomSmooth = 10f;
    [SerializeField] private float minZoom = 6f;
    [SerializeField] private float maxZoom = 18f;

    [Header("Limites do Reino")]
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;
    [SerializeField] private float minZ = -20f;
    [SerializeField] private float maxZ = 20f;

    [Header("Mouse")]
    [SerializeField] private float mouseDragSpeed = 0.025f;

    [Header("Toque")]
    [SerializeField] private float touchDragSpeed = 0.02f;
    [SerializeField] private float pinchZoomSpeed = 0.02f;

    private Camera cam;

    private Vector3 targetPosition;
    private float targetZoom;

    private Vector3 lastMousePosition;
    private bool isDraggingMouse;

    private Vector2 lastTouchPosition;
    private bool isDraggingTouch;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError(
                "KingdomCameraController precisa estar em um GameObject com componente Camera."
            );

            enabled = false;
            return;
        }

        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;

        if (!cam.orthographic)
        {
            Debug.LogWarning(
                "KingdomCameraController foi configurado para câmera Orthographic. " +
                "A câmera será alterada automaticamente."
            );

            cam.orthographic = true;
        }
    }

    private void Update()
    {
        HandleKeyboardMovement();
        HandleMouse();
        HandleTouch();

        ApplyMovement();
        ApplyZoom();
    }

    // =========================================================
    // MOVIMENTO PELO TECLADO
    // =========================================================

    private void HandleKeyboardMovement()
    {
        if (Keyboard.current == null)
            return;

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            input.y += 1f;
        }

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
        {
            input.y -= 1f;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            input.x += 1f;
        }

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            input.x -= 1f;
        }

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 movement =
            (forward * input.y + right * input.x)
            * moveSpeed
            * Time.deltaTime;

        targetPosition += movement;

        ClampTargetPosition();
    }

    // =========================================================
    // MOUSE
    // =========================================================

    private void HandleMouse()
    {
        if (Mouse.current == null)
            return;

        // Scroll
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * zoomSpeed * 0.01f;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Botão do meio para arrastar
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            isDraggingMouse = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            isDraggingMouse = false;
        }

        if (isDraggingMouse &&
            Mouse.current.middleButton.isPressed)
        {
            Vector3 currentMousePosition =
                Mouse.current.position.ReadValue();

            Vector3 delta =
                currentMousePosition - lastMousePosition;

            MoveFromScreenDelta(delta, mouseDragSpeed);

            lastMousePosition = currentMousePosition;
        }
    }

    // =========================================================
    // TOUCH
    // =========================================================

    private void HandleTouch()
    {
        if (Touchscreen.current == null)
            return;

        int touchCount =
            Touchscreen.current.touches.Count;

        if (touchCount == 0)
        {
            isDraggingTouch = false;
            return;
        }

        // -----------------------------------------------------
        // UM DEDO = MOVER CÂMERA
        // -----------------------------------------------------

        if (touchCount == 1)
        {
            var touch =
                Touchscreen.current.touches[0];

            Vector2 position =
                touch.position.ReadValue();

            if (touch.press.wasPressedThisFrame)
            {
                lastTouchPosition = position;
                isDraggingTouch = true;
            }

            if (isDraggingTouch &&
                touch.press.isPressed)
            {
                Vector2 delta =
                    position - lastTouchPosition;

                MoveFromScreenDelta(
                    delta,
                    touchDragSpeed
                );

                lastTouchPosition = position;
            }

            if (touch.press.wasReleasedThisFrame)
            {
                isDraggingTouch = false;
            }
        }

        // -----------------------------------------------------
        // DOIS DEDOS = PINCH ZOOM
        // -----------------------------------------------------

        if (touchCount >= 2)
        {
            var touch0 =
                Touchscreen.current.touches[0];

            var touch1 =
                Touchscreen.current.touches[1];

            Vector2 current0 =
                touch0.position.ReadValue();

            Vector2 current1 =
                touch1.position.ReadValue();

            Vector2 previous0 =
                current0 - touch0.delta.ReadValue();

            Vector2 previous1 =
                current1 - touch1.delta.ReadValue();

            float currentDistance =
                Vector2.Distance(
                    current0,
                    current1
                );

            float previousDistance =
                Vector2.Distance(
                    previous0,
                    previous1
                );

            float difference =
                currentDistance - previousDistance;

            targetZoom -=
                difference * pinchZoomSpeed;

            targetZoom =
                Mathf.Clamp(
                    targetZoom,
                    minZoom,
                    maxZoom
                );
        }
    }

    // =========================================================
    // MOVIMENTO POR ARRASTO
    // =========================================================

    private void MoveFromScreenDelta(
        Vector2 screenDelta,
        float speed)
    {
        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 movement =
            (-right * screenDelta.x
             - forward * screenDelta.y)
            * speed;

        targetPosition += movement;

        ClampTargetPosition();
    }

    // =========================================================
    // MOVIMENTO SUAVE
    // =========================================================

    private void ApplyMovement()
    {
        transform.position =
            Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothMovement * Time.deltaTime
            );
    }

    // =========================================================
    // ZOOM SUAVE
    // =========================================================

    private void ApplyZoom()
    {
        cam.orthographicSize =
            Mathf.Lerp(
                cam.orthographicSize,
                targetZoom,
                zoomSmooth * Time.deltaTime
            );
    }

    // =========================================================
    // LIMITES
    // =========================================================

    private void ClampTargetPosition()
    {
        targetPosition.x =
            Mathf.Clamp(
                targetPosition.x,
                minX,
                maxX
            );

        targetPosition.z =
            Mathf.Clamp(
                targetPosition.z,
                minZ,
                maxZ
            );
    }
}