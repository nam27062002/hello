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

		m_motion = GetComponent<DragonMotion>();

		m_tier = m_dragon.data.tier;

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
			m_motion.SetSpeedMultiplier(0.25f);
			m_slowedDown = true;
		} else {
			m_motion.SetSpeedMultiplier(1f);
			m_dragonBoost.ResumeBoost();
			m_slowedDown = false;
		}
	}
}