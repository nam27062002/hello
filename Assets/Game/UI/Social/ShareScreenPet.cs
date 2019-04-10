// ShareScreenSetupPet.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

#define RARITY_GRADIENT

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Individual layout for a pet share screen.
/// </summary>
public class ShareScreenPet : IShareScreen {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private Localizer m_titleText = null;
	[SerializeField] private Localizer m_petNameText = null;
	[SerializeField] private MenuPetLoader m_petLoader = null;
	[SerializeField] private PowerIcon m_powerIcon = null;
	[Space]
	[SerializeField] private RenderQueueSetter m_renderQueueSetter = null;

	// Internal references
	protected DefinitionNode m_petDef = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this screen with given data.
	/// </summary>
	/// <param name="_shareLocationSku">Location where this screen is triggered.</param>
	/// <param name="_refCamera">Reference camera. Its properties will be copied to the scene's camera.</param>
	/// <param name="_petSku">Pet to display.</param>
	/// <param name="_refTransform">Reference transform for the pet preview.</param>
	public void Init(string _shareLocationSku, Camera _refCamera, string _petSku, Transform _refTransform) {
		// Set location and camera
		SetLocation(_shareLocationSku);
		SetRefCamera(_refCamera);

		// Store pet definition and some other data
		m_petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _petSku);
		Metagame.Reward.Rarity rarity = Metagame.Reward.SkuToRarity(m_petDef.GetAsString("rarity"));

		// Initialize UI elements
		// Rarity text
		if(m_titleText != null) {
			// Text by rarity
			switch(rarity) {
				case Metagame.Reward.Rarity.COMMON: m_titleText.Localize("TID_EGG_REWARD_PET_COMMON_NAME"); break;
				case Metagame.Reward.Rarity.RARE: m_titleText.Localize("TID_EGG_REWARD_PET_RARE_NAME"); break;
				case Metagame.Reward.Rarity.EPIC: m_titleText.Localize("TID_EGG_REWARD_PET_EPIC_NAME"); break;
				default: m_titleText.Localize("TID_PET"); break;
			}

			// Tint by rarity?
			/*
#if RARITY_GRADIENT
			Gradient4 rarityGradient = UIConstants.GetRarityTextGradient(rarity);
			m_titleText.text.colorGradient = new TMPro.VertexGradient(
				rarityGradient.topLeft,
				rarityGradient.topRight,
				rarityGradient.bottomLeft,
				rarityGradient.bottomRight
			);
			m_titleText.text.enableVertexGradient = true;
			m_titleText.text.color = Color.white;
#else
			m_titleText.text.enableVertexGradient = false;
			m_titleText.text.color = UIConstants.GetRarityColor(rarity);
#endif
			*/
		}

		// Pet name
		if(m_petNameText != null) {
			m_petNameText.Localize(m_petDef.GetAsString("tidName"));
		}

		// Pet preview
		if(m_petLoader != null) {
			// Load target pet
			m_petLoader.Load(_petSku);

			// Reapply render queues
			m_renderQueueSetter.Apply();

			// Rotate pet preview to replicate the reference transform
			// m_petLoader.transform.localRotation = _refTransform.localRotation;

			// Start the animation at a random frame (usually first frame looks shitty :s)
			Animator anim = m_petLoader.petInstance.animator;
			/*
			AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);  //could replace 0 by any other animation layer index
			//anim.Play(state.fullPathHash, -1, Random.Range(0f, 1f));
			AnimatorClipInfo clipInfo = anim.GetCurrentAnimatorClipInfo(0)[0];
			int numFrames = (int)(clipInfo.clip.length * clipInfo.clip.frameRate);
			Debug.Log(Color.yellow.Tag(numFrames.ToString()));
			anim.Play(state.fullPathHash, -1, Mathf.InverseLerp(0, numFrames, 2));	// Go to frame #2
			*/
			anim.enabled = false;		
		}

		// Power Info
		if(m_powerIcon != null) {
			DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_petDef.GetAsString("powerup"));
			m_powerIcon.InitFromDefinition(powerDef, false, false, PowerIcon.Mode.PET);
		}
	}
}