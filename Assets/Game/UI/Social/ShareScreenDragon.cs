// ShareScreenSetupPet.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Individual layout for a pet share screen.
/// </summary>
public class ShareScreenDragon : IShareScreen {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private Localizer m_descText = null;
	[SerializeField] private MenuDragonLoader m_dragonLoader = null;
	[Space]
	[SerializeField] private Image m_tierIcon = null;
	[SerializeField] private GameObject m_labIcon = null;
	[Space]
	[SerializeField] private GameObject m_powerGroup = null;
	[SerializeField] private PowerIcon m_powerIcon = null;

	// Internal references
	private bool m_renderDragon = false;
	protected IDragonData m_dragonData = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this screen with given data.
	/// </summary>
	/// <param name="_shareLocationSku">Location where this screen is triggered.</param>
	/// <param name="_refCamera">Reference camera. Its properties will be copied to the scene's camera.</param>
	/// <param name="_dragonData">Dragon to display.</param>
	/// <param name="_renderDragon">Whether to render the dragon preview or not (dragon captured in the background).</param>
	/// <param name="_refTransform">Reference transform for the dragon preview.</param>
	public void Init(string _shareLocationSku, Camera _refCamera, IDragonData _dragonData, bool _renderDragon, Transform _refTransform) {
		// Set location and camera
		SetLocation(_shareLocationSku);
		SetRefCamera(_refCamera);

		// Store data
		m_dragonData = _dragonData;
		m_renderDragon = _renderDragon;

		// Aux vars
		DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, m_dragonData.disguise);
		bool hasSkin = skinDef != IDragonData.GetDefaultDisguise(m_dragonData.sku);
		bool isSpecial = m_dragonData.type == IDragonData.Type.SPECIAL;
		bool showPower = !isSpecial && hasSkin;

		// Initialize UI elements
		// Dragon Name
		if(m_nameText != null) {
			m_nameText.Localize(m_dragonData.def.GetAsString("tidName"));
		}

		// Dragon Description - only if there is no power to display
		if(m_descText != null) {
			m_descText.gameObject.SetActive(!showPower);
			m_descText.Localize(m_dragonData.def.GetAsString("tidDesc"));
		}

		// Dragon preview
		if(m_dragonLoader != null) {
			// Load preview?
			if(m_renderDragon) {
				// Make sure it's active
				m_dragonLoader.gameObject.SetActive(true);

				// Load target dragon
				m_dragonLoader.LoadDragon(m_dragonData.sku, m_dragonData.disguise);

				// Rotate preview to replicate the reference transform
				if(_refTransform != null) {
					m_dragonLoader.transform.localRotation = _refTransform.localRotation;
				}

				// Start the animation at a random frame (usually first frame looks shitty :s)
				Animator anim = m_dragonLoader.dragonInstance.animator;
				AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);  //could replace 0 by any other animation layer index
#if true
				// Random frame
				anim.Play(state.fullPathHash, -1, Random.Range(0f, 1f));
#else
				// Specific frame
				AnimatorClipInfo clipInfo = anim.GetCurrentAnimatorClipInfo(0)[0];
				int numFrames = (int)(clipInfo.clip.length * clipInfo.clip.frameRate);
				Debug.Log(Color.yellow.Tag(numFrames.ToString()));
				anim.Play(state.fullPathHash, -1, Mathf.InverseLerp(0, numFrames, 2));	// Go to frame #2		
#endif
			} else {
				// Disable dragon preview
				m_dragonLoader.gameObject.SetActive(false);
			}
		}

		// Tier Icon - only for classic dragons
		if(m_tierIcon != null) {
			m_tierIcon.gameObject.SetActive(!isSpecial);
			if(!isSpecial) {
				m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, m_dragonData.tierDef.GetAsString("icon"));
			}
		}

		// Lab Icon - only for special dragons
		if(m_labIcon != null) {
			m_labIcon.SetActive(isSpecial);
		}

		// Power Info - Only if classic dragon and a skin is equipped
		if(m_powerGroup != null) {
			if(showPower) {
				// Show power!
				m_powerGroup.SetActive(true);

				// Initialize power info
				if(m_powerIcon != null) {
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, skinDef.GetAsString("powerup"));
					m_powerIcon.InitFromDefinition(powerDef, false, false, PowerIcon.Mode.SKIN);
				}
			} else {
				// Don't show power info
				m_powerGroup.SetActive(false);
			}
		}
	}

	/// <summary>
	/// Define the AR camera, which will replace the reference camera used in Init().
	/// </summary>
	/// <param name="_arCamera">Camera being used by the AR system.</param>
	public void SetARCamera(Camera _arCamera) {
		// Copy camera parameters
		SetRefCamera(_arCamera);

		// Copy some extra parameters?

	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDE METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Layer mask for the background render.
	/// </summary>
	/// <returns>The culling mask to be assigned to the camera for the background render.</returns>
	protected override int GetBackgroundCullingMask() {
		if(m_renderDragon) {
			return LayerMask.GetMask("Ground");
		} else {
			return LayerMask.GetMask("Ground", "Default", "Player");    // Include dragon preview
		}
	}
}