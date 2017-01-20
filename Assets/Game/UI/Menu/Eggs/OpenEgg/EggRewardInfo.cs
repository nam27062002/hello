// EggRewardTitle.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to easily setup the egg reward title.
/// </summary>
[RequireComponent(typeof(Animator))]
public class EggRewardInfo : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private RarityTitleGroup m_rarityTitle = null;
	[SerializeField] private Localizer m_goldenFragmentTitle = null;
	[SerializeField] private Localizer m_goldenFragmentInfo = null;
	[SerializeField] private GameObject m_rewardPowers = null;

	// Other references
	private Animator m_animator = null;
	private Animator animator {
		get { 
			if(m_animator == null) m_animator = GetComponent<Animator>();
			return m_animator;
		}
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
	/// <summary>
	/// Initialize the title with the given Egg Reward Data and launch the animation.
	/// </summary>
	/// <param name="_rewardData">Egg reward data to be used for initialization.</param>
	public void InitAndAnimate(EggReward _rewardData) {
		// Ignore if given data is not valid or is not initialized
		if(_rewardData == null) return;
		if(_rewardData.def == null) return;

		// Aux vars
		DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, _rewardData.def.Get("rarity"));

		// Different initializations based on reward type
		switch(_rewardData.type) {
			case "pet": {
				// Rarity
				string text = _rewardData.def.GetLocalized("tidName");	// "Rare Pet"
				m_rarityTitle.InitFromRarity(rarityDef, text);

				// Pet name
				TextMeshProUGUI rewardNameText = m_rarityTitle.activeTitle.auxText;
				if(rewardNameText != null) {
					Localizer loc = rewardNameText.GetComponent<Localizer>();
					if(loc != null) loc.Localize(_rewardData.itemDef.Get("tidName"));	// Froggy
				}

				// Power icon
				PowerIcon powerIcon = m_rewardPowers.FindComponentRecursive<PowerIcon>("Power1");
				if(!_rewardData.duplicated) {
					// Initialize with powers data
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _rewardData.itemDef.GetAsString("powerup"));
					powerIcon.InitFromDefinition(powerDef, false);
				}

				// Duplicated info
				if(_rewardData.duplicated) {
					// Are all the golden eggs opened (giving coins instead if so)
					if(_rewardData.coins > 0) {
						m_goldenFragmentInfo.Localize("TID_EGG_REWARD_DUPLICATED_2", _rewardData.itemDef.GetLocalized("tidName"), StringUtils.FormatNumber(_rewardData.coins));	// %U0 already unlocked!\nYou get %U1 coins instead!
					} else {
						m_goldenFragmentInfo.Localize("TID_EGG_REWARD_DUPLICATED_1", _rewardData.itemDef.GetLocalized("tidName"));	// %U0 already unlocked!\nYou get a Golden Egg fragment instead!
					}
				}
			} break;
		}

		// Setup and launch animation
		animator.SetBool("duplicated", _rewardData.duplicated);
		animator.SetTrigger("show");
	}

	/// <summary>
	/// Hide everything!
	/// </summary>
	public void Hide() {
		animator.SetTrigger("hide");
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}