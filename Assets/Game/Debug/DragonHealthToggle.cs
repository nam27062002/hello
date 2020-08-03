using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonHealthToggle : MonoBehaviour {

	private bool m_enabled;
	private DragonHealthBehaviour m_healthComponent;

	void Start() {
		if (InstanceManager.player != null) {
			m_healthComponent = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
		}
		m_enabled = true;
	}

	public void OnToggleChange(bool _value) {
		if (m_healthComponent != null) {
			m_healthComponent.enabled = _value;
			m_enabled = _value;
		}
	}

	void Update() {
		if (m_healthComponent != null) {
			if (!m_enabled) {
				m_healthComponent.enabled = false;
			}
		}
	}
}
