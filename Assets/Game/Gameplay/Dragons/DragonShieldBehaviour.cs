using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonShieldBehaviour : MonoBehaviour {

    public float m_maxShield = 10;
    public float m_currentShield = 0;
	public float m_shieldDrain = 1;
    public float m_healthShieldRewardFactor = 0.5f;
    public List<DamageType> m_ignoreDamageTypes = new List<DamageType>();
    private DragonPlayer m_dragon;
    private DragonHealthBehaviour m_dragonHealth;

    [Header("Anim Setup")]
    public string m_animState = "";
    protected int m_animStateHash;
    public string m_animLayer = "";
    protected int m_animLayerId;
    public float[] m_animThresholds = new float[0];
    public float[] m_animFrames = new float[0];
    public float m_animHisteresisPercentage = 5.0f;
    public float m_animTime = 0.1f;
    protected Animator m_anim;
    protected ShieldHit m_shieldHit = new ShieldHit();
    

	// Use this for initialization
	void Start () {
        m_anim = GetComponentInChildren<Animator>();
        
        m_animStateHash = Animator.StringToHash(m_animState);
        m_animLayerId = m_anim.GetLayerIndex(m_animLayer);
    
        m_dragon = GetComponent<DragonPlayer>();
        m_dragonHealth = m_dragon.dragonHealthBehaviour;
        
		Messenger.AddListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnEntityKilled);
	}

    private void OnDestroy()
    {
        Messenger.RemoveListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnEntityKilled);
    }
    
    void OnEntityKilled(Transform _t, IEntity _entity, Reward _reward, KillType _type) {

        switch (_type)
        {
            case KillType.EATEN:
                if (_reward.health >= 0)
                {
                    if (FreezingObjectsRegistry.instance.IsFreezing(_entity))
                    {
                        float h = m_dragonHealth.GetBoostedHp(_reward.origin, _reward.health) * m_healthShieldRewardFactor;
                        if (h < 0)
                        {
                            m_shieldHit.broken = m_currentShield > 0 && (h + m_currentShield <= 0);
                            m_shieldHit.value = -h;
                            m_shieldHit.bigHit = -h > m_maxShield * 0.1f;
                            Broadcaster.Broadcast(BroadcastEventType.SHIELD_HIT, m_shieldHit);
                        }
                        AddShield(h);


                    }
                }

                break;

            case KillType.SMASHED:
                if (_reward.health >= 0)
                {
                    if (FreezingObjectsRegistry.instance.IsFreezing(_entity))
                    {
                        AddShield(_reward.health * m_healthShieldRewardFactor);
                    }
                }
            break;

            case KillType.BURNT:
                    if (_reward.health >= 0)
                    {
                        // if (FreezingObjectsRegistry.instance.IsFreezing(_e.machine)) // For the ice dragon burning is frozing
                        {
                            AddShield(_reward.health * m_healthShieldRewardFactor);
                        }
                    }
            break;
        }


    }
       
    protected void AddShield( float _add)
    {
        m_currentShield += _add;
        if (m_currentShield > m_maxShield)
            m_currentShield = m_maxShield;
    }
    
    public void FullShield()
    {
        m_currentShield = m_maxShield;
    }

    // Update is called once per frame
    void Update () 
    {
        m_currentShield -= m_shieldDrain * Time.deltaTime;
        if ( m_currentShield < 0 )
        {
            m_currentShield = 0;
        }

        // Update anim time
        int max = m_animThresholds.Length;
        float animTarget = 0;
        float threshold = 0;
        bool done = false;
        for (int i = 0; i < max && !done; i++)
        {
            if ( m_animThresholds[i] <= m_currentShield )
            {
                animTarget = m_animFrames[i];
                if ( i+1 < m_animThresholds.Length )
                    threshold = m_animThresholds[i+1];
            }
            else
            {
                done = true;
            }
        }
        if ( m_animTime < animTarget )
        {
            m_animTime = Mathf.Lerp(m_animTime, animTarget, Time.deltaTime * 10);    // Increase fast
        }
        else
        {
            threshold -= threshold * m_animHisteresisPercentage / 100.0f;
            if ( m_currentShield <= threshold )
            {
                // Break it
                m_animTime = animTarget;
            }
        }

        // Update animation
        m_anim.PlayInFixedTime(m_animStateHash, m_animLayerId, m_animTime);
	}
    
    public float RecieveDamage(float _amount, DamageType _type, Transform _source = null, bool _hitAnimation = true, string _damageOrigin = "", Entity _entity = null)
    {
        if ( !m_ignoreDamageTypes.Contains( _type ) && m_currentShield > 0)
        {
            m_shieldHit.value = _amount;
            m_shieldHit.bigHit = _amount > m_maxShield * 0.1f;
            if ( _amount < m_currentShield )
            {
                m_currentShield -= _amount;
                _amount = 0;
            }
            else
            {
                _amount -= m_currentShield;
                m_currentShield = 0;
            }
            m_shieldHit.broken = m_currentShield <= 0;
            Broadcaster.Broadcast(BroadcastEventType.SHIELD_HIT, m_shieldHit);
        }
        return _amount;
    }
}
