using System;
using System.Runtime.CompilerServices;

namespace Entropek.Exceptions
{

    public class ComponentNotFoundException : Exception
    {
        public ComponentNotFoundException(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string methodName = "")
        : base($"[ComponentNotFound][Class: {System.IO.Path.GetFileNameWithoutExtension(filePath)}][Method: {methodName}] {message}") { }
    }

}

