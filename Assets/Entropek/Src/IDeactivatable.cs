using System;
using UnityEngine;

namespace Entropek
{
    public interface IDeactivatable
    {   
        public event Action Activated;
        public event Action Deactivated;

        void Deactivate();
        void Activate();
    }    
}

