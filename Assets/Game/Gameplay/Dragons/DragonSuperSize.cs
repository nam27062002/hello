using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSuperSize : MonoBehaviour {

	protected DragonPlayer m_dragon;
	protected DragonBoostBehaviour m_boost;
	protected DragonMotion m_motion;
	protected DragonEatBehaviour m_eat;

	protected float m_sizeUpMultiplier;
	protected float m_speedUpMultiplier;
	protected float m_biteUpMultiplier;
	protected bool m_invincible;
	protected bool m_infiniteBoost;
	protected bool m_eatEverything;
	protected float m_modeDuration;

	protected float m_timer;

	// Use this for initialization
	void Start () 
	{
		m_dragon = GetComponent<DragonPlayer>();
		m_boost = GetComponent<DragonBoostBehaviour>();
		m_motion = GetComponent<DragonMotion>();
		m_eat = GetComponent<DragonEatBehaviour>();

		DefinitionNode def = m_dragon.data.def;

		m_sizeUpMultiplier = def.GetAsFloat("sizeUpMultiplier", 2);
		m_speedUpMultiplier = def.GetAsFloat("speedUpMultiplier", 2);
		m_biteUpMultiplier = def.GetAsFloat("biteUpMultiplier", 2);
		m_invincible = def.GetAsBool("invincible", true);
		m_infiniteBoost = def.GetAsBool("infiniteBoost", true);
		m_eatEverything = def.GetAsBool("eatEverything", true);
		m_modeDuration = def.GetAsFloat("modeDuration", 10);
		m_timer = 0;

		Messenger.AddListener(GameEvents.EARLY_ALL_HUNGRY_LETTERS_COLLECTED, OnEarlyLetters);
		Messenger.AddListener(GameEvents.ALL_HUNGRY_LETTERS_COLLECTED, OnLettersCollected);
	}

	void OnDestroy()
	{
		Messenger.RemoveListener(GameEvents.EARLY_ALL_HUNGRY_LETTERS_COLLECTED, OnEarlyLetters);
		Messenger.RemoveListener(GameEvents.ALL_HUNGRY_LETTERS_COLLECTED, OnLettersCollected);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( m_timer > 0 )	
		{
			if (!m_dragon.changingArea)
			{
				m_timer -= Time.deltaTime;
				if ( m_timer <= 0 ) 
				{
					EndSuperSize();
					Messenger.Broadcast<bool>( GameEvents.SUPER_SIZE_TOGGLE, false);
				}
			}
		}
#if UNITY_EDITOR
		if ( Input.GetKeyDown(KeyCode.F) )
		{
			StartSuperSize();
		}
#endif
	}

	bool IsActive()
	{
		return m_timer > 0;
	}

	void StartSuperSize()
	{
		m_timer = m_modeDuration;

		m_boost.superSizeInfiniteBoost = m_infiniteBoost;
		m_dragon.superSizeInvulnerable = m_invincible;
		m_dragon.SetSuperSize(m_sizeUpMultiplier);
		m_motion.superSizeSpeedMultiplier = m_speedUpMultiplier;
		m_eat.sizeUpEatSpeedFactor = m_biteUpMultiplier;
		m_eat.eatEverything = m_eatEverything;
	}

	void EndSuperSize()
	{
		m_boost.superSizeInfiniteBoost = false;
		m_dragon.superSizeInvulnerable = false;
		m_dragon.SetSuperSize(1);
		m_motion.superSizeSpeedMultiplier = 1;
		m_eat.sizeUpEatSpeedFactor = 1;
		m_eat.eatEverything = false;
	}

	void OnEarlyLetters()
	{
		m_dragon.superSizeInvulnerable = m_invincible;
	}

	void OnLettersCollected()
	{
		StartSuperSize();
		Messenger.Broadcast<bool>( GameEvents.SUPER_SIZE_TOGGLE, true);
	}

	void OnSuperSize( bool _value )
	{
		if ( _value )
		{
			StartSuperSize();
		}
	}
}
