using System;
using UnityEngine;

public class TrackerBaseDistance : TrackerBase {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	protected bool m_updateDistance;
	private float m_deltaDistance;

	private Transform m_playerTransform;
	private Vector3 m_lastPosition;



	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerBaseDistance() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBaseDistance() {

	}

	protected void UpdateLastPosition() {
		m_lastPosition = m_playerTransform.position;
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	public override void Clear() {
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);

		m_playerTransform = null;

		m_lastPosition = GameConstants.Vector3.zero;
		m_updateDistance = false;
		m_deltaDistance = 0f;

		base.Clear();
	}



	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	protected virtual void OnGameStarted() {
		m_updateDistance = false;
		m_deltaDistance = 0f;

		m_playerTransform = InstanceManager.player.transform;
		m_lastPosition = m_playerTransform.position;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnGameUpdated() {
		// We'll receive this event only while the game is actually running, so no need to check anything
		if (m_updateDistance) {
			m_deltaDistance += (m_playerTransform.position - m_lastPosition).magnitude;

			long integerPart = (long)m_deltaDistance;
			m_deltaDistance -= integerPart;

			currentValue += integerPart;

			UpdateLastPosition();
		}
	}
}
