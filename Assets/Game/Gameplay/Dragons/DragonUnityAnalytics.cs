﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonUnityAnalytics : MonoBehaviour {

	private Transform m_player;
	private bool m_track = false;
	private GameSceneController m_gameSceneController;
	private float m_timer = 0;

	// Use this for initialization
	void Start () {
		m_player = InstanceManager.player.transform;
		m_gameSceneController = InstanceManager.gameSceneController;
		m_track = m_gameSceneController != null;
		if ( m_track )
		{
			Messenger.AddListener<bool, DragonBreathBehaviour.Type>(MessengerEvents.FURY_RUSH_TOGGLED, OnFuryRushToggled);
			Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
			Messenger.AddListener(MessengerEvents.PLAYER_DIED, OnPlayerDied);
		}
		else
		{
			enabled = false;
		}
	}

	void OnDestroy()
	{
		if ( m_track )
		{
			Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(MessengerEvents.FURY_RUSH_TOGGLED, OnFuryRushToggled);
			Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
			Messenger.RemoveListener(MessengerEvents.PLAYER_DIED, OnPlayerDied);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (m_track)
		{
			m_timer -= Time.deltaTime;
			if ( m_timer <= 0 )
			{
				UnityAnalyticsHeatmap.HeatmapEvent.Send( "PlayerPosition", m_player.position, m_player.position + m_player.forward, m_gameSceneController.elapsedSeconds);
				m_timer = 0.5f;
			}
		}
	}

	void OnFuryRushToggled( bool _isOn, DragonBreathBehaviour.Type _type)
	{
		if ( m_track )
		{
			switch( _type )
			{
				case DragonBreathBehaviour.Type.Standard:
				{
					UnityAnalyticsHeatmap.HeatmapEvent.Send( "FireStandard", m_player.position, m_gameSceneController.elapsedSeconds);
				}break;
				case DragonBreathBehaviour.Type.Mega:
				{
					UnityAnalyticsHeatmap.HeatmapEvent.Send( "FireMega", m_player.position, m_gameSceneController.elapsedSeconds);
				}break;
			}
		}
	}

	void OnPlayerKo(DamageType _type, Transform _transform)
	{
		if ( m_track )
			UnityAnalyticsHeatmap.HeatmapEvent.Send( "PlayerKo", m_player.position, m_gameSceneController.elapsedSeconds);
	}

	void OnPlayerDied()
	{
		if ( m_track )
			UnityAnalyticsHeatmap.HeatmapEvent.Send( "PlayerDied", m_player.position, m_gameSceneController.elapsedSeconds);
	}
}
