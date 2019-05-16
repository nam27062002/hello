// Mission3DIconPreview.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 16/05/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class Mission3DIconPreview  : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed
    Dropdown dropDownContainer;


    // Internal



    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    public void Start()
    {

        // Get all the 3d models in the path
        var models = Resources.LoadAll(UIConstants.MISSION_3D_ICONS_PATH, typeof(GameObject));


    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    private List<T> LoadAllAssetsOfType<T> (string _path) where T:Object
    {

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(_path);

        List<T> results = new List<T>();
        foreach (Object o in assets)
        {

            if (o is T)
            {
                results.Add( (T) o );
            }

        }

        return results;

    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}