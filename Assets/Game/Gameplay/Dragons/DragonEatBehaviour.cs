using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonEatBehaviour : EatBehaviour {		

	private DragonPlayer m_dragon;
	private DragonBoostBehaviour m_dragonBoost;
	private Dictionary<string, float> m_eatingBoosts = new Dictionary<string, float>();

	//--------------

	void Start() {
		m_dragon = GetComponent<DragonPlayer>();
		m_dragonBoost = GetComponent<DragonBoostBehaviour>();
		m_motion = GetComponent<DragonMotion>();

		m_tier = m_dragon.data.tier;
		m_eatSpeedFactor = m_dragon.data.def.Get<float>("eatSpeedFactor");

		Messenger.AddListener<Transform,Reward>(GameEvents.ENTITY_EATEN, OnEntityEaten);
	}

	void OnDestroy() {
		Messenger.RemoveListener<Transform,Reward>(GameEvents.ENTITY_EATEN, OnEntityEaten);
	}

	void OnEntityEaten( Transform t, Reward reward )
	{
		// Check if origin is in power up and give proper boost
		if ( m_eatingBoosts.ContainsKey( reward.origin ) )
		{
			m_dragon.AddLife(reward.health + (reward.health * m_eatingBoosts[reward.origin] / 100.0f));
		}
		else
		{
			m_dragon.AddLife(reward.health);
		}

		m_dragon.AddEnergy(reward.energy);
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

	public void AddEatingBost( string entitySku, float value )
	{
		m_eatingBoosts.Add( entitySku, value);
	}
}