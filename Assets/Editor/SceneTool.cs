// SceneTool.cs
// Hungry Dragon
// 
// Created by Diego Campos on 04/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Object = UnityEngine.Object;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor window.
/// </summary>
public class SceneTool : EditorWindow
{

    [MenuItem("Tools/Scene pivot fixer")]
    static void ScenePivotFixer()
    {
        MeshFilter[] meshFilterList;

        List<Mesh> meshList = new List<Mesh>();

        AssetFinder.FindAssetInScene<MeshFilter>(out meshFilterList);
        for (int c = 0; c < meshFilterList.Length; c++)
        {
            Mesh mesh = meshFilterList[c].sharedMesh;
            if (!meshList.Contains(mesh))
            {
                meshList.Add(mesh);
            }
        }
        Undo.RecordObjects(meshFilterList, "Scene pivot fixer");
        Undo.RecordObjects(meshList.ToArray(), "Scene pivot fixer");

        for (int c = 0; c < meshList.Count; c++)
        {
            Mesh mesh = meshList[c];

            Vector3 offset = Vector3.zero;

            List<Vector3> vList = new List<Vector3>();
            mesh.GetVertices(vList);

            for (int a = 0; a < vList.Count; a++)
            {
                offset += vList[a];
            }
            offset /= (float)vList.Count;

            for (int a = 0; a < vList.Count; a++)
            {
                vList[a] -= offset;
            }

            mesh.SetVertices(vList);

            for (int a = 0; a < meshFilterList.Length; a++)
            {
                if (meshFilterList[a].sharedMesh == mesh)
                {
                    meshFilterList[a].sharedMesh = mesh;
                    Transform transform = meshFilterList[a].gameObject.transform;
                    Vector3 pos = transform.position + transform.TransformDirection(offset);
                    transform.position = pos;
                }
            }
        }
    }
}