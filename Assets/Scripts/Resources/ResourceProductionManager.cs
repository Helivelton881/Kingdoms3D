using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla a produção automática dos prédios produtores.
///
/// Produção atual:
/// - Madeireira: produz Madeira.
/// - Pedreira: produz Pedra.
///
/// A produção é calculada com base no nível individual
/// de cada BuildingInstance existente na cena.
///
/// A cada intervalo de produção, todos os prédios produtores
/// finalizados são contabilizados e os recursos são adicionados
/// ao ResourceManager.
/// </summary>
public class ResourceProductionManager : MonoBehaviour
{
    public static ResourceProductionManager Instance { get; private set; }


    [Header("Configuração da Produção")]
    [SerializeField]
    private float productionIntervalSeconds = 60f;


    [Header("Estado")]
    [SerializeField]
    private float productionTimer;


    [SerializeField]
    private int currentWoodProductionPerMinute;


    [SerializeField]
    private int currentStoneProductionPerMinute;


    [Header("Debug")]
    [SerializeField]
    private bool showProductionLogs = true;


    // =========================================================
    // PROPRIEDADES
    // =========================================================

    public float ProductionIntervalSeconds =>
        productionIntervalSeconds;


    public int CurrentWoodProductionPerMinute =>
        currentWoodProductionPerMinute;


    public int CurrentStoneProductionPerMinute =>
        currentStoneProductionPerMinute;


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


        productionIntervalSeconds =
            Mathf.Max(
                1f,
                productionIntervalSeconds
            );
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        RecalculateProduction();


        if (showProductionLogs)
        {
            Debug.Log(
                "ResourceProductionManager iniciado. " +
                "Produção atual: " +
                "Madeira +" +
                currentWoodProductionPerMinute +
                "/min | Pedra +" +
                currentStoneProductionPerMinute +
                "/min"
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        productionTimer += Time.deltaTime;


        if (
            productionTimer <
            productionIntervalSeconds
        )
        {
            return;
        }


        productionTimer = 0f;


        ProduceResources();
    }


    // =========================================================
    // PRODUZIR RECURSOS
    // =========================================================

    private void ProduceResources()
    {
        RecalculateProduction();


        if (ResourceManager.Instance == null)
        {
            Debug.LogError(
                "ResourceProductionManager: " +
                "ResourceManager não encontrado."
            );

            return;
        }


        int woodToAdd =
            CalculateProductionForInterval(
                currentWoodProductionPerMinute
            );


        int stoneToAdd =
            CalculateProductionForInterval(
                currentStoneProductionPerMinute
            );


        if (
            woodToAdd <= 0 &&
            stoneToAdd <= 0
        )
        {
            if (showProductionLogs)
            {
                Debug.Log(
                    "Produção concluída, mas não existem " +
                    "prédios produtores ativos."
                );
            }

            return;
        }


        ResourceManager.Instance.AddResources(
            woodToAdd,
            stoneToAdd,
            0,
            0
        );


        if (showProductionLogs)
        {
            Debug.Log(
                "Produção realizada: " +
                "Madeira +" +
                woodToAdd +
                " | Pedra +" +
                stoneToAdd +
                " | Produção atual: " +
                "Madeira +" +
                currentWoodProductionPerMinute +
                "/min | Pedra +" +
                currentStoneProductionPerMinute +
                "/min"
            );
        }
    }


    // =========================================================
    // CALCULAR PRODUÇÃO DO INTERVALO
    // =========================================================

    private int CalculateProductionForInterval(
        int productionPerMinute
    )
    {
        if (productionPerMinute <= 0)
            return 0;


        float intervalInMinutes =
            productionIntervalSeconds /
            60f;


        float production =
            productionPerMinute *
            intervalInMinutes;


        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                production
            )
        );
    }


    // =========================================================
    // RECALCULAR PRODUÇÃO
    // =========================================================

    public void RecalculateProduction()
    {
        currentWoodProductionPerMinute = 0;

        currentStoneProductionPerMinute = 0;


        BuildingInstance[] buildings =
            FindObjectsByType<BuildingInstance>(
                FindObjectsSortMode.None
            );


        foreach (
            BuildingInstance building
            in buildings
        )
        {
            if (building == null)
                continue;


            if (building.Data == null)
                continue;


            if (!building.ConstructionFinished)
                continue;


            switch (
                building.Data.BuildingType
            )
            {
                case BuildingType.WoodProduction:

                    currentWoodProductionPerMinute +=
                        building.GetWoodProductionPerMinute();

                    break;


                case BuildingType.StoneProduction:

                    currentStoneProductionPerMinute +=
                        building.GetStoneProductionPerMinute();

                    break;
            }
        }
    }


    // =========================================================
    // CONSULTAR PRODUÇÃO DE MADEIRA
    // =========================================================

    public int GetWoodProductionPerMinute()
    {
        RecalculateProduction();

        return currentWoodProductionPerMinute;
    }


    // =========================================================
    // CONSULTAR PRODUÇÃO DE PEDRA
    // =========================================================

    public int GetStoneProductionPerMinute()
    {
        RecalculateProduction();

        return currentStoneProductionPerMinute;
    }


    // =========================================================
    // CONSULTAR PRODUÇÃO TOTAL
    // =========================================================

    public void GetCurrentProduction(
        out int woodPerMinute,
        out int stonePerMinute
    )
    {
        RecalculateProduction();


        woodPerMinute =
            currentWoodProductionPerMinute;


        stonePerMinute =
            currentStoneProductionPerMinute;
    }


    // =========================================================
    // FORÇAR PRODUÇÃO
    // =========================================================

    /// <summary>
    /// Executa manualmente uma produção.
    /// Útil para testes e futuramente para sistemas
    /// de coleta ou eventos.
    /// </summary>
    public void ProduceNow()
    {
        ProduceResources();
    }


    // =========================================================
    // RESET DO TIMER
    // =========================================================

    public void ResetProductionTimer()
    {
        productionTimer = 0f;
    }
}