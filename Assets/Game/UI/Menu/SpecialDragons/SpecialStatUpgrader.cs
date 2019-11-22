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
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Standalone widget to control the logic of upgrading a Special Dragon's Stat.
/// </summary>
public class SpecialStatUpgrader : ISpecialDragonUpgrader {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum AnimState {
		READY = 0,
		LOCKED = 1,
		MAXED = 2
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[HideEnumValues(false, true)]
	[SerializeField] private DragonDataSpecial.Stat m_stat = DragonDataSpecial.Stat.HEALTH;
	[Space]
	[SerializeField] private Image m_progressBar = null;
	[Comment("Children will be considered separators. If more separators are needed, last child will be clone as many times as neede.")]
	[SerializeField] private CircularLayout m_separatorsContainer = null;
    [Space]
    [SerializeField] private Image[] m_icons = new Image [0];
    [SerializeField] private TextMeshProUGUI m_valueText = null;
    
    // Visibility
    [Space]
    [SerializeField] private GameObject m_maxedGroup;
    [SerializeField] private GameObject m_maxButton;
    [SerializeField] private GameObject m_activeGroup;
    [SerializeField] private GameObject m_upgradeButton;
    
    // Internal data
	private DragonStatData m_statData = null;

	// Internal references
	private GameObject m_separatorPrefab = null;
	private List<GameObject> m_separators = new List<GameObject>();


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
	protected override void Awake() {
		// Call parent
		base.Awake();

		// Cache existing separators
		if(m_separatorsContainer != null) {
			int childCount = m_separatorsContainer.transform.childCount;
			for(int i = 0; i < childCount; ++i) {
				m_separators.Add(m_separatorsContainer.transform.GetChild(i).gameObject);
			}

			// Store last child as separator prefab
			m_separatorPrefab = m_separators.Last();
		}
    }

    //------------------------------------------------------------------------//
    // PARENT OVERRIDES														  //
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
	public override void InitFromData(DragonDataSpecial _dragonData) {
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
        for (int i=0;i<m_icons.Length;i++)
        {
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

	}

	/// <summary>
	/// Update visuals with current data.
	/// </summary>
	/// <param name="_animate">Trigger animations?</param>
	public override void Refresh(bool _animate) {
		// Nothing to do if either dragon or stat data are not valid
		if(m_dragonData == null || m_statData == null) return;


        // Hide button if the next upgrade unlocks a new power
        if (m_showHide != null)
        {
            if (m_dragonData.IsUnlockingNewPower())
            {
                m_showHide.Hide(false);
                return;
            }
            else
            {
                if (_animate)
                {
                    m_showHide.RestartShow();
                }
                else
                {
                    m_showHide.Show(false);
                }
                
            }
        }

		// Refresh progress var value
		if(m_progressBar != null) {
			// [AOC] Match separator's circular layout min and max angles
			float from = m_progressBar.fillAmount;
			float targetAngle = Mathf.Lerp(m_separatorsContainer.minAngle, m_separatorsContainer.maxAngle, m_statData.progress);
			float fillAmount = Mathf.InverseLerp(0f, 360f, targetAngle);
			if(_animate) {
				m_progressBar.DOKill();
				m_progressBar.DOFillAmount(fillAmount, 0.15f);
			} else {
				m_progressBar.fillAmount = fillAmount;
			}
		}

        // Counter text
        if (m_valueText != null)
        {
            m_valueText.text = m_dragonData.GetStat(m_stat).level + "/" + m_dragonData.GetStat(m_stat).maxLevel;
        }

        // Refresh items visibility depending if the stat has reached its maximum level
        bool statMaxed = m_dragonData.GetStat(m_stat).IsMaxed();

        m_upgradeButton.SetActive(!statMaxed);
        m_activeGroup.SetActive(!statMaxed);
        
        m_maxedGroup.SetActive(statMaxed);
        m_maxButton.SetActive(statMaxed);


		// Refresh upgrade price
		RefreshPrice();
	}

	/// <summary>
	/// Get the price for this upgrade.
	/// </summary>
	/// <returns>The price of this upgrade.</returns>
	public override Price GetPrice() {
		return m_dragonData.GetNextStatUpgradePrice(m_stat);
	}

	/// <summary>
	/// Check the non-generic conditions needed for this upgrader to upgrade.
	/// </summary>
	/// <returns>Whether the upgrader can upgrade or not.</returns>
	public override bool CanUpgrade() {
		// Nothing to do if either dragon or stat data are not valid
		if(m_statData == null) return false;

		// If next level is unlocking a new power, something went wrong
		if(m_dragonData.IsUnlockingNewPower()) return false;

		// All checks passed!
		return true;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The upgrade purchase has been successful.
	/// </summary>
	/// <param name="_flow">The Resources Flow that triggered the event.</param>
	protected override void OnUpgradePurchaseSuccess(ResourcesFlow _flow) {
		// Let parent do its job
		base.OnUpgradePurchaseSuccess(_flow);

		// Do it
		// Visuals will get refreshed when receiving the SPECIAL_DRAGON_STAT_UPGRADED event
		m_dragonData.UpgradeStat(m_stat);
	}

    /// <summary>
    /// The value number animator needs to format a new value.
    /// </summary>
    /// <param name="_animator">The number animator requesting the formatting.</param>
    private void OnSetValueText(NumberTextAnimator _animator) {
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