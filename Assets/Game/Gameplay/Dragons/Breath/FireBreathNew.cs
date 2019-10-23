using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireBreathNew : DragonBreathBehaviour {

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

	// Arc detection values
	private const float m_minAngularSpeed = 0;
	private const float m_maxAngularSpeed = 10;
	private const float m_minArcAngle = 45;
	private const float m_maxArcAngle = 90;
	private float m_arcAngle = 50;

    [Header("Particle")]

	private int m_groundMask;
	private int m_noPlayerMask;

	private Transform m_mouthTransform;

	private float m_area;

	private int m_frame;

	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;

	private Cage[] m_checkCages = new Cage[50];
	private int m_numCheckCages = 0;

    public GameObject m_dragonFlameStandard = null;
    public GameObject m_dragonFlameSuper = null;

    private FireBreathDynamic dragonFlameStandardInstance = null;
    private FireBreathDynamic dragonFlameSuperInstance = null;
    //    private FireBreathDynamic dragonFlameInstance = null;

    private bool m_insideWater = false;
    private bool m_waterMode = false;
    private float m_waterY = 0;

    private DragonMotion m_motion;


    override protected void ExtendedStart() {

        Transform cacheTransform = transform;
        Transform mouth = cacheTransform.FindTransformRecursive("mouth");
        m_mouthTransform = mouth;

        GameObject tempFire = Instantiate<GameObject>(m_dragonFlameStandard);
        Transform t = tempFire.transform;
        t.SetParent(mouth, true);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f));
        dragonFlameStandardInstance = tempFire.GetComponent<FireBreathDynamic>();

        tempFire = Instantiate<GameObject>(m_dragonFlameSuper);
        tempFire.transform.SetParent(mouth, true);
        tempFire.transform.localPosition = Vector3.zero;
        tempFire.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f));
        dragonFlameSuperInstance = tempFire.GetComponent<FireBreathDynamic>();

        m_groundMask = GameConstants.Layers.GROUND_WATER_FIREBLOCK;
        m_noPlayerMask = ~GameConstants.Layers.PLAYER;

        m_actualLength = m_length;
       
		m_direction = Vector2.zero;

		m_frame = 0;

        dragonFlameStandardInstance.EnableFlame(false);
        dragonFlameStandardInstance.gameObject.SetActive(false);
        dragonFlameSuperInstance.EnableFlame(false);
        dragonFlameSuperInstance.gameObject.SetActive(false);

		m_motion = GetComponent<DragonMotion>();
    }

    override public void RecalculateSize()
    {
    	if ( m_dragon )
    	{
            float furyBaseLength = m_dragon.data.furyBaseLength;
			m_length = furyBaseLength + furyBaseLength * lengthPowerUpPercentage / 100.0f;

			dragonFlameStandardInstance.setEffectScale(m_length / 2.0f, transform.localScale.x);
			dragonFlameSuperInstance.setEffectScale(m_length * m_superFuryLengthMultiplier / 2.0f, transform.localScale.x);

			m_length *= transform.localScale.x;
		}
    }

  

    override public bool IsInsideArea(Vector2 _point) { 
	
		if (IsFuryOn()) {
			if (m_bounds2D.Contains(_point)) {
				return MathUtils.TestArcVsPoint( m_mouthTransform.position, m_arcAngle, m_actualLength, m_direction, _point);
			}
		}

		return false; 
	}

	override public bool Overlaps(CircleAreaBounds _circle)
	{
		if (IsFuryOn()) 
		{
			Vector3 center = _circle.center;
			center.z = 0;
			return MathUtils.TestArcVsCircle( m_mouthTransform.position, m_arcAngle, m_actualLength, m_direction, center, _circle.radius);
		}
		return false;
	}

	override protected void BeginFury(Type _type) 
	{
		base.BeginFury( _type);
		StartCoroutine( StartFlame(0.25f, _type));
        
    }

	IEnumerator StartFlame( float delay, Type _type )
    {
    	yield return new WaitForSeconds(delay);
        if ( !isFuryPaused )
        {
    		if (_type == Type.Standard)
            {
                dragonFlameStandardInstance.EnableFlame(true, m_insideWater);
            }
            else
            {
                dragonFlameSuperInstance.EnableFlame(true, m_insideWater);
            }
        }
    }


	override protected void EndFury(bool increase_mega_fury = true) 
	{
        if (m_type == Type.Standard)
        {
            dragonFlameStandardInstance.EnableFlame(false);
        }
        else
        {
            dragonFlameSuperInstance.EnableFlame(false);
        }
		base.EndFury(increase_mega_fury);

    }

    override protected void Breath()
    {
        m_direction = -m_mouthTransform.right;
		m_direction.Normalize();

        float localLength = m_length;
		if ( m_type == Type.Mega )
			localLength = m_length * m_superFuryLengthMultiplier;

		float angularSpeed = m_motion.angularVelocity.magnitude;
		// Debug.Log("Angular: " + angularSpeed);
		m_arcAngle = Util.Remap(angularSpeed, m_minAngularSpeed, m_maxAngularSpeed, m_minArcAngle, m_maxArcAngle);

		Vector3 flamesUpDir = Vector3.up;
		if (m_frame == 0) {
			// Raycast to ground
			RaycastHit ground;				
			if (Physics.Linecast(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * localLength, out ground, m_groundMask)) 
			{
				m_actualLength = ground.distance;
			} 
			else 
			{				
				m_actualLength = localLength;
			}

			if (Physics.Linecast(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * localLength, out ground, m_noPlayerMask)) 
			{
				flamesUpDir = Vector3.Reflect( m_direction, ground.normal);
				flamesUpDir.Normalize();

			}
		}

		m_frame = (m_frame + 1) % 4;

		m_bounds2D.Set( m_mouthTransform.position.x - m_actualLength, m_mouthTransform.position.y - m_actualLength, m_actualLength * 2, m_actualLength * 2);

		//--------------------------------------------------------------------------------------------
		// try to burn things!!!
		// search for preys!
		// Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_sphCenter, m_sphRadius);
		m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities(m_mouthTransform.position, m_actualLength, m_checkEntities);
		for (int i = 0; i < m_numCheckEntities; i++) 
		{
			Entity prey = m_checkEntities[i];
			if ((prey.circleArea != null && Overlaps((CircleAreaBounds)prey.circleArea.bounds)) || IsInsideArea(prey.transform.position)) 
			{
				if (prey.IsBurnable()) {
					if (prey.IsBurnable(m_tier) || m_type == Type.Mega) {
						AI.IMachine machine =  m_checkEntities[i].machine;
						if (machine != null) {
							machine.Burn(transform, IEntity.Type.PLAYER, false, m_type == Type.Mega, m_currentColor);
						}
					} else {
						// Show message saying I cannot burn it
						Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, DragonTier.COUNT, prey.sku);
					}
				}
			}
		}

		// pick cages 
		m_numCheckCages = EntityManager.instance.GetCagesInRange2DNonAlloc(m_mouthTransform.position, m_actualLength, m_checkCages);
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
			if (m_mouthTransform.position.y > m_waterY)
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

	void OnDrawGizmos() {
		if (IsFuryOn()) {
			Gizmos.color = Color.magenta;

			// Arc Drawing
			Vector3 dir = m_direction;
			Gizmos.DrawWireSphere(m_mouthTransform.position, m_actualLength);
			Debug.DrawLine(m_mouthTransform.position, m_mouthTransform.position + dir * m_actualLength);
			Vector3 dUp = dir.RotateXYDegrees(m_arcAngle/2.0f);
			Debug.DrawLine( m_mouthTransform.position, m_mouthTransform.position + (dUp * m_actualLength));
			Vector3 dDown = dir.RotateXYDegrees(-m_arcAngle/2.0f);
			Debug.DrawLine( m_mouthTransform.position, m_mouthTransform.position + (dDown * m_actualLength));

			Gizmos.color = Color.green;
			switch(m_type)
			{
				case Type.Standard:
				{
					Gizmos.DrawLine(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length);
				}break;
				case Type.Mega:
				{
					Gizmos.DrawLine(m_mouthTransform.position, m_mouthTransform.position + (Vector3)m_direction * m_length * m_superFuryLengthMultiplier);
				}break;
			}
		}
	}

	void OnTriggerEnter(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			m_insideWater = true;
			if ( m_mouthTransform )
				m_waterY = m_mouthTransform.position.y;
			m_waterMode = true;
			ShowWaterMode();
		}
	}

	private void ShowWaterMode()
	{
		if ( IsFuryOn() && !m_isFuryPaused )
		{
			// Change to water modes
			switch( m_type )
			{
				case Type.Standard:
				{
					dragonFlameStandardInstance.SwitchToWaterMode();
				}break;
				case Type.Mega:
				{
					dragonFlameSuperInstance.SwitchToWaterMode();
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
		if ( IsFuryOn() && !m_isFuryPaused)
		{
			switch( m_type )
			{
				case Type.Standard:
				{
					dragonFlameStandardInstance.SwitchToNormalMode();
				}break;
				case Type.Mega:
				{
					dragonFlameSuperInstance.SwitchToNormalMode();
				}break;
			}
		}
	}

    public override void PauseFury()
    {
        base.PauseFury();
        if (m_type == Type.Standard)
        {
            dragonFlameStandardInstance.EnableFlame(false);
        }
        else
        {
            dragonFlameSuperInstance.EnableFlame(false);
        }
    }

    public override void ResumeFury()
    {
        base.ResumeFury();
        if (m_type == Type.Standard)
        {
			dragonFlameStandardInstance.EnableFlame(true, m_insideWater);
        }
        else
        {
			dragonFlameSuperInstance.EnableFlame(true, m_insideWater);
        }
    }

}
