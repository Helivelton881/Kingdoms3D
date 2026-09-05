using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detecta a célula do Grid que está sob o mouse ou toque.
/// Cria visualmente um marcador sobre a célula selecionada.
/// </summary>
public class KingdomCellSelector : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private KingdomGrid kingdomGrid;
    [SerializeField] private Camera kingdomCamera;

    [Header("Raycast")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Marcador")]
    [SerializeField] private float markerHeight = 0.05f;
    [SerializeField] private float markerSize = 1.9f;

    [Header("Cores")]
    [SerializeField]
    private Color validColor =
        new Color(0.15f, 1f, 0.25f, 1f);

    [SerializeField]
    private Color invalidColor =
        new Color(1f, 0.15f, 0.15f, 1f);

    private GameObject selectionMarker;
    private Renderer markerRenderer;
    private Material markerMaterial;

    private Vector2Int currentCell;
    private bool hasSelection;

    private void Awake()
    {
        if (kingdomCamera == null)
            kingdomCamera = Camera.main;

        if (kingdomGrid == null)
            kingdomGrid = FindFirstObjectByType<KingdomGrid>();

        CreateSelectionMarker();

        HideSelection();
    }

    private void Update()
    {
        HandleMouse();
        HandleTouch();
    }

    // =========================================================
    // MOUSE
    // =========================================================

    private void HandleMouse()
    {
        if (Mouse.current == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        UpdateSelectionFromScreenPosition(mousePosition);
    }

    // =========================================================
    // TOUCH
    // =========================================================

    private void HandleTouch()
    {
        if (Touchscreen.current == null)
            return;

        var touches =
            Touchscreen.current.touches;

        if (touches.Count == 0)
            return;

        var touch = touches[0];

        if (!touch.press.isPressed)
            return;

        Vector2 touchPosition =
            touch.position.ReadValue();

        UpdateSelectionFromScreenPosition(touchPosition);
    }

    // =========================================================
    // RAYCAST
    // =========================================================

    private void UpdateSelectionFromScreenPosition(
        Vector2 screenPosition)
    {
        if (kingdomCamera == null)
            return;

        if (kingdomGrid == null)
            return;

        Ray ray =
            kingdomCamera.ScreenPointToRay(
                screenPosition
            );

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                1000f,
                groundLayer))
        {
            HideSelection();
            return;
        }

        Vector2Int cell =
            kingdomGrid.WorldToCell(
                hit.point
            );

        if (!kingdomGrid.IsInsideGrid(cell))
        {
            HideSelection();
            return;
        }

        SelectCell(cell);
    }

    // =========================================================
    // SELECIONAR CÉLULA
    // =========================================================

    private void SelectCell(Vector2Int cell)
    {
        currentCell = cell;
        hasSelection = true;

        Vector3 worldPosition =
            kingdomGrid.CellToWorld(cell);

        worldPosition.y += markerHeight;

        selectionMarker.transform.position =
            worldPosition;

        selectionMarker.SetActive(true);

        SetMarkerColor(validColor);
    }

    // =========================================================
    // CRIAR MARCADOR
    // =========================================================

    private void CreateSelectionMarker()
    {
        selectionMarker =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        selectionMarker.name =
            "CellSelectionMarker";

        selectionMarker.transform.localScale =
            new Vector3(
                markerSize,
                0.03f,
                markerSize
            );

        Collider collider =
            selectionMarker.GetComponent<Collider>();

        if (collider != null)
            Destroy(collider);

        markerRenderer =
            selectionMarker.GetComponent<Renderer>();

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit"
            );

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        markerMaterial =
            new Material(shader);

        markerRenderer.material =
            markerMaterial;

        SetMarkerColor(validColor);
    }

    // =========================================================
    // COR
    // =========================================================

    private void SetMarkerColor(Color color)
    {
        if (markerMaterial == null)
            return;

        markerMaterial.color = color;
    }

    // =========================================================
    // ESCONDER
    // =========================================================

    private void HideSelection()
    {
        hasSelection = false;

        if (selectionMarker != null)
            selectionMarker.SetActive(false);
    }

    // =========================================================
    // INFORMAÇÕES PÚBLICAS
    // =========================================================

    public bool HasSelection()
    {
        return hasSelection;
    }

    public Vector2Int GetSelectedCell()
    {
        return currentCell;
    }
}