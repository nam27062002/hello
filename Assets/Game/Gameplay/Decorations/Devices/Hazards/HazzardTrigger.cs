using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazzardTrigger : MonoBehaviour {

	private bool m_triggered;
	public bool isTriggered { get { return m_triggered; } }

	// Use this for initialization
	void Start () {
		m_triggered = false;
	}

	private void OnTriggerEnter(Collider _collider) {
		m_triggered = !m_triggered;
	}

	private void OnDrawGizmos() {
		Gizmos.color = Colors.silver;
		Gizmos.DrawWireCube(transform.position, transform.localScale);
	}
}
