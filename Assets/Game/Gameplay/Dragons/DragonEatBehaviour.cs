using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonEatBehaviour : EatBehaviour {		

	private DragonPlayer m_dragon;
	private DragonBoostBehaviour m_dragonBoost;

	//--------------

	void Start() {
		m_dragon = GetComponent<DragonPlayer>();
		m_dragonBoost = GetComponent<DragonBoostBehaviour>();

		DragonMotion dragonMotion = GetComponent<DragonMotion>();
		m_mouth = dragonMotion.tongue;
		m_tongueDirection = dragonMotion.tongue.position - dragonMotion.head.position;
		m_tongueDirection.Normalize();

		m_motion = dragonMotion;

		m_tier = m_dragon.data.def.tier;
		m_biteSkill = m_dragon.data.biteSkill.value;

		Messenger.AddListener<Transform,Reward>(GameEvents.ENTITY_EATEN, OnEntityEaten);
	}

	void OnDestroy() {
		Messenger.RemoveListener<Transform,Reward>(GameEvents.ENTITY_EATEN, OnEntityEaten);
	}

	void OnEntityEaten( Transform t, Reward reward )
	{
		m_dragon.AddLife(reward.health);
		m_dragon.AddFury(reward.fury);
	}

	protected override void SlowDown(bool _enable) {
		if (_enable) {
			m_dragonBoost.StopBoost();
			m_dragon.SetSpeedMultiplier(0.25f);
			m_slowedDown = true;
		} else {
			m_dragon.SetSpeedMultiplier(1f);
			m_dragonBoost.ResumeBoost();
			m_slowedDown = false;
		}
	}
}