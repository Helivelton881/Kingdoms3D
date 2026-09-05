using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controla o construtor do reino.
///
/// Regras atuais:
/// - Existe apenas 1 construtor.
/// - Apenas 1 construção ou upgrade por vez.
/// - Cada tipo de construção pode ter no máximo 10 unidades.
/// - Upgrades também utilizam o único construtor.
/// - Exibe um contador de tempo sobre a construção/trabalho atual.
/// </summary>
public class ConstructionManager : MonoBehaviour
{
    public static ConstructionManager Instance { get; private set; }


    [Header("Configuração")]
    [SerializeField]
    private int numberOfBuilders = 1;

    [SerializeField]
    private int maximumBuildingsPerType = 10;


    [Header("Estado da construção")]
    [SerializeField]
    private bool constructionInProgress;

    [SerializeField]
    private BuildingData currentBuilding;

    [SerializeField]
    private BuildingInstance currentUpgradeTarget;

    [SerializeField]
    private float constructionProgress;

    [SerializeField]
    private float constructionDuration;


    [Header("Timer Visual")]
    [SerializeField]
    private bool showConstructionTimer = true;

    [SerializeField]
    private Vector3 constructionTimerOffset =
        new Vector3(0f, 3f, 0f);


    private Coroutine constructionCoroutine;

    private ConstructionTimer currentTimer;


    // =========================================================
    // PROPRIEDADES
    // =========================================================

    public bool IsBusy => constructionInProgress;

    public int NumberOfBuilders => numberOfBuilders;

    public int MaximumBuildingsPerType =>
        maximumBuildingsPerType;

    public BuildingData CurrentBuilding =>
        currentBuilding;

    public BuildingInstance CurrentUpgradeTarget =>
        currentUpgradeTarget;

    public float ConstructionProgress =>
        constructionProgress;

    public float ConstructionDuration =>
        constructionDuration;


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

        numberOfBuilders =
            Mathf.Max(
                1,
                numberOfBuilders
            );

        maximumBuildingsPerType =
            Mathf.Max(
                1,
                maximumBuildingsPerType
            );
    }


    // =========================================================
    // VERIFICAR CONSTRUTOR
    // =========================================================

    public bool CanStartConstruction()
    {
        return !constructionInProgress;
    }


    public bool CanStartUpgrade()
    {
        return !constructionInProgress;
    }


    // =========================================================
    // CONTAR CONSTRUÇÕES
    // =========================================================

    public int GetBuildingCount(
        BuildingType buildingType
    )
    {
        BuildingInstance[] buildings =
            FindObjectsByType<BuildingInstance>(
                FindObjectsSortMode.None
            );


        int count = 0;


        foreach (
            BuildingInstance building
            in buildings
        )
        {
            if (building == null)
                continue;


            if (building.Data == null)
                continue;


            if (
                building.Data.BuildingType
                == buildingType
            )
            {
                count++;
            }
        }


        // Construção em andamento ainda não possui
        // BuildingInstance, então contamos a reserva.
        if (
            constructionInProgress &&
            currentBuilding != null &&
            currentUpgradeTarget == null &&
            currentBuilding.BuildingType == buildingType
        )
        {
            count++;
        }


        return count;
    }


    // =========================================================
    // VERIFICAR LIMITE
    // =========================================================

    public bool CanBuildMore(
        BuildingData buildingData
    )
    {
        if (buildingData == null)
            return false;


        int currentCount =
            GetBuildingCount(
                buildingData.BuildingType
            );


        return currentCount <
               maximumBuildingsPerType;
    }


    // =========================================================
    // INICIAR CONSTRUÇÃO
    // =========================================================

    public bool StartConstruction(
        BuildingData buildingData,
        Vector3 position,
        Action<GameObject> onFinished
    )
    {
        if (buildingData == null)
        {
            Debug.LogWarning(
                "ConstructionManager: BuildingData inválido."
            );

            return false;
        }


        // =====================================================
        // VERIFICAR CONSTRUTOR
        // =====================================================

        if (constructionInProgress)
        {
            Debug.Log(
                "Construtor ocupado. Aguarde a construção terminar."
            );

            return false;
        }


        // =====================================================
        // VERIFICAR LIMITE
        // =====================================================

        if (!CanBuildMore(buildingData))
        {
            Debug.Log(
                "Limite atingido: máximo de " +
                maximumBuildingsPerType +
                " construções do tipo " +
                buildingData.BuildingType +
                "."
            );

            return false;
        }


        // =====================================================
        // TEMPO
        // =====================================================

        constructionDuration =
            Mathf.Max(
                0.1f,
                buildingData.ConstructionTime
            );


        currentBuilding =
            buildingData;

        currentUpgradeTarget =
            null;

        constructionProgress = 0f;

        constructionInProgress = true;


        // =====================================================
        // TIMER VISUAL
        // =====================================================

        CreateConstructionTimer(
            position,
            "CONSTRUINDO"
        );


        // =====================================================
        // INICIAR COROUTINE
        // =====================================================

        constructionCoroutine =
            StartCoroutine(
                ConstructionRoutine(
                    buildingData,
                    position,
                    onFinished
                )
            );


        Debug.Log(
            "Construção iniciada: " +
            buildingData.BuildingName +
            " | Tipo: " +
            buildingData.BuildingType +
            " | Quantidade atual: " +
            GetBuildingCount(
                buildingData.BuildingType
            ) +
            "/" +
            maximumBuildingsPerType +
            " | Tempo: " +
            constructionDuration +
            " segundos."
        );


        return true;
    }


    // =========================================================
    // ROTINA DA CONSTRUÇÃO
    // =========================================================

    private IEnumerator ConstructionRoutine(
        BuildingData buildingData,
        Vector3 position,
        Action<GameObject> onFinished
    )
    {
        float elapsed = 0f;


        while (
            elapsed <
            constructionDuration
        )
        {
            elapsed += Time.deltaTime;


            constructionProgress =
                Mathf.Clamp01(
                    elapsed /
                    constructionDuration
                );


            UpdateCurrentTimer();


            yield return null;
        }


        constructionProgress = 1f;

        UpdateCurrentTimer();


        // =====================================================
        // CRIAR PRÉDIO
        // =====================================================

        GameObject buildingObject = null;


        if (buildingData.Prefab != null)
        {
            buildingObject =
                Instantiate(
                    buildingData.Prefab,
                    position,
                    Quaternion.identity
                );


            buildingObject.name =
                buildingData.BuildingName;


            BuildingInstance instance =
                buildingObject.GetComponent<BuildingInstance>();


            if (instance == null)
            {
                instance =
                    buildingObject.AddComponent<BuildingInstance>();
            }


            instance.Initialize(
                buildingData,
                1
            );
        }
        else
        {
            Debug.LogError(
                "ConstructionManager: o BuildingData '" +
                buildingData.BuildingName +
                "' não possui Prefab."
            );
        }


        // =====================================================
        // DESTRUIR TIMER
        // =====================================================

        DestroyCurrentTimer();


        // =====================================================
        // FINALIZAÇÃO
        // =====================================================

        Debug.Log(
            "Construção concluída: " +
            buildingData.BuildingName +
            " | LVL 1"
        );


        onFinished?.Invoke(
            buildingObject
        );


        FinishCurrentJob();
    }


    // =========================================================
    // INICIAR UPGRADE
    // =========================================================

    public bool StartUpgrade(
        BuildingInstance building,
        Action<BuildingInstance> onFinished
    )
    {
        if (building == null)
        {
            Debug.LogWarning(
                "ConstructionManager: construção inválida para upgrade."
            );

            return false;
        }


        if (building.Data == null)
        {
            Debug.LogWarning(
                "ConstructionManager: a construção não possui BuildingData."
            );

            return false;
        }


        if (!building.CanUpgrade())
        {
            Debug.Log(
                "A construção já está no nível máximo " +
                "ou não pode evoluir."
            );

            return false;
        }


        if (constructionInProgress)
        {
            Debug.Log(
                "Construtor ocupado. Aguarde o trabalho atual terminar."
            );

            return false;
        }


        constructionDuration =
            Mathf.Max(
                0.1f,
                building.GetUpgradeTime()
            );


        currentBuilding =
            building.Data;

        currentUpgradeTarget =
            building;

        constructionProgress = 0f;

        constructionInProgress = true;


        // =====================================================
        // TIMER VISUAL SOBRE O PRÉDIO
        // =====================================================

        CreateUpgradeTimer(
            building
        );


        // =====================================================
        // INICIAR COROUTINE
        // =====================================================

        constructionCoroutine =
            StartCoroutine(
                UpgradeRoutine(
                    building,
                    onFinished
                )
            );


        Debug.Log(
            "Upgrade iniciado: " +
            building.Data.BuildingName +
            " | LVL " +
            building.Level +
            " → " +
            building.GetNextLevel() +
            " | Tempo: " +
            constructionDuration +
            " segundos."
        );


        return true;
    }


    // =========================================================
    // ROTINA DO UPGRADE
    // =========================================================

    private IEnumerator UpgradeRoutine(
        BuildingInstance building,
        Action<BuildingInstance> onFinished
    )
    {
        float elapsed = 0f;


        while (
            elapsed <
            constructionDuration
        )
        {
            elapsed += Time.deltaTime;


            constructionProgress =
                Mathf.Clamp01(
                    elapsed /
                    constructionDuration
                );


            UpdateCurrentTimer();


            yield return null;
        }


        constructionProgress = 1f;

        UpdateCurrentTimer();


        // =====================================================
        // FINALIZAR UPGRADE
        // =====================================================

        if (building != null)
        {
            building.CompleteUpgrade();


            Debug.Log(
                "Upgrade concluído: " +
                building.Data.BuildingName +
                " | LVL " +
                building.Level
            );


            onFinished?.Invoke(
                building
            );
        }


        // =====================================================
        // DESTRUIR TIMER
        // =====================================================

        DestroyCurrentTimer();


        FinishCurrentJob();
    }


    // =========================================================
    // CRIAR TIMER DE CONSTRUÇÃO
    // =========================================================

    private void CreateConstructionTimer(
        Vector3 position,
        string actionText
    )
    {
        DestroyCurrentTimer();


        if (!showConstructionTimer)
            return;


        GameObject timerObject =
            new GameObject(
                "ConstructionTimer"
            );


        timerObject.transform.position =
            position +
            constructionTimerOffset;


        currentTimer =
            timerObject.AddComponent<
                ConstructionTimer
            >();


        currentTimer.Initialize(
            constructionDuration,
            actionText
        );
    }


    // =========================================================
    // CRIAR TIMER DE UPGRADE
    // =========================================================

    private void CreateUpgradeTimer(
        BuildingInstance building
    )
    {
        DestroyCurrentTimer();


        if (!showConstructionTimer)
            return;


        if (building == null)
            return;


        GameObject timerObject =
            new GameObject(
                "UpgradeTimer"
            );


        timerObject.transform.SetParent(
            building.transform
        );


        timerObject.transform.localPosition =
            constructionTimerOffset;


        currentTimer =
            timerObject.AddComponent<
                ConstructionTimer
            >();


        currentTimer.Initialize(
            constructionDuration,
            "UPGRADE"
        );
    }


    // =========================================================
    // ATUALIZAR TIMER
    // =========================================================

    private void UpdateCurrentTimer()
    {
        if (currentTimer == null)
            return;


        currentTimer.SetProgress(
            constructionProgress
        );
    }


    // =========================================================
    // DESTRUIR TIMER
    // =========================================================

    private void DestroyCurrentTimer()
    {
        if (currentTimer == null)
            return;


        Destroy(
            currentTimer.gameObject
        );


        currentTimer = null;
    }


    // =========================================================
    // FINALIZAR TRABALHO
    // =========================================================

    private void FinishCurrentJob()
    {
        constructionInProgress = false;

        currentBuilding = null;

        currentUpgradeTarget = null;

        constructionProgress = 0f;

        constructionDuration = 0f;

        constructionCoroutine = null;
    }


    // =========================================================
    // CANCELAR
    // =========================================================

    public void CancelConstruction()
    {
        if (!constructionInProgress)
            return;


        if (constructionCoroutine != null)
        {
            StopCoroutine(
                constructionCoroutine
            );

            constructionCoroutine = null;
        }


        DestroyCurrentTimer();


        constructionInProgress = false;

        currentBuilding = null;

        currentUpgradeTarget = null;

        constructionProgress = 0f;

        constructionDuration = 0f;


        Debug.Log(
            "Construção/upgrade cancelado."
        );
    }
}