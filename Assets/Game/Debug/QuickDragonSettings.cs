using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuickDragonSettings : MonoBehaviour {

	public Slider m_sliderVelocity;
	public Slider m_sliderUp;
	public Slider m_sliderDown;
	public Dropdown m_moveTypeDropDown;
	public Slider m_sliderGravity;
	public Dropdown m_eatTypeDropDown;
	private DragonMotion m_motion;
	private EatBehaviour m_eatBehaviour;

	void OnEnable()
	{
		if ( InstanceManager.player != null )
		{
			m_motion = InstanceManager.player.GetComponent<DragonMotion>();
			m_eatBehaviour = InstanceManager.player.GetComponent<EatBehaviour>();

			m_sliderVelocity.value = m_motion.m_dargonAcceleration;
			m_sliderUp.value = m_motion.m_dragonMass;
			m_sliderDown.value = m_motion.m_dragonFricction;
			m_moveTypeDropDown.value = DragonMotion.movementType;
			switch( m_eatBehaviour.eatCheckType )
			{
				case EatBehaviour.EatCheckType.EntitiesManager: m_eatTypeDropDown.value = 0;break;
				case EatBehaviour.EatCheckType.Capsule: m_eatTypeDropDown.value = 1;break;
				case EatBehaviour.EatCheckType.Box: m_eatTypeDropDown.value = 2;break;
			}
			m_sliderGravity.value = m_motion.m_dragonGravityModifier;
		}		
	}
	
	public void SetVelocityBend(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dargonAcceleration = _size;
	}

	public void SetUpBend(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonMass = _size;
	}

	public void SetDownBend(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonFricction = _size;
	}

	public void SetMoveType(int type)
	{
		if ( m_motion != null )
			DragonMotion.movementType = type;
	}

	public void SetGravityModifier(float _size) 
	{
		if ( m_motion != null )
			m_motion.m_dragonGravityModifier = _size;
	}

	public void SetEatType(int type)
	{
		if (m_eatBehaviour)	
		{
			switch( type )
			{
				case 0:m_eatBehaviour.eatCheckType = EatBehaviour.EatCheckType.EntitiesManager;break;
				case 1:m_eatBehaviour.eatCheckType = EatBehaviour.EatCheckType.Capsule;break;
				case 2:m_eatBehaviour.eatCheckType = EatBehaviour.EatCheckType.Box;break;
			}
		}
	}
}
