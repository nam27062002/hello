// LabStatUpgrader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Standalone widget to control the logic of upgrading a Special Dragon's Stat.
/// </summary>
public class LabStatUpgrader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum AnimState {
		READY = 0,
		LOCKED = 1,
		MAXED = 2
	}

	private enum Mode {
		PERCENTAGE_BONUS,
		ABSOLUTE_VALUE,
		LEVEL_PROGRESSION
	}
	private const Mode MODE = Mode.LEVEL_PROGRESSION;

	private const string ANIM_STATE_PARAM_ID = "state";


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[HideEnumValues(false, true)]
	[SerializeField] private DragonDataSpecial.Stat m_stat = DragonDataSpecial.Stat.HEALTH;
	[Space]
	[SerializeField] private Animator m_stateAnimator = null;
	[Space]
	[SerializeField] private Image m_progressBar = null;
	[Comment("Children will be considered separators. If more separators are needed, last child will be clone as many times as neede.")]
	[SerializeField] private CircularLayout m_separatorsContainer = null;
	[Space]
	[SerializeField] private Image[] m_icons = new Image[0];
    [SerializeField] private TextMeshProUGUI m_counterText = null;
    [SerializeField] private Localizer m_priceText = null;
	[SerializeField] private NumberTextAnimator m_valueText = null;
	[SerializeField] private RectTransform m_feedbackAnchor = null;

	// Internal data
	private DragonDataSpecial m_dragonData = null;
	private DragonStatData m_statData = null;

	// Internal references
	private GameObject m_separatorPrefab = null;
	private List<GameObject> m_separators = new List<GameObject>();
    private ShowHideAnimator m_showHide = null;

	// Other internal vars
	private string m_formattedStepValue = "";

	// Tooltip
	private UITooltipTrigger m_trigger = null;
	private UITooltipTrigger trigger {
		get {
			if(m_trigger == null) m_trigger = GetComponentInChildren<UITooltipTrigger>();
			return m_trigger;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Cache existing separators
		if(m_separatorsContainer != null) {
			int childCount = m_separatorsContainer.transform.childCount;
			for(int i = 0; i < childCount; ++i) {
				m_separators.Add(m_separatorsContainer.transform.GetChild(i).gameObject);
			}

			// Store last child as separator prefab
			m_separatorPrefab = m_separators.Last();
		}

		// Custom value formatting
		if(m_valueText != null) {
			m_valueText.CustomTextSetter = OnSetValueText;
		}

        // Cache showHideAnimator
        m_showHide = GetComponent<ShowHideAnimator>();

        // Subscribe to external events
        Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnDragonLevelUpgraded);

        // Make sure we're displaying the right info
        // [AOC] Delay by one frame to do it when the object is actually enabled
        UbiBCN.CoroutineManager.DelayedCallByFrames(
            () => { Refresh(false); },
            1
        );

    }


    public void OnShowPreAnimation()
    {
        Refresh(false);
    }



    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize with the given dragon data and stat type.
    /// </summary>
    /// <param name="_dragonData">Dragon data to be used.</param>
    /// <param name="_stat">Stat to be used.</param>
    public void InitFromData(DragonDataSpecial _dragonData, DragonDataSpecial.Stat _stat) {
		// Overwrite default stat and use default initializer
		m_stat = _stat;
		InitFromData(_dragonData);
	}

    

	/// <summary>
	/// Initialize with the given dragon data pre-defined stat type.
	/// </summary>
	/// <param name="_dragonData">Dragon data to be used.</param>
	public void InitFromData(DragonDataSpecial _dragonData) {
		// Store data references
		// Check for invalid params
		if(_dragonData == null || m_stat == DragonDataSpecial.Stat.COUNT) {
			m_dragonData = null;
			m_statData = null;
			this.gameObject.SetActive(false);
		} else {
			m_dragonData = _dragonData;
			m_statData = m_dragonData.GetStat(m_stat);
			this.gameObject.SetActive(true);
		}

		// Nothing else to do if either dragon or stat data are not valid
		if(m_dragonData == null || m_statData == null) return;

		// Stat icon
		Sprite iconSprite = Resources.Load<Sprite>(UIConstants.DRAGON_STATS_ICONS_PATH + m_statData.def.GetAsString("icon"));
		for(int i = 0; i < m_icons.Length; ++i) {
			m_icons[i].sprite = iconSprite;
		}

		// Initialize progress bar separators
		if(m_separatorsContainer != null) {
			// Toggle existing separators
			int numSeparators = m_statData.maxLevel + 1;    // [AOC] One extra separator for the last level
			for(int i = 0; i < numSeparators; ++i) {
				// If not enough separators, instantiate a new one
				if(i >= m_separators.Count) {
					if(m_separatorPrefab != null) {
						GameObject newSeparator = GameObject.Instantiate<GameObject>(m_separatorPrefab, m_separatorsContainer.transform, false);
						newSeparator.SetActive(true);
						m_separators.Add(newSeparator);
					}
				} else {
					m_separators[i].SetActive(true);
				}
			}

			// Hide remaining separators
			for(int i = numSeparators; i < m_separators.Count; ++i) {
				m_separators[i].SetActive(false);
			}
		}

		// Update visuals
		Refresh(false);
	}

	/// <summary>
	/// Update visuals with current data.
	/// </summary>
	/// <param name="_animate">Trigger animations?</param>
	public void Refresh(bool _animate) {
		// Nothing to do if either dragon or stat data are not valid
		if(m_dragonData == null || m_statData == null) return;


        // Hide button if the next upgrade unlocks a new power
        if (m_showHide != null)
        {
            if (m_dragonData.IsUnlockingNewPower())
            {
                m_showHide.Hide();
                return;
            }
            else
            {
                m_showHide.Show();
            }
        }

		// Refresh progress var value
		if(m_progressBar != null) {
			// [AOC] Match separator's circular layout min and max angles
			float from = m_progressBar.fillAmount;
			float targetAngle = m_separatorsContainer.angleRange.Lerp(m_statData.progress);
			float fillAmount = Mathf.InverseLerp(0f, 360f, targetAngle);
			if(_animate) {
				m_progressBar.DOKill();
				m_progressBar.DOFillAmount(fillAmount, 0.15f);
			} else {
				m_progressBar.fillAmount = fillAmount;
			}
		}

		// Refresh value text
		if(m_valueText != null) {
			switch(MODE) {
				case Mode.PERCENTAGE_BONUS: {
					// [AOC] Because number animator only works with longs, convert to 100 to have double digit precision.
					//       The custom text formatter will properly display the percentage amount
					long longValue = (long)Mathf.RoundToInt(m_statData.value * 100f);
					m_valueText.SetValue(longValue, _animate);
				} break;

				case Mode.ABSOLUTE_VALUE: {
					float value = 0f;
					switch(m_stat) {
						case DragonDataSpecial.Stat.HEALTH: value = m_dragonData.maxHealth; break;
						case DragonDataSpecial.Stat.SPEED: value = m_dragonData.maxSpeed * 10f; break;
						case DragonDataSpecial.Stat.ENERGY: value = m_dragonData.baseEnergy; break;
					}
					long longValue = (long)Mathf.RoundToInt(value);
					m_valueText.SetValue(longValue, _animate);
				} break;

				case Mode.LEVEL_PROGRESSION: {
					long longValue = (long)m_statData.level;
					m_valueText.SetValue(longValue, false);		// [AOC] We're going 1 by 1, so animating looks super-weird
				} break;
			}
		}

        // Counter text
        if (m_counterText != null)
        {
            m_counterText.text = m_dragonData.GetStat(m_stat).level + "/" + m_dragonData.GetStat(m_stat).maxLevel;
        }


        // Refresh upgrade price
        Price upgradePrice = m_dragonData.GetNextUpgradePrice();

        if (m_priceText != null && upgradePrice != null)
            {
            switch (MODE)
            {
                case Mode.PERCENTAGE_BONUS:
                    {
                        m_formattedStepValue = StringUtils.MultiplierToPercentageIncrease(m_statData.valueStep + 1, true);

                        m_priceText.text.text = UIConstants.GetIconString(
                            upgradePrice.Amount.ToString(),
                            UIConstants.GetCurrencyIcon(upgradePrice.Currency),
                            UIConstants.IconAlignment.LEFT
                        );
                    }
                    break;

                case Mode.ABSOLUTE_VALUE:
                    {
                        float baseValue = 0f;
                        switch (m_stat)
                        {
                            case DragonDataSpecial.Stat.HEALTH: baseValue = m_dragonData.specialTierDef.GetAsFloat("health"); break;
                            case DragonDataSpecial.Stat.SPEED: baseValue = m_dragonData.specialTierDef.GetAsFloat("force") * 10f; break;
                            case DragonDataSpecial.Stat.ENERGY: baseValue = m_dragonData.specialTierDef.GetAsFloat("energyBase"); break;
                        }
                        long longValue = (long)Mathf.RoundToInt(baseValue * m_statData.valueStep);
                        m_formattedStepValue = "+" + StringUtils.FormatNumber(longValue);

                        m_priceText.text.text = UIConstants.GetIconString(
                            upgradePrice.Amount.ToString(),
                            UIConstants.GetCurrencyIcon(upgradePrice.Currency),
                            UIConstants.IconAlignment.LEFT
                        );
                    }
                    break;

                case Mode.LEVEL_PROGRESSION:
                    {
                        m_formattedStepValue = string.Empty;

                        // Disable localizer and just show the price
                        m_priceText.enabled = false;
                        m_priceText.text.text = UIConstants.GetIconString(
                            upgradePrice.Amount.ToString(),
                            UIConstants.GetCurrencyIcon(upgradePrice.Currency),
                            UIConstants.IconAlignment.LEFT
                        );
                    }
                    break;
            }
        }

		// Change animation state
		if(m_stateAnimator != null) {
			// Figure out state for this dragon/stat combo
			AnimState state = AnimState.READY;

			// Is it maxed out?
			if(m_statData.level == m_statData.maxLevel) {
				state = AnimState.MAXED;
			}

			// Is it locked? (don't check if maxed out)
			else if(!m_dragonData.CanUpgradeStats()) {
				state = AnimState.LOCKED;
			}

			// Apply!
			m_stateAnimator.SetInteger(ANIM_STATE_PARAM_ID, (int)state);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The upgrade button has been pressed!
	/// </summary>
	public void OnUpgradeButton() {
		// Nothing to do if either dragon or stat data are not valid
		if(m_dragonData == null || m_statData == null) return;

        // OTA: we prevent the upgrade if the asset bundles are not downloaded
        // because some of the upgraded dragons need the downloadable content
        Downloadables.Handle allContentHandle = HDAddressablesManager.Instance.GetHandleForAllDownloadables();

        if (!allContentHandle.IsAvailable())
        {
            // Get the download flow from the parent lab screen
            AssetsDownloadFlow assetsDownloadFlow = InstanceManager.menuSceneController.GetScreenData(MenuScreen.LAB_DRAGON_SELECTION).
                                                        ui.GetComponent<LabDragonSelectionScreen>().assetsDownloadFlow;

            assetsDownloadFlow.InitWithHandle(allContentHandle);
            PopupAssetsDownloadFlow popup = assetsDownloadFlow.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY,
                                                                                AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_UPGRADE_SPECIAL);
            // Abort the upgrade
            return;
        }

        // Get the price of the current upgrade
        Price upgradePrice = m_dragonData.GetNextUpgradePrice();

        // Launch transaction
        if (upgradePrice != null)
        {
            ResourcesFlow purchaseFlow = new ResourcesFlow("UPGRADE_SPECIAL_DRAGON_STAT");
            purchaseFlow.OnSuccess.AddListener(OnUpgradePurchaseSuccess);
            purchaseFlow.Begin(
                System.Convert.ToInt64(upgradePrice.Amount),
                upgradePrice.Currency,
                HDTrackingManager.EEconomyGroup.UNKNOWN, //HDTrackingManager.EEconomyGroup.SPECIAL_DRAGON_UPGRADE,
                null
            );
        }
	}

	/// <summary>
	/// The upgrade purchase has been successful.
	/// </summary>
	/// <param name="_flow">The Resources Flow that triggered the event.</param>
	private void OnUpgradePurchaseSuccess(ResourcesFlow _flow) {
		// Tracking
		// [AOC] TODO!!
		//HDTrackingManager.Instance.Notify_DragonUnlocked(dragonData.def.sku, dragonData.GetOrder());

		// Show a nice feedback animation
		switch(MODE) {
			case Mode.PERCENTAGE_BONUS:
			case Mode.ABSOLUTE_VALUE: {
				// Preformatted step value
				UIFeedbackText.CreateAndLaunch(
					m_formattedStepValue,
					m_feedbackAnchor,
					GameConstants.Vector2.zero,
					m_feedbackAnchor
				);
			} break;

			case Mode.LEVEL_PROGRESSION: {
				// "Level up!"
				UIFeedbackText feedbackText = UIFeedbackText.CreateAndLaunch(
					LocalizationManager.SharedInstance.Localize("TID_FEEDBACK_LEVEL_UP"),
					m_feedbackAnchor,
					GameConstants.Vector2.zero,
					m_feedbackAnchor
				);
                
                Canvas c =feedbackText.GetComponentInParent<Canvas>();
                feedbackText.transform.SetParent(c.transform);
                feedbackText.transform.SetAsLastSibling();    
            
			} break;
		}

		// Trigger SFX
		AudioController.Play("hd_reward_golden_fragments");

		// Do it
		// Visuals will get refreshed when receiving the SPECIAL_DRAGON_STAT_UPGRADED event
		m_dragonData.UpgradeStat(m_stat);
	}

	/// <summary>
	/// A special dragon level has been upgraded.
	/// </summary>
	/// <param name="_dragonData">Target dragon data.</param>
	/// <param name="_stat">Target stat.</param>
	private void OnDragonLevelUpgraded(DragonDataSpecial _dragonData) {
		// Refresh visuals regardles of the stat (we might get locked)
		Refresh(true);
	}

    /// <summary>
    /// The value number animator needs to format a new value.
    /// </summary>
    /// <param name="_animator">The number animator requesting the formatting.</param>
    private void OnSetValueText(NumberTextAnimator _animator) {
		switch(MODE) {
			case Mode.PERCENTAGE_BONUS: {
				// Percentage bonus format
				// [AOC] Because number animator only works with longs, the value is converted to 100s to have double digit precision.
				//       Format it properly
				float value = _animator.currentValue / 100f;
				_animator.text.text = StringUtils.MultiplierToPercentageIncrease(1f + value, true);
			} break;

			case Mode.ABSOLUTE_VALUE: {
				_animator.text.text = StringUtils.FormatNumber(_animator.currentValue);
			} break;

			case Mode.LEVEL_PROGRESSION: {
				if(m_statData == null) {
					_animator.text.text = StringUtils.FormatNumber(_animator.currentValue);
				} else {
					// "5/30"
					_animator.text.text = LocalizationManager.SharedInstance.Localize(
						"TID_FRACTION",
						StringUtils.FormatNumber(_animator.currentValue),
						StringUtils.FormatNumber(m_statData.maxLevel)
					);
				}
			} break;
		}
	}

	/// <summary>
	/// A tooltip is about to be opened.
	/// If the trigger is attached to this object, initialize tooltip with this object's data.
	/// Link it via the inspector.
	/// </summary>
	/// <param name="_tooltip">The tooltip about to be opened.</param>
	/// <param name="_trigger">The button which triggered the event.</param>
	public void OnTooltipOpen(UITooltip _tooltip, UITooltipTrigger _trigger) {
		// Make sure the trigger that opened the tooltip is linked to this object
		if(_trigger != trigger) return;

		// Nothing to do if either dragon or stat data are not valid
		if(m_dragonData == null || m_statData == null) return;

		// Stat name
		// [AOC] Exception for ENERGY stat, who has a custom name for each dragon!
		string statName = m_statData.def.GetLocalized("tidName");
		if(m_stat == DragonDataSpecial.Stat.ENERGY) {
			statName = m_dragonData.def.GetLocalized("tidBoostAction", statName);	// Default to base stat name
		}

		// Stat value
		float value = 0f;
		switch(m_stat) {
			case DragonDataSpecial.Stat.HEALTH: value = m_dragonData.maxHealth; 		break;
			case DragonDataSpecial.Stat.SPEED:	value = m_dragonData.maxSpeed * 10f; 	break;
			case DragonDataSpecial.Stat.ENERGY: value = m_dragonData.baseEnergy; 		break;
		}

		// Initialize tooltip
		_tooltip.InitWithText(
			UIConstants.GetDragonStatColor(m_stat).Tag(statName),	// Stat name of the color of the stat
			StringUtils.FormatNumber(value, 0)
		);
	}
}