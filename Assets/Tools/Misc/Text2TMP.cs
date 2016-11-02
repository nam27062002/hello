// Text2TMP.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/10/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tool to help migrating from Unity Text to Text Mesh Pro.
/// </summary>
//[CreateAssetMenu]
public class Text2TMP : ScriptableObject {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Auxiliar classes
	[System.Serializable]
	public class FontReplacement {
		public Font sourceFont = null;
		public TMP_FontAsset targetFont = null;
		public List<int> sizes = new List<int>();
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed font list
	[SerializeField] public List<FontReplacement> m_fontReplacements = new List<FontReplacement>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// UTILS																  //
	//------------------------------------------------------------------------//


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}