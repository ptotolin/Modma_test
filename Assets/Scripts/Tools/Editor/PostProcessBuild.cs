using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class PostProcessBuild
{
    [PostProcessBuild(1000)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            SetupIOSSigning(pathToBuiltProject);
        }
    }

    private static void SetupIOSSigning(string pathToBuiltProject)
    {
        Debug.Log("Setting up iOS signing automatically...");
        
        string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);
        
        string targetGuid = project.GetUnityMainTargetGuid();
        
        // Настройки подписи
        project.SetBuildProperty(targetGuid, "CODE_SIGN_STYLE", "Automatic");
        project.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", "");
        project.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE_SPECIFIER", "");
        
        // Сохранить изменения
        project.WriteToFile(projectPath);
        
        Debug.Log("✅ iOS signing configured automatically!");
    }
}