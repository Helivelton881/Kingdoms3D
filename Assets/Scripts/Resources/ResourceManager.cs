using System;
using UnityEngine;

public enum ResourceType
{
    Wood,
    Stone,
    Food,
    Gold
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Recursos Iniciais - Valores Temporários")]
    [SerializeField] private int startingWood = 1000;
    [SerializeField] private int startingStone = 1000;
    [SerializeField] private int startingFood = 1000;
    [SerializeField] private int startingGold = 500;

    [Header("Recursos Atuais")]
    [SerializeField] private int wood;
    [SerializeField] private int stone;
    [SerializeField] private int food;
    [SerializeField] private int gold;

    public int Wood => wood;
    public int Stone => stone;
    public int Food => food;
    public int Gold => gold;

    /// <summary>
    /// Disparado sempre que algum recurso é alterado.
    /// O painel superior de recursos poderá usar este evento futuramente.
    /// </summary>
    public event Action OnResourcesChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeResources();
    }

    private void InitializeResources()
    {
        wood = Mathf.Max(0, startingWood);
        stone = Mathf.Max(0, startingStone);
        food = Mathf.Max(0, startingFood);
        gold = Mathf.Max(0, startingGold);

        NotifyResourcesChanged();
    }

    // =========================================================
    // CONSULTA
    // =========================================================

    public int GetAmount(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Wood:
                return wood;

            case ResourceType.Stone:
                return stone;

            case ResourceType.Food:
                return food;

            case ResourceType.Gold:
                return gold;

            default:
                return 0;
        }
    }

    // =========================================================
    // VERIFICAÇÃO DE RECURSOS
    // =========================================================

    public bool CanAfford(
        int woodCost,
        int stoneCost,
        int foodCost,
        int goldCost)
    {
        if (woodCost < 0 ||
            stoneCost < 0 ||
            foodCost < 0 ||
            goldCost < 0)
        {
            return false;
        }

        return wood >= woodCost &&
               stone >= stoneCost &&
               food >= foodCost &&
               gold >= goldCost;
    }

    // =========================================================
    // GASTAR RECURSOS
    // =========================================================

    public bool TrySpend(
        int woodCost,
        int stoneCost,
        int foodCost,
        int goldCost)
    {
        if (!CanAfford(
                woodCost,
                stoneCost,
                foodCost,
                goldCost))
        {
            return false;
        }

        wood -= woodCost;
        stone -= stoneCost;
        food -= foodCost;
        gold -= goldCost;

        NotifyResourcesChanged();

        return true;
    }

    // =========================================================
    // ADICIONAR RECURSOS
    // =========================================================

    public void AddResource(ResourceType resourceType, int amount)
    {
        if (amount <= 0)
            return;

        switch (resourceType)
        {
            case ResourceType.Wood:
                wood += amount;
                break;

            case ResourceType.Stone:
                stone += amount;
                break;

            case ResourceType.Food:
                food += amount;
                break;

            case ResourceType.Gold:
                gold += amount;
                break;
        }

        NotifyResourcesChanged();
    }

    public void AddResources(
        int woodAmount,
        int stoneAmount,
        int foodAmount,
        int goldAmount)
    {
        if (woodAmount > 0)
            wood += woodAmount;

        if (stoneAmount > 0)
            stone += stoneAmount;

        if (foodAmount > 0)
            food += foodAmount;

        if (goldAmount > 0)
            gold += goldAmount;

        NotifyResourcesChanged();
    }

    // =========================================================
    // DEFINIR RECURSO
    // =========================================================

    public void SetResource(ResourceType resourceType, int amount)
    {
        amount = Mathf.Max(0, amount);

        switch (resourceType)
        {
            case ResourceType.Wood:
                wood = amount;
                break;

            case ResourceType.Stone:
                stone = amount;
                break;

            case ResourceType.Food:
                food = amount;
                break;

            case ResourceType.Gold:
                gold = amount;
                break;
        }

        NotifyResourcesChanged();
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetResources()
    {
        InitializeResources();
    }

    // =========================================================
    // EVENTO
    // =========================================================

    private void NotifyResourcesChanged()
    {
        OnResourcesChanged?.Invoke();
    }
}