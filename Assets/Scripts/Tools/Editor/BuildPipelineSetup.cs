using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildPipelineSetup : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS)
        {
            // Настройки перед билдом
            PlayerSettings.iOS.appleDeveloperTeamID = "";
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS)
        {
            // Настройки после билда
            Debug.Log("✅ iOS build completed with auto-signing!");
        }
    }
}