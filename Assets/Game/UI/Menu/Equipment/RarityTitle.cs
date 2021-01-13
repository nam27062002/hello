// RarityTitle.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple struct for a single rarity title.
/// </summary>
public class RarityTitle : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SkuListAttribute(DefinitionsCategory.RARITIES, false)]
	[SerializeField] private string m_rarity = "";
	public string rarity {
		get { return m_rarity; }
	}

	[SerializeField] private TextMeshProUGUI m_text = null;
	public TextMeshProUGUI text {
		get { return m_text; }
	}

	[Comment("Optional", 10)]
	[SerializeField] private TextMeshProUGUI m_auxText = null;
	public TextMeshProUGUI auxText {
		get { return m_auxText; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}