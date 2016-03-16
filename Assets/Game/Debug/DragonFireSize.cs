using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonFireSize : MonoBehaviour {

	private Slider m_slider;
	private AnimationCurve m_size;
	private FireBreath m_fireBreath;

	void Start() {
		m_slider = GetComponent<Slider>();

		if (InstanceManager.player != null) 
		{
			m_fireBreath = InstanceManager.player.GetComponent<FireBreath>();
			if ( m_fireBreath )
			{
				m_size = m_fireBreath.curve;
				m_slider.value = m_size[ m_size.length-1 ].value;
			}

		}

		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, LevelUp);
	}

	void OnDestroy()
	{
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, LevelUp);
	}
	
	public void SetSize(float _size) 
	{
		if (InstanceManager.player != null && _size != 0) 
		{
			float multiplier = _size / m_size[ m_size.length - 1 ].value;
			for( int i = 0; i<m_size.length; i++ )
			{
				Keyframe kf = m_size[ i ];
				kf.value = kf.value * multiplier;				
				m_size.MoveKey( i, kf );
			}
			if (m_fireBreath != null)
				m_fireBreath.curve = m_size;
		}
	}

	private void LevelUp(DragonData _data) 
	{
	}
}
