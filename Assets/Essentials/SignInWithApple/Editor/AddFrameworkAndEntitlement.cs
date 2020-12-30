using System.IO;

#if UNITY_2018_4_OR_NEWER || (UNITY_STANDALONE_OSX && UNITY_2019_3_OR_NEWER)
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Content;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.iOS.Xcode;

public class PostProcessForSignInWithApple : IPostprocessBuildWithReport
{
    public int callbackOrder
    {
        get { return 999; }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
    }
    
    public void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget != BuildTarget.iOS && buildTarget != BuildTarget.tvOS && buildTarget != BuildTarget.StandaloneOSX)
            return;

        if (buildTarget == BuildTarget.StandaloneOSX && !path.EndsWith("xcodeproj"))
        {
            Debug.LogWarning("Adding Sign in with Apple to macOS builds requires that a Xcode project be created so entitlements can be set up appropriately.");
            return;
        }

		if (buildTarget == BuildTarget.StandaloneOSX)
			PostprocessForMacOS(path);
		else
			PostprocessFortvOSiOS(path);
    }

    void PostprocessFortvOSiOS(string path)
    {
        // Read in the Xcode project
        var projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        // Get the main target GUID
    #if UNITY_2019_3_OR_NEWER
        var mainTarget = proj.GetUnityMainTargetGuid();
    #else
        var mainTarget = proj.TargetGuidByName("Unity-iPhone");
    #endif
        var entitlementsFile = $"{Application.productName}.entitlements";

        string entitlementsFileName = proj.GetBuildPropertyForAnyConfig(new [] {
            mainTarget
        }, "CODE_SIGN_ENTITLEMENTS");
        
        
        // If the project has an entitlements file path set, use it
        if (entitlementsFileName != null)
        {
            entitlementsFile = entitlementsFileName;

            string[] entitlementsFiles = Directory.GetFiles(path, Path.GetFileName(entitlementsFileName), SearchOption.AllDirectories);

            // If that file exists and we're appending the build then copy it to the staging area
            // so we can save the existing entitlements
            if (entitlementsFiles.Length > 0)
            {
                entitlementsFile = entitlementsFileName;
            }
        }
        
#if UNITY_2019_3_OR_NEWER
        // AuthenticationServices should be added to the Framework target on Unity 2019.3+
        proj.AddFrameworkToProject(proj.GetUnityFrameworkTargetGuid(), "AuthenticationServices.framework", false);
        proj.WriteToFile(projPath);
        
        // Finally add the capability and entitlement to the project
        // Note: The function AddSignInWithApple was added in 2018.4.12f1, 2019.2.11f1 and 2019.3.0.
        // If you see an error make sure that your editor version is at or above one of those versions.
        var capManager = new ProjectCapabilityManager(projPath, entitlementsFile, targetGuid: mainTarget);
#else
        var capManager = new ProjectCapabilityManager(projPath, entitlementsFile, "Unity-iPhone");
#endif
        
        capManager.AddSignInWithApple();
        capManager.WriteToFile();
    }
    
    void PostprocessForMacOS(string path)
    {
        // Read in the Xcode project
        var projPath = Path.Combine(path, "project.pbxproj");
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        // Get the main target GUID
        var mainTarget = proj.TargetGuidByName(Application.productName);
        var entitlementsFile = $"{Application.productName}.entitlements";

        string entitlementsFileName = proj.GetBuildPropertyForAnyConfig(new [] {
            mainTarget
        }, "CODE_SIGN_ENTITLEMENTS");
        
        
        // If the project has an entitlements file path set, use it
        if (entitlementsFileName != null)
        {
            entitlementsFile = entitlementsFileName;

            string[] entitlementsFiles = Directory.GetFiles(path, Path.GetFileName(entitlementsFileName), SearchOption.AllDirectories);

            // If that file exists and we're appending the build then copy it to the staging area
            // so we can save the existing entitlements
            if (entitlementsFiles.Length > 0)
            {
                entitlementsFile = entitlementsFileName;
            }
        }
        
#if UNITY_2019_3_OR_NEWER
        proj.AddFrameworkToProject(mainTarget, "AuthenticationServices.framework", false);
        proj.WriteToFile(projPath);
        
        // Finally add the capability and entitlement to the project
        // Note: The function AddSignInWithApple was added in 2018.4.12f1, 2019.2.11f1 and 2019.3.0.
        // If you see an error make sure that your editor version is at or above one of those versions.
        var capManager = new ProjectCapabilityManager(projPath, entitlementsFile, targetGuid: mainTarget);
#else
        var capManager = new ProjectCapabilityManager(projPath, entitlementsFile, Application.productName);
#endif
        
        capManager.AddSignInWithApple();
        capManager.WriteToFile();
    }
    
}
#endif
