// PopupDragonInfoEntitesLayout.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Layout for every tier to display 3D previews of the entities that the tier can eat.
/// Does nothing for now, used for the custom editor utilities.
/// </summary>
public class PopupDragonInfoEntitesLayout : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    public const string FOG_PARAMETER = "FOG";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

    /// <summary>
    /// Process the destructible prefabs after they have been loaded.
    /// </summary>
    public void ProcessDestructiblePrefab ()
    {
        // [JOM] With this we fix the bug HDK-6464:
        // Set the fog parameter to disable, as this parameter is breaking the prefab in the rewards screen

        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
        {
            foreach (Material material in renderer.materials)
            {
                if (material.IsKeywordEnabled(FOG_PARAMETER))
                {
                    material.DisableKeyword(FOG_PARAMETER);
                }
            }
        }
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}