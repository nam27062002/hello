// ChildrenDisabler.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 30/05/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Disable all the entity´s children specified in the list
/// </summary>
public class ChildrenDisabler : MonoBehaviour {
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [SerializeField] private List<string> m_childrenNames;

    /// <summary>
    /// Iterate all the children and disable the ones that appear in the list
    /// </summary>
    public void Run ()
    {
        foreach (Transform child in GetComponentsInChildren<Transform> (false) )
        {
            if ( m_childrenNames.Contains(child.name))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

}