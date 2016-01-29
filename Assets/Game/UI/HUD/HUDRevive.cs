using UnityEngine;
using System.Collections;

public class HUDRevive : MonoBehaviour {

	[SerializeField] private GameObject m_reviveButton;
	[SerializeField] private float m_reviveAvailableSecs = 5f;

	private float m_timer;

	// Use this for initialization
	void Start() {
		Messenger.AddListener(GameEvents.PLAYER_KO, OnPlayerKo);
		m_timer = 0;
	}

	void OnEnable() {
		m_reviveButton.SetActive(false);
	}

	void OnDisable() {
		Messenger.RemoveListener(GameEvents.PLAYER_KO, OnPlayerKo);
	}

	// Update is called once per frame
	void Update() {
		if (m_timer > 0) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_timer = 0;
				m_reviveButton.SetActive(false);
				Messenger.Broadcast(GameEvents.PLAYER_DIED);
			}
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	public void OnRevive() {
		InstanceManager.player.ResetStats(true);
		m_reviveButton.SetActive(false);
		m_timer = 0;
	}

	private void OnPlayerKo() {
		m_reviveButton.SetActive(true);
		m_timer = m_reviveAvailableSecs;
	}

}
