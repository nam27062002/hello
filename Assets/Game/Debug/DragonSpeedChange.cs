using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonSpeedChange : MonoBehaviour {

	void Start() {
		if (InstanceManager.player != null) {
			Slider slider = GetComponent<Slider>();
			slider.value = InstanceManager.player.data.speed.value;
		}
	}

	public void SetSpeed(float _speed) {
		if (InstanceManager.player != null) {
			InstanceManager.player.data.OverrideSpeedValue(_speed);
		}
	}
}
