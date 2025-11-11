using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public event Action<ItemAddedContext> ItemAdded;
    public event Action<ItemRemovedContext> ItemRemoved;
    [RuntimeField] Dictionary<Currency, uint> Currencies = new();
    [RuntimeField] Dictionary<Item, uint> Items = new();

    /// <summary>
    /// Get the amount of a currenecy stored in this inventory.
    /// </summary>
    /// <param name="currency">The currency to query.</param>
    /// <returns>A uint value representing the amount of currency currently stored.</returns>

    public uint GetCurrencyAmount(Currency currency)
    {
        return Currencies.ContainsKey(currency) == true
        ? Currencies[currency]
        : 0; 
    }

    /// <summary>
    /// Adds a currency to this inventory.
    /// </summary>
    /// <param name="currency">The specified currency type.</param>
    /// <param name="amount">The amount to add.</param>

    public void AddCurrency(Currency currency, uint amount)
    {
        // short-circuit if no amount is to be added.

        if(amount == 0)
        {
            return;
        }

        if (Currencies.ContainsKey(currency) == false)
        {
            // add the currency as an entry if it is not in the inventory.

            Currencies.Add(currency, amount);
        }
        else
        {
            Currencies[currency] += amount;
        }
    }

    /// <summary>
    /// Removes a currency from this inventory.
    /// </summary>
    /// <param name="currency">The specified currency type.</param>
    /// <param name="amount">The amount to remove.</param>
    /// <returns>true when successful to remove the required amount; false if failed.</returns>

    public bool RemoveCurrency(Currency currency, uint amount)
    {
        // short-circuit if nothing is to be removed.

        if (amount == 0)
        {
            return true;
        }

        if(HasSufficientCurrency(currency, amount) == false)
        {
            return false;
        }

        // remove the specified amount; removing the currency entry if there is none left in this inventory.

        if((Currencies[currency] -= amount) <= 0)
        {
            Currencies.Remove(currency);
        }
        
        return true;
    }

    /// <summary>
    /// Checks whether this Inventory has a sufficient amount of currency.
    /// </summary>
    /// <param name="currency">The currency type.</param>
    /// <param name="amount">The amount required.</param>
    /// <returns>true if this inventory has the required amount; otherwise false.</returns>

    public bool HasSufficientCurrency(Currency currency, uint amount)
    {            
        // fail if this inventory does not contain the currency.

        if(Currencies.ContainsKey(currency) == false)
        {
            if (amount == 0)
            {
                return true;
            }

            return false;
        }

        // fail if there is not enough of the currency in this inventory.

        if(Currencies[currency] < amount)
        {
            return false;
        }

        return true;   
    }

    public bool RemoveItem(Item item, uint amount)
    {
        if(Items.ContainsKey(item) == true)
        {
            uint storedAmount = Items[item];
            storedAmount -= amount;
            
            // check if the item can be removed.

            if(storedAmount < 0)
            {
                return false;
            }

            // remove the item.

            if(storedAmount == 0)
            {
                Items.Remove(item);
            }
            else
            {
                Items[item] = storedAmount;
            }
            
            ItemRemoved?.Invoke(new ItemRemovedContext(item, amount));

            return true;
        }
        else
        {
            return false;
        }
    }


    public void AddItem(Item item, uint amount)
    {
        
        // Add item.

        if (Items.ContainsKey(item))
        {
            Items[item] += amount;
        }
        else
        {
            Items.Add(item, amount);
        }

        ItemAdded?.Invoke(new ItemAddedContext(item, amount));
    }
}    

