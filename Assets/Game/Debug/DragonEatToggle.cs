using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonEatToggle : MonoBehaviour {

	private bool m_enabled;
	private Toggle m_toggle;
	private DragonEatBehaviour m_eatComponent;
	
	void Start() {
		m_toggle = GetComponent<Toggle>();
		if (InstanceManager.player != null) {
			m_eatComponent = InstanceManager.player.GetComponent<DragonEatBehaviour>();
		}
		m_enabled = true;
	}
	
	public void OnToggleChange(bool _value) {
		if (m_eatComponent != null) {
			m_eatComponent.enabled = _value;
			m_enabled = _value;
		}
	}
	
	void Update() {
		if (m_eatComponent != null) {
			if (!m_enabled) {
				m_eatComponent.enabled = false;
			}
		}
	}
}
