using System;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Entropek.Logger{
public static class Log{

    public static NotImplementedException MethodNotImplemented(object instance, [CallerMemberName] string methodName = "") 
        => new NotImplementedException($"'{instance.GetType().Name}' class has not implemented: '{methodName}'.");

    public static void MethodCall([CallerFilePath] string filePath = "", [CallerMemberName] string methodName = ""){
        string className = System.IO.Path.GetFileNameWithoutExtension(filePath);
        Debug.Log($"[MethodCall]: {className} : {methodName}");
    }

    public static void MethodCall(Action methodToCall, [CallerFilePath] string filePath = ""){
        string methodName = methodToCall.Method.Name;
        string className = System.IO.Path.GetFileNameWithoutExtension(filePath);
        Debug.Log($"[MethodCall]: {className} : {methodName}");
        // Call the passed method
        methodToCall?.Invoke();
    }
}
}

