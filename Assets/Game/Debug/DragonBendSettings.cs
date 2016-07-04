using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonBendSettings : MonoBehaviour {

	public Slider m_sliderVelocity;
	public Slider m_sliderUp;
	public Slider m_sliderDown;

	private DragonMotion m_dragonMotion;

	void OnEnable()
	{
		if (InstanceManager.player != null) 
		{
			m_dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
			m_sliderVelocity.value = DragonMotion.s_velocityBlendRate;
			m_sliderUp.value = DragonMotion.s_velocityUpBlendRate;
			m_sliderDown.value = DragonMotion.s_velocityDownBlendRate;
		}
	}
	
	public void SetVelocityBend(float _size) 
	{
		if (m_dragonMotion != null) 
		{
			DragonMotion.s_velocityBlendRate = _size;
		}
	}

	public void SetUpBend(float _size) 
	{
		if (m_dragonMotion != null) 
		{
			DragonMotion.s_velocityUpBlendRate = _size;
		}
	}

	public void SetDownBend(float _size) 
	{
		if (m_dragonMotion != null) 
		{
			DragonMotion.s_velocityDownBlendRate = _size;
		}
	}
}
