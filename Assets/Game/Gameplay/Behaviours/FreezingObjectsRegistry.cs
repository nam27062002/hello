using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Freezing objects registry.
/// Freezing system. This system will take care of npc freezing related stuff
/// </summary>
public class FreezingObjectsRegistry : MonoBehaviour, IBroadcastListener
{
    public enum FreezingCheckType
    {
        NONE,
        EAT,
        BURN
    };

    /// <summary>
    /// Registry.
    /// </summary>
	public class Registry
	{
        
		public Transform m_transform;
		public float m_distanceSqr;
        public bool m_checkTier;
        public FreezingCheckType m_checkType;
        public DragonTier m_dragonTier;
        public Registry()
        {
            m_checkTier = false;
            m_dragonTier = DragonTier.TIER_0;
            m_checkType = FreezingCheckType.NONE;
        }
	};

    public bool m_killOnFrozen;
    public float[] m_killTiers;

    // Added registrys
	List<Registry> m_registry;
    // Machines to check if start feezing
    List<Entity> m_entities = new List<Entity>();   // to froze machines
    
    // Machines with freezint level
    List<Entity> m_freezingEntities = new List<Entity>();   // already freezing
    
    // Freezing levels of the machines
    List<float> m_freezingLevels = new List<float>();
    // If the freezing level has to be killed
    List<bool> m_freezingKills = new List<bool>();

    // Machines going from check to freezing level
    List<Entity> m_toFreeze = new List<Entity>();
    // if kill a Machines going from check to freezing level
    List<bool> m_toKill = new List<bool>();
    
	public static float m_freezinSpeed = 1;
	public static float m_defrostSpeed = 0.5f;
	public static float m_minFreezeSpeedMultiplier = 0.25f;

    public AnimationCurve m_scaleUpCurve;
    public float m_scaleUpDuration = 0.25f;
    protected ParticleData m_freezeParticle;
    public ParticleData freezeParticle{
        get { return m_freezeParticle; }
    }
    
    // Particles Scaling up
    public List<GameObject> m_scaleUpParticles = new List<GameObject>();    
    public List<float> m_scaleUpParticlesTimers = new List<float>();
    public List<float> m_scaleUpTargetScale = new List<float>();
    
    // Paticle Scaling down
    public List<GameObject> m_scaleDownParticles = new List<GameObject>();    
    public List<float> m_scaleDownParticlesTimers = new List<float>();
    public List<float> m_scaleDownStartScale = new List<float>();
    public delegate void ScaleDownDone();
    public List<ScaleDownDone> m_scaleDownCallbacks = new List<ScaleDownDone>();

    protected static FreezingObjectsRegistry m_instance;
    public static FreezingObjectsRegistry instance {
        get {
            return m_instance; 
        }
    }



	public void Awake()
	{
        m_instance = this;
		m_registry = new List<Registry>();
		DefinitionNode node = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.FREEZE_CONSTANTS, "freezeConstant");
		if ( node != null )
		{
			m_freezinSpeed = node.GetAsFloat( "freezingSpeed", 1.0f );
			m_defrostSpeed = node.GetAsFloat( "defrostSpeed", 0.5f );
			m_minFreezeSpeedMultiplier = node.GetAsFloat("minFreezeSpeedMultiplier", 0.25f);
		}
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
	}


    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
    }
    
    public void CustomUpdate()
    {
        CheckFreeze();
        UpdateScaleParticles();
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
                CreatePool();
                break;
        }
    }


    void CreatePool()
    {
        if (m_freezeParticle == null) {
            m_freezeParticle = new ParticleData("FX_FrozenSmallNPC","", GameConstants.Vector3.zero );
        }
        m_freezeParticle.CreatePool();
    }

    public Registry Register( Transform tr, float distance )
	{
		Registry reg = new Registry();
		reg.m_transform = tr;
		reg.m_distanceSqr = distance * distance;
		m_registry.Add( reg );
        return reg;
	}
    
    public void AddRegister( Registry registry )
    {
        m_registry.Add( registry );
    }

	public void RemoveRegister( Registry registry )
	{
        m_registry.Remove(registry);
	}

	public Registry Overlaps( CircleAreaBounds bounds )
	{
        Registry ret = null;
		for( int i = 0; i<m_registry.Count && ret == null; i++ )
		{
			Vector2 v = (Vector2)(bounds.center - m_registry[i].m_transform.position);
            if (v.sqrMagnitude < m_registry[i].m_distanceSqr)
                ret = m_registry[i];
		}
		return ret;
	}
    
    /// <summary>
    /// Checks the freeze. Freeziing Logic
    /// </summary>
    public void CheckFreeze()
    {
        float freezingChange = Time.deltaTime * m_freezinSpeed;
        float defrostSpeed = Time.deltaTime * m_defrostSpeed;
        int max;
       
        max = m_entities.Count;
        m_toFreeze.Clear();
        m_toKill.Clear();
        if ( m_registry.Count > 0 )
        {
            for (int i = max-1; i >=0; i--)
            {
                Registry freezing = Overlaps((CircleAreaBounds)m_entities[i].circleArea.bounds);
                if ( freezing != null )
                {
                    // Check if tier pass
                    Entity entity = m_entities[i];
                    bool isValid = false;
                    switch( freezing.m_checkType )
                    {
                        case FreezingCheckType.NONE:
                        {
                            isValid = true;
                        }break;
                        case FreezingCheckType.EAT:
                        {
                            isValid = entity.IsEdible( freezing.m_dragonTier) || entity.CanBeHolded( freezing.m_dragonTier );
                        }break;
                        case FreezingCheckType.BURN:
                        {
                            isValid = entity.IsBurnable(freezing.m_dragonTier);
                        }break;
                    }

                    if ( isValid )
                    {
                        m_toFreeze.Add( m_entities[i] );
                        if ( m_killOnFrozen )
                        {
                            // Check random
                            if (m_entities[i].edibleFromTier < DragonTier.COUNT)
                            {
                                m_toKill.Add( Random.Range(0, 100) < m_killTiers[ (int)m_entities[i].edibleFromTier ] );
                            }else{
                                m_toKill.Add(false);
                            }
                        }
                        else
                        {
                            m_toKill.Add(false);
                        }
                        m_entities.RemoveAt( i );
                    }
                }   
            }
        }
        
        max = m_freezingEntities.Count;
        for (int i = max-1; i >=0 ; i--)
        {
            Registry freezing = Overlaps((CircleAreaBounds)m_freezingEntities[i].circleArea.bounds);
            if ( freezing == null)
            {
                m_freezingLevels[i] -= defrostSpeed;
                if ( m_freezingLevels[i] <= 0 )
                {
                    m_freezingEntities[i].SetFreezingLevel(0);
                        // Add to non freezing
                    m_entities.Add( m_freezingEntities[i] );
                        // Remove from freezing
                    m_freezingEntities.RemoveAt(i);
                    m_freezingLevels.RemoveAt(i);
                    m_freezingKills.RemoveAt(i);
                        
                }
                else
                {
                    m_freezingEntities[i].SetFreezingLevel(m_freezingLevels[i]);
                }
            }
            else
            {
                m_freezingLevels[i] += freezingChange;
                if (m_freezingLevels[i] > 1.0f)
                {
                    m_freezingLevels[i] = 1.0f;
                }
                m_freezingEntities[i].SetFreezingLevel(m_freezingLevels[i]);
                if ( m_freezingKills[i] && m_freezingLevels[i] >= 1.0f )
                {
                    m_freezingEntities[i].machine.Smash(IEntity.Type.PLAYER);
                }
            }
        }

        max = m_toFreeze.Count;
        for (int i = 0; i < max; i++)
        {
            m_freezingLevels.Add( freezingChange );
            m_freezingEntities.Add( m_toFreeze[i] );
            m_freezingKills.Add( m_toKill[i] );
            
            m_toFreeze[i].SetFreezingLevel( freezingChange );
        }
    }
    
    public void RegisterEntity( Entity _entity)
    {
        if ( _entity.circleArea != null && _entity is Entity)
        { 
            _entity.SetFreezingLevel(0);
            m_entities.Add( _entity as Entity);
        }
    }
    
    public void UnregisterEntity(Entity _entity)
    {
        int max = m_entities.Count;
        for (int i = 0; i < max; i++)
        {
            if ( m_entities[i] == _entity )
            {
                m_entities.RemoveAt(i);
                break;
            }
        }

        max = m_freezingEntities.Count;
        for (int i = 0; i < max; i++)
        {
            if ( m_freezingEntities[i] == _entity )
            {
                m_freezingEntities.RemoveAt(i);
                m_freezingLevels.RemoveAt(i);
                m_freezingKills.RemoveAt(i);
                break;
            }
        }
    }
    
    public bool IsFreezing(IEntity _entity){
        bool ret = false;

        int max = m_freezingEntities.Count;
        for (int i = 0; i < max && !ret; i++)
        {
            ret = m_freezingEntities[i] == _entity;
        }
        return ret;
    }
    
    public bool IsFreezing(AI.IMachine _machine){
        bool ret = false;
        int max = m_freezingEntities.Count;
        for (int i = 0; i < max && !ret; i++)
        {
            ret = m_freezingEntities[i].machine == _machine;
        }
        return ret;
    }
    
    
    /// <summary>
    /// Freezing view Update
    /// </summary>
    public void UpdateScaleParticles()
    {
        float delta = Time.deltaTime;
        int max;
        max = m_scaleUpParticles.Count-1;
        for (int i = max; i >= 0; i--)
        {
            m_scaleUpParticlesTimers[i] += delta;
            float d = m_scaleUpParticlesTimers[i] / m_scaleUpDuration;
            m_scaleUpParticles[i].transform.localScale = Vector3.one * m_scaleUpCurve.Evaluate(d) * m_scaleUpTargetScale[i];
            if (m_scaleUpParticlesTimers[i] > m_scaleUpDuration)
            {
                // Done
                m_scaleUpParticles.RemoveAt(i);
                m_scaleUpParticlesTimers.RemoveAt(i);
                m_scaleUpTargetScale.RemoveAt(i);
            }
        }
        
        max = m_scaleDownParticles.Count-1;
        for (int i = max; i >= 0; i--)
        {
            m_scaleDownParticlesTimers[i] -= delta;
            float d = m_scaleDownParticlesTimers[i] / m_scaleUpDuration;
            m_scaleDownParticles[i].transform.localScale = Vector3.one * m_scaleUpCurve.Evaluate(d) * m_scaleDownStartScale[i];
            if (m_scaleDownParticlesTimers[i] <= 0)
            {
                // Done
                m_scaleDownParticles.RemoveAt(i);
                m_scaleDownParticlesTimers.RemoveAt(i);
                m_scaleDownStartScale.RemoveAt(i);
                // Callback?
                m_scaleDownCallbacks[i]();
                m_scaleDownCallbacks.RemoveAt(i);
                
            }
        }
    }


    public void ScaleUpParticle( GameObject instance, float scale )
    {
        RemoveScaleDownParticle( instance );
        m_scaleUpParticles.Add(instance);
        m_scaleUpTargetScale.Add( scale );
        m_scaleUpParticlesTimers.Add(0);
    }
    
    public void ScaleDownParticle( GameObject instance, ScaleDownDone _callbacks )
    {
        RemoveScaleUpParticle( instance );
        Debug.Log( "ScaleDownParticle" );
        m_scaleDownParticles.Add(instance);
        m_scaleDownParticlesTimers.Add(m_scaleUpDuration);
        m_scaleDownStartScale.Add( instance.transform.localScale.x );
        m_scaleDownCallbacks.Add(_callbacks);
    }

    public void ForceReturnInstance( GameObject instance )
    {
        RemoveScaleUpParticle( instance );
        RemoveScaleDownParticle( instance );
    }
    
    public void RemoveScaleUpParticle( GameObject instance )
    {
        if ( m_scaleUpParticles.Contains( instance ) )
        {
            int index = m_scaleUpParticles.IndexOf( instance );
            m_scaleUpParticles.RemoveAt(index);
            m_scaleUpTargetScale.RemoveAt(index);
            m_scaleUpParticlesTimers.RemoveAt( index );
        }
    }
    
    public void RemoveScaleDownParticle( GameObject instance )
    {
        Debug.Log( "RemoveScaleDownParticle" );
        if ( m_scaleDownParticles.Contains( instance ) )
        {
            int index = m_scaleDownParticles.IndexOf( instance );
            m_scaleDownParticles.RemoveAt(index);
            m_scaleDownParticlesTimers.RemoveAt( index );
            m_scaleDownStartScale.RemoveAt(index);
            m_scaleDownCallbacks.RemoveAt(index);
        }
    }
    
    public void ClearEntities()
    {
        m_entities.Clear();
        m_freezingEntities.Clear();
        m_freezingLevels.Clear();
        m_freezingKills.Clear();
        m_toFreeze.Clear();
        m_toKill.Clear();
    }
    
    public void ClearScalings()
    {
        m_scaleUpParticles.Clear();
        m_scaleUpParticlesTimers.Clear();
        m_scaleUpTargetScale.Clear();
        
        // Paticle Scaling down
        m_scaleDownParticles.Clear();
        m_scaleDownParticlesTimers.Clear();
        m_scaleDownStartScale.Clear();
        m_scaleDownCallbacks.Clear();
    }

}
