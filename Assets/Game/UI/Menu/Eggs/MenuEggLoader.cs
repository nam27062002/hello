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
		// If a egg sku was defined from inspector, load it now (unless an egg is already loaded)
		if(!string.IsNullOrEmpty(m_eggSku) && m_eggScene3D == null) {
			// Create a new egg
			Egg newEgg = Egg.CreateFromSku(m_eggSku);
			newEgg.ChangeState(Egg.State.SHOWROOM);	// By default it will be on showroom state

			// Load it into this scene!
			Load(newEgg);
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
	/// <summary>
	/// Load the given egg's preview.
	/// If it's the same that is already loaded, nothing will be done.
	/// If a different egg was loaded, it will be unloaded.
	/// </summary>
	/// <param name="_egg">The egg to be displayed. <c>null</c> to unload any active preview.</param>
	public void Load(Egg _egg) {
		// Instantiate the 3D scene and initialize the raw image if not done
		if(m_eggScene3D == null) {
			m_eggScene3D = EggUIScene3D.CreateEmpty();
			m_eggScene3D.InitRawImage(ref m_targetRawImage);
		}

		// Just do it!
		m_eggScene3D.SetEgg(_egg);

		// Store new egg's sku
		if(_egg != null) {
			m_eggSku = _egg.def.sku;
		} else {
			m_eggSku = string.Empty;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}