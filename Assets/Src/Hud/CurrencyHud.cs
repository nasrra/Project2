using TMPro;
using UnityEngine;

public class CurrencyHud : MonoBehaviour
{

    
    [Header("Components")]
    [SerializeField] Currency currencyScriptableObject;
    [SerializeField] TextMeshProUGUI text;
    int currentAmount = 0;
    private Inventory inventory;


    /// 
    /// Base.
    /// 


    private void OnDestroy()
    {
        UnlinkFromInventory();
    }


    ///
    /// Functions.
    /// 


    /// <summary>
    /// Links this instance to a Inventory system for listening to Currency callbacks.
    /// </summary>
    /// <param name="inventory">The specified Inventory to link to.</param>
    /// <exception cref="System.Exception">Thrown when this instance is already linked to a Inventory.</exception>

    public void LinkToInventory(Inventory inventory)
    {
        if(this.inventory != null)
        {
            throw new System.Exception("CurrencyHud cannot link to more then one inventory at a time!");
        }

        this.inventory = inventory;

        inventory.CurrencyAdded += OnCurrencyAdded;
        inventory.CurrencyRemoved += OnCurrencyRemoved;
    }

    /// <summary>
    /// Unlinks the currently linked Inventory component from this instance.
    /// </summary>

    public void UnlinkFromInventory()
    {
        if(inventory == null)
        {
            return;
        }

        inventory.CurrencyAdded -= OnCurrencyAdded;
        inventory.CurrencyRemoved -= OnCurrencyRemoved;
        
        inventory = null;
    }

    private void OnCurrencyAdded(Currency currency, int amount)
    {
        if(currency == currencyScriptableObject)
        {
            currentAmount += amount;
            text.text = currentAmount.ToString();
        }
    }

    private void OnCurrencyRemoved(Currency currency, int amount)
    {
        if(currency == currencyScriptableObject)
        {
            currentAmount -= amount;
            text.text = currentAmount.ToString();
        }        
    }
}
