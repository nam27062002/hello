// ResultsScreenDisguiseFlag.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/11/2016.
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
/// Controller for the flag displayed on the results screen when unlocking a disguise.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ResultsScreenDisguiseFlag : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private UIScene3DLoader m_preview = null;

	// Internal
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private Animator m_animator = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_animator = GetComponent<Animator>();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the flag with data from the given disguise definition.
	/// </summary>
	/// <param name="_disguiseDef">Disguise definition to be used.</param>
	public void InitFromDef(DefinitionNode _disguiseDef) {
		// Set disguise name
		m_nameText.Localize(_disguiseDef.Get("tidName"));

		// Load preview
		MenuDragonLoader dragonLoader = m_preview.scene.FindComponentRecursive<MenuDragonLoader>();
		if(dragonLoader != null) {
			// Load dragon
			dragonLoader.removeFresnel = true;
			dragonLoader.LoadDragon(_disguiseDef.Get("dragonSku"));
			dragonLoader.dragonInstance.SetAnim(MenuDragonPreview.Anim.POSE_FLY);

			// Apply disguise
			DragonEquip eq = dragonLoader.dragonInstance.GetComponent<DragonEquip>();
			eq.EquipDisguise(_disguiseDef.sku);
			dragonLoader.dragonInstance.SetFresnelColor(Color.black);

			// Disguise Accesories need to be changed to the correct layer
			dragonLoader.dragonInstance.gameObject.SetLayerRecursively( dragonLoader.gameObject.layer );

		}

		// Store definition
		m_def = _disguiseDef;
	}

	/// <summary>
	/// Launch the whole unlock animation.
	/// </summary>
	public void LaunchAnim() {
		m_animator.SetTrigger("unlock");
	}

	/// <summary>
	/// Fold/Unfold the flag.
	/// </summary>
	/// <param name="_folded">Whether to fold or unfold the flag.</param>
	public void ToggleFold(bool _folded) {
		if(_folded) {
			m_animator.SetTrigger("fold");
		} else {
			m_animator.SetTrigger("unfold");
		}
	}

	/// <summary>
	/// Highlight the flag?
	/// </summary>
	/// <param name="_highlighted">Whether to highlight or not the flag.</param>
	public void ToggleHighlight(bool _highlighted) {
		// Figure out target color
		Color targetColor = _highlighted ? Color.white : Colors.gray;

		// Tint all children graphics
		Graphic[] graphics = GetComponentsInChildren<Graphic>();
		for(int i = 0; i < graphics.Length; i++) {
			//graphics[i].color = targetColor;
			graphics[i].CrossFadeColor(targetColor, 1f, false, true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}