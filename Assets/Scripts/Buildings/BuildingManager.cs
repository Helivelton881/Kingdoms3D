using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Controla o sistema de construção do reino.
///
/// Responsabilidades:
/// - Selecionar o tipo de construção
/// - Mostrar a prévia no terreno
/// - Validar a posição
/// - Impedir reutilização do clique da UI
/// - Controlar BuildPanel
/// - Controlar ProductionPanel
/// - Permitir selecionar Casa, Madeireira e Pedreira
/// - Permitir apenas uma construção por vez
/// - Reservar o terreno durante a construção
/// - Finalizar e registrar a construção
/// </summary>
public class BuildingManager : MonoBehaviour
{
    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("Referências")]
    [SerializeField]
    private KingdomGrid kingdomGrid;

    [SerializeField]
    private KingdomCellSelector cellSelector;

    [SerializeField]
    private ConstructionManager constructionManager;


    // =========================================================
    // PAINEL DE CONSTRUÇÃO
    // =========================================================

    [Header("Painel de Construção")]
    [SerializeField]
    private GameObject buildPanel;


    // =========================================================
    // PAINEL DE PRODUÇÃO
    // =========================================================

    [Header("Painel de Produção")]
    [SerializeField]
    private GameObject productionPanel;


    // =========================================================
    // BOTÕES DE PRODUÇÃO
    // =========================================================

    [Header("Botões de Produção")]
    [SerializeField]
    private GameObject productionButton;

    [SerializeField]
    private GameObject woodButton;

    [SerializeField]
    private GameObject stoneButton;

    [SerializeField]
    private GameObject productionBackButton;


    // =========================================================
    // DADOS DAS CONSTRUÇÕES DE PRODUÇÃO
    // =========================================================

    [Header("Construções de Produção")]
    [SerializeField]
    private BuildingData woodBuilding;

    [SerializeField]
    private BuildingData stoneBuilding;


    // =========================================================
    // CONSTRUÇÃO SELECIONADA
    // =========================================================

    [Header("Construção")]
    [SerializeField]
    private BuildingData selectedBuilding;


    // =========================================================
    // VISUAL DA PRÉVIA
    // =========================================================

    [Header("Visual da prévia")]

    [SerializeField]
    private Color validColor =
        new Color(0.15f, 1f, 0.25f, 0.65f);

    [SerializeField]
    private Color invalidColor =
        new Color(1f, 0.1f, 0.1f, 0.65f);

    [SerializeField]
    private float previewHeight = 0f;


    // =========================================================
    // PRÉVIA
    // =========================================================

    private GameObject previewObject;

    private Renderer[] previewRenderers;

    private Material[] previewMaterials;


    // =========================================================
    // GRID
    // =========================================================

    private bool[,] occupiedCells;


    // =========================================================
    // ESTADO DA CONSTRUÇÃO
    // =========================================================

    private bool constructionMode;

    private Vector2Int currentCell;

    private bool currentPositionValid;


    // =========================================================
    // CONTROLE DO CLIQUE
    // =========================================================

    private bool blockBuildUntilPointerRelease;

    private bool buildArmed;


    // =========================================================
    // CÉLULAS DA PRÉVIA
    // =========================================================

    private readonly List<Vector2Int> previewCells =
        new List<Vector2Int>();


    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void Awake()
    {
        // -----------------------------------------------------
        // KingdomGrid
        // -----------------------------------------------------

        if (kingdomGrid == null)
        {
            kingdomGrid =
                FindFirstObjectByType<KingdomGrid>();
        }


        // -----------------------------------------------------
        // KingdomCellSelector
        // -----------------------------------------------------

        if (cellSelector == null)
        {
            cellSelector =
                FindFirstObjectByType<KingdomCellSelector>();
        }


        // -----------------------------------------------------
        // ConstructionManager
        // -----------------------------------------------------

        FindOrCreateConstructionManager();


        // -----------------------------------------------------
        // Criar matriz de células ocupadas
        // -----------------------------------------------------

        if (kingdomGrid != null)
        {
            occupiedCells =
                new bool[
                    kingdomGrid.Width,
                    kingdomGrid.Height
                ];
        }
        else
        {
            Debug.LogError(
                "BuildingManager: KingdomGrid não encontrado."
            );
        }


        // -----------------------------------------------------
        // Localizar BuildPanel
        // -----------------------------------------------------

        if (buildPanel == null)
        {
            GameObject panel =
                GameObject.Find("BuildPanel");

            if (panel != null)
            {
                buildPanel = panel;
            }
        }


        // -----------------------------------------------------
        // Localizar ProductionPanel
        // -----------------------------------------------------

        FindProductionUI();


        // -----------------------------------------------------
        // Garantir estado inicial
        // -----------------------------------------------------

        if (productionPanel != null)
        {
            productionPanel.SetActive(false);
        }
    }


    // =========================================================
    // ENCONTRAR UI DE PRODUÇÃO
    // =========================================================

    private void FindProductionUI()
    {
        if (buildPanel == null)
            return;


        // -----------------------------------------------------
        // ProductionPanel
        // -----------------------------------------------------

        if (productionPanel == null)
        {
            Transform panelTransform =
                buildPanel.transform.Find(
                    "ProductionPanel"
                );

            if (panelTransform != null)
            {
                productionPanel =
                    panelTransform.gameObject;
            }
        }


        // -----------------------------------------------------
        // BTN_Production
        // -----------------------------------------------------

        if (productionButton == null)
        {
            Transform buttonTransform =
                buildPanel.transform.Find(
                    "BTN_Production"
                );

            if (buttonTransform != null)
            {
                productionButton =
                    buttonTransform.gameObject;
            }
        }


        // -----------------------------------------------------
        // BTN_Wood
        // -----------------------------------------------------

        if (woodButton == null &&
            productionPanel != null)
        {
            Transform buttonTransform =
                productionPanel.transform.Find(
                    "BTN_Wood"
                );

            if (buttonTransform != null)
            {
                woodButton =
                    buttonTransform.gameObject;
            }
        }


        // -----------------------------------------------------
        // BTN_Stone
        // -----------------------------------------------------

        if (stoneButton == null &&
            productionPanel != null)
        {
            Transform buttonTransform =
                productionPanel.transform.Find(
                    "BTN_Stone"
                );

            if (buttonTransform != null)
            {
                stoneButton =
                    buttonTransform.gameObject;
            }
        }


        // -----------------------------------------------------
        // BTN_Back
        // -----------------------------------------------------

        if (productionBackButton == null &&
            productionPanel != null)
        {
            Transform buttonTransform =
                productionPanel.transform.Find(
                    "BTN_Back"
                );

            if (buttonTransform != null)
            {
                productionBackButton =
                    buttonTransform.gameObject;
            }
        }


        // -----------------------------------------------------
        // Registrar eventos automaticamente
        // -----------------------------------------------------

        SetupProductionButtons();
    }


    // =========================================================
    // CONFIGURAR BOTÕES DE PRODUÇÃO
    // =========================================================

    private void SetupProductionButtons()
    {
        // -----------------------------------------------------
        // BTN_Production
        // -----------------------------------------------------

        if (productionButton != null)
        {
            UnityEngine.UI.Button button =
                productionButton.GetComponent<
                    UnityEngine.UI.Button
                >();

            if (button != null)
            {
                button.onClick.RemoveListener(
                    OpenProductionPanel
                );

                button.onClick.AddListener(
                    OpenProductionPanel
                );
            }
        }


        // -----------------------------------------------------
        // BTN_Wood
        // -----------------------------------------------------

        if (woodButton != null)
        {
            UnityEngine.UI.Button button =
                woodButton.GetComponent<
                    UnityEngine.UI.Button
                >();

            if (button != null)
            {
                button.onClick.RemoveListener(
                    SelectWoodBuilding
                );

                button.onClick.AddListener(
                    SelectWoodBuilding
                );
            }
        }


        // -----------------------------------------------------
        // BTN_Stone
        // -----------------------------------------------------

        if (stoneButton != null)
        {
            UnityEngine.UI.Button button =
                stoneButton.GetComponent<
                    UnityEngine.UI.Button
                >();

            if (button != null)
            {
                button.onClick.RemoveListener(
                    SelectStoneBuilding
                );

                button.onClick.AddListener(
                    SelectStoneBuilding
                );
            }
        }


        // -----------------------------------------------------
        // BTN_Back
        // -----------------------------------------------------

        if (productionBackButton != null)
        {
            UnityEngine.UI.Button button =
                productionBackButton.GetComponent<
                    UnityEngine.UI.Button
                >();

            if (button != null)
            {
                button.onClick.RemoveListener(
                    CloseProductionPanel
                );

                button.onClick.AddListener(
                    CloseProductionPanel
                );
            }
        }
    }


    // =========================================================
    // ENCONTRAR OU CRIAR CONSTRUCTION MANAGER
    // =========================================================

    private void FindOrCreateConstructionManager()
    {
        if (constructionManager != null)
            return;


        constructionManager =
            FindFirstObjectByType<ConstructionManager>();


        if (constructionManager != null)
        {
            return;
        }


        constructionManager =
            GetComponent<ConstructionManager>();


        if (constructionManager != null)
        {
            return;
        }


        constructionManager =
            gameObject.AddComponent<ConstructionManager>();


        if (constructionManager != null)
        {
            Debug.Log(
                "BuildingManager: ConstructionManager " +
                "foi criado automaticamente no mesmo GameObject."
            );
        }
        else
        {
            Debug.LogError(
                "BuildingManager: não foi possível criar " +
                "o ConstructionManager."
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        HandleTestBuildingInput();


        if (!constructionMode)
            return;


        UpdatePreview();

        HandleBuildInput();

        HandleCancelInput();
    }


    // =========================================================
    // TESTE TEMPORÁRIO
    // =========================================================

    private void HandleTestBuildingInput()
    {
        if (Keyboard.current == null)
            return;


        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (
                selectedBuilding != null &&
                constructionMode
            )
            {
                return;
            }


            if (selectedBuilding != null)
            {
                SelectBuilding(
                    selectedBuilding
                );
            }
        }
    }


    // =========================================================
    // ABRIR PAINEL DE PRODUÇÃO
    // =========================================================

    public void OpenProductionPanel()
    {
        blockBuildUntilPointerRelease = true;

        buildArmed = false;


        if (productionPanel == null)
        {
            FindProductionUI();
        }


        if (buildPanel != null)
        {
            buildPanel.SetActive(true);
        }


        if (productionPanel != null)
        {
            productionPanel.SetActive(true);
        }


        Debug.Log(
            "Painel de Produção aberto."
        );
    }


    // =========================================================
    // FECHAR PAINEL DE PRODUÇÃO
    // =========================================================

    public void CloseProductionPanel()
    {
        blockBuildUntilPointerRelease = true;

        buildArmed = false;


        if (productionPanel != null)
        {
            productionPanel.SetActive(false);
        }


        if (buildPanel != null)
        {
            buildPanel.SetActive(true);
        }


        Debug.Log(
            "Painel de Produção fechado."
        );
    }


    // =========================================================
    // SELECIONAR MADEIREIRA
    // =========================================================

    public void SelectWoodBuilding()
    {
        if (woodBuilding == null)
        {
            Debug.LogError(
                "BuildingManager: BD_Wood não foi configurado."
            );

            return;
        }


        SelectBuilding(
            woodBuilding
        );
    }


    // =========================================================
    // SELECIONAR PEDREIRA
    // =========================================================

    public void SelectStoneBuilding()
    {
        if (stoneBuilding == null)
        {
            Debug.LogError(
                "BuildingManager: BD_Stone não foi configurado."
            );

            return;
        }


        SelectBuilding(
            stoneBuilding
        );
    }


    // =========================================================
    // SELECIONAR CONSTRUÇÃO
    // =========================================================

    public void SelectBuilding(
        BuildingData building
    )
    {
        if (building == null)
            return;


        // -----------------------------------------------------
        // O clique do botão da UI não pode construir.
        // -----------------------------------------------------

        blockBuildUntilPointerRelease = true;

        buildArmed = false;


        // -----------------------------------------------------
        // Fechar painel de produção
        // -----------------------------------------------------

        if (productionPanel != null)
        {
            productionPanel.SetActive(false);
        }


        // -----------------------------------------------------
        // Salvar construção selecionada
        // -----------------------------------------------------

        selectedBuilding = building;


        constructionMode = true;

        currentPositionValid = false;


        // -----------------------------------------------------
        // Garantir ConstructionManager
        // -----------------------------------------------------

        FindOrCreateConstructionManager();


        // -----------------------------------------------------
        // Criar prévia
        // -----------------------------------------------------

        CreatePreview();


        // -----------------------------------------------------
        // Fechar BuildPanel
        // -----------------------------------------------------

        HideBuildPanel();

        StartCoroutine(
            HideBuildPanelNextFrame()
        );


        Debug.Log(
            "Modo de construção iniciado: " +
            selectedBuilding.BuildingName
        );
    }


    // =========================================================
    // FECHAR PAINEL DE CONSTRUÇÃO
    // =========================================================

    private void HideBuildPanel()
    {
        if (buildPanel == null)
        {
            buildPanel =
                GameObject.Find("BuildPanel");
        }


        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }
    }


    private IEnumerator HideBuildPanelNextFrame()
    {
        yield return null;

        HideBuildPanel();
    }


    // =========================================================
    // CRIAR PRÉVIA
    // =========================================================

    private void CreatePreview()
    {
        DestroyPreview();


        if (selectedBuilding == null)
            return;


        if (selectedBuilding.Prefab == null)
        {
            Debug.LogError(
                "BuildingManager: o BuildingData não possui Prefab."
            );

            return;
        }


        previewObject =
            Instantiate(
                selectedBuilding.Prefab
            );


        previewObject.name =
            selectedBuilding.BuildingName +
            "_Preview";


        previewRenderers =
            previewObject.GetComponentsInChildren<Renderer>();


        CreatePreviewMaterials();

        DisablePreviewColliders();

        SetPreviewColor(validColor);
    }


    // =========================================================
    // MATERIAIS DA PRÉVIA
    // =========================================================

    private void CreatePreviewMaterials()
    {
        if (previewRenderers == null)
            return;


        previewMaterials =
            new Material[
                previewRenderers.Length
            ];


        for (
            int i = 0;
            i < previewRenderers.Length;
            i++
        )
        {
            Renderer renderer =
                previewRenderers[i];


            if (renderer == null)
                continue;


            Material material =
                renderer.material;


            previewMaterials[i] =
                material;
        }
    }


    // =========================================================
    // DESABILITAR COLLIDERS DA PRÉVIA
    // =========================================================

    private void DisablePreviewColliders()
    {
        if (previewObject == null)
            return;


        Collider[] colliders =
            previewObject
                .GetComponentsInChildren<Collider>();


        foreach (
            Collider collider
            in colliders
        )
        {
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }


    // =========================================================
    // ATUALIZAR PRÉVIA
    // =========================================================

    private void UpdatePreview()
    {
        if (previewObject == null)
            return;


        if (cellSelector == null)
            return;


        if (!cellSelector.HasSelection())
        {
            previewObject.SetActive(false);

            currentPositionValid = false;

            return;
        }


        currentCell =
            cellSelector.GetSelectedCell();


        if (
            kingdomGrid == null ||
            !kingdomGrid.IsInsideGrid(
                currentCell
            )
        )
        {
            previewObject.SetActive(false);

            currentPositionValid = false;

            return;
        }


        previewObject.SetActive(true);


        currentPositionValid =
            CheckBuildingPosition(
                currentCell
            );


        Vector3 worldPosition =
            GetBuildingWorldPosition(
                currentCell,
                selectedBuilding.Width,
                selectedBuilding.Height
            );


        previewObject.transform.position =
            worldPosition;


        SetPreviewColor(
            currentPositionValid
                ? validColor
                : invalidColor
        );
    }


    // =========================================================
    // CALCULAR POSIÇÃO DA CONSTRUÇÃO
    // =========================================================

    private Vector3 GetBuildingWorldPosition(
        Vector2Int cell,
        int width,
        int height
    )
    {
        if (kingdomGrid == null)
            return Vector3.zero;


        float cellSize =
            kingdomGrid.CellSize;


        Vector3 origin =
            kingdomGrid.transform.position;


        Vector3 position =
            origin +
            new Vector3(
                cell.x * cellSize,
                previewHeight,
                cell.y * cellSize
            );


        position +=
            new Vector3(
                width *
                cellSize *
                0.5f,

                0f,

                height *
                cellSize *
                0.5f
            );


        return position;
    }


    // =========================================================
    // VERIFICAR POSIÇÃO
    // =========================================================

    private bool CheckBuildingPosition(
        Vector2Int startCell
    )
    {
        previewCells.Clear();


        if (selectedBuilding == null)
            return false;


        if (occupiedCells == null)
            return false;


        if (kingdomGrid == null)
            return false;


        int width =
            selectedBuilding.Width;


        int height =
            selectedBuilding.Height;


        for (
            int x = 0;
            x < width;
            x++
        )
        {
            for (
                int z = 0;
                z < height;
                z++
            )
            {
                Vector2Int cell =
                    new Vector2Int(
                        startCell.x + x,
                        startCell.y + z
                    );


                if (
                    !kingdomGrid.IsInsideGrid(
                        cell
                    )
                )
                {
                    previewCells.Clear();

                    return false;
                }


                if (
                    occupiedCells[
                        cell.x,
                        cell.y
                    ]
                )
                {
                    previewCells.Clear();

                    return false;
                }


                previewCells.Add(cell);
            }
        }


        return true;
    }


    // =========================================================
    // CONFIRMAR CONSTRUÇÃO
    // =========================================================

    private void HandleBuildInput()
    {
        bool pointerPressed = false;

        bool pointerReleased = false;

        bool pointerDownThisFrame = false;


        // =====================================================
        // MOUSE
        // =====================================================

        if (Mouse.current != null)
        {
            pointerPressed =
                Mouse.current.leftButton.isPressed;

            pointerReleased =
                Mouse.current.leftButton
                    .wasReleasedThisFrame;

            pointerDownThisFrame =
                Mouse.current.leftButton
                    .wasPressedThisFrame;
        }


        // =====================================================
        // TOUCH
        // =====================================================

        else if (Touchscreen.current != null)
        {
            var touch =
                Touchscreen.current.primaryTouch;


            pointerPressed =
                touch.press.isPressed;

            pointerReleased =
                touch.press.wasReleasedThisFrame;

            pointerDownThisFrame =
                touch.press.wasPressedThisFrame;
        }


        // =====================================================
        // BLOQUEAR CLIQUE DA UI
        // =====================================================

        if (blockBuildUntilPointerRelease)
        {
            if (
                pointerReleased ||
                !pointerPressed
            )
            {
                blockBuildUntilPointerRelease =
                    false;
            }
            else
            {
                return;
            }
        }


        // =====================================================
        // AGUARDAR NOVO CLIQUE
        // =====================================================

        if (!buildArmed)
        {
            if (!pointerDownThisFrame)
                return;


            if (IsPointerOverUI())
            {
                blockBuildUntilPointerRelease =
                    true;

                return;
            }


            buildArmed = true;
        }


        // =====================================================
        // AGUARDAR SOLTAR O CLIQUE
        // =====================================================

        if (!pointerReleased)
            return;


        // =====================================================
        // NÃO CONSTRUIR SOBRE UI
        // =====================================================

        if (IsPointerOverUI())
            return;


        // =====================================================
        // POSIÇÃO INVÁLIDA
        // =====================================================

        if (!currentPositionValid)
            return;


        // =====================================================
        // INICIAR CONSTRUÇÃO
        // =====================================================

        PlaceBuilding();


        buildArmed = false;
    }


    // =========================================================
    // VERIFICAR SE O CLIQUE ESTÁ SOBRE A UI
    // =========================================================

    private bool IsPointerOverUI()
    {
        EventSystem eventSystem =
            EventSystem.current;


        if (eventSystem == null)
            return false;


        Vector2 pointerPosition;


        // -----------------------------------------------------
        // Mouse
        // -----------------------------------------------------

        if (Mouse.current != null)
        {
            pointerPosition =
                Mouse.current.position
                    .ReadValue();
        }


        // -----------------------------------------------------
        // Touch
        // -----------------------------------------------------

        else if (
            Touchscreen.current != null &&
            Touchscreen.current
                .primaryTouch
                .press
                .isPressed
        )
        {
            pointerPosition =
                Touchscreen.current
                    .primaryTouch
                    .position
                    .ReadValue();
        }


        else
        {
            return false;
        }


        PointerEventData pointerData =
            new PointerEventData(
                eventSystem
            )
            {
                position =
                    pointerPosition
            };


        List<RaycastResult> results =
            new List<RaycastResult>();


        eventSystem.RaycastAll(
            pointerData,
            results
        );


        return results.Count > 0;
    }


    // =========================================================
    // INICIAR CONSTRUÇÃO
    // =========================================================

    private void PlaceBuilding()
    {
        if (selectedBuilding == null)
            return;


        if (selectedBuilding.Prefab == null)
        {
            Debug.LogError(
                "BuildingManager: o BuildingData não possui Prefab."
            );

            return;
        }


        // =====================================================
        // GARANTIR CONSTRUCTION MANAGER
        // =====================================================

        FindOrCreateConstructionManager();


        if (constructionManager == null)
        {
            Debug.LogError(
                "BuildingManager: não foi possível obter " +
                "o ConstructionManager."
            );

            return;
        }


        // =====================================================
        // VERIFICAR CONSTRUTOR
        // =====================================================

        if (constructionManager.IsBusy)
        {
            Debug.Log(
                "O construtor está ocupado. " +
                "Aguarde a construção terminar."
            );

            return;
        }


        // =====================================================
        // VERIFICAR POSIÇÃO NOVAMENTE
        // =====================================================

        if (
            !CheckBuildingPosition(
                currentCell
            )
        )
        {
            Debug.Log(
                "Posição inválida para construção."
            );

            return;
        }


        // =====================================================
        // CALCULAR POSIÇÃO
        // =====================================================

        Vector3 position =
            GetBuildingWorldPosition(
                currentCell,
                selectedBuilding.Width,
                selectedBuilding.Height
            );


        // =====================================================
        // GUARDAR REFERÊNCIA
        // =====================================================

        BuildingData buildingToConstruct =
            selectedBuilding;


        // =====================================================
        // INICIAR CONSTRUÇÃO
        // =====================================================

        bool started =
            constructionManager.StartConstruction(
                buildingToConstruct,
                position,
                OnConstructionFinished
            );


        // =====================================================
        // VERIFICAR SE INICIOU
        // =====================================================

        if (!started)
        {
            Debug.Log(
                "Não foi possível iniciar a construção."
            );

            return;
        }


        // =====================================================
        // RESERVAR TERRENO
        // =====================================================

        RegisterOccupiedCells();


        Debug.Log(
            "Construção iniciada: " +
            buildingToConstruct.BuildingName
        );


        // =====================================================
        // ENCERRAR SELEÇÃO
        // =====================================================

        constructionMode = false;

        currentPositionValid = false;

        buildArmed = false;

        blockBuildUntilPointerRelease = false;


        DestroyPreview();


        selectedBuilding = null;
    }


    // =========================================================
    // CONSTRUÇÃO FINALIZADA
    // =========================================================

    private void OnConstructionFinished(
        GameObject buildingObject
    )
    {
        if (buildingObject == null)
        {
            Debug.LogError(
                "BuildingManager: construção terminou " +
                "mas o GameObject é nulo."
            );

            return;
        }


        // =====================================================
        // PEGAR BuildingInstance
        // =====================================================

        BuildingInstance instance =
            buildingObject.GetComponent<
                BuildingInstance
            >();


        // =====================================================
        // SE NÃO EXISTIR, CRIAR
        // =====================================================

        if (instance == null)
        {
            instance =
                buildingObject.AddComponent<
                    BuildingInstance
                >();


            if (constructionManager != null)
            {
                instance.Initialize(
                    constructionManager.CurrentBuilding,
                    1
                );
            }
        }


        // =====================================================
        // LOG
        // =====================================================

        if (instance.Data != null)
        {
            Debug.Log(
                "Construção concluída: " +
                instance.Data.BuildingName +
                " | LVL " +
                instance.Level
            );
        }
        else
        {
            Debug.Log(
                "Construção concluída."
            );
        }
    }


    // =========================================================
    // REGISTRAR CÉLULAS
    // =========================================================

    private void RegisterOccupiedCells()
    {
        if (kingdomGrid == null)
            return;


        if (occupiedCells == null)
            return;


        foreach (
            Vector2Int cell
            in previewCells
        )
        {
            if (
                kingdomGrid.IsInsideGrid(
                    cell
                )
            )
            {
                occupiedCells[
                    cell.x,
                    cell.y
                ] = true;
            }
        }
    }


    // =========================================================
    // COR DA PRÉVIA
    // =========================================================

    private void SetPreviewColor(
        Color color
    )
    {
        if (previewMaterials == null)
            return;


        foreach (
            Material material
            in previewMaterials
        )
        {
            if (material == null)
                continue;


            if (
                material.HasProperty(
                    "_BaseColor"
                )
            )
            {
                material.SetColor(
                    "_BaseColor",
                    color
                );
            }
            else if (
                material.HasProperty(
                    "_Color"
                )
            )
            {
                material.SetColor(
                    "_Color",
                    color
                );
            }
        }
    }


    // =========================================================
    // CANCELAR
    // =========================================================

    private void HandleCancelInput()
    {
        if (Keyboard.current == null)
            return;


        if (
            Keyboard.current
                .escapeKey
                .wasPressedThisFrame
        )
        {
            ExitConstructionMode();
        }
    }


    // =========================================================
    // SAIR DO MODO CONSTRUÇÃO
    // =========================================================

    public void ExitConstructionMode()
    {
        constructionMode = false;

        currentPositionValid = false;

        blockBuildUntilPointerRelease = false;

        buildArmed = false;

        DestroyPreview();


        selectedBuilding = null;
    }


    // =========================================================
    // DESTRUIR PRÉVIA
    // =========================================================

    private void DestroyPreview()
    {
        if (previewObject != null)
        {
            Destroy(
                previewObject
            );

            previewObject = null;
        }


        previewRenderers = null;

        previewMaterials = null;

        previewCells.Clear();
    }


    // =========================================================
    // API PÚBLICA
    // =========================================================

    public bool IsConstructionMode()
    {
        return constructionMode;
    }


    public BuildingData GetSelectedBuilding()
    {
        return selectedBuilding;
    }


    public bool IsCurrentPositionValid()
    {
        return currentPositionValid;
    }


    // =========================================================
    // INFORMAÇÕES DO CONSTRUTOR
    // =========================================================

    public bool IsBuilderBusy()
    {
        FindOrCreateConstructionManager();


        if (constructionManager == null)
            return false;


        return constructionManager.IsBusy;
    }


    public float GetConstructionProgress()
    {
        FindOrCreateConstructionManager();


        if (constructionManager == null)
            return 0f;


        return constructionManager.ConstructionProgress;
    }


    public float GetConstructionDuration()
    {
        FindOrCreateConstructionManager();


        if (constructionManager == null)
            return 0f;


        return constructionManager.ConstructionDuration;
    }


    public BuildingData GetCurrentConstruction()
    {
        FindOrCreateConstructionManager();


        if (constructionManager == null)
            return null;


        return constructionManager.CurrentBuilding;
    }
}