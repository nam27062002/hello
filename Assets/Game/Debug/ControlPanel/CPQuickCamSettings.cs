using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CPQuickCamSettings : MonoBehaviour 
{
	public Slider m_dragonDefaultSizeSlider;
	public Slider m_dragonFrameModifierSlider;

	private GameCamera m_cam;

	void OnEnable()
	{
		m_cam = Camera.main.GetComponent<GameCamera>();
		if ( m_cam != null )
		{
			m_dragonDefaultSizeSlider.value = m_cam.lastSize;
			m_dragonFrameModifierSlider.value = m_cam.lastFrameWidthModifier;
		}		
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
}
