using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Freezing objects registry.
/// Freezing system. This system will take care of npc freezing related stuff
/// </summary>
public class FreezingObjectsRegistry : MonoBehaviour, IBroadcastListener
{
    /// <summary>
    /// Registry.
    /// </summary>
	public class Registry
	{
		public Transform m_transform;
		public float m_distanceSqr;
        public bool m_checkTier;
        public DragonTier m_dragonTier;
        public bool m_killOnFrozen;
        public float[] m_killTiers;
        
        public Registry()
        {
            m_checkTier = false;
            m_killOnFrozen = false;
            m_dragonTier = DragonTier.TIER_0;
        }
	};

    // Added registrys
	List<Registry> m_registry;
    // Machines to check if start feezing
    List<AI.Machine> m_machines = new List<AI.Machine>();   // to froze machines
    // Machines with freezint level
    List<AI.Machine> m_freezingMachines = new List<AI.Machine>();   // already freezing
    // Freezing levels of the machines
    List<float> m_freezingLevels = new List<float>();
    // If the freezing level has to be killed
    List<bool> m_freezingKills = new List<bool>();

    // Machines going from check to freezing level
    List<AI.Machine> m_toFreeze = new List<AI.Machine>();
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
    
    // Paticle Scaling down
    public List<GameObject> m_scaleDownParticles = new List<GameObject>();    
    public List<float> m_scaleDownParticlesTimers = new List<float>();
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
        reg.m_killOnFrozen = false;
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
    
    
    public void CheckFreeze()
    {
        float freezingChange = Time.deltaTime * m_freezinSpeed;
        float defrostSpeed = Time.deltaTime * m_defrostSpeed;
        int max;
       
        max = m_machines.Count;
        m_toFreeze.Clear();
        m_toKill.Clear();
        for (int i = max-1; i >=0 ; i--)
        {
            Registry freezing = Overlaps((CircleAreaBounds)m_machines[i].entity.circleArea.bounds);
            if ( freezing != null )
            {
                // Check if tier pass
                Entity entity = m_machines[i].entity as Entity;
                if (!freezing.m_checkTier || ( entity != null && (entity.IsEdible( freezing.m_dragonTier)|| entity.CanBeHolded( freezing.m_dragonTier ))))
                {
                    m_toFreeze.Add( m_machines[i] );
                    if ( freezing.m_killOnFrozen )
                    {
                        // Check random
                        if (m_machines[i].entity.edibleFromTier < DragonTier.COUNT)
                        {
                            m_toKill.Add( Random.Range(0, 100) < freezing.m_killTiers[ (int)m_machines[i].entity.edibleFromTier ] );
                        }else{
                            m_toKill.Add(false);
                        }
                    }
                    else
                    {
                        m_toKill.Add(false);
                    }
                    m_machines.RemoveAt( i );
                }


               
            }   
        }
        
        max = m_freezingMachines.Count;
        for (int i = max-1; i >=0 ; i--)
        {
            Registry freezing = Overlaps((CircleAreaBounds)m_freezingMachines[i].entity.circleArea.bounds);
            if ( freezing == null)
            {
                m_freezingLevels[i] -= defrostSpeed;
                if ( m_freezingLevels[i] <= 0 )
                {
                    m_freezingMachines[i].SetFreezingLevel(0);
                        // Add to non freezing
                    m_machines.Add( m_freezingMachines[i] );
                        // Remove from freezing
                    m_freezingMachines.RemoveAt(i);
                    m_freezingLevels.RemoveAt(i);
                    m_freezingKills.RemoveAt(i);
                        
                }
                else
                {
                    m_freezingMachines[i].SetFreezingLevel(m_freezingLevels[i]);
                }
            }
            else
            {
                m_freezingLevels[i] += freezingChange;
                if (m_freezingLevels[i] > 1.0f)
                {
                    m_freezingLevels[i] = 1.0f;
                }
                m_freezingMachines[i].SetFreezingLevel(m_freezingLevels[i]);
                if ( m_freezingKills[i] && m_freezingLevels[i] >= 1.0f )
                {
                    m_freezingMachines[i].Smash(IEntity.Type.PLAYER);
                }
            }
        }

        max = m_toFreeze.Count;
        for (int i = 0; i < max; i++)
        {
            m_freezingLevels.Add( freezingChange );
            m_freezingMachines.Add( m_toFreeze[i] );
            m_freezingKills.Add( m_toKill[i] );
            
            m_toFreeze[i].SetFreezingLevel( freezingChange );
        }
    }
    
    public void RegisterMachine(AI.Machine _machine)
    {
        if ( _machine.entity != null && _machine.entity.circleArea != null )
            m_machines.Add( _machine );
    }
    
    public void UnregisterMachine(AI.Machine _machine)
    {
        if (m_machines.Contains(_machine))
            m_machines.Remove( _machine );
        if ( m_freezingMachines.Contains( _machine ) )
        {
            _machine.SetFreezingLevel(0);
            int index = m_freezingMachines.IndexOf( _machine );
            
            m_freezingMachines.RemoveAt( index );
            m_freezingLevels.RemoveAt(index);
            m_freezingKills.RemoveAt(index);
        }
    }
    
    public bool IsFreezing(AI.IMachine _machine){
        bool ret = false;
        if ( _machine is AI.Machine )
        {
            ret = m_freezingMachines.Contains( _machine as AI.Machine );
        }
        return ret;
    }
    
    public void UpdateScaleParticles()
    {
        float delta = Time.deltaTime;
        int max;
        max = m_scaleUpParticles.Count-1;
        for (int i = max; i >= 0; i--)
        {
            m_scaleUpParticlesTimers[i] += delta;
            float d = m_scaleUpParticlesTimers[i] / m_scaleUpDuration;
            m_scaleUpParticles[i].transform.localScale = Vector3.one * m_scaleUpCurve.Evaluate(d);
            if (m_scaleUpParticlesTimers[i] > m_scaleUpDuration)
            {
                // Done
                m_scaleUpParticles.RemoveAt(i);
                m_scaleUpParticlesTimers.RemoveAt(i);
            }
        }
        
        max = m_scaleDownParticles.Count-1;
        for (int i = max; i >= 0; i--)
        {
            m_scaleDownParticlesTimers[i] -= delta;
            float d = m_scaleDownParticlesTimers[i] / m_scaleUpDuration;
            m_scaleDownParticles[i].transform.localScale = Vector3.one * m_scaleUpCurve.Evaluate(d);
            if (m_scaleDownParticlesTimers[i] <= 0)
            {
                // Done
                m_scaleDownParticles.RemoveAt(i);
                m_scaleDownParticlesTimers.RemoveAt(i);
                // Callback?
                m_scaleDownCallbacks[i]();
                m_scaleDownCallbacks.RemoveAt(i);
                
            }
        }
    }


    public void ScaleUpParticle( GameObject instance )
    {
        RemoveScaleDownParticle( instance );
        m_scaleUpParticles.Add(instance);
        m_scaleUpParticlesTimers.Add(0);
    }
    
    public void ScaleDownParticle( GameObject instance, ScaleDownDone _callbacks )
    {
        RemoveScaleUpParticle( instance );
        m_scaleDownParticles.Add(instance);
        m_scaleDownParticlesTimers.Add(m_scaleUpDuration);
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
            m_scaleUpParticlesTimers.RemoveAt( index );
        }
    }
    
    public void RemoveScaleDownParticle( GameObject instance )
    {
        if ( m_scaleDownParticles.Contains( instance ) )
        {
            int index = m_scaleDownParticles.IndexOf( instance );
            m_scaleDownParticles.RemoveAt(index);
            m_scaleDownParticlesTimers.RemoveAt( index );
            m_scaleDownCallbacks.RemoveAt(index);
        }
    }

}
