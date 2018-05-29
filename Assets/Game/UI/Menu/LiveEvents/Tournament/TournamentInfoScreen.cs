using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentInfoScreen : MonoBehaviour {
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

	//----------------------------------------------------------------//

	private HDTournamentDefinition m_definition;

	private float m_elapsedTime;

	//----------------------------------------------------------------//


	void Awake() {
		Refresh();
	}

	//TEMP
	void Refresh() {
		HDTournamentManager tournament = HDLiveEventsManager.instance.m_tournament;
		m_definition = tournament.data.definition as HDTournamentDefinition;

		//TITLE
		m_titleText.Localize(m_definition.m_name);
		m_timerText.text = "w.i.p.";

		m_timerText.text = "w.i.p: ";


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

		//
		m_elapsedTime = 0f;
	}

	// Update timers!
	void Update() {
		m_elapsedTime += Time.deltaTime;

		if (m_elapsedTime >= 1f) {
			System.DateTime end = m_definition.m_endTimestamp;
			System.DateTime now = System.DateTime.Now;
			System.TimeSpan delta = end - now;

			m_timerText.text = "End in: " + TimeUtils.FormatTime(delta.TotalSeconds, TimeUtils.EFormat.DIGITS, 4, TimeUtils.EPrecision.DAYS, true);

			m_elapsedTime -= 1f;
		}
	}
}
