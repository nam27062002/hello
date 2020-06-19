// EditorCommandWindow.cs
// Hungry Dragon
// 
// Created by Jordi Riambau on 18/06/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class EditorCommandWindow : EditorWindow
{
    public static Action<string> onResponse = new Action<string>(OnResponse);
    public static Action<string> onError = new Action<string>(OnError);

    readonly static List<string> output = new List<string>();
    readonly static List<string> error = new List<string>();

    Vector2 scrollPosition;
    GUIStyle labelStyle = null;
    GUIStyle errorStyle = null;

    public void Init()
    {
        output.Clear();
        error.Clear();
    }

    public static EditorCommandWindow GetWindow(string windowTitle = "Output")
    {
        EditorCommandWindow window = (EditorCommandWindow) GetWindow(typeof(EditorCommandWindow), true, windowTitle);
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 800, 600);
        return window;
    }

    static void OnResponse(string line)
    {
        output.Add(line);
    }

    static void OnError(string line)
    {
        error.Add(line);
    }

    void OnGUI()
	{
        if (labelStyle == null)
            labelStyle = new GUIStyle(EditorStyles.label) { richText = true };

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // Standard output
        for (int i = 0; i < output.Count; i++)
        {
            GUILayout.Label(output[i], labelStyle);
        }
       
        // Standard error
        if (error.Count > 0)
        {
            if (errorStyle == null)
                errorStyle = new GUIStyle(EditorStyles.boldLabel);

            GUILayout.Label("-- An error has occured --", errorStyle);
            for (int i = 0; i < error.Count; i++)
            {
                GUILayout.Label(error[i], errorStyle);
            }
        }
        GUILayout.EndScrollView();
	}
}
