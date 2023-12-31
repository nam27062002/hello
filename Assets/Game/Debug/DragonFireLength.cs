﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonFireLength : MonoBehaviour {

	private Slider m_slider;
	private float m_length = 0;

	private FireBreath m_fireBreath;
	private FireLightning m_fireLightning;

	void Awake() {
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, LevelUp);
	}

	void Start() {
		m_slider = GetComponent<Slider>();

		if (InstanceManager.player != null) 
		{
			m_fireBreath = InstanceManager.player.GetComponent<FireBreath>();
			if ( m_fireBreath )
				m_length = m_fireBreath.length;

			m_fireLightning = InstanceManager.player.GetComponent<FireLightning>();
			if ( m_fireLightning )
				m_length = m_fireLightning.length;

			m_slider.value = m_length;
		}
	}

	void OnDestroy()
	{
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, LevelUp);
	}

	public void SetLength(float _length) 
	{
		if (InstanceManager.player != null) 
		{
			m_length = _length;
			if ( m_fireBreath != null )
				m_fireBreath.length = _length;
			if ( m_fireLightning != null )
				m_fireLightning.length = _length;
		}
	}

	void LevelUp( IDragonData _data )
	{

	}
}
