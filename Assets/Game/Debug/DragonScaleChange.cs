using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonScaleChange : MonoBehaviour {

	private Slider m_slider;
	private float m_scale;

	void Start() {
		m_slider = GetComponent<Slider>();

		if (InstanceManager.player != null) {
			m_scale = InstanceManager.player.data.scale;
			m_slider.value = m_scale;
		}

		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, LevelUp);
	}
	
	public void SetScale(float _scale) {
		if (InstanceManager.player != null) {
			InstanceManager.player.data.SetOffsetScaleValue(_scale - m_scale);
			InstanceManager.player.transform.localScale = new Vector3(_scale, _scale, _scale);
			m_scale = _scale;
		}
	}

	private void LevelUp(IDragonData _data) {
		m_scale = _data.scale;
		m_slider.value = m_scale;
	}
}
