using Entropek.Exceptions;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class CurrencyHudManager : MonoBehaviour
{


    public static CurrencyHudManager Singleton {get; private set;} = null;

    [SerializeField] CurrencyHud[] currencyHuds;


    /// 
    /// Base.
    /// 


    void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else if(Singleton != this)
        {
            throw new SingletonException();
        }    
    }

    void OnDestroy()
    {
        if (Singleton == this)
        {
            Singleton = null;
        }
    }


    ///
    /// Functions.
    /// 


    /// <summary>
    /// Links all stored CurrencyHuds's to a single Inventory instance.
    /// </summary>
    /// <param name="inventory"></param>

    public void LinkHudsToInventory(Inventory inventory)
    {
        for(int i = 0; i < currencyHuds.Length; i++)
        {
            currencyHuds[i].LinkToInventory(inventory);
        }
    }

    /// <summary>
    /// Unlinks all stored CurrencyHud's from their linked inventory.
    /// </summary>

    public void UnlinkHudsFromInventory()
    {
        for(int i = 0; i < currencyHuds.Length; i++)
        {
            currencyHuds[i].UnlinkFromInventory();
        }
    }

}
