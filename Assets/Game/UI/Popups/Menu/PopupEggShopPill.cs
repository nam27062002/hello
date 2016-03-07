// PopupEggShopPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Pill representing a single egg for the shop popup.
/// </summary>
[RequireComponent(typeof(ScrollRectSnapPoint))]
public class PopupEggShopPill : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Image m_previewArea = null;
	[SerializeField] private Text m_nameText = null;

	// Internal References
	private ScrollRectSnapPoint m_snapPoint = null;
	public ScrollRectSnapPoint snapPoint {
		get { return m_snapPoint; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_previewArea != null, "Required Field!");
		Debug.Assert(m_nameText != null, "Required Field!");

		// Get references
		m_snapPoint = GetComponent<ScrollRectSnapPoint>();
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this pill with the given egg definition.
	/// </summary>
	/// <param name="_eggDef">The definition to be used, from the EGGS category.</param>
	public void InitFromDef(DefinitionNode _eggDef) {

	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}
