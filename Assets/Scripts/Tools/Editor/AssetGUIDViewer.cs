using UnityEditor;
using UnityEngine;

public static class AssetGUIDViewer
{
    [MenuItem("Assets/Show GUID", false, 2000)]
    private static void ShowAssetGUID()
    {
        // Get selected asset(s) in Project window
        var selectedObjects = Selection.objects;
        
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("[GUID Viewer] No assets selected!");
            return;
        }
        
        Debug.Log("=== ASSET GUID VIEWER ===");
        
        foreach (var obj in selectedObjects)
        {
            if (obj == null) continue;
            
            var assetPath = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            
            Debug.Log($"📁 Asset: {obj.name}");
            Debug.Log($"🔗 Path: {assetPath}");
            Debug.Log($"🆔 GUID: {guid}");
            Debug.Log($"📋 Type: {obj.GetType().Name}");
            Debug.Log("---");
        }
        
        Debug.Log($"Total assets: {selectedObjects.Length}");
    }
    
    [MenuItem("Assets/Show GUID", true)]
    private static bool ValidateShowAssetGUID()
    {
        // Only show menu item if we have selected assets
        return Selection.objects.Length > 0;
    }
    
    [MenuItem("GameObject/Show GUID", false, 0)]
    private static void ShowGameObjectGUID()
    {
        // Get selected GameObject(s) in Scene/Hierarchy
        var selectedGameObjects = Selection.gameObjects;
        
        if (selectedGameObjects.Length == 0)
        {
            Debug.LogWarning("[GUID Viewer] No GameObjects selected!");
            return;
        }
        
        Debug.Log("=== GAMEOBJECT GUID VIEWER ===");
        
        foreach (var go in selectedGameObjects)
        {
            if (go == null) continue;
            
            // For GameObjects in scene, we show prefab GUID if it's a prefab instance
            var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
            
            if (!string.IsNullOrEmpty(prefabAssetPath))
            {
                var prefabGuid = AssetDatabase.AssetPathToGUID(prefabAssetPath);
                Debug.Log($"🎮 GameObject: {go.name}");
                Debug.Log($"📦 Prefab Source: {prefabAssetPath}");
                Debug.Log($"🆔 Prefab GUID: {prefabGuid}");
                Debug.Log($"🌍 Scene Position: {go.transform.position}");
                
                // Also show connected prefab info
                var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
                if (prefabAsset != null)
                {
                    Debug.Log($"📋 Prefab Type: {prefabAsset.GetType().Name}");
                }
            }
            else
            {
                Debug.Log($"🎮 GameObject: {go.name}");
                Debug.Log($"⚠️ Not a prefab instance (scene-only object)");
                Debug.Log($"🌍 Scene Position: {go.transform.position}");
            }
            
            // Show component info
            var components = go.GetComponents<Component>();
            Debug.Log($"🔧 Components: {components.Length}");
            foreach (var comp in components)
            {
                if (comp != null)
                {
                    Debug.Log($"   - {comp.GetType().Name}");
                }
            }
            
            Debug.Log("---");
        }
        
        Debug.Log($"Total GameObjects: {selectedGameObjects.Length}");
    }
    
    [MenuItem("GameObject/Show GUID", true)]
    private static bool ValidateShowGameObjectGUID()
    {
        // Only show menu item if we have selected GameObjects
        return Selection.gameObjects.Length > 0;
    }
    
    [MenuItem("Assets/Copy GUID to Clipboard", false, 2001)]
    private static void CopyAssetGUIDToClipboard()
    {
        var selectedObject = Selection.activeObject;
        
        if (selectedObject == null)
        {
            Debug.LogWarning("[GUID Viewer] No asset selected!");
            return;
        }
        
        var assetPath = AssetDatabase.GetAssetPath(selectedObject);
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        
        EditorGUIUtility.systemCopyBuffer = guid;
        Debug.Log($"📋 Copied GUID to clipboard: {guid}");
        Debug.Log($"📁 Asset: {selectedObject.name}");
    }
    
    [MenuItem("Assets/Copy GUID to Clipboard", true)]
    private static bool ValidateCopyAssetGUIDToClipboard()
    {
        return Selection.activeObject != null;
    }
    
    [MenuItem("Assets/Show Asset Dependencies", false, 2002)]
    private static void ShowAssetDependencies()
    {
        var selectedObject = Selection.activeObject;
        
        if (selectedObject == null)
        {
            Debug.LogWarning("[GUID Viewer] No asset selected!");
            return;
        }
        
        var assetPath = AssetDatabase.GetAssetPath(selectedObject);
        var dependencies = AssetDatabase.GetDependencies(assetPath, false); // Direct dependencies only
        
        Debug.Log($"=== DEPENDENCIES FOR: {selectedObject.name} ===");
        Debug.Log($"📁 Asset Path: {assetPath}");
        Debug.Log($"🔗 Dependencies ({dependencies.Length}):");
        
        for (int i = 0; i < dependencies.Length; i++)
        {
            var depPath = dependencies[i];
            var depGuid = AssetDatabase.AssetPathToGUID(depPath);
            var depAsset = AssetDatabase.LoadAssetAtPath<Object>(depPath);
            
            Debug.Log($"   {i + 1}. {depAsset?.name ?? "Unknown"} ({depAsset?.GetType().Name ?? "Unknown"})");
            Debug.Log($"      Path: {depPath}");
            Debug.Log($"      GUID: {depGuid}");
        }
        
        if (dependencies.Length == 0)
        {
            Debug.Log("   (No dependencies)");
        }
    }
    
    [MenuItem("Assets/Show Asset Dependencies", true)]
    private static bool ValidateShowAssetDependencies()
    {
        return Selection.activeObject != null;
    }
    
    [MenuItem("Assets/Analyze Reference Types", false, 2003)]
    private static void AnalyzeReferenceTypes()
    {
        var selectedObject = Selection.activeObject;
        
        if (selectedObject == null)
        {
            Debug.LogWarning("[GUID Viewer] No asset selected!");
            return;
        }
        
        var assetPath = AssetDatabase.GetAssetPath(selectedObject);
        
        Debug.Log($"=== REFERENCE TYPE ANALYSIS: {selectedObject.name} ===");
        Debug.Log($"📁 Asset Path: {assetPath}");
        
        try {
            var content = System.IO.File.ReadAllText(assetPath);
            var lines = content.Split('\n');
            
            Debug.Log("🔍 Searching for references...");
            
            for (int i = 0; i < lines.Length; i++) {
                var line = lines[i];
                
                // Look for AssetReference patterns
                if (line.Contains("m_AssetGUID:")) {
                    var guid = ExtractGuidFromLine(line);
                    if (!string.IsNullOrEmpty(guid)) {
                        var referencedPath = AssetDatabase.GUIDToAssetPath(guid);
                        var referencedAsset = AssetDatabase.LoadAssetAtPath<Object>(referencedPath);
                        
                        Debug.Log($"📎 AssetReference found:");
                        Debug.Log($"   Target: {referencedAsset?.name ?? "Unknown"} ({referencedAsset?.GetType().Name ?? "Unknown"})");
                        Debug.Log($"   GUID: {guid}");
                        Debug.Log($"   Path: {referencedPath}");
                        Debug.Log($"   Line {i + 1}: {line.Trim()}");
                    }
                }
                
                // Look for Direct Reference patterns
                if (line.Contains("fileID:") && line.Contains("guid:")) {
                    var guid = ExtractGuidFromLine(line);
                    if (!string.IsNullOrEmpty(guid)) {
                        var referencedPath = AssetDatabase.GUIDToAssetPath(guid);
                        var referencedAsset = AssetDatabase.LoadAssetAtPath<Object>(referencedPath);
                        
                        Debug.Log($"🔗 Direct Reference found:");
                        Debug.Log($"   Target: {referencedAsset?.name ?? "Unknown"} ({referencedAsset?.GetType().Name ?? "Unknown"})");
                        Debug.Log($"   GUID: {guid}");
                        Debug.Log($"   Path: {referencedPath}");
                        Debug.Log($"   Line {i + 1}: {line.Trim()}");
                    }
                }
            }
            
            Debug.Log("✅ Analysis complete!");
            
        } catch (System.Exception e) {
            Debug.LogError($"❌ Error analyzing file: {e.Message}");
        }
    }
    
    [MenuItem("Assets/Analyze Reference Types", true)]
    private static bool ValidateAnalyzeReferenceTypes()
    {
        return Selection.activeObject != null;
    }
    
    private static string ExtractGuidFromLine(string line)
    {
        // Extract GUID from lines like:
        // m_AssetGUID: 7ede30aaacfd74896936186216911cb7
        // {fileID: 11400000, guid: 7ede30aaacfd74896936186216911cb7, type: 2}
        
        var guidStart = line.IndexOf("guid: ");
        if (guidStart == -1) {
            guidStart = line.IndexOf("m_AssetGUID: ");
            if (guidStart == -1) return null;
            guidStart += "m_AssetGUID: ".Length;
        } else {
            guidStart += "guid: ".Length;
        }
        
        var guidEnd = line.IndexOfAny(new char[] { ',', '}', ' ', '\n', '\r' }, guidStart);
        if (guidEnd == -1) {
            guidEnd = line.Length;
        }
        
        var guid = line.Substring(guidStart, guidEnd - guidStart).Trim();
        
        // Validate GUID format (32 hex characters)
        if (guid.Length == 32 && System.Text.RegularExpressions.Regex.IsMatch(guid, "^[a-fA-F0-9]+$")) {
            return guid;
        }
        
        return null;
    }
}
