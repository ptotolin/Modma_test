using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class DebugLogOnGUI : SingletonMonobehaviour<DebugLogOnGUI>
{
    private Dictionary<string, Func<object>> watchedVariables = new();
    private bool initialized;
    private GUIStyle style;

    public void Initialize(int fontSize)
    {
        initialized = true;
        
        style = new GUIStyle {
            fontSize = fontSize
        };
    }

    public void WatchVariable(string name, Func<object> getter)
    {
        watchedVariables[name] = getter;
    }
    
    public void UnwatchVariable(string name)
    {
        watchedVariables.Remove(name);
    }
    
    private void OnGUI()
    {
        var sb = new StringBuilder();
        var y = 10;
        foreach (var kvp in watchedVariables) {
            try {
                object value = kvp.Value();
                sb.AppendLineFormat("{0}: {1}\n", kvp.Key, value);
            }
            catch {
                sb.AppendLineFormat("{0}: ERROR\n", kvp.Key);
            }
        }
        var str = sb.ToString();
        GUI.Label(new Rect(10, 150, 200, 30), $"{str}", style);
    }
}