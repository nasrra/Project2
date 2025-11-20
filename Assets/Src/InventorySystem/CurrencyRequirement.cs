using System;
using UnityEngine;

[Serializable]
public class CurrencyRequirement
{
    [Serializable]
    private enum EvaluateType : byte
    {
        Remove,
        Contains
    }

    [SerializeField] Currency currencyType;
    [SerializeField] int requiredAmount;
    [SerializeField] EvaluateType evaluateType;

    /// <summary>
    /// Evaluates whether the Currency requirement of this instance is met.
    /// </summary>
    /// <param name="inventoryGameObject">The gameobject with an Inventory component to evaluate against.</param>
    /// <returns>true, if the requirement was met; otherwise false.</returns>

    public bool FullfillRequirement(GameObject inventoryGameObject)
    {
        return FullfillRequirement(inventoryGameObject.GetComponent<Inventory>());
    }

    /// <summary>
    /// Evaluates whether the Currency requirement of this instance is met.
    /// </summary>
    /// <param name="inventory">The Inventory to evaulate against.</param>
    /// <returns>true, if the requirement was met; otherwise false.</returns>

    public bool FullfillRequirement(Inventory inventory)
    {
        switch (evaluateType)
        {
            case EvaluateType.Remove:
                return inventory.RemoveCurrency(currencyType, requiredAmount);
            case EvaluateType.Contains:
                return inventory.HasSufficientCurrency(currencyType, requiredAmount);
            default:
                return false;
        }
    }
}
