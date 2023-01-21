using System.Reflection;
using UnityEditor;

class UsefulShortcuts
{
    [MenuItem("Tools/Clear Console %/")] // CMD + SHIFT + C
    static void ClearConsole()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var clearMethod = type.GetMethod("Clear");
        clearMethod?.Invoke(null, null);
    }
}