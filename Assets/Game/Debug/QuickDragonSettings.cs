using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class QuickDragonSettings : MonoBehaviour {

	public Slider m_sliderAcceleration;
	public Slider m_sliderMass;
	public Slider m_sliderFricction;
	public Slider m_sliderGravity;
	public TMP_Dropdown m_eatTypeDropDown;	
	private DragonMotion m_motion;

	void OnEnable()
	{
		if ( InstanceManager.player != null )
		{
			m_motion = InstanceManager.player.GetComponent<DragonMotion>();

			m_sliderAcceleration.value = m_motion.m_dragonForce;
			m_sliderMass.value = m_motion.m_dragonMass;
			m_sliderFricction.value = m_motion.m_dragonFricction;
			switch( EntityManager.instance.overlapingMethod )
			{
				case EntityManager.OverlapingMethod.EntitiesManager: m_eatTypeDropDown.value = 0;break;
				case EntityManager.OverlapingMethod.Capsule: m_eatTypeDropDown.value = 1;break;
				case EntityManager.OverlapingMethod.Box: m_eatTypeDropDown.value = 2;break;
			}
			m_sliderGravity.value = m_motion.m_dragonGravityModifier;

		}	
	}
	
	public void SetDragonAcceleration(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonForce = _size;
	}

	public void SetDragonMass(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonMass = _size;
	}

	public void SetDragonFricction(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonFricction = _size;
	}

	public void SetGravityModifier(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonGravityModifier = _size;
	}

	public void SetEatType(int type)
	{
		if (EntityManager.instance != null)	
		{
			switch( type )
			{
				case 0:EntityManager.instance.overlapingMethod = EntityManager.OverlapingMethod.EntitiesManager;break;
				case 1:EntityManager.instance.overlapingMethod = EntityManager.OverlapingMethod.Capsule;break;
				case 2:EntityManager.instance.overlapingMethod = EntityManager.OverlapingMethod.Box;break;
			}
		}
	}	

	public void SetRespawnTo0( bool _value )
	{
		DebugSettings.ignoreSpawnTime = _value;
	}

	public void SetSpawnChance0( bool _value )
	{
		DebugSettings.spawnChance0 = _value;
	}

	public void SetSpawnChance100( bool _value )
	{
		DebugSettings.spawnChance100 = _value;
	}


	public void Toggle60FPS( bool _value )
	{
		if ( _value )
		{
			Application.targetFrameRate = 60;
			Time.captureFramerate = 60;
		}
		else
		{
			Application.targetFrameRate = 30;
			Time.captureFramerate = 30;
		}
	}

	public void ToggleHitStop( bool _value )
	{
		DebugSettings.hitStopEnabled = _value;
	}
}
