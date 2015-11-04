using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonFireToggle : MonoBehaviour {

	private bool m_enabled;
	private DragonBreathBehaviour m_breathComponent;
	
	void Start() {
		if (InstanceManager.player != null) {
			m_breathComponent = InstanceManager.player.GetComponent<DragonBreathBehaviour>();
		}
		m_enabled = true;
	}
	
	public void OnToggleChange(bool _value) {
		if (m_breathComponent != null) {
			m_breathComponent.enabled = _value;
			m_enabled = _value;
		}
	}
	
	void Update() {
		if (m_breathComponent != null) {
			if (!m_enabled) {
				m_breathComponent.enabled = false;
			}
		}
	}
}
