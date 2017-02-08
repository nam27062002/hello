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
	[Space]
	[SerializeField] private PowerIcon m_rewardPower = null;
	[Space]
	[SerializeField] private Localizer m_goldenFragmentTitle = null;
	[SerializeField] private Localizer m_goldenFragmentInfo = null;
	[SerializeField] private TextMeshProUGUI m_goldenFragmentCounter = null;

	[Separator("Fragments Counter Animation Parameters")]
	[SerializeField] private float m_counterDelay = 3.5f;
	[SerializeField] private float m_counterInDuration = 0.15f;
	[SerializeField] private float m_counterIdleInDuration = 0.2f;
	[SerializeField] private float m_counterIdleOutDuration = 0.2f;
	[SerializeField] private float m_counterOutDuration = 0.15f;
	[SerializeField] private float m_counterScaleFactor = 2f;
	[SerializeField] private Ease m_counterEaseIn = Ease.OutCubic;
	[SerializeField] private Ease m_counterEaseOut = Ease.InCubic;

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
				if(!_rewardData.duplicated) {
					// Initialize with powers data
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _rewardData.itemDef.GetAsString("powerup"));
					m_rewardPower.InitFromDefinition(powerDef, false);
				}
			} break;
		}

		// Duplicated info
		m_goldenFragmentCounter.gameObject.SetActive(_rewardData.fragments > 0);
		if(_rewardData.duplicated) {
			// Are all the golden eggs opened (giving coins instead if so)
			if(_rewardData.coins > 0) {
				m_goldenFragmentInfo.Localize("TID_EGG_REWARD_DUPLICATED_2", _rewardData.itemDef.GetLocalized("tidName"), StringUtils.FormatNumber(_rewardData.coins));	// %U0 already unlocked!\nYou get %U1 coins instead!
			} else {
				m_goldenFragmentTitle.Localize("TID_EGG_REWARD_FRAGMENT", StringUtils.FormatNumber(_rewardData.fragments));	// %U0 Golden Egg Fragments
				m_goldenFragmentInfo.Localize("TID_EGG_REWARD_DUPLICATED_1", _rewardData.itemDef.GetLocalized("tidName"), StringUtils.FormatNumber(_rewardData.fragments));	// %U0 already unlocked!\nYou get %U1 Golden Egg fragments instead!

				// Fragments counter
				RefreshGoldenFragmentCounter(EggManager.goldenEggFragments - _rewardData.fragments, false);	// Reward has already been given at this point, so show the current amount minus the rewarded amount
				DOVirtual.DelayedCall(m_counterDelay, () => { RefreshGoldenFragmentCounter(EggManager.goldenEggFragments, true); }, false);	// Sync with animation
			}
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
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the golden fragment counter text.
	/// </summary>
	/// <param name="_amount">Amount to display.</param>
	/// <param name="_animate">Whether to animate or not.</param>
	private void RefreshGoldenFragmentCounter(int _amount, bool _animate) {
		// Compose new string
		string newText = UIConstants.TMP_SPRITE_GOLDEN_EGG_FRAGMENT + " " + 
			LocalizationManager.SharedInstance.Localize("TID_FRACTION", StringUtils.FormatNumber(_amount), StringUtils.FormatNumber(EggManager.goldenEggRequiredFragments));

		// Animate?
		if(_animate) {
			DOTween.Sequence()
				.Append(m_goldenFragmentCounter.transform.DOScale(m_counterScaleFactor, m_counterInDuration).SetRecyclable(true).SetEase(m_counterEaseIn))
				.AppendInterval(m_counterIdleInDuration)
				.AppendCallback(() => { m_goldenFragmentCounter.text = newText; })
				.AppendInterval(m_counterIdleOutDuration)
				.Append(m_goldenFragmentCounter.transform.DOScale(1f, m_counterOutDuration).SetRecyclable(true).SetEase(m_counterEaseOut));
		} else {
			// Set text
			m_goldenFragmentCounter.text = newText;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}