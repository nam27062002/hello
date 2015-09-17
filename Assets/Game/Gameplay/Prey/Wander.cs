using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(PreyBehaviour))]
public class Wander : Steering {


	[SerializeField] private float m_changeTargetTime;


	private float m_timer;
	private Vector2 m_target;


	void Update() {

		if (m_prey.area != null) {
			// Update Wander behaviour
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				
				m_target = m_prey.area.RandomInside();
				m_timer = m_changeTargetTime;
			}
		}
	}

	public Vector2 GetTarget() {
		return m_target;
	}
}
