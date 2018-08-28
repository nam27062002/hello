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
	[SerializeField] private bool m_updateEventState = false;
	[Space]
	[SerializeField] private TextMeshProUGUI m_objectiveText = null;
	[SerializeField] private Image m_objectiveIcon = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[Space]
	[SerializeField] private GlobalEventsProgressBar m_progressBar = null;
	[SerializeField] private ParticleSystem m_receiveContributionFX = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Program periodic update call
		InvokeRepeating("UpdatePeriodic", 0f, EVENT_COUNTDOWN_UPDATE_INTERVAL);

		//Refresh();

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
		if(m_timerText != null) {
			m_timerText.text = TimeUtils.FormatTime(
				System.Math.Max(0, remainingTime), // Just in case, never go negative
				TimeUtils.EFormat.ABBREVIATIONS,
				4
			);
		}

		// If enabled, update quest state
		if(m_updateEventState) {
			if ( remainingTime <= 0 )
			{
				HDLiveEventsManager.instance.m_quest.UpdateStateFromTimers();
			}

			if ( !HDLiveEventsManager.instance.m_quest.IsRunning() )
			{
				Messenger.Broadcast(MessengerEvents.LIVE_EVENT_STATES_UPDATED);
			}
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
		if(m_objectiveText != null) m_objectiveText.text = questManager.GetGoalDescription();

		// Target icon
		if(m_objectiveIcon != null) m_objectiveIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + def.m_goal.m_icon);

		// Progress
		if (m_progressBar != null) {
			m_progressBar.RefreshRewards( def, questManager.m_questData.m_globalScore );
			m_progressBar.RefreshProgress( questManager.m_questData.m_globalScore );
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

	public void MoveScoreTo(long _to, float _duration) {
		long currentValue = 0;
		if(m_progressBar != null) {
			HDQuestManager questManager = HDLiveEventsManager.instance.m_quest;
			if(questManager.EventExists()) {
				HDQuestData data = questManager.data as HDQuestData;
				HDQuestDefinition def = data.definition as HDQuestDefinition;
				currentValue = (long)m_progressBar.progressBar.value;
			}
		}

		MoveScoreTo(currentValue, _to, _duration);
	}

	public void MoveScoreTo( long _from, long _to, float _duration )
	{
		// If not animating, just set the final value directly
		if(_duration <= 0f) {
			// Set initial value
			HDQuestManager questManager = HDLiveEventsManager.instance.m_quest;
			if(questManager.EventExists()) {
				HDQuestData data = questManager.data as HDQuestData;
				HDQuestDefinition def = data.definition as HDQuestDefinition;
				if(m_progressBar != null) {
					m_progressBar.RefreshRewards( def, _to );
					m_progressBar.RefreshProgress( _to );
				}
			}
		} else {
			StartCoroutine( GoingUp( _from, _to, _duration ) );
		}
	}

	IEnumerator GoingUp( long _from, long _to, float _duration )
	{
		HDQuestManager questManager = HDLiveEventsManager.instance.m_quest;
		if(questManager.EventExists())
		{
			HDQuestData data = questManager.data as HDQuestData;
			HDQuestDefinition def = data.definition as HDQuestDefinition;

			// Start
			if (m_progressBar != null) 
			{
				m_progressBar.RefreshRewards( def, _from );
				m_progressBar.RefreshProgress( _from );
			}

			if(m_receiveContributionFX != null) {
				m_receiveContributionFX.Play(true);
			}
			yield return null;

			// Running
			float t = 0;
			while( t < _duration)
			{
				t += Time.deltaTime;
				long v = _from + (long)((_to - _from) * (t / _duration));
				if (m_progressBar != null) 
				{
					m_progressBar.RefreshProgress( v , -1f, false );
					m_progressBar.RefreshAchieved( true );
				}
				yield return null;
			}

			// Finished!
			if (m_progressBar != null) 
			{
				//m_progressBar.RefreshAchieved( def, _to );
				m_progressBar.RefreshProgress( _to );
			}

			if(m_receiveContributionFX != null) {
				m_receiveContributionFX.Stop();
			}
		}
	}
}