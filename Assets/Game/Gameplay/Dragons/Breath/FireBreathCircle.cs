using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireBreathCircle : DragonBreathBehaviour {

	[Header("Emitter")]
	[SerializeField] private float m_length = 6f;
	public float length
	{
		get
		{
			return m_length;
		}
		set
		{
			m_length = value;
		}
	}

    public string m_fireCenter = "Dragon_Hip";

    [Header("Particle")]
    public string m_particleCenter = "FirePoint";
    public string m_fireParticle = "FireCircle/PS_SonicFireRush";
    private ParticleSystem m_fireParticleInstance;
    public string m_fireParticleStart = "FireCircle/PS_SonicFireRushBoost";
    private ParticleSystem m_fireParticleStartInstance;
    
    public string m_megaFireParticle = "FireCircle/PS_SonicMegaFireRush";
    private ParticleSystem m_megaFireParticleInstance;
    public string m_megaFireParticleStart = "FireCircle/PS_SonicMegaFireRushBoost";
    private ParticleSystem m_megaFireParticleStartInstance;
    
	private int m_groundMask;
	private int m_noPlayerMask;

	private Transform m_centerTransform;

	private int m_frame;

	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;

	private Cage[] m_checkCages = new Cage[50];
	private int m_numCheckCages = 0;

    private bool m_insideWater = false;
    private bool m_waterMode = false;
    private float m_waterY = 0;


    override protected void ExtendedStart() {
        m_centerTransform  = transform.FindTransformRecursive(m_fireCenter);

        m_groundMask = GameConstants.Layers.GROUND_WATER_FIREBLOCK;
        m_noPlayerMask = ~GameConstants.Layers.PLAYER;

        m_actualLength = m_length;
		m_direction = Vector2.zero;
		m_frame = 0;
        
        Transform particleCenter = transform.FindTransformRecursive(m_particleCenter);
        m_fireParticleInstance = ParticleManager.InitLeveledParticle( m_fireParticle, particleCenter);
        m_fireParticleStartInstance = ParticleManager.InitLeveledParticle( m_fireParticleStart, particleCenter);
        m_megaFireParticleInstance = ParticleManager.InitLeveledParticle( m_megaFireParticle, particleCenter);
        m_megaFireParticleStartInstance = ParticleManager.InitLeveledParticle( m_megaFireParticleStart, particleCenter);
    }

    override public void RecalculateSize()
    {
    	if ( m_dragon )
    	{
            float furyBaseLength = m_dragon.data.furyBaseLength;
			m_length = furyBaseLength + furyBaseLength * lengthPowerUpPercentage / 100.0f;

			m_length *= transform.localScale.x;
            
            //  Scale particle
		}
    }

  

    override public bool IsInsideArea(Vector2 _point) {

        bool ret = false;
		if (IsFuryOn()) {
            Vector3 p = _point;
            float sqrMagnitude = (p - m_centerTransform.position).sqrMagnitude;
            ret = sqrMagnitude < m_actualLength * m_actualLength;    
            
		}
		return false; 
	}

	override public bool Overlaps(CircleAreaBounds _circle)
	{
        bool ret = false;
		if (IsFuryOn()) 
		{
			Vector3 center = _circle.center;
			center.z = 0;
            float sqrMagnitude = (center - m_centerTransform.position).sqrMagnitude;
            ret = sqrMagnitude < m_actualLength * m_actualLength;            
		}
		return ret;
	}

	override protected void BeginFury(Type _type) 
	{
		base.BeginFury( _type);
        switch( _type )
        {
            case Type.Standard:
            {
                m_fireParticleStartInstance.gameObject.SetActive(true);
                m_fireParticleStartInstance.Play();
            }break;
            case Type.Mega:
            {
                m_megaFireParticleStartInstance.gameObject.SetActive(true);
                m_megaFireParticleStartInstance.Play();
            }break;
        }
		StartCoroutine( StartFlame(0.25f, _type));
        
    }

	IEnumerator StartFlame( float delay, Type _type )
    {
    	yield return new WaitForSeconds(delay);
        if ( !isFuryPaused )
        {
    		if (_type == Type.Standard)
            {
                // Enable particle
                m_fireParticleInstance.gameObject.SetActive(true);
            }
            else
            {
                // Enable super particle
                m_megaFireParticleInstance.gameObject.SetActive(true);
            }
        }
    }


	override protected void EndFury(bool increase_mega_fury = true) 
	{
        if (m_type == Type.Standard)
        {
            // Disable Particle
            m_fireParticleInstance.gameObject.SetActive(false);
            m_fireParticleStartInstance.gameObject.SetActive(false);
        }
        else
        {
            // Disable Super Particle
            m_megaFireParticleInstance.gameObject.SetActive(false);
            m_megaFireParticleStartInstance.gameObject.SetActive(false);
        }
		base.EndFury(increase_mega_fury);

    }

    override protected void Breath()
    {
        m_direction = -m_centerTransform.right;
		m_direction.Normalize();

        float localLength = m_length;
		if ( m_type == Type.Mega )
			localLength = m_length * m_superFuryLengthMultiplier;

		if (m_frame == 0) {
			// Raycast to ground
			RaycastHit ground;				
			if (Physics.Linecast(m_centerTransform.position, m_centerTransform.position + (Vector3)m_direction * localLength, out ground, m_groundMask)) 
			{
				m_actualLength = ground.distance;
			} 
			else 
			{				
				m_actualLength = localLength;
			}
		}

		m_frame = (m_frame + 1) % 4;

		m_bounds2D.Set( m_centerTransform.position.x - m_actualLength, m_centerTransform.position.y - m_actualLength, m_actualLength * 2, m_actualLength * 2);

		//--------------------------------------------------------------------------------------------
		// try to burn things!!!
		// search for preys!
		// Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_sphCenter, m_sphRadius);
		m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities(m_centerTransform.position, m_actualLength, m_checkEntities);
		for (int i = 0; i < m_numCheckEntities; i++) 
		{
			Entity prey = m_checkEntities[i];
			if ((prey.circleArea != null && Overlaps((CircleAreaBounds)prey.circleArea.bounds)) || IsInsideArea(prey.transform.position)) 
			{
				if (prey.IsBurnable()) {
					if (prey.IsBurnable(m_tier) || m_type == Type.Mega) {
						AI.IMachine machine =  m_checkEntities[i].machine;
						if (machine != null) {
							machine.Burn(transform, IEntity.Type.PLAYER, false, m_type == Type.Mega);
						}
					} else {
						// Show message saying I cannot burn it
						Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, DragonTier.COUNT, prey.sku);
					}
				}
			}
		}

		// pick cages 
		m_numCheckCages = EntityManager.instance.GetCagesInRange2DNonAlloc(m_centerTransform.position, m_actualLength, m_checkCages);
		for (int i = 0; i < m_numCheckCages; i++) 
		{
			Cage cage = m_checkCages[i];
			if ((cage.circleArea != null && Overlaps((CircleAreaBounds)cage.circleArea.bounds)) || IsInsideArea(cage.transform.position)) 
			{
				cage.behaviour.Break();
			}
		}

		if ( m_insideWater )
		{
			if (m_centerTransform.position.y > m_waterY)
			{
				if ( m_waterMode )
				{	
					ShowNormalMode();
					m_waterMode = false;
				}
			}
			else
			{
				if (!m_waterMode)
				{
					ShowWaterMode();
					m_waterMode = true;
				}
			}
		}
		base.Breath();
	}

    private void LateUpdate()
    {
        if ( m_state == State.BREATHING )
        {
            switch(m_type)
        {
            case Type.Standard:
            {
                m_fireParticleInstance.transform.rotation = Quaternion.identity;
                m_fireParticleStartInstance.transform.rotation = Quaternion.identity;
            }break;
            case Type.Mega:
            {
                m_megaFireParticleInstance.transform.rotation = Quaternion.identity;
                m_megaFireParticleStartInstance.transform.rotation = Quaternion.identity;
            }break;
        }
        }
    }

    void OnDrawGizmos() {
		if (IsFuryOn()) {
			Gizmos.color = Color.magenta;

			// Arc Drawing
			Vector3 dir = m_direction;
			Gizmos.DrawWireSphere(m_centerTransform.position, m_actualLength);

			Gizmos.color = Color.green;
			switch(m_type)
			{
				case Type.Standard:
				{
					Gizmos.DrawLine(m_centerTransform.position, m_centerTransform.position + (Vector3)m_direction * m_length);
				}break;
				case Type.Mega:
				{
					Gizmos.DrawLine(m_centerTransform.position, m_centerTransform.position + (Vector3)m_direction * m_length * m_superFuryLengthMultiplier);
				}break;
			}
		}
	}

	void OnTriggerEnter(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			m_insideWater = true;
			if ( m_centerTransform )
				m_waterY = m_centerTransform.position.y;
			m_waterMode = true;
			ShowWaterMode();
		}
	}

	private void ShowWaterMode()
	{
		if ( IsFuryOn() )
		{
			// Change to water modes
			switch( m_type )
			{
				case Type.Standard:
				{
					// dragonFlameStandardInstance.SwitchToWaterMode();
				}break;
				case Type.Mega:
				{
					// dragonFlameSuperInstance.SwitchToWaterMode();
				}break;
			}
		}
	}


	void OnTriggerExit(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			m_insideWater = false;
			m_waterMode = false;
			ShowNormalMode();
		}
	}

	private void ShowNormalMode()
	{
		if ( IsFuryOn() )
		{
			switch( m_type )
			{
				case Type.Standard:
				{
					// dragonFlameStandardInstance.SwitchToNormalMode();
				}break;
				case Type.Mega:
				{
					// dragonFlameSuperInstance.SwitchToNormalMode();
				}break;
			}
		}
	}

    public override void PauseFury()
    {
        base.PauseFury();
        if (m_type == Type.Standard)
        {
            // Disable Particle
            m_fireParticleInstance.gameObject.SetActive(false);
        }
        else
        {
            // Disable Super Particle
            m_megaFireParticleInstance.gameObject.SetActive(false);
        }
    }

    public override void ResumeFury()
    {
        base.ResumeFury();
        if (m_type == Type.Standard)
        {
            // Enable Particle
            m_fireParticleInstance.gameObject.SetActive(true);
        }
        else
        {
			// Enable Super Particle
            m_megaFireParticleInstance.gameObject.SetActive(true);
        }
    }

}
