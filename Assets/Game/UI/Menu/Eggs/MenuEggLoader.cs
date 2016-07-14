// MenuEggLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/07/2016.
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
/// Auxiliar component to easily load a 3D egg preview into the UI.
/// </summary>
public class MenuEggLoader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SkuList(DefinitionsCategory.EGGS)]
	[SerializeField] private string m_eggSku = "";
	[SerializeField] private RawImage m_targetRawImage = null;

	private EggUIScene3D m_eggScene3D = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Intialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Initialization.
	/// </summary>
	private void OnEnable() {
		// Instantiate the 3D scene and initialize the raw image
		if(m_eggScene3D == null) {
			m_eggScene3D = EggUIScene3D.CreateEmpty();
			m_eggScene3D.InitRawImage(ref m_targetRawImage);
		}

		// If a egg sku was defined from inspector and we have no loaded egg, create it now
		if(m_eggScene3D.egg == null && !string.IsNullOrEmpty(m_eggSku)) {
			// Create a new egg
			Egg newEgg = Egg.CreateFromSku(m_eggSku);

			// Load it into this scene!
			m_eggScene3D.SetEgg(newEgg);
		}
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		// Destroy egg 3D scene
		if(m_eggScene3D != null) {
			UIScene3DManager.Remove(m_eggScene3D);
			m_eggScene3D = null;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}