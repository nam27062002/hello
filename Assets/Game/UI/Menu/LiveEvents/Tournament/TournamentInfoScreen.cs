using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentInfoScreen : MonoBehaviour {
	//----------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 1f;	// Seconds

	//----------------------------------------------------------------//

	[SerializeField] private Localizer m_titleText;
	[SerializeField] private TextMeshProUGUI m_timerText;

	[SeparatorAttribute("Goal")]
	[SerializeField] private Localizer m_goalText;
	[SerializeField] private Image m_goalIcon;

	[SeparatorAttribute("Modifiers")]
	[SerializeField] private ModifierIcon[] m_modifier;

	[SeparatorAttribute("Location")]
	[SerializeField] private GameObject m_mapContainer;
	[SerializeField] private TextMeshProUGUI m_areaText;
	[SerializeField] private Image m_areaIcon;

	[SeparatorAttribute("Leaderboard")]
	[SerializeField] private TournamentLeaderboardView m_leaderboard;


	//----------------------------------------------------------------//
	private HDTournamentManager m_tournament;
	private HDTournamentDefinition m_definition;

	//----------------------------------------------------------------//


	//TEMP
	void Refresh() {
		m_tournament = HDLiveEventsManager.instance.m_tournament;
		m_definition = m_tournament.data.definition as HDTournamentDefinition;

		if (m_definition != null) {
			//TITLE
			m_titleText.Localize(m_definition.m_name);

			//GOALS
			m_goalText.Localize(m_definition.m_goal.m_desc);
			m_goalIcon.sprite = Resources.Load<Sprite>(UIConstants.LIVE_EVENTS_ICONS_PATH + m_definition.m_goal.m_icon);

			//MODIFIERS
			List<IModifierDefinition> mods = new List<IModifierDefinition>();
			for (int i = 0; i < m_definition.m_dragonMods.Count; ++i) {
				mods.Add(m_definition.m_dragonMods[i]);
			}

			for (int i = 0; i < m_definition.m_otherMods.Count; ++i) {
				mods.Add(m_definition.m_otherMods[i]);
			}

			for (int i = 0; i < m_modifier.Length; ++i) {
				if (i < mods.Count) {
					m_modifier[i].InitFromDefinition(mods[i]);
				} else {
					m_modifier[i].gameObject.SetActive(false);
				}
			}

			//MAP
			m_areaText.text = m_definition.m_goal.m_area;


			m_leaderboard.Refresh();
			/*
			//LEADERBOARD
			if (m_tournament.data.m_state <= HDLiveEventData.State.NOT_JOINED) {
				m_leaderboard.gameObject.SetActive(false);
			} else {
				m_leaderboard.gameObject.SetActive(true);
			}*/


			//TIMER
			UpdatePeriodic();
		}
	}

	// Update timers periodically
	void UpdatePeriodic() {
		if (m_definition != null) {
			m_timerText.text = "Ends in: " + TimeUtils.FormatTime(m_definition.timeToEnd.TotalSeconds, TimeUtils.EFormat.DIGITS, 4, TimeUtils.EPrecision.DAYS, true);	// [AOC] HARDCODED!!
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a refresh every time we enter the tab!
	/// </summary>
	public void OnShowPreAnimation() {
		Refresh();

		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);
	}
}
