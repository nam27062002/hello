// EggRewardTitle.cs
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
	[Separator("Reward Info")]
	[SerializeField] private RarityTitleGroup m_rarityTitle = null;
	[SerializeField] private PowerIcon m_rewardPower = null;

	[Separator("Golden Egg Fragments Info")]
	[SerializeField] private Localizer m_goldenFragmentTitle = null;
	[SerializeField] private Localizer m_goldenFragmentInfo = null;

	[Separator("Golden Egg Fragments Counter")]
	[SerializeField] private ShowHideAnimator m_goldenFragmentCounter = null;
	[SerializeField] private TextMeshProUGUI m_goldenFragmentCounterText = null;
	[SerializeField] private ShowHideAnimator m_goldenEggCompletedInfo = null;
	[SerializeField] private ParticleSystem m_goldenFragmentCounterFX = null;

	[Separator("Animation Parameters")]
	[SerializeField] private float m_counterDelay = 3f;
	[SerializeField] private float m_counterDuration = 1f;

	// Events
	[Separator("Events")]
	public UnityEvent OnAnimFinished = new UnityEvent();

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

		// Golden egg fragments counter
		m_goldenFragmentCounter.Set(_rewardData.fragments > 0, false);
		m_goldenEggCompletedInfo.Set(false, false);	// Will be activated after the animation, if needed

		// Duplicated info
		if(_rewardData.duplicated) {
			// Are all the golden eggs opened (giving coins instead if so)
			if(_rewardData.coins > 0) {
				// Giving coins
				m_goldenFragmentTitle.Localize("TID_EGG_REWARD_COINS", StringUtils.FormatNumber(_rewardData.coins));	// %U0 Coins!
				m_goldenFragmentInfo.Localize("TID_EGG_REWARD_DUPLICATED_2", _rewardData.itemDef.GetLocalized("tidName"), StringUtils.FormatNumber(_rewardData.coins));	// %U0 already unlocked!\nYou get %U1 coins instead!
			} else {
				// Giving fragments
				m_goldenFragmentTitle.Localize("TID_EGG_REWARD_FRAGMENT", StringUtils.FormatNumber(_rewardData.fragments));	// %U0 Golden Egg Fragments
				m_goldenFragmentInfo.Localize("TID_EGG_REWARD_DUPLICATED_1", _rewardData.itemDef.GetLocalized("tidName"), StringUtils.FormatNumber(_rewardData.fragments));	// %U0 already unlocked!\nYou get %U1 Golden Egg fragments instead!

				// Fragments counter
				RefreshGoldenFragmentCounter(EggManager.goldenEggFragments - _rewardData.fragments, false);	// Reward has already been given at this point, so show the current amount minus the rewarded amount
				UbiBCN.CoroutineManager.DelayedCall(() => { RefreshGoldenFragmentCounter(EggManager.goldenEggFragments, true); }, m_counterDelay, false);	// Sync with animation
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

	/// <summary>
	/// Show everything, no setup!
	/// </summary>
	public void Show() {
		animator.SetTrigger("show");
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
		// If we've actually completed the egg, show completed info instead
		bool goldenEggCompleted = (_amount >= EggManager.goldenEggRequiredFragments);
		if(!goldenEggCompleted) {
			m_goldenFragmentCounterText.text = UIConstants.GetIconString(
				LocalizationManager.SharedInstance.Localize("TID_FRACTION", StringUtils.FormatNumber(_amount), StringUtils.FormatNumber(EggManager.goldenEggRequiredFragments)),
				UIConstants.IconType.GOLDEN_FRAGMENTS,
				UIConstants.IconAlignment.LEFT
			);
		}

		// Set different elements visibility
		m_goldenFragmentCounter.Set(!goldenEggCompleted, _animate);
		m_goldenEggCompletedInfo.Set(goldenEggCompleted, _animate);

		// Animate?
		if(_animate) {
			// Trigger Particle FX
			m_goldenFragmentCounterFX.Play();

			// Program finish callback
			UbiBCN.CoroutineManager.DelayedCall(() => { OnAnimFinished.Invoke(); }, m_counterDuration, false);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}