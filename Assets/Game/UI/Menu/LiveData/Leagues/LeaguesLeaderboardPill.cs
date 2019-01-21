// GlobalEventsLeaderboardPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
public enum LeagueLeaderboardAreas {
    Promotion = 0,
    Default,
    Demotion
}

/// <summary>
/// Data class.
/// </summary>
public class LeaguesLeaderboardPillData {
	public HDLiveData.Leaderboard.Record record = null;
    public Metagame.Reward reward = null;
    public LeagueLeaderboardAreas area = LeagueLeaderboardAreas.Default;
}

/// <summary>
/// Item class.
/// </summary>
public class LeaguesLeaderboardPill : ScrollRectItem<LeaguesLeaderboardPillData> {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed members
    [Space]
    [SerializeField] private TextMeshProUGUI m_rankText = null;
	[SerializeField] private Text m_nameText = null;	// [AOC] Name text uses a dynamic font, so any special character should be properly displayed. On the other hand, instantiation time is increased for each pill containing non-cached characters.
    [SerializeField] private TextMeshProUGUI m_scoreText = null;

    [Space]
    [SerializeField] private Image m_pillBGImage = null;
    [Tooltip("Special colors for promotion / demotion positions!")]
    [SerializeField] private Color m_promotedColor = Color.white;
    [SerializeField] private Color m_defaultColor = Color.white;
    [SerializeField] private Color m_demotedColor = Color.white;

    [Space]
    [SerializeField] private Image m_rewardIcon = null;
    [SerializeField] private TextMeshProUGUI m_rewardText = null;

    [Space]
    [SerializeField] private Image m_dragonIcon = null;
    [SerializeField] private TextMeshProUGUI m_levelText = null;

	[Space]
	[SerializeField] private UITooltipTrigger m_tooltipTrigger = null;

	// Internal
	private LeaguesLeaderboardPillData m_lastUsedData = null;

    //------------------------------------------------------------------------//
	// ScrollRectItem IMPLEMENTATION										  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize the pill with the given user data.
    /// </summary>
    /// <param name="_data">The user to be displayed in the pill.</param>
    public override void InitWithData(LeaguesLeaderboardPillData _data) {
        // Ranking info
		// We might not get a valid position if the player hasn't yet participated in the event
		if(_data.record.rank >= 0) {
            m_rankText.text = StringUtils.FormatNumber(_data.record.rank + 1);
		} else {
            m_rankText.text = "?";
		}

		if(m_nameText != null) {
			m_nameText.text = _data.record.name;
		}

		if(m_scoreText != null) {
			m_scoreText.text = StringUtils.FormatNumber(_data.record.score);
		}

		if(m_pillBGImage != null) {
			switch(_data.area) {
				case LeagueLeaderboardAreas.Promotion: m_pillBGImage.color = m_promotedColor; break;
				case LeagueLeaderboardAreas.Default: m_pillBGImage.color = m_defaultColor; break;
				case LeagueLeaderboardAreas.Demotion: m_pillBGImage.color = m_demotedColor; break;
			}
		}

        // Reward
        if (_data.reward != null) {
            m_rewardIcon.sprite = UIConstants.GetIconSprite(UIConstants.GetCurrencyIcon(_data.reward.currency));
            m_rewardText.text = StringUtils.FormatNumber(_data.reward.amount);
        }

        // Build
        DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _data.record.build.skin);
		string icon = IDragonData.DEFAULT_SKIN_ICON;
        if (skinDef != null) {
            icon = skinDef.Get("icon");
        }
        m_dragonIcon.sprite = Resources.Load<Sprite>(UIConstants.DISGUISE_ICONS_PATH + _data.record.build.dragon + "/" + icon);

        m_levelText.text = StringUtils.FormatNumber(_data.record.build.level);

		// Store last used data
		m_lastUsedData = _data;
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_index"></param>
	public override void Animate(int _index) {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Setup a tooltip to be triggered by this pill's tooltip trigger.
	/// </summary>
	/// <param name="_tooltip">Tooltip to be setup.</param>
	public void SetupTooltip(LeaguesPlayerInfoTooltip _tooltip) {
		// Ignore if tooltip trigger not defined
		if(m_tooltipTrigger == null) return;
		
		// Link the given tooltip to the trigger
		m_tooltipTrigger.tooltip = _tooltip;

		// Listen to tooltip's open event
		m_tooltipTrigger.OnTooltipOpen.AddListener(OnTooltipOpen);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A tooltip is about to be opened.
	/// </summary>
	/// <param name="_tooltip">Tooltip that will be opened.</param>
	/// <param name="_trigger">The one who triggered the tooltip.</param>
	private void OnTooltipOpen(UITooltip _tooltip, UITooltipTrigger _trigger) {
		// Ignore if it comes from another trigger
		if(_trigger != m_tooltipTrigger) return;

		// Ignore if we have no data to display
		if(m_lastUsedData == null) return;

		// Initialize tooltip with this pill's player data
		(_tooltip as LeaguesPlayerInfoTooltip).Init(m_lastUsedData.record);
	}
}