// ABTestSelector.cs
// Hungry Dragon
// 
// Created by  on 03/04/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// For AB test purposes sometimes is required to show two different variants of a prefab
/// This component allows you to instantiate the proper prefab depending on which version A/B we are
/// </summary>
public class ABTestSelector : MonoBehaviour {


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	[SerializeField] private ABTest.Test m_abTest;

    [Tooltip("Variant A should be the old version")] 
	[SerializeField] private GameObject m_prefabVariantA;
    [Tooltip("Variant B should be the new version that we are testing")]
    [SerializeField] private GameObject m_prefabVariantB;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

        if ( ! ABTest.Evaluate (m_abTest) )
        {
            // Variant A is the old version
		    InstantiatePrefabVariant(m_prefabVariantA);
        }
        else
        {
            // Variant B is the new one
		    InstantiatePrefabVariant(m_prefabVariantB);
	    }

	}

	

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

    /// <summary>
    /// Creates an instance of the selected prefab variant and replace the
    /// the current object with this new instance.
    /// </summary>
    /// <param name="m_prefabVariant">The prefab to be instantiated</param>
    private void InstantiatePrefabVariant (GameObject _prefabVariant)
    {
        // Instantiate the prefab
		GameObject instance = Instantiate(_prefabVariant, transform.parent);

		// Make sure it retains the same position in the hierarchy as the current gameobject
		instance.transform.SetSiblingIndex(transform.GetSiblingIndex());

		// Remove the current gameobject
		GameObject.DestroyImmediate(this.gameObject);

    }



	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}