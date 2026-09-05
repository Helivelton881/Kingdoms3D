using UnityEngine;

public enum BuildingType
{
    Residential,
    WoodProduction,
    StoneProduction,
    Military,
    Defense,
    Economy
}

[CreateAssetMenu(
    fileName = "BuildingData",
    menuName = "Kingdoms 3D/Building Data"
)]
public class BuildingData : ScriptableObject
{
    // =========================================================
    // IDENTIFICAÇÃO
    // =========================================================

    [Header("Identificação")]

    [SerializeField]
    private string buildingId;

    [SerializeField]
    private string buildingName;

    [TextArea(2, 4)]
    [SerializeField]
    private string description;


    // =========================================================
    // TIPO
    // =========================================================

    [Header("Tipo")]

    [SerializeField]
    private BuildingType buildingType =
        BuildingType.Residential;


    // =========================================================
    // VISUAL
    // =========================================================

    [Header("Visual")]

    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private Sprite icon;


    // =========================================================
    // GRID
    // =========================================================

    [Header("Grid")]

    [SerializeField]
    private int width = 1;

    [SerializeField]
    private int height = 1;


    // =========================================================
    // CONSTRUÇÃO INICIAL
    // =========================================================

    [Header("Construção Inicial")]

    [Tooltip("Tempo necessário para construir a unidade no LVL 1.")]
    [SerializeField]
    private float constructionTime = 5f;


    // =========================================================
    // CUSTO DA CONSTRUÇÃO INICIAL
    // =========================================================

    [Header("Custo da Construção Inicial")]

    [SerializeField]
    private int woodCost;

    [SerializeField]
    private int stoneCost;

    [SerializeField]
    private int foodCost;

    [SerializeField]
    private int goldCost;


    // =========================================================
    // REQUISITOS
    // =========================================================

    [Header("Requisitos")]

    [SerializeField]
    private int requiredLevel = 1;


    // =========================================================
    // NÍVEIS
    // =========================================================

    [Header("Níveis")]

    [Tooltip("Nível máximo da construção.")]
    [SerializeField]
    private int maxLevel = 10;


    // =========================================================
    // POPULAÇÃO POR NÍVEL
    // =========================================================

    [Header("População por nível")]

    [Tooltip(
        "População fornecida pela construção em cada nível."
    )]
    [SerializeField]
    private int[] populationByLevel =
    {
        10,
        15,
        20,
        25,
        30,
        35,
        40,
        45,
        50,
        55
    };


    // =========================================================
    // PRODUÇÃO DE MADEIRA POR NÍVEL
    // =========================================================

    [Header("Produção de Madeira por minuto")]

    [Tooltip(
        "Quantidade de madeira produzida por minuto em cada nível."
    )]
    [SerializeField]
    private int[] woodProductionByLevel =
    {
        8,
        16,
        24,
        32,
        40,
        48,
        56,
        64,
        72,
        80
    };


    // =========================================================
    // PRODUÇÃO DE PEDRA POR NÍVEL
    // =========================================================

    [Header("Produção de Pedra por minuto")]

    [Tooltip(
        "Quantidade de pedra produzida por minuto em cada nível."
    )]
    [SerializeField]
    private int[] stoneProductionByLevel =
    {
        8,
        16,
        24,
        32,
        40,
        48,
        56,
        64,
        72,
        80
    };


    // =========================================================
    // CUSTOS DE UPGRADE
    // =========================================================
    //
    // Cada posição representa o upgrade:
    //
    // índice 0 = LVL 1 → LVL 2
    // índice 1 = LVL 2 → LVL 3
    // índice 2 = LVL 3 → LVL 4
    // índice 3 = LVL 4 → LVL 5
    // índice 4 = LVL 5 → LVL 6
    // índice 5 = LVL 6 → LVL 7
    // índice 6 = LVL 7 → LVL 8
    // índice 7 = LVL 8 → LVL 9
    // índice 8 = LVL 9 → LVL 10
    //
    // O índice 9 não é utilizado porque o LVL 10
    // não possui próximo nível.
    // =========================================================

    [Header("Custo de Madeira - Upgrade")]

    [SerializeField]
    private int[] upgradeWoodCostByLevel =
    {
        20,
        40,
        80,
        160,
        300,
        500,
        800,
        1200,
        1800
    };


    [Header("Custo de Pedra - Upgrade")]

    [SerializeField]
    private int[] upgradeStoneCostByLevel =
    {
        10,
        20,
        40,
        80,
        150,
        250,
        400,
        600,
        900
    };


    [Header("Custo de Comida - Upgrade")]

    [SerializeField]
    private int[] upgradeFoodCostByLevel =
    {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
    };


    [Header("Custo de Ouro - Upgrade")]

    [SerializeField]
    private int[] upgradeGoldCostByLevel =
    {
        10,
        20,
        40,
        80,
        150,
        250,
        400,
        600,
        900
    };


    // =========================================================
    // TEMPO DE UPGRADE
    // =========================================================

    [Header("Tempo de Upgrade")]

    [Tooltip(
        "Tempo de cada upgrade. " +
        "Índice 0 = LVL 1 → 2."
    )]
    [SerializeField]
    private float[] upgradeTimeByLevel =
    {
        10f,
        20f,
        40f,
        60f,
        120f,
        180f,
        300f,
        420f,
        600f
    };


    // =========================================================
    // PROPRIEDADES
    // =========================================================

    public string BuildingId => buildingId;

    public string BuildingName => buildingName;

    public string Description => description;

    public BuildingType BuildingType => buildingType;

    public GameObject Prefab => prefab;

    public Sprite Icon => icon;

    public int Width => width;

    public int Height => height;

    public float ConstructionTime => constructionTime;

    public int WoodCost => woodCost;

    public int StoneCost => stoneCost;

    public int FoodCost => foodCost;

    public int GoldCost => goldCost;

    public int RequiredLevel => requiredLevel;

    public int MaxLevel =>
        Mathf.Clamp(
            maxLevel,
            1,
            10
        );


    // =========================================================
    // POPULAÇÃO
    // =========================================================

    public int GetPopulation(int level)
    {
        level =
            Mathf.Clamp(
                level,
                1,
                MaxLevel
            );


        if (
            populationByLevel == null ||
            populationByLevel.Length == 0
        )
        {
            return 0;
        }


        int index =
            level - 1;


        if (
            index >=
            populationByLevel.Length
        )
        {
            index =
                populationByLevel.Length - 1;
        }


        return populationByLevel[index];
    }


    // =========================================================
    // PRODUÇÃO DE MADEIRA
    // =========================================================

    public int GetWoodProductionPerMinute(
        int level
    )
    {
        level =
            Mathf.Clamp(
                level,
                1,
                MaxLevel
            );


        if (
            woodProductionByLevel == null ||
            woodProductionByLevel.Length == 0
        )
        {
            return 0;
        }


        int index =
            level - 1;


        if (
            index >=
            woodProductionByLevel.Length
        )
        {
            index =
                woodProductionByLevel.Length - 1;
        }


        return woodProductionByLevel[index];
    }


    // =========================================================
    // PRODUÇÃO DE PEDRA
    // =========================================================

    public int GetStoneProductionPerMinute(
        int level
    )
    {
        level =
            Mathf.Clamp(
                level,
                1,
                MaxLevel
            );


        if (
            stoneProductionByLevel == null ||
            stoneProductionByLevel.Length == 0
        )
        {
            return 0;
        }


        int index =
            level - 1;


        if (
            index >=
            stoneProductionByLevel.Length
        )
        {
            index =
                stoneProductionByLevel.Length - 1;
        }


        return stoneProductionByLevel[index];
    }


    // =========================================================
    // CUSTO DE MADEIRA DO UPGRADE
    // =========================================================

    public int GetUpgradeWoodCost(
        int currentLevel
    )
    {
        if (
            currentLevel < 1 ||
            currentLevel >= MaxLevel
        )
        {
            return 0;
        }


        if (
            upgradeWoodCostByLevel == null ||
            upgradeWoodCostByLevel.Length == 0
        )
        {
            return 0;
        }


        int index =
            currentLevel - 1;


        if (
            index >=
            upgradeWoodCostByLevel.Length
        )
        {
            return 0;
        }


        return Mathf.Max(
            0,
            upgradeWoodCostByLevel[index]
        );
    }


    // =========================================================
    // CUSTO DE PEDRA DO UPGRADE
    // =========================================================

    public int GetUpgradeStoneCost(
        int currentLevel
    )
    {
        if (
            currentLevel < 1 ||
            currentLevel >= MaxLevel
        )
        {
            return 0;
        }


        if (
            upgradeStoneCostByLevel == null ||
            upgradeStoneCostByLevel.Length == 0
        )
        {
            return 0;
        }


        int index =
            currentLevel - 1;


        if (
            index >=
            upgradeStoneCostByLevel.Length
        )
        {
            return 0;
        }


        return Mathf.Max(
            0,
            upgradeStoneCostByLevel[index]
        );
    }


    // =========================================================
    // CUSTO DE COMIDA DO UPGRADE
    // =========================================================

    public int GetUpgradeFoodCost(
        int currentLevel
    )
    {
        if (
            currentLevel < 1 ||
            currentLevel >= MaxLevel
        )
        {
            return 0;
        }


        if (
            upgradeFoodCostByLevel == null ||
            upgradeFoodCostByLevel.Length == 0
        )
        {
            return 0;
        }


        int index =
            currentLevel - 1;


        if (
            index >=
            upgradeFoodCostByLevel.Length
        )
        {
            return 0;
        }


        return Mathf.Max(
            0,
            upgradeFoodCostByLevel[index]
        );
    }


    // =========================================================
    // CUSTO DE OURO DO UPGRADE
    // =========================================================

    public int GetUpgradeGoldCost(
        int currentLevel
    )
    {
        if (
            currentLevel < 1 ||
            currentLevel >= MaxLevel
        )
        {
            return 0;
        }


        if (
            upgradeGoldCostByLevel == null ||
            upgradeGoldCostByLevel.Length == 0
        )
        {
            return 0;
        }


        int index =
            currentLevel - 1;


        if (
            index >=
            upgradeGoldCostByLevel.Length
        )
        {
            return 0;
        }


        return Mathf.Max(
            0,
            upgradeGoldCostByLevel[index]
        );
    }


    // =========================================================
    // TEMPO DO UPGRADE
    // =========================================================

    public float GetUpgradeTime(
        int currentLevel
    )
    {
        if (
            currentLevel < 1 ||
            currentLevel >= MaxLevel
        )
        {
            return 0f;
        }


        if (
            upgradeTimeByLevel == null ||
            upgradeTimeByLevel.Length == 0
        )
        {
            return 0f;
        }


        int index =
            currentLevel - 1;


        if (
            index >=
            upgradeTimeByLevel.Length
        )
        {
            return 0f;
        }


        return Mathf.Max(
            0.1f,
            upgradeTimeByLevel[index]
        );
    }


    // =========================================================
    // VERIFICAR SE POSSUI UPGRADE
    // =========================================================

    public bool HasUpgrade(
        int currentLevel
    )
    {
        return
            currentLevel >= 1 &&
            currentLevel < MaxLevel;
    }


    // =========================================================
    // PRÓXIMO NÍVEL
    // =========================================================

    public int GetNextLevel(
        int currentLevel
    )
    {
        if (
            !HasUpgrade(
                currentLevel
            )
        )
        {
            return currentLevel;
        }


        return currentLevel + 1;
    }
}