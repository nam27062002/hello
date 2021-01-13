// GitEditorMenu.cs
// Hungry Dragon
// 
// Created by Jordi Riambau on 18/06/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;

public class GitEditorMenu : EditorWindow
{
    [MenuItem("Assets/Git/Log file...")]
    public static void GitLog()
    {
        Object objectSelected = Selection.activeObject;
        if (objectSelected == null)
            return;

        string path = AssetDatabase.GetAssetPath(objectSelected);
        if (string.IsNullOrEmpty(path))
            return;

        string command = "git log --graph --pretty=format:'<color=grey>%h</color> -<color=yellow>%d</color> %s <color=green>(%cr)</color> <b><color=orange><%an></color></b>' --abbrev-commit " + path;
        EditorCommand.Execute(cmd: command, showOutputWindow: true, onCompleted: null, windowTitle: "git log " + Path.GetFileName(path));
    }

    [MenuItem("Tools/Git/Checkout...")]
    public static void GitCheckout()
    {
        if (EditorUtility.DisplayDialog("git checkout", "Your local changes will be lost. Are you sure to checkout?", "Yes", "Cancel"))
        {
            EditorCommand.Execute(cmd: "git checkout -- .", showOutputWindow: false, onCompleted: AssetDatabase.Refresh);
        }
    }

    [MenuItem("Tools/Git/Checkout + Clean unstaged...")]
    public static void GitCheckoutUnstaged()
    {
        if (EditorUtility.DisplayDialog("git checkout and clean", "Your local changes and unstaged files will be lost. Are you sure to checkout?", "Yes", "Cancel"))
        {
            EditorCommand.Execute(cmd: "git checkout -- . && git clean -df", showOutputWindow: false, onCompleted: AssetDatabase.Refresh);
        }
    }
}
