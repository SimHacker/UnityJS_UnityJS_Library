using System.Collections;
using System.IO;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif


public class UnityJSPostProcessBuild {

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        Debug.Log("PostProcessBuild: OnPostProcessBuild: buildTarget: " + buildTarget + " path: " + path);

        switch (buildTarget) {

#if UNITY_IOS
            case BuiltTarget.iOS: {
                string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
                PBXProject proj = new PBXProject();
                proj.ReadFromString(File.ReadAllText(projPath));
                string target = proj.TargetGuidByName("Unity-iPhone");
                proj.AddFrameworkToProject(target, "WebKit.framework", false);
                File.WriteAllText(projPath, proj.WriteToString());
                break;
            }
#endif

        }

    }

}
