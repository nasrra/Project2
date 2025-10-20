using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Entropek.Logger.Exceptions{

public class SingletonException : Exception{
    public SingletonException([CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "")
    : base($"In class '{System.IO.Path.GetFileNameWithoutExtension(filePath)}' during method call '{methodName}'"){}    
}

}
