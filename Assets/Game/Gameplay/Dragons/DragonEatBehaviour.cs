using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonEatBehaviour : EatBehaviour {		

	private DragonPlayer m_dragon;
	private DragonBoostBehaviour m_dragonBoost;
	private Dictionary<string, float> m_eatingBoosts = new Dictionary<string, float>();

	//--------------
	protected void Start() 
	{
		m_dragon = GetComponent<DragonPlayer>();
		m_dragonBoost = GetComponent<DragonBoostBehaviour>();
		m_motion = GetComponent<DragonMotion>();

		m_tier = m_dragon.data.tier;
		m_eatSpeedFactor = m_dragon.data.def.Get<float>("eatSpeedFactor");

		Messenger.AddListener<Transform,Reward>(GameEvents.ENTITY_EATEN, OnEntityEaten);
		Messenger.AddListener(GameEvents.SCORE_MULTIPLIER_LOST, OnMultiplierLost);

		SetupHoldParametersForTier( m_dragon.data.tierDef.sku );
	}

	void OnDestroy() {
		Messenger.RemoveListener<Transform,Reward>(GameEvents.ENTITY_EATEN, OnEntityEaten);
		Messenger.RemoveListener(GameEvents.SCORE_MULTIPLIER_LOST, OnMultiplierLost);
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

	void OnMultiplierLost()
	{
		if ( Random.Range(0, 100.0f) <= 60.0f )
		{
			Burp();
		}
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

	override protected void StartHold(EdibleBehaviour _prey) 
	{
		base.StartHold(_prey);
		// TODO (miguel) this has to be adapted to the pet
		DragonMotion motion = GetComponent<DragonMotion>();
		motion.StartHoldPreyMovement( m_holdTransform);
	}

	override protected void EndHold()
	{
		base.EndHold();
		// TODO (miguel) this has to be adapted to the pet
		DragonMotion motion = GetComponent<DragonMotion>();
		motion.EndHoldMovement();
	}
}