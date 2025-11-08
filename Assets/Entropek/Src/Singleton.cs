using Entropek.Exceptions;
using UnityEngine;

namespace Entropek
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Singleton {get; private set;} = null;

        protected virtual void Awake()
        {
            if(Singleton == null)
            {
                Singleton = this as T;
            }
            else if(Singleton != this)
            {
                Destroy(gameObject);
                throw new SingletonException(GetType().ToString());
            }
        }

        protected virtual void OnDestroy()
        {
            if (Singleton == this)
            {
                Singleton = null;
            }
        }
    }    
}

