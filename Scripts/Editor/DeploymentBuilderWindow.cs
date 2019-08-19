////////////////////////////////////////////////////////////////////////
// DeploymentBuilderWindow.cs
// By Don Hopkins.
// Copyright (C) 2014 by Deployment Corporation.


////////////////////////////////////////////////////////////////////////


#pragma warning disable 0414
#pragma warning disable 0219
#pragma warning disable 0168


////////////////////////////////////////////////////////////////////////
// Notes:


// EditorUserBuildSettings.activeBuildTarget


////////////////////////////////////////////////////////////////////////


using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;


////////////////////////////////////////////////////////////////////////


namespace UnityJS {


public class DeploymentBuilderWindow : EditorWindow {


    ////////////////////////////////////////////////////////////////////////
    // Instance Variables


    private Vector2 scrollPos1 = Vector2.zero;
    private float maxScrollViewHeight1 = 400.0f;
    private Vector2 scrollPos2 = Vector2.zero;
    private float maxScrollViewHeight2 = 400.0f;

    private static readonly GUIStyle titleFontStyle = new GUIStyle();
    private static readonly GUIStyle listFontStyle = new GUIStyle();
    private static readonly GUIStyle listFontStyleSelected = new GUIStyle();
    private static readonly GUIStyle listFontStyleCurrent = new GUIStyle();
    private static readonly GUIStyle listFontStyleCurrentSelected = new GUIStyle();

    ////////////////////////////////////////////////////////////////////////
    // Static Methods


    [MenuItem("Window/UnityJS Deployment Builder Window")]
    public static void ShowWindow()
    {
        var w = GetWindow(typeof(DeploymentBuilderWindow), false, "UnityJS Deployment Builder") as DeploymentBuilderWindow;
        if (w != null) {
            w.Show();
        }
    }


    ////////////////////////////////////////////////////////////////////////
    // Instance Methods


    private void Awake()
    {
        RectOffset margin = new RectOffset(10, 10, 10, 10);

        titleFontStyle.fontSize = 20;
        titleFontStyle.fontStyle = FontStyle.Bold;
        titleFontStyle.margin = margin;
        //titleFontStyle.normal.textColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        //titleFontStyle.normal.background = MakeTex(1, 1, new Color(0.8f, 0.8f, 0.8f, 1.0f));

        listFontStyle.fontSize = 12;
        listFontStyle.fontStyle = FontStyle.Normal;
        listFontStyle.margin = margin;
        listFontStyle.normal.textColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        //listFontStyle.normal.background = MakeTex(1, 1, new Color(0.8f, 0.8f, 0.8f, 1.0f));

        listFontStyleSelected.fontSize = 12;
        listFontStyleSelected.fontStyle = FontStyle.Normal;
        listFontStyleSelected.margin = margin;
        listFontStyleSelected.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        listFontStyleSelected.normal.background = MakeTex(1, 1, new Color(0.2f, 0.2f, 0.2f, 1.0f));

        listFontStyleCurrent.fontSize = 12;
        listFontStyleCurrent.fontStyle = FontStyle.Bold;
        listFontStyleCurrent.margin = margin;
        listFontStyleCurrent.normal.textColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);
        listFontStyleCurrent.normal.background = MakeTex(1, 1, new Color(0.8f, 0.8f, 0.8f, 1.0f));

        listFontStyleCurrentSelected.fontSize = 12;
        listFontStyleCurrentSelected.fontStyle = FontStyle.Bold;
        listFontStyleCurrentSelected.margin = margin;
        listFontStyleCurrentSelected.normal.textColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);
        listFontStyleCurrentSelected.normal.background = MakeTex(1, 1, new Color(0.2f, 0.2f, 0.2f, 1.0f));
    }


    private void OnGUI()
    {
        string applicationDataPath = Application.dataPath;

        GUILayout.Label(
            "UnityJS Deployment Builder Window");

        GUILayout.Space(5);

        if (GUILayout.Button(
            "\nReload Deployment Configuration:\nResources/Config/DeploymentConfiguration.txt\n")) {

            DeploymentBuilder.ReloadDeploymentConfiguration();

        }

        JObject deploymentConfiguration = 
            DeploymentBuilder.GetDeploymentConfiguration();

        if (deploymentConfiguration == null) {

            GUILayout.Space(5);

            GUILayout.Label(
                "The deployment configuration in Resources/Config/DeploymentConfiguration.txt was not found!");
            return;

        }

        string[] scenes = deploymentConfiguration["scenes"].ToObject<string[]>();
        string scenePath = (scenes.Length > 0) ? scenes[0] : "";

        UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.GetActiveScene();

        if ((activeScene == null) ||
            (activeScene.path != scenePath)) {

            GUILayout.Space(5);

            if (GUILayout.Button(
                "\nLoad Scene:\n" +
                scenePath +
                "\n")) {

                UnityEngine.SceneManagement.Scene scene = 
                    EditorSceneManager.OpenScene(scenePath);

                Debug.Log("Opened scenePath: " + scenePath + " scene: " + scene);

            }

        } else {

            GUILayout.Space(5);

            if (GUILayout.Button(
                "\nApply Deployment Configuration\n")) {
                DeploymentBuilder.ConfigureDeployment(false);
            }

            GUILayout.Space(5);

            if (GUILayout.Button(
                "\nBuild Scene:\n" +
                scenePath +
                "\n")) {

                DeploymentBuilder.ConfigureDeployment(true);

            }

        }

        GUILayout.Space(5);

        GUILayout.Label("JSON Deployment Configuration:");

        scrollPos2 = 
            EditorGUILayout.BeginScrollView(
                scrollPos2, 
                GUILayout.ExpandHeight(true),
                GUILayout.MaxHeight(maxScrollViewHeight2));

        string json = 
            JsonConvert.SerializeObject(
                deploymentConfiguration, 
                Formatting.Indented);

        GUILayout.Label(
            json);

        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
    }


    private static Texture2D MakeTex(int width, int height, Color col)
    {
        var pix = new Color[width*height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        var result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }


}


}


////////////////////////////////////////////////////////////////////////
