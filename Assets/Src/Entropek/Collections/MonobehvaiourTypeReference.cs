using System;
using UnityEngine;


namespace Entropek.Collections{


[Serializable]
public class MonobehvaiourTypeReference{
    [SerializeField] private string typeName;
    private Type type;
    public Type Type{
        get{
            if(type ==null){
                type = Type.GetType(typeName);
            }
            return type;
        }
    }
}


}
