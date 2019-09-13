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
using System.Collections.Generic;
using System.Text;

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
	[System.Serializable]
	public class DragonPoseData {
		[SkuList(DefinitionsCategory.DRAGONS, false)]
		public string sku = "";
		public Vector3 offset = GameConstants.Vector3.zero;
		public float scale = 1f;
		public uint animationFrame = 1;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private Localizer m_descText = null;
	[SerializeField] private MenuDragonLoader m_dragonLoader = null;
	public MenuDragonLoader dragonLoader { get { return m_dragonLoader; } }
	[Space]
	[SerializeField] private Image m_tierIcon = null;
	[Space]
	[SerializeField] private GameObject m_powerGroup = null;
	[SerializeField] private PowerIcon m_powerIcon = null;
	[SerializeField] private Localizer m_skinNameText = null;
	[Space]
	[SerializeField] private List<DragonPoseData> m_dragonPoses = new List<DragonPoseData>();

	// Internal references
	private bool m_renderDragon = false;
	protected IDragonData m_dragonData = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
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
				// Load target dragon
				m_dragonLoader.LoadDragon(m_dragonData.sku, m_dragonData.disguise);

				// Rotate preview to replicate the reference transform
				if(_refTransform != null) {
					m_dragonLoader.transform.localRotation = _refTransform.localRotation;
				}

				// Grab pose setup for this dragon (if defined)
				DragonPoseData pose = null;
				for(int i = 0; i < m_dragonPoses.Count; ++i) {
					if(m_dragonPoses[i].sku == m_dragonData.sku) {
						pose = m_dragonPoses[i];
					}
				}

				// If no pose was defined, use default parameters
				if(pose == null) pose = new DragonPoseData();

				// Apply position offset and scale
				Transform t = m_dragonLoader.transform;
				t.localPosition = pose.offset;
				t.SetLocalScale(pose.scale);

				// Start the animation at a random frame (usually first frame looks shitty :s)
				Animator anim = m_dragonLoader.dragonInstance.animator;
				AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);  //could replace 0 by any other animation layer index
#if false
				// Random frame
				anim.Play(state.fullPathHash, -1, Random.Range(0f, 1f));
#else
				// Specific frame
				AnimatorClipInfo clipInfo = anim.GetCurrentAnimatorClipInfo(0)[0];
				int numFrames = (int)(clipInfo.clip.length * clipInfo.clip.frameRate);
				anim.Play(state.fullPathHash, -1, Mathf.InverseLerp(0, numFrames, pose.animationFrame));
				//anim.speed = 0;	// Serpent dragons look weird with the animation paused
#endif
			} else {
				// Disable dragon preview
				m_dragonLoader.UnloadDragon();
			}
		}

		// Tier Icon 
		if(m_tierIcon != null) {
            string sprite = m_dragonData.tierDef.GetAsString("icon");
            m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, sprite);
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

				// Skin name
				if(m_skinNameText) {
					m_skinNameText.Localize(skinDef.GetAsString("tidName"));
				}
			} else {
				// Don't show power info
				m_powerGroup.SetActive(false);
			}
		}
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

	//------------------------------------------------------------------------//
	// EDITOR HELPER METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Dark trick to initialize new array elements with the proper default values.
	/// From https://forum.unity.com/threads/default-values-for-serializable-class-not-supported.42499/
	/// </summary>
	private void Reset() {
		m_dragonPoses = new List<DragonPoseData>() { new DragonPoseData() };
	}

	/// <summary>
	/// Log the current poses into the console.
	/// </summary>
	public void LogPoses() {
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < m_dragonPoses.Count; ++i) {
			DragonPoseData p = m_dragonPoses[i];
			sb.Append(p.sku).AppendLine(" {");
			sb.Append("   offset: ").AppendLine(p.offset.ToString());
			sb.Append("   scale: ").AppendLine(p.scale.ToString());
			sb.Append("   animFrame: ").AppendLine(p.animationFrame.ToString());
			sb.Append("}");
			if(i < m_dragonPoses.Count - 2) sb.Append(",");
			sb.AppendLine();
		}

		Debug.Log(sb.ToString());
	}
}