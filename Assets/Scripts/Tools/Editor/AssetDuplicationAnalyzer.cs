using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public class AssetDuplicationAnalyzer : EditorWindow
{
    private Vector2 scrollPosition;
    private Dictionary<string, DuplicationAnalysis> analysisResults;
    private bool isAnalyzing = false;
    
    // Filter options
    private int selectedReasonFilter = 0;
    private int selectedTypeFilter = 0;
    private bool showFilters = true;
    
    // Cache for performance optimization
    private string[] _allProjectAssets;

    [System.Serializable]
    public class DuplicationAnalysis
    {
        public string assetName;
        public string assetType;
        public string guid;
        public long fileSize;
        public List<DuplicationSource> sources = new List<DuplicationSource>();
        public List<DependencyChain> dependencyChains = new List<DependencyChain>();
        public DuplicationReason reason;
    }

    [System.Serializable]
    public class DuplicationSource
    {
        public SourceType type;
        public string location;
        public string details;
    }

    [System.Serializable]
    public class DependencyChain
    {
        public string rootAsset;
        public List<string> chain = new List<string>();
        public SourceType sourceType;
    }

    public enum SourceType
    {
        DirectReference, // Direct reference in prefab/scene
        AddressableAsset, // Addressable asset
        BuildInclusion, // Automatically included in build
        ResourcesFolder // In Resources folder
    }

    public enum DuplicationReason
    {
        DirectAndAddressable, // Has both direct references and Addressable
        MultipleAddressableGroups, // In multiple Addressable groups
        ResourcesAndAddressable, // In Resources and Addressable
        BuildDependency, // Pulled in as build dependency
        SceneDependency, // Scene dependency
        Unknown
    }

    [MenuItem("Tools/Asset Duplication Analyzer")]
    public static void ShowWindow()
    {
        GetWindow<AssetDuplicationAnalyzer>("Asset Duplication Analyzer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Asset Duplication Analyzer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Analyze Project")) {
            AnalyzeProject();
        }

        EditorGUILayout.Space();

        if (isAnalyzing) {
            EditorGUILayout.LabelField("Analyzing...", EditorStyles.helpBox);
            return;
        }

        if (analysisResults != null && analysisResults.Count > 0) {
            DisplayResults();
        }
    }

    private void AnalyzeProject()
    {
        isAnalyzing = true;
        analysisResults = new Dictionary<string, DuplicationAnalysis>();

        try {
            // Initialize cache once for all methods - huge performance improvement!
            _allProjectAssets = AssetDatabase.FindAssets("t:GameObject t:ScriptableObject t:Scene");
            Debug.Log($"Found {_allProjectAssets.Length} project assets to analyze");
            
            // 1. Register all assets (without sources)
            ScanProjectAssets();

            // 2. Add Addressables-only assets (without sources)  
            ScanAddressableAssets();

            // 3. ONLY place where we determine sources
            AnalyzeSources();

            // 4. ✅ Analyze dependencies
            AnalyzeDependencies();

            // 5. Determine duplication reasons
            DetermineReasons();

            Debug.Log($"Analysis complete. Found {analysisResults.Count(x => x.Value.sources.Count > 1)} duplicated assets.");
        }
        catch (Exception e) {
            Debug.LogError($"Analysis failed: {e.Message}");
        }
        finally {
            // Clear cache after analysis to free memory
            _allProjectAssets = null;
            isAnalyzing = false;
        }
    }

    private void ScanProjectAssets()
    {
        var allAssets = AssetDatabase.FindAssets("");

        foreach (var guid in allAssets) {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Skip system files
            if (assetPath.StartsWith("Packages/") ||
                assetPath.StartsWith("ProjectSettings/") ||
                assetPath.EndsWith(".meta") ||
                AssetDatabase.IsValidFolder(assetPath))
                continue;

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null) continue;

            // ✅ ONLY create entry - DO NOT determine sources!
            if (!analysisResults.ContainsKey(guid)) {
                analysisResults[guid] = new DuplicationAnalysis
                {
                    assetName = asset.name,
                    assetType = asset.GetType().Name,
                    guid = guid,
                    fileSize = GetFileSize(assetPath)
                    // sources = empty list!
                };
            }
        }
    }

    private void ScanAddressableAssets()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;

        foreach (var group in settings.groups) {
            foreach (var entry in group.entries) {
                var guid = entry.guid;

                // ✅ Create entry if it doesn't exist (Addressables-only assets)
                if (!analysisResults.ContainsKey(guid)) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

                    analysisResults[guid] = new DuplicationAnalysis
                    {
                        assetName = asset?.name ?? "Unknown",
                        assetType = asset?.GetType().Name ?? "Unknown",
                        guid = guid,
                        fileSize = GetFileSize(assetPath)
                    };
                }

                // ✅ DO NOT add sources here!
            }
        }
    }

    private void AnalyzeSources()
    {
        foreach (var kvp in analysisResults) {
            var guid = kvp.Key;
            var analysis = kvp.Value;
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Check each source type ONCE

            // Resources?
            if (assetPath.StartsWith("Assets/Resources/")) {
                analysis.sources.Add(new DuplicationSource
                {
                    type = SourceType.ResourcesFolder,
                    location = assetPath,
                    details = "Located in Resources folder - automatically included in build"
                });
            }

            // Addressable?
            var addressableInfo = GetAddressableInfo(guid);
            if (addressableInfo != null) {
                analysis.sources.Add(new DuplicationSource
                {
                    type = SourceType.AddressableAsset,
                    location = $"Addressables: {addressableInfo.groupName}",
                    details = $"Address: {addressableInfo.address}, Group: {addressableInfo.groupName}"
                });
            }

            // Direct Reference? (only count references that actually cause build inclusion)
            var buildReferencingAssets = FindBuildIncludingReferences(assetPath);
            if (buildReferencingAssets.Any()) {
                analysis.sources.Add(new DuplicationSource
                {
                    type = SourceType.DirectReference,
                    location = "Project References",
                    details = $"Referenced by build-included assets: {string.Join(", ", buildReferencingAssets.Take(3))}"
                });
            }

            // If no sources - then BuildInclusion
            if (!analysis.sources.Any()) {
                analysis.sources.Add(new DuplicationSource
                {
                    type = SourceType.BuildInclusion,
                    location = "Build Dependencies",
                    details = "Automatically included as build dependency"
                });
            }
        }
    }

    private class AddressableInfo
    {
        public string groupName;
        public string address;
        public bool isLocal;
    }

    private AddressableInfo GetAddressableInfo(string guid)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return null;

        foreach (var group in settings.groups) {
            if (group == null) continue;

            foreach (var entry in group.entries) {
                if (entry.guid == guid) {
                    return new AddressableInfo
                    {
                        groupName = group.Name,
                        address = entry.address,
                        isLocal = IsLocalGroup(group)
                    };
                }
            }
        }

        return null; // Not found in Addressables
    }

    private bool IsLocalGroup(AddressableAssetGroup group)
    {
        // Check group schema to understand Local or Remote
        var bundledAssetGroupSchema = group.GetSchema<BundledAssetGroupSchema>();
        if (bundledAssetGroupSchema != null) {
            // If BuildPath contains Local - then it's a local group
            var buildPath = bundledAssetGroupSchema.BuildPath.GetValue(group.Settings);
            return buildPath.Contains("Local") ||
                   buildPath.Contains("{UnityEngine.AddressableAssets.Addressables.BuildPath}");
        }

        return true; // Consider local by default
    }

    private void AnalyzeDependencies()
    {
        foreach (var kvp in analysisResults) {
            var guid = kvp.Key;
            var analysis = kvp.Value;
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Find what references this asset
            var referencingAssets = FindReferencingAssets(assetPath);

            foreach (var refAsset in referencingAssets) {
                var chain = BuildDependencyChain(refAsset, assetPath);
                if (chain != null) {
                    analysis.dependencyChains.Add(chain);
                }
            }
        }
    }

    private void DetermineReasons()
    {
        foreach (var analysis in analysisResults.Values) {
            if (analysis.sources.Count <= 1) {
                continue; // Not duplicated
            }

            var hasDirectRef = analysis.sources.Any(s => s.type == SourceType.DirectReference);
            var hasAddressable = analysis.sources.Any(s => s.type == SourceType.AddressableAsset);
            var hasResources = analysis.sources.Any(s => s.type == SourceType.ResourcesFolder);
            var addressableCount = analysis.sources.Count(s => s.type == SourceType.AddressableAsset);

            if (hasDirectRef && hasAddressable) {
                analysis.reason = DuplicationReason.DirectAndAddressable;
            }
            else if (hasResources && hasAddressable) {
                analysis.reason = DuplicationReason.ResourcesAndAddressable;
            }
            else if (addressableCount > 1) {
                analysis.reason = DuplicationReason.MultipleAddressableGroups;
            }
            else if (analysis.dependencyChains.Any()) {
                analysis.reason = DuplicationReason.BuildDependency;
            }
            else {
                analysis.reason = DuplicationReason.Unknown;
            }
        }
    }

    private SourceType DetermineSourceType(string assetPath)
    {
        if (assetPath.StartsWith("Assets/Resources/")) {
            return SourceType.ResourcesFolder;
        }

        // Check if there are build-including references
        var buildReferencingAssets = FindBuildIncludingReferences(assetPath);
        if (buildReferencingAssets.Any()) {
            return SourceType.DirectReference;
        }

        // Check if asset is Addressable
        if (IsAddressableAsset(assetPath)) {
            return SourceType.AddressableAsset;
        }

        return SourceType.BuildInclusion;
    }

    private bool IsAddressableAsset(string assetPath)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return false;

        var guid = AssetDatabase.AssetPathToGUID(assetPath);

        foreach (var group in settings.groups) {
            if (group == null) continue;

            foreach (var entry in group.entries) {
                if (entry.guid == guid)
                    return true;
            }
        }

        return false;
    }

    private string GetSourceDetails(string assetPath, SourceType sourceType)
    {
        switch (sourceType) {
            case SourceType.ResourcesFolder:
                return "Located in Resources folder - automatically included in build";

            case SourceType.DirectReference:
                var refs = FindBuildIncludingReferences(assetPath);
                return $"Referenced by build-included assets: {string.Join(", ", refs.Take(3))}";

            case SourceType.AddressableAsset:
                var groupInfo = GetAddressableGroupInfo(assetPath);
                return $"Addressable asset in group: {groupInfo.groupName}, Address: {groupInfo.address}";

            case SourceType.BuildInclusion:
                return "Automatically included in build as dependency";

            default:
                return "Included in build";
        }
    }

    private (string groupName, string address) GetAddressableGroupInfo(string assetPath)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return ("Unknown", "Unknown");

        var guid = AssetDatabase.AssetPathToGUID(assetPath);

        foreach (var group in settings.groups) {
            if (group == null) continue;

            foreach (var entry in group.entries) {
                if (entry.guid == guid) {
                    return (group.Name, entry.address);
                }
            }
        }

        return ("Not Found", "Not Found");
    }

    private List<string> FindReferencingAssets(string targetPath)
    {
        var references = new List<string>();
        var targetGuid = AssetDatabase.AssetPathToGUID(targetPath);

        // Use cached assets instead of calling FindAssets again - performance boost!
        var allAssets = _allProjectAssets ?? AssetDatabase.FindAssets("t:GameObject t:ScriptableObject t:Scene");

        foreach (var guid in allAssets) {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var dependencies = AssetDatabase.GetDependencies(assetPath, false);

            if (dependencies.Contains(targetPath)) {
                references.Add(assetPath);
            }
        }

        return references;
    }
    
    private List<string> FindBuildIncludingReferences(string targetPath)
    {
        var references = new List<string>();
        var targetGuid = AssetDatabase.AssetPathToGUID(targetPath);

        // Use cached assets instead of calling FindAssets again - performance boost!
        var allAssets = _allProjectAssets ?? AssetDatabase.FindAssets("t:GameObject t:ScriptableObject t:Scene");

        foreach (var guid in allAssets) {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // Skip script files and other non-asset files
            if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".js") || assetPath.EndsWith(".dll")) {
                continue;
            }
            
            // Skip if this asset uses AssetReference to target (not direct reference)
            if (UsesAssetReference(assetPath, targetPath)) {
                continue; // AssetReference doesn't cause build inclusion
            }
            
            var dependencies = AssetDatabase.GetDependencies(assetPath, false);

            if (dependencies.Contains(targetPath)) {
                // Only include if the referencing asset itself causes build inclusion
                if (WillAssetBeIncludedInBuild(assetPath)) {
                    references.Add(assetPath);
                }
            }
        }

        return references;
    }
    
    private bool WillAssetBeIncludedInBuild(string assetPath)
    {
        // Resources folder - always included
        if (assetPath.StartsWith("Assets/Resources/")) {
            return true;
        }
        
        // Scene files - always included if in build settings
        if (assetPath.EndsWith(".unity")) {
            return IsSceneInBuildSettings(assetPath);
        }
        
        // Addressables - NOT automatically included in build
        if (IsAddressableAsset(assetPath)) {
            return false;
        }
        
        // Check if referenced by build-included assets (recursive check with depth limit)
        return HasBuildIncludingDependency(assetPath, maxDepth: 3);
    }
    
    private bool IsSceneInBuildSettings(string scenePath)
    {
        var buildScenes = EditorBuildSettings.scenes;
        return buildScenes.Any(scene => scene.path == scenePath && scene.enabled);
    }
    
    private bool HasBuildIncludingDependency(string assetPath, int maxDepth, HashSet<string> visited = null)
    {
        if (maxDepth <= 0) return false;
        
        visited ??= new HashSet<string>();
        if (visited.Contains(assetPath)) return false; // Prevent infinite recursion
        visited.Add(assetPath);
        
        var referencingAssets = FindDirectReferencingAssets(assetPath);
        
        foreach (var refAsset in referencingAssets) {
            // Direct build inclusion
            if (refAsset.StartsWith("Assets/Resources/") || 
                (refAsset.EndsWith(".unity") && IsSceneInBuildSettings(refAsset))) {
                return true;
            }
            
            // Recursive check (but skip Addressables)
            if (!IsAddressableAsset(refAsset) && 
                HasBuildIncludingDependency(refAsset, maxDepth - 1, visited)) {
                return true;
            }
        }
        
        return false;
    }
    
    private List<string> FindDirectReferencingAssets(string targetPath)
    {
        var references = new List<string>();
        var allAssets = _allProjectAssets ?? AssetDatabase.FindAssets("t:GameObject t:ScriptableObject t:Scene");

        foreach (var guid in allAssets) {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // Skip script files and other non-asset files
            if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".js") || assetPath.EndsWith(".dll")) {
                continue;
            }
            
            // Skip if this asset uses AssetReference to target (not direct reference)
            if (UsesAssetReference(assetPath, targetPath)) {
                continue; // AssetReference doesn't cause build inclusion
            }
            
            var dependencies = AssetDatabase.GetDependencies(assetPath, false);
            if (dependencies.Contains(targetPath)) {
                references.Add(assetPath);
            }
        }

        return references;
    }
    
    private bool UsesAssetReference(string assetPath, string targetPath)
    {
        // Skip script files - they don't contain asset references
        if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".js") || assetPath.EndsWith(".dll")) {
            return false;
        }
        
        // Check the actual reference pattern in the file
        var targetGuid = AssetDatabase.AssetPathToGUID(targetPath);
        
        try {
            var content = System.IO.File.ReadAllText(assetPath);
            
            // AssetReference pattern: m_AssetGUID: without fileID
            bool hasAssetReference = content.Contains($"m_AssetGUID: {targetGuid}");
            
            // Direct reference pattern: fileID + guid
            bool hasDirectReference = content.Contains($"fileID:") && content.Contains($"guid: {targetGuid}");
            
            // If both exist, it's mixed - treat as direct reference
            if (hasAssetReference && hasDirectReference) {
                return false; // Mixed → treat as direct reference
            }
            
            // Pure AssetReference
            if (hasAssetReference && !hasDirectReference) {
                return true;
            }
            
            // Default: direct reference
            return false;
        }
        catch (System.Exception e) {
            Debug.LogError($"[Analyzer] Error reading {assetPath}: {e.Message}");
            return false; // Fallback to direct reference
        }
    }

    private DependencyChain BuildDependencyChain(string rootAsset, string targetAsset)
    {
        // Simplified version - can be extended for full chain building
        return new DependencyChain
        {
            rootAsset = rootAsset,
            chain = new List<string> { rootAsset, targetAsset },
            sourceType = DetermineSourceType(rootAsset)
        };
    }

private void DisplayResults()
{
    // Filter controls
    EditorGUILayout.BeginVertical("box");
    showFilters = EditorGUILayout.Foldout(showFilters, "🔍 Filters", true);
    
    if (showFilters)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Reason filter
        EditorGUILayout.LabelField("Filter by Reason:", GUILayout.Width(120));
        var reasonOptions = GetReasonFilterOptions();
        selectedReasonFilter = EditorGUILayout.Popup(selectedReasonFilter, reasonOptions);
        
        EditorGUILayout.Space();
        
        // Type filter
        EditorGUILayout.LabelField("Filter by Type:", GUILayout.Width(100));
        var typeOptions = GetTypeFilterOptions();
        selectedTypeFilter = EditorGUILayout.Popup(selectedTypeFilter, typeOptions);
        
        EditorGUILayout.EndHorizontal();
        
        // Clear filters button
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Clear Filters", GUILayout.Width(100)))
        {
            selectedReasonFilter = 0;
            selectedTypeFilter = 0;
        }
        EditorGUILayout.EndHorizontal();
    }
    EditorGUILayout.EndVertical();
    EditorGUILayout.Space();
    
    var duplicatedAssets = GetFilteredResults();
    
    // ✅ Size statistics
    var totalDuplicatedSize = duplicatedAssets.Sum(x => x.Value.fileSize);
    var totalProjectSize = analysisResults.Sum(x => x.Value.fileSize);
    var wastedPercentage = totalProjectSize > 0 ? (totalDuplicatedSize * 100.0f / totalProjectSize) : 0;
    
    // Header with sizes
    EditorGUILayout.BeginVertical("box");
    EditorGUILayout.LabelField($"Found {duplicatedAssets.Count} duplicated assets:", EditorStyles.boldLabel);
    
    var oldColor = GUI.color;
    GUI.color = Color.red;
    EditorGUILayout.LabelField($"💾 Wasted Space: {FormatFileSize(totalDuplicatedSize)} ({wastedPercentage:F1}% of project)", EditorStyles.boldLabel);
    GUI.color = oldColor;
    
    EditorGUILayout.LabelField($"📊 Total Project Size: {FormatFileSize(totalProjectSize)}");
    EditorGUILayout.LabelField($"💰 Potential Savings: {FormatFileSize(totalDuplicatedSize)}");
    EditorGUILayout.EndVertical();
    
    EditorGUILayout.Space();
    
    // Show statistics only if not filtered or show filtered stats
    if (selectedReasonFilter == 0 && selectedTypeFilter == 0)
    {
        // Grouping by types
        var byType = duplicatedAssets
            .GroupBy(x => x.Value.assetType)
            .OrderByDescending(g => g.Sum(x => x.Value.fileSize))
            .ToList();
        
        if (byType.Any())
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📈 Duplication by Asset Type:", EditorStyles.boldLabel);
            
            foreach (var group in byType.Take(5))
            {
                var typeSize = group.Sum(x => x.Value.fileSize);
                var typeCount = group.Count();
                
                EditorGUILayout.LabelField($"  • {group.Key}: {typeCount} assets, {FormatFileSize(typeSize)}");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        // Grouping by reasons
        var byReason = duplicatedAssets
            .GroupBy(x => x.Value.reason)
            .OrderByDescending(g => g.Count())
            .ToList();
        
        if (byReason.Any())
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🎯 Duplication by Reason:", EditorStyles.boldLabel);
            
            foreach (var group in byReason)
            {
                var reasonSize = group.Sum(x => x.Value.fileSize);
                var reasonCount = group.Count();
                
                EditorGUILayout.LabelField($"  • {GetReasonDescription(group.Key)}: {reasonCount} assets, {FormatFileSize(reasonSize)}");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
    else
    {
        // Show filtered info
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🔍 Filtered Results:", EditorStyles.boldLabel);
        
        var reasonFilter = selectedReasonFilter > 0 ? GetReasonFilterOptions()[selectedReasonFilter] : "All";
        var typeFilter = selectedTypeFilter > 0 ? GetTypeFilterOptions()[selectedTypeFilter] : "All";
        
        EditorGUILayout.LabelField($"Reason: {reasonFilter}");
        EditorGUILayout.LabelField($"Type: {typeFilter}");
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    // List of duplicated assets
    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
    
    foreach (var kvp in duplicatedAssets)
    {
        var analysis = kvp.Value;
        
        EditorGUILayout.BeginVertical("box");
        
        // Header with severity icon
        var severityColor = GetSeverityColor(analysis.reason);
        var oldAssetColor = GUI.color;
        GUI.color = severityColor;
        
        EditorGUILayout.LabelField($"🔥 {analysis.assetName} ({analysis.assetType})", EditorStyles.boldLabel);
        GUI.color = oldAssetColor;
        
        EditorGUILayout.LabelField($"Reason: {GetReasonDescription(analysis.reason)}", EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Size: {FormatFileSize(analysis.fileSize)}");
        
        // Sources
        EditorGUILayout.LabelField("Sources:", EditorStyles.miniBoldLabel);
        foreach (var source in analysis.sources)
        {
            EditorGUILayout.LabelField($"  • {source.type}: {source.location}");
            if (!string.IsNullOrEmpty(source.details))
            {
                EditorGUILayout.LabelField($"    {source.details}", EditorStyles.miniLabel);
            }
        }
        
        // Dependency chains
        if (analysis.dependencyChains.Any())
        {
            EditorGUILayout.LabelField("Dependency Chains:", EditorStyles.miniBoldLabel);
            foreach (var chain in analysis.dependencyChains.Take(3))
            {
                EditorGUILayout.LabelField($"  • {chain.rootAsset} → ... → {analysis.assetName}");
            }
            
            if (analysis.dependencyChains.Count > 3)
            {
                EditorGUILayout.LabelField($"    ... and {analysis.dependencyChains.Count - 3} more chains", EditorStyles.miniLabel);
            }
        }
        
        // Action buttons
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Select Asset"))
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(analysis.guid);
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }
        
        if (GUILayout.Button("Show in Project"))
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(analysis.guid);
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset != null)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }
        
        if (GUILayout.Button("Copy Path"))
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(analysis.guid);
            EditorGUIUtility.systemCopyBuffer = assetPath;
            Debug.Log($"Copied to clipboard: {assetPath}");
        }
        
        if (GUILayout.Button("Fix Automatically"))
        {
            FixDuplication(analysis);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    EditorGUILayout.EndScrollView();
    
    // Action buttons at bottom
    EditorGUILayout.Space();
    EditorGUILayout.BeginHorizontal();
    
    if (GUILayout.Button("Fix All Addressable Duplicates"))
    {
        FixAllAddressableDuplicates();
    }
    
    if (GUILayout.Button("Export Report"))
    {
        ExportReport();
    }
    
    if (GUILayout.Button("Refresh Analysis"))
    {
        AnalyzeProject();
    }
    
    EditorGUILayout.EndHorizontal();
}

private void FixAllAddressableDuplicates()
{
    var addressableDuplicates = analysisResults.Values
        .Where(x => x.sources.Count > 1 && x.reason == DuplicationReason.DirectAndAddressable)
        .ToList();
    
    foreach (var duplicate in addressableDuplicates)
    {
        FixDuplication(duplicate);
    }
    
    Debug.Log($"Attempted to fix {addressableDuplicates.Count} Addressable duplicates");
}

private void ExportReport()
{
    var duplicatedAssets = analysisResults.Where(x => x.Value.sources.Count > 1).ToList();
    var reportPath = EditorUtility.SaveFilePanel("Export Duplication Report", "", "duplication_report.txt", "txt");
    
    if (!string.IsNullOrEmpty(reportPath))
    {
        try
        {
            using (var writer = new System.IO.StreamWriter(reportPath))
            {
                writer.WriteLine("Asset Duplication Report");
                writer.WriteLine($"Generated: {System.DateTime.Now}");
                writer.WriteLine($"Found {duplicatedAssets.Count} duplicated assets");
                writer.WriteLine();
                
                foreach (var kvp in duplicatedAssets)
                {
                    var analysis = kvp.Value;
                    writer.WriteLine($"Asset: {analysis.assetName} ({analysis.assetType})");
                    writer.WriteLine($"Size: {FormatFileSize(analysis.fileSize)}");
                    writer.WriteLine($"Reason: {GetReasonDescription(analysis.reason)}");
                    writer.WriteLine("Sources:");
                    
                    foreach (var source in analysis.sources)
                    {
                        writer.WriteLine($"  - {source.type}: {source.location}");
                        if (!string.IsNullOrEmpty(source.details))
                        {
                            writer.WriteLine($"    {source.details}");
                        }
                    }
                    
                    writer.WriteLine();
                }
            }
            
            Debug.Log($"Report exported to: {reportPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export report: {e.Message}");
        }
    }
}

    private Color GetSeverityColor(DuplicationReason reason)
    {
        // 🔴 Red: Critical (Direct + Addressable)
        // 🟡 Yellow: Warning (Resources + Addressable)
        // 🟠 Orange: Problem (Multiple groups)
        switch (reason) {
            case DuplicationReason.DirectAndAddressable:
                return Color.red;
            case DuplicationReason.ResourcesAndAddressable:
                return Color.yellow;
            case DuplicationReason.MultipleAddressableGroups:
                return new Color(1, 0.647f, 0);
            default:
                return Color.white;
        }
    }

    private string GetReasonDescription(DuplicationReason reason)
    {
        switch (reason) {
            case DuplicationReason.DirectAndAddressable:
                return "Asset exists both as direct reference AND in Addressables";
            case DuplicationReason.ResourcesAndAddressable:
                return "Asset exists both in Resources folder AND in Addressables";
            case DuplicationReason.MultipleAddressableGroups:
                return "Asset exists in multiple Addressable groups";
            case DuplicationReason.BuildDependency:
                return "Asset is pulled in as build dependency";
            case DuplicationReason.SceneDependency:
                return "Asset is referenced by scene objects";
            default:
                return "Unknown duplication cause";
        }
    }

    private void FixDuplication(DuplicationAnalysis analysis)
    {
        switch (analysis.reason) {
            case DuplicationReason.DirectAndAddressable:
                Debug.Log(
                    $"🔧 To fix: Replace direct references with AssetReference in prefabs/scripts for {analysis.assetName}");
                break;
            case DuplicationReason.MultipleAddressableGroups:
                Debug.Log($"🔧 To fix: Remove {analysis.assetName} from duplicate Addressable groups");
                break;
            default:
                Debug.Log($"🔧 Manual fix required for {analysis.assetName}");
                break;
        }
    }

    private long GetFileSize(string path)
    {
        try {
            return new FileInfo(path).Length;
        }
        catch {
            return 0;
        }
    }
    
    private void DisplayDetailedStatistics()
    {
        var duplicatedAssets = analysisResults.Where(x => x.Value.sources.Count > 1).ToList();
    
        // Grouping by types
        var byType = duplicatedAssets
            .GroupBy(x => x.Value.assetType)
            .OrderByDescending(g => g.Sum(x => x.Value.fileSize))
            .ToList();
    
        EditorGUILayout.LabelField("📈 Duplication by Asset Type:", EditorStyles.boldLabel);
    
        foreach (var group in byType.Take(5))
        {
            var typeSize = group.Sum(x => x.Value.fileSize);
            var typeCount = group.Count();
        
            EditorGUILayout.LabelField($"  • {group.Key}: {typeCount} assets, {FormatFileSize(typeSize)}");
        }
    
        // Grouping by reasons
        var byReason = duplicatedAssets
            .GroupBy(x => x.Value.reason)
            .OrderByDescending(g => g.Count())
            .ToList();
    
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🎯 Duplication by Reason:", EditorStyles.boldLabel);
    
        foreach (var group in byReason)
        {
            var reasonSize = group.Sum(x => x.Value.fileSize);
            var reasonCount = group.Count();
        
            EditorGUILayout.LabelField($"  • {GetReasonDescription(group.Key)}: {reasonCount} assets, {FormatFileSize(reasonSize)}");
        }
    }
    
    private string FormatFileSize(long bytes)
    {
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024.0f * 1024.0f):F1} MB";
        else if (bytes >= 1024)
            return $"{bytes / 1024.0f:F1} KB";
        else
            return $"{bytes} B";
    }
    
    private string[] GetReasonFilterOptions()
    {
        if (analysisResults == null) return new[] { "All Reasons" };
        
        var reasons = new[] { "All Reasons" }
            .Concat(System.Enum.GetValues(typeof(DuplicationReason))
                .Cast<DuplicationReason>()
                .Select(r => GetReasonDescription(r)))
            .ToArray();
        
        return reasons;
    }
    
    private string[] GetTypeFilterOptions()
    {
        if (analysisResults == null) return new[] { "All Types" };
        
        var types = new[] { "All Types" }
            .Concat(analysisResults.Values
                .Select(x => x.assetType)
                .Distinct()
                .OrderBy(x => x))
            .ToArray();
        
        return types;
    }
    
    private List<KeyValuePair<string, DuplicationAnalysis>> GetFilteredResults()
    {
        if (analysisResults == null) return new List<KeyValuePair<string, DuplicationAnalysis>>();
        
        var duplicatedAssets = analysisResults.Where(x => x.Value.sources.Count > 1);
        
        // Filter by reason
        if (selectedReasonFilter > 0)
        {
            var reasonValues = System.Enum.GetValues(typeof(DuplicationReason)).Cast<DuplicationReason>().ToArray();
            var selectedReason = reasonValues[selectedReasonFilter - 1];
            duplicatedAssets = duplicatedAssets.Where(x => x.Value.reason == selectedReason);
        }
        
        // Filter by type
        if (selectedTypeFilter > 0)
        {
            var typeOptions = GetTypeFilterOptions();
            var selectedType = typeOptions[selectedTypeFilter];
            duplicatedAssets = duplicatedAssets.Where(x => x.Value.assetType == selectedType);
        }
        
        return duplicatedAssets.ToList();
    }
}