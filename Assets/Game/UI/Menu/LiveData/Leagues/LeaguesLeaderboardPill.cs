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
    [SerializeField] private TextMeshProUGUI m_nameText = null;
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



    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
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

        m_nameText.text = _data.record.name;   // [AOC] Name text uses a dynamic font, so any special character should be properly displayed. On the other hand, instantiation time is increased for each pill containing non-cached characters.
        m_scoreText.text = StringUtils.FormatNumber(_data.record.score);

        switch (_data.area) {
            case LeagueLeaderboardAreas.Promotion:  m_pillBGImage.color = m_promotedColor; break;
            case LeagueLeaderboardAreas.Default:    m_pillBGImage.color = m_defaultColor; break;
            case LeagueLeaderboardAreas.Demotion:   m_pillBGImage.color = m_demotedColor; break;
        }



        // Reward
        m_rewardIcon.sprite = UIConstants.GetIconSprite(UIConstants.GetCurrencyIcon(_data.reward.currency));
        m_rewardText.text = StringUtils.FormatNumber(_data.reward.amount);


        // Build
        DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _data.record.build.skin);
        m_dragonIcon.sprite = Resources.Load<Sprite>(UIConstants.DISGUISE_ICONS_PATH + _data.record.build.dragon + "/" + skinDef.Get("icon"));

        m_levelText.text = StringUtils.FormatNumber(_data.record.build.level);
    }

	public override void Animate(int _index) {}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}