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

	// Use this for initialization
	void Start () {
        m_dragon = GetComponent<DragonPlayer>();
        m_dragonHealth = m_dragon.dragonHealthBehaviour;
		Messenger.AddListener<Transform,Reward>(MessengerEvents.ENTITY_EATEN, OnEntityEaten);
	}

    private void OnDestroy()
    {
        Messenger.RemoveListener<Transform,Reward>(MessengerEvents.ENTITY_EATEN, OnEntityEaten);
    }
    
    void OnEntityEaten(Transform t, Reward reward) {
        if (reward.health >= 0) {
            float h = m_dragonHealth.GetBoostedHp(reward.origin, reward.health) * m_healthShieldRewardFactor;
            AddShield(h);
        }
    }

    private void OnEntityDestroyed(Transform _entity, Reward _reward) {
        if (_reward.health >= 0) {
            AddShield( _reward.health );
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
