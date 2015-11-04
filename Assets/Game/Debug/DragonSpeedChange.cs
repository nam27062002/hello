using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonSpeedChange : MonoBehaviour {

	private float m_speed;

	void Start() {
		if (InstanceManager.player != null) {
			Slider slider = GetComponent<Slider>();
			m_speed = InstanceManager.player.data.speed.value;
			slider.value = m_speed;
		}
	}

	public void SetSpeed(float _speed) {
		if (InstanceManager.player != null) {
			InstanceManager.player.data.OffsetSpeedValue(_speed - m_speed);
			m_speed = _speed;
		}
	}
}
