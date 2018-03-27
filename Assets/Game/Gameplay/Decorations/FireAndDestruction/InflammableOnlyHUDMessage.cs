using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflammableOnlyHUDMessage : MonoBehaviour {

	private float m_timer = 0f;

	void Update() {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (m_timer <= 0f) {
			if (collision.transform.CompareTag("Player")) {
				Messenger.Broadcast(MessengerEvents.BREAK_OBJECT_WITH_FIRE);
				m_timer = 3f;
			}
		}
	}
}
