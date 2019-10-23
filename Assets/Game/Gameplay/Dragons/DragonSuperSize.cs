using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSuperSize : MonoBehaviour, IBroadcastListener {

	public enum Source {
		NONE = -1,
		LETTERS = 0,
		COLLECTIBLE
	}

	protected DragonPlayer m_dragon;
	protected DragonBoostBehaviour m_boost;
	protected DragonMotion m_motion;
	protected DragonEatBehaviour m_eat;

	protected float m_sizeUpMultiplier;
    public float sizeUpMultiplier{
        get{ return m_sizeUpMultiplier; }
    }
	protected float m_speedUpMultiplier;
	protected float m_biteUpMultiplier;
	protected bool m_invincible;
	protected bool m_infiniteBoost;
	protected bool m_eatEverything;
	protected float m_modeDuration;
	public float modeDuration { get { return m_modeDuration; } }

	protected float m_timer;
	public float time { get { return m_timer; } }
	public void AddTime(float _time) {
		if (m_timer > 0f) {
			m_timer = Mathf.Min(m_timer + _time, m_modeDuration);
		}
	}

	private Source m_source;	

	// Use this for initialization
	void Start () 
	{
		m_dragon = GetComponent<DragonPlayer>();
		m_boost = GetComponent<DragonBoostBehaviour>();
		m_motion = GetComponent<DragonMotion>();
		m_eat = GetComponent<DragonEatBehaviour>();

		DefinitionNode def = m_dragon.data.def;
        IDragonData data = m_dragon.data;

        m_sizeUpMultiplier = data.superSizeUpMultiplier;
        m_speedUpMultiplier = data.superSpeedUpMultiplier;
        m_biteUpMultiplier = data.superBiteUpMultiplier;
        m_invincible = data.superInvincible;
        m_infiniteBoost = data.superInfiniteBoost;
        m_eatEverything = data.superEatEverything;
        m_modeDuration = data.superModeDuration;
        
		m_timer = 0;
		m_source = Source.NONE;

		Messenger.AddListener(MessengerEvents.EARLY_ALL_HUNGRY_LETTERS_COLLECTED, OnEarlyLetters);
		Messenger.AddListener(MessengerEvents.ALL_HUNGRY_LETTERS_COLLECTED, OnLettersCollected);
		Broadcaster.AddListener(BroadcastEventType.START_COLLECTIBLE_HUNGRY_MODE, this);
	}

	void OnDestroy()
	{
		Messenger.RemoveListener(MessengerEvents.EARLY_ALL_HUNGRY_LETTERS_COLLECTED, OnEarlyLetters);
		Messenger.RemoveListener(MessengerEvents.ALL_HUNGRY_LETTERS_COLLECTED, OnLettersCollected);
		Broadcaster.RemoveListener(BroadcastEventType.START_COLLECTIBLE_HUNGRY_MODE, this);
	}

	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
	{
		switch( eventType )		
		{
			case BroadcastEventType.START_COLLECTIBLE_HUNGRY_MODE:
			{
				OnCakeEaten();
			}break;
		}
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
				}
			}
		}
#if UNITY_EDITOR
		if ( Input.GetKeyDown(KeyCode.H) )
		{
			HungryLettersManager lettersManager = FindObjectOfType<HungryLettersManager>();
			HungryLetter[] letters = FindObjectsOfType<HungryLetter>();
			for( int i = 0; i<letters.Length; ++i )
			{
				if (!lettersManager.IsLetterCollected( letters[i].letter))
				{
					letters[i].OnLetterCollected();
				}
			}
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

		Messenger.Broadcast<bool, Source>(MessengerEvents.SUPER_SIZE_TOGGLE, true, m_source);
	}

	void EndSuperSize()
	{
		m_boost.superSizeInfiniteBoost = false;
		m_dragon.superSizeInvulnerable = false;
		m_dragon.SetSuperSize(1);
		m_motion.superSizeSpeedMultiplier = 1;
		m_eat.sizeUpEatSpeedFactor = 1;
		m_eat.eatEverything = false;

		Source lastSource = m_source;
		m_source = Source.NONE;
		Messenger.Broadcast<bool, Source>(MessengerEvents.SUPER_SIZE_TOGGLE, false, lastSource);		
	}

	void OnEarlyLetters()
	{
		m_dragon.superSizeInvulnerable = m_invincible;
	}

	void OnLettersCollected()
	{
		m_source = Source.LETTERS;
		StartSuperSize();		
	}

	void OnCakeEaten() 
	{
		m_source = Source.COLLECTIBLE;
		StartSuperSize();		
	}
}
