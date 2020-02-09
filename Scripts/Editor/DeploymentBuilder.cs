////////////////////////////////////////////////////////////////////////
// DeploymentBuilder.cs
// By Don Hopkins.
// Copyright (C) 2019 by Don Hopkins.


////////////////////////////////////////////////////////////////////////
// Notes:


// https://github.com/ludo6577/VrMultiplatform


////////////////////////////////////////////////////////////////////////


#pragma warning disable 0414
#pragma warning disable 0219
#pragma warning disable 0168


////////////////////////////////////////////////////////////////////////


using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;


////////////////////////////////////////////////////////////////////////


namespace UnityJS {


public class DeploymentBuilder : MonoBehaviour {


    ////////////////////////////////////////////////////////////////////////
    // Static Class Variables


    private static string deploymentConfigurationFileName = "Config/DeploymentConfiguration";
    private static JObject deploymentConfiguration = null;


    ////////////////////////////////////////////////////////////////////////
    // Static Methods


    public static void ConfigureDeployment(bool build=false)
    {
        JObject config = GetDeploymentConfiguration();
        if (config == null) {
            Debug.LogError("DeploymentBuilder: ConfigureDeployment: Resources/" + deploymentConfigurationFileName + ".txt missing from project!");
            return;
        }

        // Configure scenes, and load initial scene.

        string[] scenes = config["scenes"].ToObject<string[]>();
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: scenes: " + scenes);
        if (scenes == null) {
            Debug.LogError("DeploymentBuilder: ConfigureDeployment: scenes missing from config: "  + config);
            return;
        }

        // Make a list of EditorBuilderSettingsScenes for the EditorBuildingSettings.scenes array.

        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        foreach (string scenePath in scenes)
        {
            EditorBuildSettingsScene editorBuildSettingsScene = new EditorBuildSettingsScene(scenePath, true);
            editorBuildSettingsScenes.Add(editorBuildSettingsScene);
            Debug.Log("editorBuildSettingsScene: " + editorBuildSettingsScene + " scenePath: " + scenePath);
        }

        // Set the EditorBuildingSettings.scenes array.

        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        //Debug.Log("EditorBuildSettings.scenes: " + EditorBuildSettings.scenes);

        // Open the initial scene.

        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(scenes[0]);
        if (scene == null) {
            Debug.LogError("DeploymentBuilder: ConfigureDeployment: can't open scene: "  + scenes[0]);
            return;
        }

        // Fish out the Bridge.

        GameObject bridgeObj = GameObject.Find("Bridge");
        if (bridgeObj == null) {
            Debug.LogError("DeploymentBuilder: ConfigureDeployment: Can't find Bridge GameObject in scene: " + scenes[0]);
            return;
        }

        // Configure the Booter, if present.

        Booter booter =
            bridgeObj.GetComponent<Booter>();
        if (booter != null) {

            Undo.RecordObject(bridgeObj, "Configure Booter");
            EditorUtility.SetDirty(booter);

            string bootConfigurationsKey = (string)config["bootConfigurationsKey"];
            //Debug.Log("DeploymentBuilder: ConfigureDeployment: bootConfigurationsKey: " + bootConfigurationsKey);
            if (bootConfigurationsKey == null) {
                bootConfigurationsKey = "";
            }
            booter.bootConfigurationsKey = bootConfigurationsKey;

        }

        // Configure the Bridge, which must be present.

        Bridge bridge =
            bridgeObj.GetComponent<Bridge>();
        if (bridge == null) {
            Debug.LogError("DeploymentBuilder: ConfigureDeployment: Can't find Bridge component on Bridge GameObject bridgeObj: " + bridgeObj);
            return;
        }

        Undo.RecordObject(bridgeObj, "Configure Bridge");
        EditorUtility.SetDirty(bridge);

        string deployment = (string)config["deployment"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: deployment: " + deployment);
        bridge.deployment = deployment;

        string title = (string)config["title"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: title: " + title);
        bridge.title = title;

        string gameID = (string)config["gameID"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: gameID: " + gameID);
        bridge.gameID = gameID;

        string url = (string)config["url"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: url: " + url);
        bridge.url = url;

        string configuration = (string)config["configuration"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: configuration: " + configuration);
        bridge.configuration = configuration;

#if USE_SOCKETIO && UNITY_EDITOR

        bool useSocketIO = (bool)config["useSocketIO"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: useSocketIO: " + useSocketIO);
        bridge.useSocketIO = useSocketIO;

        string socketIOAddress = (string)config["socketIOAddress"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: socketIOAddress: " + socketIOAddress);
        bridge.socketIOAddress = socketIOAddress;

#endif

        // Configure the PlayerSettings.

        string productName = (string)config["productName"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: productName: " + productName);
        PlayerSettings.productName = productName;

        BuildTargetGroup buildTargetGroup = Bridge.ToEnum<BuildTargetGroup>(config["buildTargetGroup"]);
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: buildTargetGroup: " + buildTargetGroup);

        string bundleIdentifier = (string)config["bundleIdentifier"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: bundleIdentifier: " + bundleIdentifier);
        PlayerSettings.SetApplicationIdentifier(buildTargetGroup, bundleIdentifier);

        string defineSymbols = (string)config["defineSymbols"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: defineSymbols: " + defineSymbols);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defineSymbols);

        string bundleVersion = (string)config["bundleVersion"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: bundleVersion: " + bundleVersion);
        PlayerSettings.bundleVersion = bundleVersion;

        bool virtualRealitySupported = (bool)config["virtualRealitySupported"];
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: virtualRealitySupported: " + virtualRealitySupported);
        PlayerSettings.virtualRealitySupported = virtualRealitySupported;

        // Configure the PlayerSettings for the build target.

        BuildTarget buildTarget = Bridge.ToEnum<BuildTarget>(config["buildTarget"]);
        //Debug.Log("DeploymentBuilder: ConfigureDeployment: buildTarget: " + buildTarget);

        switch (buildTarget) {

            case BuildTarget.WebGL: {

                string webGLTemplate = (string)config["webGLTemplate"];
                //Debug.Log("DeploymentBuilder: ConfigureDeployment: webGLTemplate: " + webGLTemplate);
                PlayerSettings.WebGL.template = webGLTemplate;

                break;
            }

            case BuildTarget.iOS: {

                string buildNumber = (string)config["buildNumber"];
                //Debug.Log("DeploymentBuilder: ConfigureDeployment: buildNumber: " + buildNumber);
                PlayerSettings.iOS.buildNumber = buildNumber;

                break;
            }

            case BuildTarget.Android: {

                string sdk = 
                    Environment.GetEnvironmentVariable(
                        //"ANDROID_SDK_ROOT"
                        "ANDROID_HOME"
                    );

                if (!string.IsNullOrEmpty(sdk)) {
                    EditorPrefs.SetString("AndroidSdkRoot", sdk);
                    //Debug.Log("DeploymentBuilder: ConfigureDeployment: Android sdk: " + sdk);
                }

                int bundleVersionCode = (int)config["bundleVersionCode"];
                //Debug.Log("DeploymentBuilder: ConfigureDeployment: bundleVersionCode: " + bundleVersionCode);
                PlayerSettings.Android.bundleVersionCode = bundleVersionCode;

                string keystorePath = (string)config["keystorePath"];
                //Debug.Log("DeploymentBuilder: ConfigureDeployment: keystorePath: " + keystorePath);
                PlayerSettings.Android.keystoreName = keystorePath;

                string keyaliasName = (string)config["keyaliasName"];
                //Debug.Log("DeploymentBuilder: ConfigureDeployment: keyaliasName: " + keyaliasName);
                PlayerSettings.Android.keyaliasName = keyaliasName;

                string keyaliasPass = (string)config["keyaliasPass"];
                //Debug.Log("DeploymentBuilder: ConfigureDeployment: keyaliasPass: " + keyaliasPass);
                PlayerSettings.Android.keyaliasPass = keyaliasPass;

                break;
            }

            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64: {
                break;
            }

            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSX: {
                break;
            }

            default: {
                Debug.LogError("DeploymentBuilder: ConfigureDeployment: Unknown buildTarget: " + buildTarget);
                return;
            }

        } // switch buildTarget

        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);

        // Make sure all the changes are saved.

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        // Build this configuration if build is enabled.

        if (build) {

            BuildOptions buildOptions = Bridge.ToEnumMask<BuildOptions>(config["buildOptions"]);
            Debug.Log("DeploymentBuilder: ConfigureDeployment: buildOptions: " + buildOptions);

            string buildLocation = (string)config["buildLocation"];
            Debug.Log("DeploymentBuilder: ConfigureDeployment: buildLocation: " + buildLocation);

            // Switch to the build target.
            //EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);

            // Delete Emacs turds.
            foreach (string pattern in new string[] { "*~", "#*", }) {
                foreach (string fileName in Directory.EnumerateFiles(".", pattern, SearchOption.AllDirectories)) {
                    Debug.Log("DeploymentBuilder: ConfigureDeployment: deleting file: " + fileName);
                    File.Delete(fileName);
                }
            }

            BuildReport buildReport =
                BuildPipeline.BuildPlayer(
                    scenes, 
                    buildLocation, 
                    buildTarget, 
                    buildOptions);

            if (buildReport.summary.result != BuildResult.Succeeded) {
                Debug.Log("DeploymentBuilder: ");
                throw new Exception("Build failed!");
            }

        } // if build

    }
    

    public static JObject GetDeploymentConfiguration()
    {
        LoadDeploymentConfiguration();
        return deploymentConfiguration;
    }


    public static void ReloadDeploymentConfiguration()
    {
        deploymentConfiguration = null;
        LoadDeploymentConfiguration();
    }


    private static void LoadDeploymentConfiguration()
    {
        if (deploymentConfiguration != null) {
            return;
        }

        TextAsset resource = Resources.Load<TextAsset>(deploymentConfigurationFileName);
        if (resource == null) {
            Debug.LogError("DeploymentBuilder: LoadDeploymentConfigurations: Resource is not TextAsset!");
            return;
        }

        string text = resource.text;
        Resources.UnloadAsset(resource);
        if (string.IsNullOrEmpty(text)) {
            Debug.LogError("DeploymentBuilder: LoadDeploymentConfigurations: Text resource is empty!");
            return;
        }

        JToken data = JToken.Parse(text);
        if (data == null) {
            Debug.LogError("DeploymentBuilder: LoadDeploymentConfigurations: Error parsing JSON text:\n" + text);
            return;
        }

        deploymentConfiguration = (JObject)data;
        if (deploymentConfiguration == null) {
            Debug.LogError("DeploymentBuilder: LoadDeploymentConfigurations: JSON should be an object! text:\n" + text);
            return;
        }

    }


    private static JToken LoadJSONFile(string fileName)
    {
        string text = LoadTextFile(fileName);
        JToken data = JToken.Parse(text);
        //Debug.Log("DeploymentBuilder: LoadJSONFile: fileName: " + fileName + " data: " + data);
        return data;
    }


    public static string LoadTextFile(string fileName)
    {
        string result;

        TextAsset textFile =
            Resources.Load<TextAsset>(fileName);
        result = textFile.text;
        Resources.UnloadAsset(textFile);

        return result;
    }


}


}


////////////////////////////////////////////////////////////////////////
