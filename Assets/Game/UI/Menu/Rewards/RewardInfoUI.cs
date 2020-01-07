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
	[SerializeField] private ShareButton m_shareButton = null;
	[SerializeField] private RewardTypeSetupDictionary m_typeSetups = new RewardTypeSetupDictionary();

	[Separator("Pet Reward")]
	[SerializeField] private RarityTitleGroup m_petRarityTitle = null;
    [SerializeField] private GameObject m_petPowerLayout = null;
    [SerializeField] private Localizer m_petPowerName = null;
    [SerializeField] private Localizer m_petPowerDescription = null;
    [SerializeField] private PowerIcon m_petPowerIcon = null;

	[Separator("Golden Egg Fragments Reward")]
	[SerializeField] private Localizer m_goldenFragmentTitle = null;
	[Space]
	[SerializeField] private string m_goldenFragmentsSFX = "";
	

	[SeparatorAttribute("SC Reward")]
	[SerializeField] private Localizer m_scTitle = null;
	[SerializeField] private string m_scSFX = "";

	[Separator("PC Reward")]
	[SerializeField] private Localizer m_pcTitle = null;
	[SerializeField] private string m_pcSFX = "";

	[Separator("Skin Reward")]
	[SerializeField] private Localizer m_skinTitle = null;
	[SerializeField] private PowerIcon m_skinPower = null;

	[Separator("Dragon Reward")]
	[SerializeField] private Localizer m_dragonName = null;
	[SerializeField] private Localizer m_dragonDesc = null;
	[Space]
	[SerializeField] private Image m_dragonTierIcon = null;
	[SerializeField] private ShowHideAnimator m_newPreysAnimator = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_healthText = null;
	[SerializeField] private TextMeshProUGUI m_energyText = null;
	[SerializeField] private TextMeshProUGUI m_speedText = null;

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

		if(m_shareButton != null) m_shareButton.gameObject.SetActive(false);
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
		float totalAnimDuration = m_typeSetups.Get(_rewardData.type).animDuration;  // Used to dispatch the OnAnimFinished event. Depends on reward type.
		bool showShareButton = false;

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

                    // Power data
                    DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _rewardData.def.GetAsString("powerup"));

                    // Power icon - don't show if pet will be replaced
                    m_petPowerLayout.gameObject.SetActive(!_rewardData.WillBeReplaced());

                    if (!_rewardData.WillBeReplaced())
                    {
                        // Power icon
                        if (m_petPowerIcon != null)
                        {
                            m_petPowerIcon.InitFromDefinition(powerDef, false);
                        }

                        // Power name
                        if (m_petPowerName != null)
                        {
                            m_petPowerName.Localize(powerDef.GetAsString("tidName"));
                        }

                        // Power description
                        if (m_petPowerDescription != null)
                        {
                            m_petPowerDescription.Set (DragonPowerUp.GetDescription(powerDef.sku, false, true));
                        }
                    }

                    // Show share button!
                    showShareButton = !_rewardData.WillBeReplaced();
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

				// Show share button!
				showShareButton = !_rewardData.WillBeReplaced();
			} break;

			// Dragon
			case Metagame.RewardDragon.TYPE_CODE: {
                // Select the rewarded dragon
                InstanceManager.menuSceneController.dragonSelector.SetSelectedDragon(_rewardData.sku);

                // Aux vars
                IDragonData dragonData = DragonManager.GetDragonData(_rewardData.sku);

				// Initialize dragon info
				if(m_dragonName != null) m_dragonName.Localize("TID_DRAGON_UNLOCK", dragonData.def.GetLocalized("tidName"));
				if(m_dragonDesc != null) m_dragonDesc.Localize(dragonData.def.GetAsString("tidDesc"));
				if(m_dragonTierIcon != null) m_dragonTierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, dragonData.tierDef.GetAsString("icon"));
				if(m_healthText != null) m_healthText.text = StringUtils.FormatNumber(dragonData.maxHealth, 0);
				if(m_energyText != null) m_energyText.text = StringUtils.FormatNumber(dragonData.baseEnergy, 0);
				if(m_speedText != null) m_speedText.text = StringUtils.FormatNumber(dragonData.maxSpeed * 10f, 0);  // x10 to show nicer numbers

				// If the unlocked dragon is of different tier as the dragon used to unlocked it, show 'new preys' banner
				if(m_newPreysAnimator != null) {
					DefinitionNode previousDragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, dragonData.def.GetAsString("previousDragonSku"));
					if(previousDragonDef != null && previousDragonDef.Get("tier") != dragonData.tierDef.sku) {
						// Show!
						m_newPreysAnimator.RestartShow();   // Should have the proper delay
					} else {
						// Hide! (no animation)
						m_newPreysAnimator.ForceHide(false);
					}
				}

				// Show share button!
				showShareButton = !_rewardData.WillBeReplaced();
			} break;

			// Golden Fragments
			case Metagame.RewardGoldenFragments.TYPE_CODE: {
				// Title - singular?
				string tid = (_rewardData.amount == 1) ? "TID_EGG_REWARD_FRAGMENT_SING" : "TID_EGG_REWARD_FRAGMENT";
				m_goldenFragmentTitle.Localize(tid, StringUtils.FormatNumber(_rewardData.amount));	// %U0 Golden Egg Fragments
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
		if(m_extraInfoText != null) {
			m_extraInfoText.gameObject.SetActive(!string.IsNullOrEmpty(_extraInfo));
			m_extraInfoText.text = _extraInfo;
		}

		// Share button
		showShareButton &= ShareButton.CanBeDisplayed();
		if(m_shareButton != null) m_shareButton.gameObject.SetActive(showShareButton);

		// Program finish callback
		UbiBCN.CoroutineManager.DelayedCall(OnAnimationFinished, totalAnimDuration, false);
	}

	/// <summary>
	/// Toggle displayed info based on reward type. Triggers animation corresponding to that type.
	/// </summary>
	/// <param name="_rewardType">Type of reward whose info we want to show. Use empty string to hide everything.</param>
	public void SetRewardType(string _rewardType) {
		// Hide extra info, will be activated if needed
		if(m_extraInfoText != null) m_extraInfoText.gameObject.SetActive(false);

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
					PopupManager.EnqueuePopup(PopupHalloweenPetInfo.PATH);
				}
			} break;
		}

		// Notify listeners
		OnAnimFinished.Invoke();
	}

	/// <summary>
	/// Share button has been pressed.
	/// </summary>
	public void OnShareButton() {
		// Ignore if unknown reward
		if(m_reward == null) return;

		// Grab some shared vars
		// Get the share screen instance and initialize it with current data
		// Different share screens based on reward type
		switch(m_reward.type) {
			case Metagame.RewardPet.TYPE_CODE: {
				ShareScreenPet shareScreen = ShareScreensManager.GetShareScreen("pet_acquired") as ShareScreenPet;
				shareScreen.Init(
					"pet_acquired",
					SceneController.GetMainCameraForCurrentScene(),
					m_reward.sku,
					null
				);
				shareScreen.TakePicture();
			} break;

			case Metagame.RewardSkin.TYPE_CODE: {
				// Initialize and open share screen
				ShareScreenDragon shareScreen = ShareScreensManager.GetShareScreen("skin_acquired") as ShareScreenDragon;
				shareScreen.Init(
					"skin_acquired",
					SceneController.GetMainCameraForCurrentScene(),
					IDragonData.CreateFromSkin(m_reward.sku),   // Create a sample dragon data object to initialize the share screen
					true,
					null
				);
				shareScreen.TakePicture();
			} break;

			case Metagame.RewardDragon.TYPE_CODE: {
				// Create a sample dragon data object to initialize the share screen
				IDragonData sampleData = IDragonData.CreateFromDef(m_reward.def);

				// Initialize and open share screen
				ShareScreenDragon shareScreen = ShareScreensManager.GetShareScreen("dragon_acquired") as ShareScreenDragon;
				shareScreen.Init(
					"dragon_acquired",
					SceneController.GetMainCameraForCurrentScene(),
					sampleData,
					true,
					null
				);
				shareScreen.TakePicture();
			} break;
		}
	}

    /// <summary>
    /// Just play an SFX.
    /// </summary>
    /// <param name="_id"></param>
    public void PlaySFX(string _id) {
        if(!string.IsNullOrEmpty(_id)) {
            AudioController.Play(_id);
        }
    }
}