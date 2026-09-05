using UnityEngine;

/// <summary>
/// Representa uma construção individual existente no reino.
///
/// Cada construção possui:
/// - seu próprio BuildingData
/// - seu próprio nível
/// - seu próprio estado de construção
///
/// Exemplo:
/// Casa 1 = LVL 4
/// Casa 2 = LVL 1
/// Casa 3 = LVL 7
/// </summary>
public class BuildingInstance : MonoBehaviour
{
    [Header("Dados da construção")]
    [SerializeField]
    private BuildingData buildingData;

    [Header("Estado")]
    [SerializeField]
    private int level = 1;

    [SerializeField]
    private bool constructionFinished = true;


    // =========================================================
    // PROPRIEDADES
    // =========================================================

    public BuildingData Data => buildingData;

    public int Level => level;

    public bool ConstructionFinished => constructionFinished;


    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    public void Initialize(
        BuildingData data,
        int startingLevel = 1
    )
    {
        buildingData = data;

        if (buildingData != null)
        {
            level = Mathf.Clamp(
                startingLevel,
                1,
                buildingData.MaxLevel
            );
        }
        else
        {
            level = 1;
        }

        constructionFinished = true;
    }


    // =========================================================
    // CONSTRUÇÃO
    // =========================================================

    public void SetConstructionFinished(bool finished)
    {
        constructionFinished = finished;
    }


    // =========================================================
    // NÍVEL
    // =========================================================

    public bool CanUpgrade()
    {
        if (buildingData == null)
            return false;

        if (!constructionFinished)
            return false;

        return level < buildingData.MaxLevel;
    }


    public int GetNextLevel()
    {
        if (!CanUpgrade())
            return level;

        return level + 1;
    }


    public void SetLevel(int newLevel)
    {
        if (buildingData == null)
            return;

        level = Mathf.Clamp(
            newLevel,
            1,
            buildingData.MaxLevel
        );
    }


    // =========================================================
    // POPULAÇÃO
    // =========================================================

    public int GetPopulation()
    {
        if (buildingData == null)
            return 0;

        return buildingData.GetPopulation(level);
    }


    public int GetNextLevelPopulation()
    {
        if (!CanUpgrade())
            return GetPopulation();

        return buildingData.GetPopulation(
            level + 1
        );
    }


    // =========================================================
    // PRODUÇÃO DE MADEIRA
    // =========================================================

    public int GetWoodProductionPerMinute()
    {
        if (buildingData == null)
            return 0;

        return buildingData.GetWoodProductionPerMinute(
            level
        );
    }


    public int GetNextLevelWoodProductionPerMinute()
    {
        if (!CanUpgrade())
            return GetWoodProductionPerMinute();

        return buildingData.GetWoodProductionPerMinute(
            level + 1
        );
    }


    // =========================================================
    // PRODUÇÃO DE PEDRA
    // =========================================================

    public int GetStoneProductionPerMinute()
    {
        if (buildingData == null)
            return 0;

        return buildingData.GetStoneProductionPerMinute(
            level
        );
    }


    public int GetNextLevelStoneProductionPerMinute()
    {
        if (!CanUpgrade())
            return GetStoneProductionPerMinute();

        return buildingData.GetStoneProductionPerMinute(
            level + 1
        );
    }


    // =========================================================
    // CUSTOS DO PRÓXIMO UPGRADE
    // =========================================================

    public int GetUpgradeWoodCost()
    {
        if (buildingData == null)
            return 0;

        return buildingData.GetUpgradeWoodCost(level);
    }


    public int GetUpgradeStoneCost()
    {
        if (buildingData == null)
            return 0;

        return buildingData.GetUpgradeStoneCost(level);
    }


    public int GetUpgradeFoodCost()
    {
        if (buildingData == null)
            return 0;

        return buildingData.GetUpgradeFoodCost(level);
    }


    public int GetUpgradeGoldCost()
    {
        if (buildingData == null)
            return 0;

        return buildingData.GetUpgradeGoldCost(level);
    }


    // =========================================================
    // TEMPO DO PRÓXIMO UPGRADE
    // =========================================================

    public float GetUpgradeTime()
    {
        if (buildingData == null)
            return 0f;

        return buildingData.GetUpgradeTime(level);
    }


    // =========================================================
    // FINALIZAR UPGRADE
    // =========================================================

    public void CompleteUpgrade()
    {
        if (!CanUpgrade())
            return;

        level++;

        Debug.Log(
            "Upgrade concluído: " +
            buildingData.BuildingName +
            " | Novo nível: LVL " +
            level
        );
    }
}