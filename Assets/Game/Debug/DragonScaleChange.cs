using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonScaleChange : MonoBehaviour {

	void Start() {
		if (InstanceManager.player != null) {
			Slider slider = GetComponent<Slider>();
			slider.value = InstanceManager.player.transform.localScale.x;
		}
	}
	
	public void SetScale(float _scale) {
		if (InstanceManager.player != null) {
			InstanceManager.player.transform.localScale = new Vector3(_scale, _scale, _scale);
		}
	}
}
