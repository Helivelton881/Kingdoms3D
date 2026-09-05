using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla o painel de informações das construções.
///
/// Mostra informações diferentes de acordo com o tipo
/// da construção.
/// </summary>
public class BuildingUpgradeUI : MonoBehaviour
{
    public static BuildingUpgradeUI Instance { get; private set; }


    [Header("Painel")]
    [SerializeField]
    private GameObject panel;


    [Header("Textos")]
    [SerializeField]
    private TMP_Text buildingNameText;

    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private TMP_Text populationText;

    [SerializeField]
    private TMP_Text productionText;

    [SerializeField]
    private TMP_Text upgradeCostText;

    [SerializeField]
    private TMP_Text upgradeTimeText;


    [Header("Botões")]
    [SerializeField]
    private Button upgradeButton;

    [SerializeField]
    private Button closeButton;


    [Header("Câmera")]
    [SerializeField]
    private Camera mainCamera;


    private BuildingInstance selectedBuilding;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }


        if (panel != null)
        {
            panel.SetActive(false);
        }


        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();

            upgradeButton.onClick.AddListener(
                UpgradeSelectedBuilding
            );
        }


        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();

            closeButton.onClick.AddListener(
                ClosePanel
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectBuilding();
        }


        UpdateUpgradeState();
    }


    // =========================================================
    // SELECIONAR CONSTRUÇÃO
    // =========================================================

    private void TrySelectBuilding()
    {
        if (IsPointerOverUI())
            return;


        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }


        if (mainCamera == null)
            return;


        Ray ray =
            mainCamera.ScreenPointToRay(
                Input.mousePosition
            );


        if (
            Physics.Raycast(
                ray,
                out RaycastHit hit,
                1000f
            )
        )
        {
            BuildingInstance building =
                hit.collider.GetComponentInParent<BuildingInstance>();


            if (building != null)
            {
                SelectBuilding(building);
            }
        }
    }


    // =========================================================
    // SELECIONAR
    // =========================================================

    public void SelectBuilding(
        BuildingInstance building
    )
    {
        if (building == null)
            return;


        if (building.Data == null)
            return;


        selectedBuilding = building;


        RefreshPanel();


        if (panel != null)
        {
            panel.SetActive(true);
        }


        Debug.Log(
            "Construção selecionada: " +
            building.Data.BuildingName +
            " | LVL " +
            building.Level
        );
    }


    // =========================================================
    // ATUALIZAR PAINEL
    // =========================================================

    public void RefreshPanel()
    {
        if (selectedBuilding == null)
            return;


        if (selectedBuilding.Data == null)
            return;


        BuildingData data =
            selectedBuilding.Data;


        // =====================================================
        // NOME
        // =====================================================

        if (buildingNameText != null)
        {
            buildingNameText.text =
                data.BuildingName;
        }


        // =====================================================
        // NÍVEL
        // =====================================================

        if (levelText != null)
        {
            levelText.text =
                "LVL " +
                selectedBuilding.Level;
        }


        // =====================================================
        // POPULAÇÃO
        // =====================================================

        if (populationText != null)
        {
            if (data.BuildingType ==
                BuildingType.Residential)
            {
                populationText.text =
                    "População: " +
                    selectedBuilding.GetPopulation();

                populationText.gameObject.SetActive(true);
            }
            else
            {
                populationText.gameObject.SetActive(false);
            }
        }


        // =====================================================
        // PRODUÇÃO
        // =====================================================

        if (productionText != null)
        {
            string production = "";


            switch (data.BuildingType)
            {
                case BuildingType.WoodProduction:

                    production =
                        "Madeira: +" +
                        selectedBuilding
                            .GetWoodProductionPerMinute();

                    break;


                case BuildingType.StoneProduction:

                    production =
                        "Pedra: +" +
                        selectedBuilding
                            .GetStoneProductionPerMinute();

                    break;


                default:

                    production = "—";

                    break;
            }


            productionText.text =
                production;


            productionText.gameObject.SetActive(true);
        }


        // =====================================================
        // CUSTOS DO PRÓXIMO UPGRADE
        // =====================================================

        if (upgradeCostText != null)
        {
            if (selectedBuilding.CanUpgrade())
            {
                upgradeCostText.text =
                    "Próximo nível:\n" +
                    "Madeira: " +
                    selectedBuilding.GetUpgradeWoodCost() +
                    "\n" +
                    "Pedra: " +
                    selectedBuilding.GetUpgradeStoneCost() +
                    "\n" +
                    "Comida: " +
                    selectedBuilding.GetUpgradeFoodCost() +
                    "\n" +
                    "Ouro: " +
                    selectedBuilding.GetUpgradeGoldCost();
            }
            else
            {
                upgradeCostText.text =
                    "NÍVEL MÁXIMO";
            }
        }


        // =====================================================
        // TEMPO
        // =====================================================

        if (upgradeTimeText != null)
        {
            if (selectedBuilding.CanUpgrade())
            {
                upgradeTimeText.text =
                    "Tempo: " +
                    selectedBuilding.GetUpgradeTime() +
                    " segundos";
            }
            else
            {
                upgradeTimeText.text =
                    "";
            }
        }
    }


    // =========================================================
    // ESTADO DO BOTÃO
    // =========================================================

    private void UpdateUpgradeState()
    {
        if (upgradeButton == null)
            return;


        if (selectedBuilding == null)
        {
            upgradeButton.interactable = false;

            return;
        }


        if (ConstructionManager.Instance == null)
        {
            upgradeButton.interactable =
                selectedBuilding.CanUpgrade();

            return;
        }


        upgradeButton.interactable =
            selectedBuilding.CanUpgrade() &&
            !ConstructionManager.Instance.IsBusy;
    }


    // =========================================================
    // UPGRADE
    // =========================================================

    public void UpgradeSelectedBuilding()
    {
        if (selectedBuilding == null)
            return;


        if (!selectedBuilding.CanUpgrade())
            return;


        if (ConstructionManager.Instance == null)
        {
            Debug.LogError(
                "BuildingUpgradeUI: ConstructionManager não encontrado."
            );

            return;
        }


        bool started =
            ConstructionManager.Instance.StartUpgrade(
                selectedBuilding,
                OnUpgradeFinished
            );


        if (!started)
            return;


        Debug.Log(
            "Upgrade iniciado para " +
            selectedBuilding.Data.BuildingName +
            " | LVL " +
            selectedBuilding.Level +
            " → LVL " +
            selectedBuilding.GetNextLevel()
        );
    }


    // =========================================================
    // UPGRADE FINALIZADO
    // =========================================================

    private void OnUpgradeFinished(
        BuildingInstance building
    )
    {
        if (building == null)
            return;


        if (selectedBuilding != building)
            return;


        RefreshPanel();


        Debug.Log(
            "Painel atualizado após upgrade. " +
            building.Data.BuildingName +
            " agora está LVL " +
            building.Level
        );
    }


    // =========================================================
    // FECHAR
    // =========================================================

    public void ClosePanel()
    {
        selectedBuilding = null;


        if (panel != null)
        {
            panel.SetActive(false);
        }
    }


    // =========================================================
    // UI
    // =========================================================

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;


        return EventSystem.current.IsPointerOverGameObject();
    }
}