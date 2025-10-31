using System.Collections.Generic;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.InventorySystem
{
    public class Inventory : MonoBehaviour
    {
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
    }    
}

