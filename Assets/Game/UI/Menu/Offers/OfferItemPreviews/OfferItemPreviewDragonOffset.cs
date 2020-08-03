// OfferItemPreviewDragonOffset.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/03/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar component for offer item previews to define a custom offset and scale
/// for every dragon.
/// </summary>
public class OfferItemPreviewDragonOffset : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[System.Serializable]
	public class DragonSetup {
		[SkuList(DefinitionsCategory.DRAGONS, false)]
		public string sku = "";
		public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private MenuDragonLoader m_targetDragonLoader = null;
	[SerializeField] private Transform m_targetTransform = null;
	[SerializeField] private DragonSetup[] m_dragonSetups = new DragonSetup[0];

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Param check
		Debug.Assert(m_targetDragonLoader != null, "Required reference m_targetDragonLoader not defined!");
		Debug.Assert(m_targetTransform != null, "Required reference m_targetDragonLoader not defined!");

		// Do an initial apply
		Apply();

		// Subscribe to external events
		m_targetDragonLoader.onDragonLoaded += OnDragonLoaded;
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		m_targetDragonLoader.onDragonLoaded -= OnDragonLoaded;
	}

	/// <summary>
	/// A change has happened in the inspector.
	/// </summary>
	private void OnValidate() {
		// Live preview
		Apply();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply current offsets to the loader.
	/// </summary>
	public void Apply() {
		// Check required params
		if(m_targetTransform == null) return;

		// Reset first
		Reset();

		// Find out target offset setup
		DragonSetup setup = null;
		if(m_targetDragonLoader != null) {
			// [AOC] TODO!! Find a more optimal way to do it
			for(int i = 0; i < m_dragonSetups.Length; ++i) {
				// Is it the target dragon?
				if(m_dragonSetups[i].sku == m_targetDragonLoader.dragonSku) {
					setup = m_dragonSetups[i];
					break;
				}
			}
		}

		// Apply! If no suitable setup was found, reset +
		if(setup != null) {
			m_targetTransform.localPosition = setup.position;
			m_targetTransform.localScale = setup.scale;
		}
	}

	/// <summary>
	/// Reset to default values.
	/// </summary>
	public void Reset() {
		// Check required params
		if(m_targetTransform == null) return;

		// Reset to default values
		// [AOC] TODO!! Store original values?
		m_targetTransform.localPosition = Vector3.zero;
		m_targetTransform.localScale = Vector3.one;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new dragon has been loaded.
	/// </summary>
	/// <param name="_loader">The loader that triggered the event.</param>
	private void OnDragonLoaded(MenuDragonLoader _loader) {
		Apply();
	}
}