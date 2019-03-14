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
    

	// Use this for initialization
	void Start () {
        m_anim = GetComponentInChildren<Animator>();
        
        m_animStateHash = Animator.StringToHash(m_animState);
        m_animLayerId = m_anim.GetLayerIndex(m_animLayer);
    
        m_dragon = GetComponent<DragonPlayer>();
        m_dragonHealth = m_dragon.dragonHealthBehaviour;
        
		Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_EATEN, OnEntityEaten);
        Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_DESTROYED, OnEntityDestroyed);
        Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, OnEntityBurned);
	}

    private void OnDestroy()
    {
        Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_EATEN, OnEntityEaten);
        Messenger.RemoveListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_DESTROYED, OnEntityDestroyed);
        Messenger.AddListener<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, OnEntityBurned);
    }
    
    void OnEntityEaten(Transform t, IEntity entity, Reward reward) {
        if (reward.health >= 0) {
            if ( FreezingObjectsRegistry.instance.IsFreezing(entity.machine) )
            {
                float h = m_dragonHealth.GetBoostedHp(reward.origin, reward.health) * m_healthShieldRewardFactor;
                AddShield(h);
            }
        }
    }

    private void OnEntityDestroyed(Transform _entity,  IEntity _e, Reward _reward) {
        if (_reward.health >= 0) {
            if (FreezingObjectsRegistry.instance.IsFreezing(_e.machine))
            {
                AddShield(_reward.health * m_healthShieldRewardFactor);
            }
        }
    }
    
    private void OnEntityBurned(Transform _entity,  IEntity _e, Reward _reward) {
        if (_reward.health >= 0) {
            // if (FreezingObjectsRegistry.instance.IsFreezing(_e.machine)) // For the ice dragon burning is frozing
            {
                AddShield(_reward.health * m_healthShieldRewardFactor);
            }
        }
    }
    
    protected void AddShield( float _add)
    {
        m_currentShield += _add;
        if (m_currentShield > m_maxShield)
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
        if ( !m_ignoreDamageTypes.Contains( _type ) )
        {
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
        }
        return _amount;
    }
}
