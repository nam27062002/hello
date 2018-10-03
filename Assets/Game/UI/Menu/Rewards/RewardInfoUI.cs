﻿// EggRewardTitle.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to easily setup the egg reward title.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RewardInfoUI : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable]
	public class RewardTypeSetup {
		public GameObject rootObj = null;
		public string animTrigger = "";
		public float animDuration = 1f;
	}

	[Serializable]
	public class RewardTypeSetupDictionary : SerializableDictionary<string, RewardTypeSetup> { }

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Shared")]
	[SerializeField] private TextMeshProUGUI m_extraInfoText = null;
	[SerializeField] private RewardTypeSetupDictionary m_typeSetups = new RewardTypeSetupDictionary();

	[Separator("Pet Reward")]
	[SerializeField] private RarityTitleGroup m_petRarityTitle = null;
	[SerializeField] private PowerIcon m_petPower = null;

	[Separator("Golden Egg Fragments Reward")]
	[SerializeField] private Localizer m_goldenFragmentTitle = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_goldenFragmentCounter = null;
	[SerializeField] private TextMeshProUGUI m_goldenFragmentCounterText = null;
	[SerializeField] private ShowHideAnimator m_goldenEggCompletedInfo = null;
	[SerializeField] private ShowHideAnimator m_goldenEggAllCollectedInfo = null;
	[SerializeField] private ParticleSystem m_goldenFragmentCounterFX = null;
	[SerializeField] private string m_goldenFragmentsSFX = "";
	[Space]
	[SerializeField] private float m_goldFragmentsCounterDelay = 3f;

	[SeparatorAttribute("SC Reward")]
	[SerializeField] private Localizer m_scTitle = null;
	[SerializeField] private string m_scSFX = "";

	[Separator("PC Reward")]
	[SerializeField] private Localizer m_pcTitle = null;
	[SerializeField] private string m_pcSFX = "";

	[Separator("Skin Reward")]
	[SerializeField] private Localizer m_skinTitle = null;
	[SerializeField] private PowerIcon m_skinPower = null;

	// Events
	[Separator("Events")]
	public UnityEvent OnAnimFinished = new UnityEvent();

	// Non-exposed setup, to be set from code
	private string m_goldenEggCompletedSFX = "";
	public string goldenEggCompletedSFX {
		get { return m_goldenEggCompletedSFX; }
		set { m_goldenEggCompletedSFX = value; }
	}

	// Other references
	private Animator m_animator = null;
	private Animator animator {
		get { 
			if(m_animator == null) m_animator = GetComponent<Animator>();
			return m_animator;
		}
	}

	private ShowHideAnimator m_showHideAnimator = null;
	public ShowHideAnimator showHideAnimator {
		get {
			if(m_showHideAnimator == null) m_showHideAnimator = GetComponent<ShowHideAnimator>();
			return m_showHideAnimator;
		}
	}

	// Internal
	private Metagame.Reward m_reward = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Start with everything hidden
		SetRewardType(string.Empty);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the title with the given Egg Reward Data and launch the animation.
	/// </summary>
	/// <param name="_rewardData">Egg reward data to be used for initialization.</param>
	public void InitAndAnimate(Metagame.Reward _rewardData, string _extraInfo = "") {
		// Ignore if given data is not valid or is not initialized
		if(_rewardData == null) return;

		// Store reward
		m_reward = _rewardData;

		// If we're not visible, show ourselves now!
		showHideAnimator.Show(false);

		// Show target object
		SetRewardType(_rewardData.type);

		// Aux vars
		float totalAnimDuration = m_typeSetups.Get(_rewardData.type).animDuration;	// Used to dispatch the OnAnimFinished event. Depends on reward type.

		// Different initializations based on reward type
		switch(_rewardData.type) {
			// Pet
			case Metagame.RewardPet.TYPE_CODE: {
				// Rarity - Try to extract special name from egg rewards definitions. Otherwise don't show rarity title.
				string petRewardSku = "pet_" + _rewardData.def.GetAsString("rarity");
				DefinitionNode petRewardDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGG_REWARDS, petRewardSku);
				if(petRewardDef != null) {
					string text = petRewardDef.GetLocalized("tidName");	// "Rare Pet"
					m_petRarityTitle.gameObject.SetActive(true);

					DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, _rewardData.def.Get("rarity"));
					m_petRarityTitle.InitFromRarity(rarityDef, text);
				} else {
					m_petRarityTitle.gameObject.SetActive(false);
				}

				// Pet name
				TextMeshProUGUI rewardNameText = m_petRarityTitle.activeTitle.auxText;
				if(rewardNameText != null) {
					Localizer loc = rewardNameText.GetComponent<Localizer>();
					if(loc != null) loc.Localize(_rewardData.def.Get("tidName"));	// Froggy
				}

				// Power icon - don't show if pet will be replaced
				m_petPower.gameObject.SetActive(!_rewardData.WillBeReplaced());
				if(!_rewardData.WillBeReplaced()) {
					// Initialize with powers data
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _rewardData.def.GetAsString("powerup"));
					m_petPower.InitFromDefinition(powerDef, false);
				}
			} break;

			// Skin
			case Metagame.RewardSkin.TYPE_CODE: {
				// Skin name
				if(m_skinTitle != null) {
					m_skinTitle.Localize(_rewardData.def.GetAsString("tidName"));
				}

				// Power icon
				if(m_skinPower != null) {
					// Initialize with powers data
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _rewardData.def.GetAsString("powerup"));
					m_skinPower.InitFromDefinition(powerDef, false);
				}
			} break;

			// Golden Fragments
			case Metagame.RewardGoldenFragments.TYPE_CODE: {
				// Title - singular?
				string tid = (_rewardData.amount == 1) ? "TID_EGG_REWARD_FRAGMENT_SING" : "TID_EGG_REWARD_FRAGMENT";
				m_goldenFragmentTitle.Localize(tid, StringUtils.FormatNumber(_rewardData.amount));	// %U0 Golden Egg Fragments

				// Fragments counter
				m_goldenEggCompletedInfo.Set(false, false);	// Will be activated after the animation, if needed
				RefreshGoldenFragmentCounter(EggManager.goldenEggFragments - _rewardData.amount, false);	// Reward has already been given at this point, so show the current amount minus the rewarded amount
				UbiBCN.CoroutineManager.DelayedCall(() => { RefreshGoldenFragmentCounter(EggManager.goldenEggFragments, true); }, m_goldFragmentsCounterDelay, false);	// Sync with animation
			} break;

			// Coins
			case Metagame.RewardSoftCurrency.TYPE_CODE: {
				// Set text
				m_scTitle.Localize("TID_EGG_REWARD_COINS", StringUtils.FormatNumber(_rewardData.amount));	// %U0 Coins!
			} break;

			// PC
			case Metagame.RewardHardCurrency.TYPE_CODE: {
				// Set text
				m_pcTitle.Localize("TID_REWARD_PC", StringUtils.FormatNumber(_rewardData.amount));	// %U0 Gems!
			} break;

			// Egg
			case Metagame.RewardEgg.TYPE_CODE:
			case Metagame.RewardMultiEgg.TYPE_CODE: {
				// Nothing to do
			} break;
		}

		// Launch animation!
		SetRewardType(m_reward.type);

		// Aux text
		m_extraInfoText.gameObject.SetActive(!string.IsNullOrEmpty(_extraInfo));
		m_extraInfoText.text = _extraInfo;

		// Program finish callback
		UbiBCN.CoroutineManager.DelayedCall(OnAnimationFinished, totalAnimDuration, false);
	}

	/// <summary>
	/// Toggle displayed info based on reward type. Triggers animation corresponding to that type.
	/// </summary>
	/// <param name="_rewardType">Type of reward whose info we want to show. Use empty string to hide everything.</param>
	public void SetRewardType(string _rewardType) {
		// Hide extra info, will be activated if needed
		m_extraInfoText.gameObject.SetActive(false);

		/// Toggle everything off except the target type!
		foreach(KeyValuePair<string, RewardTypeSetup> kvp in m_typeSetups.dict) {
			bool match = _rewardType == kvp.Key;
			kvp.Value.rootObj.SetActive(match);

			// If match, trigger the target animation!
			if(match) {
				animator.SetTrigger(kvp.Value.animTrigger);
			}
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the golden fragment counter text.
	/// </summary>
	/// <param name="_amount">Amount to display.</param>
	/// <param name="_animate">Whether to animate or not.</param>
	private void RefreshGoldenFragmentCounter(long _amount, bool _animate) {
		// Special case if we've actually completed the egg
		// Special case if all golden eggs have already been collected
		bool goldenEggCompleted = (_amount >= EggManager.goldenEggRequiredFragments);
		bool allEggsCollected = EggManager.goldenEggRequiredFragments < 0;	// Will return -1 if all eggs are collected

		// Compose new string
		if(!goldenEggCompleted && !allEggsCollected) {
			m_goldenFragmentCounterText.text = UIConstants.GetIconString(
				LocalizationManager.SharedInstance.Localize("TID_FRACTION", StringUtils.FormatNumber(_amount), StringUtils.FormatNumber(EggManager.goldenEggRequiredFragments)),
				UIConstants.IconType.GOLDEN_FRAGMENTS,
				UIConstants.IconAlignment.LEFT
			);
		}

		// Set different elements visibility
		m_goldenFragmentCounter.Set(!goldenEggCompleted && !allEggsCollected, _animate);
		m_goldenEggCompletedInfo.Set(goldenEggCompleted && !allEggsCollected, _animate);
		m_goldenEggAllCollectedInfo.Set(allEggsCollected, _animate);

		// Animate?
		if(_animate) {
			// Trigger Particle FX
			m_goldenFragmentCounterFX.Play();

			// Trigger SFX
			AudioController.Play(m_goldenEggCompletedSFX);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// ANIMATION EVENTS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Animation event.
	/// </summary>
	public void OnRewardSCIn() {
		AudioController.Play(m_scSFX);
	}

	/// <summary>
	/// Animation event.
	/// </summary>
	public void OnRewardPCIn() {
		AudioController.Play(m_pcSFX);
	}

	/// <summary>
	/// Animation event.
	/// </summary>
	public void OnRewardGoldenFragmentsIn() {
		AudioController.Play(m_goldenFragmentsSFX);
	}

	/// <summary>
	/// Animation finished callback.
	/// </summary>
	private void OnAnimationFinished() {
		// Perform some extra stuff
		switch(m_reward.type) {
			// Pet
			case Metagame.RewardPet.TYPE_CODE: {
				// [AOC] 1.14 Halloween pet needs some explanation, so let's show a popup for this one
				if(m_reward.def.sku == PopupHalloweenPetInfo.PET_SKU && !m_reward.WillBeReplaced()) {	// Not when it's a duplicate!
					PopupManager.OpenPopupInstant(PopupHalloweenPetInfo.PATH);
				}
			} break;
		}

		// Notify listeners
		OnAnimFinished.Invoke();
	}
}