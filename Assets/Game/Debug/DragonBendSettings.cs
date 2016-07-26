﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DragonBendSettings : MonoBehaviour {

	public Slider m_sliderVelocity;
	public Slider m_sliderUp;
	public Slider m_sliderDown;


	void OnEnable()
	{
		m_sliderVelocity.value = DragonMotion.s_dargonAcceleration;
		m_sliderUp.value = DragonMotion.s_dragonMass;
		m_sliderDown.value = DragonMotion.s_dragonFricction;
		
	}
	
	public void SetVelocityBend(float _size) 
	{
		DragonMotion.s_dargonAcceleration = _size;
	}

	public void SetUpBend(float _size) 
	{
		DragonMotion.s_dragonMass = _size;
	}

	public void SetDownBend(float _size) 
	{
		DragonMotion.s_dragonFricction = _size;
	}
}
