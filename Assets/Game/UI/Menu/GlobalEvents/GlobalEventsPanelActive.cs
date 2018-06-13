// GlobalEventsScreenActivePanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Panel corresponding to an active global event.
/// </summary>
public class GlobalEventsPanelActive : GlobalEventsPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float EVENT_COUNTDOWN_UPDATE_INTERVAL = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private TextMeshProUGUI m_objectiveText = null;
	[SerializeField] private Image m_objectiveIcon = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[Space]
	[SerializeField] private GlobalEventsProgressBar m_progressBar = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Program periodic update call
		InvokeRepeating("UpdatePeriodic", 0f, EVENT_COUNTDOWN_UPDATE_INTERVAL);

		Refresh();

		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.QUEST_SCORE_UPDATED, OnEventDataUpdated);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Clear periodic update call
		CancelInvoke();

		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.QUEST_SCORE_UPDATED, OnEventDataUpdated);
	}

	/// <summary>
	/// Called periodically.
	/// </summary>
	private void UpdatePeriodic() {
		// Just in case
		if ( !HDLiveEventsManager.instance.m_quest.EventExists() ) return;

		double remainingTime = System.Math.Max(0, HDLiveEventsManager.instance.m_quest.m_questData.remainingTime.TotalSeconds);

		// Update countdown text
		m_timerText.text = TimeUtils.FormatTime(
			remainingTime,	// Never show negative time!
			TimeUtils.EFormat.ABBREVIATIONS,
			4
		);

		if ( remainingTime <= 0 )
		{
			HDLiveEventsManager.instance.m_quest.UpdateStateFromTimers();
		}

		if ( !HDLiveEventsManager.instance.m_quest.IsRunning() )
		{
			Messenger.Broadcast(MessengerEvents.LIVE_EVENT_STATES_UPDATED);
		}
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh displayed data.
	/// </summary>
	override public void Refresh() {
		// Get current event
		HDQuestManager questManager = HDLiveEventsManager.instance.m_quest;
		if(!questManager.EventExists()) return;

		HDQuestData data = questManager.data as HDQuestData;
		HDQuestDefinition def = data.definition as HDQuestDefinition;

		// Initialize visuals
		// Event description
		m_objectiveText.text = questManager.GetGoalDescription();

		// Target icon
		m_objectiveIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + def.m_goal.m_icon);

		// Progress
		if (m_progressBar != null) {
			m_progressBar.RefreshRewards( def, questManager.m_questData.m_globalScore );
			m_progressBar.RefreshProgress( questManager.progress );
		}

		// Force a first update on the timer
		UpdatePeriodic();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// We have new data concerning the event.
	/// </summary>
	/// <param name="_requestType">Request type.</param>
	private void OnEventDataUpdated() {
		// Nothing to do if disabled
		if(!isActiveAndEnabled) return;

		Refresh();

	}


	public void MoveScoreTo( long _from, long _to )
	{
		StartCoroutine( GoingUp( _from, _to ) );
	}

	IEnumerator GoingUp( long _from, long _to )
	{
		HDQuestManager questManager = HDLiveEventsManager.instance.m_quest;
		if(questManager.EventExists())
		{
			HDQuestData data = questManager.data as HDQuestData;
			HDQuestDefinition def = data.definition as HDQuestDefinition;

			// Starting setup
			if (m_progressBar != null) 
			{
				m_progressBar.RefreshRewards( def, _from );
				m_progressBar.RefreshProgress( _from / (float) def.m_goal.m_amount );
			}
			yield return null;
			float duration = 0.2f;
			float t = 0;
			while( t < duration)
			{
				t += Time.deltaTime;
				long v = _from + (long)((_to - _from) * (t / duration));
				if (m_progressBar != null) 
				{
					m_progressBar.RefreshAchieved( def, v );
					m_progressBar.RefreshProgress( v / (float) def.m_goal.m_amount );
				}
				yield return null;
			}

			if (m_progressBar != null) 
			{
				m_progressBar.RefreshAchieved( def, _to );
				m_progressBar.RefreshProgress( _to / (float) def.m_goal.m_amount );
			}
		}
	}

}