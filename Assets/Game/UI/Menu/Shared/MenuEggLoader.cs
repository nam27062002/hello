// MenuEggLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

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
	// Exposed
	[SkuList(DefinitionsCategory.EGGS)]
	[SerializeField] private string m_eggSku = "";

	// Internal
	private EggView m_eggView = null;
	public EggView eggView {
		get { return m_eggView; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Intialization.
	/// </summary>
	private void Awake() {
		// Egg previews are not serializable, so we can't reuse any existing egg view
		// Clear any existing object interfering with the egg.
		Unload();
	}

	/// <summary>
	/// Initialization.
	/// </summary>
	private void OnEnable() {     
        // If a egg sku was defined from inspector, load it now (unless an egg is already loaded)
		if(!string.IsNullOrEmpty(m_eggSku) && m_eggView == null) {
			Load(m_eggSku);
		}
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
	/// Load the given egg's preview.
	/// If an egg was loaded, it will be unloaded.
    /// </summary>
	/// <param name="_egg">The egg to be displayed. <c>null</c> to unload any active preview.</param>
	public void Load(Egg _egg) {
		// If not null, create a new view for the target egg
		if(_egg != null) {
			// Unless it's the same egg we have loaded, in wich case we won't do anything
			if(m_eggView != null && m_eggView.eggData == _egg) {
				return;
			}

			// Unload previous egg
			Unload();

			// Create and initialize the new egg view
			m_eggView = EggView.CreateFromData(_egg);
			ResetView();

			// Store new egg's sku
			m_eggSku = _egg.def.sku;
		} else {
			// Just unload current egg
			Unload();
		}
	}

	/// <summary>
	/// Load the egg with the given sku. A new egg in the SHOWROOM state will be created.
	/// </summary>
	/// <param name="_sku">Egg sku.</param>
	public void Load(string _sku) {
		// Skip if already loaded and sku is the same
		if(m_eggView != null && m_eggSku == _sku) {
			return;
		}

		// Unload previous egg
		Unload();

		// Create and initialize the new egg view
		m_eggView = EggView.CreateFromSku(_sku);
		ResetView();

		// Store new egg's sku
		if(m_eggView != null) {
			m_eggSku = _sku;
		} else {
			m_eggSku = string.Empty;
		}
	}

	/// <summary>
	/// Reload egg preview.
	/// </summary>
	public void Reload() {
		// Force unloading current view and load again with current sku
		Unload();
		Load(m_eggSku);
	}

	/// <summary>
	/// Destroy current loaded egg, if any.
	/// </summary>
    public void Unload() {
		// Destroy all childs of the loader and clear references
		while(transform.childCount > 0) {
			GameObject.DestroyImmediate(transform.GetChild(0).gameObject);	// Immediate so it can be called from the editor
		}
		m_eggView = null;
    }

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Resets the current egg view transformations.
	/// </summary>
	private void ResetView() {
		// View must be valid
		if(m_eggView == null) return;

		// Reset everything
		m_eggView.transform.SetParent(this.transform);
		m_eggView.transform.localPosition = Vector3.zero;
		m_eggView.transform.localRotation = Quaternion.identity;
		m_eggView.transform.localScale = Vector3.one;
		m_eggView.gameObject.SetLayerRecursively(this.gameObject.layer);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}