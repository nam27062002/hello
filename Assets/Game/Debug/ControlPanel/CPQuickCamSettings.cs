using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CPQuickCamSettings : MonoBehaviour 
{
	public Slider m_dragonDefaultSizeSlider;
	public Slider m_dragonFrameModifierSlider;

	public Slider m_moveDamp;
	public Slider m_lookDamp;

	private GameCamera m_cam;

	void OnEnable()
	{
		if ( Camera.main != null )
		{
			m_cam = Camera.main.GetComponent<GameCamera>();
			if ( m_cam != null )
			{
				m_dragonDefaultSizeSlider.value = m_cam.lastSize;
				m_dragonFrameModifierSlider.value = m_cam.lastFrameWidthModifier;
			}
		}		

		m_moveDamp.value = GameCamera.m_moveDamp;
		m_lookDamp.value = GameCamera.m_lookDamp;
	}

	public void SetDefaultSize(float _size) 
	{
		if ( m_cam != null )
			m_cam.SetFrameWidthIncrement( _size, m_cam.lastFrameWidthModifier);
	}

	public void SetFrameModifier(float _size) 
	{
		if ( m_cam != null )
			m_cam.SetFrameWidthIncrement( m_cam.lastSize, _size);
	}

	public void SetMoveDamp(float _size)
	{
		GameCamera.m_moveDamp = _size;
	}

	public void SetLookDamp(float _size)
	{
		GameCamera.m_lookDamp = _size;
	}
}
