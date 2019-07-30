
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireBreathDynamic : MonoBehaviour , IBroadcastListener
{
    [System.Serializable]
    public struct CollisionPrefab
    {
    	public enum Type
    	{
    		NORMAL,
    		INSIDE_WATER,
    		WATER_SURFACE
    			
    	}
        [SerializeField]
        public string[] m_CollisionLayers;
        [SerializeField]
        public string m_CollisionPrefab;
        public float m_CollisionDelay;
        
        [HideInInspector, SerializeField]
        public int m_iCollisionLayerMask;
        [HideInInspector, SerializeField]
        public string m_CollisionPrefabPath;

		[SerializeField]
		public Type m_type;

		[HideInInspector]
		public ParticleHandler m_handler;
    }


    // consts
	public static readonly float CURRENT_DRAGONBABY_REFERENCEVALUE = 2.68f; //2.1985f;
    public static readonly float CURRENT_DRAGONCLASSIC_REFERENCEVALUE = 5.535f;


    public float m_distance = 1;
    public float m_aplitude = 6;
    private float m_splits = 5;

    private int m_numPos = 0;

    public float m_fireFlexFactor = 1.0f;

    private int m_collisionSplit = 0;

    public Color m_initialColor;
    public Color m_flameColor;
    public Color m_collisionColor;

    public AnimationCurve m_shapeCurve;
    public AnimationCurve m_FlameAnimation;

    public CollisionPrefab[] m_collisionPrefabs;

    private float m_collisionMaxDistance = 0.0f;
    private float m_collisionDistance = 0.0f;

    private int m_AllLayerMask;
    private int m_WaterLayerMask;

    private GameObject m_whipEnd;
    private GameObject m_collisionPlane;

    private float m_effectScale = 1.0f;
    public float m_debugScale = -1.0f;

    private float m_lastTime;


    private bool enableState = false;

	private float m_showFlameTimer = 0.0f;
    private bool m_showFlame = false;

    [System.Serializable]
    public struct ParticleSetup
    {
    	public string name;
    	public Transform anchor;
    	public bool deactivate;
    }

	public List<ParticleSetup> m_fireParticles;
	protected List<ParticleSystem> m_fireParticlesInstances = new List<ParticleSystem>();

	public List<ParticleSetup> m_underWaterParticles;
	protected List<ParticleSystem> m_underWaterParticlesInstances = new List<ParticleSystem>();


    private struct matInstanceBackup
    {
        public Material mat;
        public int renderQueue;
    };

    private List<matInstanceBackup> m_fireRushMaterials = new List<matInstanceBackup>();

	private bool m_insideWater = false;
	private float m_waterHeigth = 0;
    private bool m_darkCavesEffect = false;

    private ParticleSystem[] m_particleList;

    public float m_reflectionScale = 5.0f;
    bool m_loadingParticles = false;
   

    void OnEnable()
    {
        Broadcaster.AddListener(BroadcastEventType.BOOST_TOGGLED, this);
    }
    void OnDisable()
    {
        Broadcaster.RemoveListener(BroadcastEventType.BOOST_TOGGLED, this);
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.BOOST_TOGGLED:
            {
                ToggleParam toggleParam = (ToggleParam)broadcastEventInfo;
                OnBoostToggled(toggleParam.value); 
            }break;
        }
    }

    void OnBoostToggled(bool value)
    {
        ParticleSystem.InheritVelocityModule iVelocityModule;
        foreach (ParticleSystem psystem in m_particleList)
        {
            iVelocityModule = psystem.inheritVelocity;
//            iVelocityModule.curveMultiplier = value ? 2.0f : 0.7f;
            iVelocityModule.curveMultiplier = value ? 0.975f : 0.975f;
        }
        // Messenger.AddListener<bool>(GameEvents.BOOST_TOGGLED, OnBoostToggled);
    }


    public void setEffectScale(float furyBaseLength, float dragonScale)
    {
        m_collisionMaxDistance = (furyBaseLength * dragonScale);

        if (m_debugScale > 0.0f)
        {
            m_effectScale = m_debugScale;
        }
        else
        {
            m_effectScale = 1.0f + ((m_collisionMaxDistance - CURRENT_DRAGONBABY_REFERENCEVALUE) / (CURRENT_DRAGONCLASSIC_REFERENCEVALUE - CURRENT_DRAGONBABY_REFERENCEVALUE));
        }

        m_collisionMaxDistance *= 2.0f;

        for (int i = 0; i < m_fireParticlesInstances.Count; i++)
            m_fireParticlesInstances[i].transform.SetLocalScale(m_effectScale);

        for (int i = 0; i < m_underWaterParticlesInstances.Count; i++)
            m_underWaterParticlesInstances[i].transform.SetLocalScale(m_effectScale);

    }

    
    IEnumerator WaitToLoadParticles( List<AddressablesOp> loadingFlames)
    {
        bool done = false;
        while(!done)
        {
            yield return null;
            done = true;
            for (int i = 0; i < loadingFlames.Count && done; i++)
            {
                done = done && loadingFlames[i].isDone;
            }
        }

        ParticleSystem ps;
        int max;
        m_fireRushMaterials.Clear();

        max = m_fireParticles.Count;
        for (int i = 0; i < max; i++)
        {
            ps = ParticleManager.InitLeveledParticle(m_fireParticles[i].name, m_fireParticles[i].anchor);
            if (ps != null)
            {
                m_fireParticlesInstances.Add(ps);

                ParticleSystemRenderer psRend = ps.GetComponent<ParticleSystemRenderer>();
                matInstanceBackup mb = new matInstanceBackup();
                mb.mat = psRend.material;
                mb.renderQueue = mb.mat.renderQueue;
                m_fireRushMaterials.Add(mb);
            }
        }
        
        max = m_underWaterParticles.Count;
        for (int i = 0; i < max; i++)
        {
            ps = ParticleManager.InitLeveledParticle( m_underWaterParticles[i].name, m_underWaterParticles[i].anchor);
            if ( ps != null) { 
                m_underWaterParticlesInstances.Add( ps );
                ParticleSystemRenderer psRend = ps.GetComponent<ParticleSystemRenderer>();
                matInstanceBackup mb = new matInstanceBackup();
                mb.mat = psRend.material;
                mb.renderQueue = mb.mat.renderQueue;
                m_fireRushMaterials.Add(mb);
            }
        }    
        yield return null;
        m_loadingParticles = false;
        if (enableState)
        {
            EnableFlame(enableState, !m_showFlame);
        }
        setEffectScale(m_collisionMaxDistance / 2.0f, 1);
    }


    //Update fire rush materials for dark cave effect
    private void updateMaterialsForDarkEffect()
    {
        if (m_fireRushMaterials.Count > 0.0f)
        {
            bool darkEffect = HUDDarkZoneEffect.IsEnabled;
            if (m_darkCavesEffect != darkEffect)
            {
                foreach (matInstanceBackup mb in m_fireRushMaterials)
                {
                    if (darkEffect)
                    {
                        mb.mat.renderQueue = mb.renderQueue + 1000;
                    }
                    else
                    {
                        mb.mat.renderQueue = mb.renderQueue;
                    }
                }
                m_darkCavesEffect = darkEffect;
            }
        }
    }

            
    // Use this for initialization
    IEnumerator Start()
	{
		m_WaterLayerMask = 1 << LayerMask.NameToLayer("Water");
        m_AllLayerMask = 0;
        for (int i = 0; i < m_collisionPrefabs.Length; i++)
        {
            int layerMask = 0;
            for (int c = 0; c < m_collisionPrefabs[i].m_CollisionLayers.Length; c++)
            {
                layerMask |= LayerMask.GetMask(m_collisionPrefabs[i].m_CollisionLayers[c]);
            }
            m_collisionPrefabs[i].m_iCollisionLayerMask = layerMask;
            m_AllLayerMask |= layerMask;

            string cp = m_collisionPrefabs[i].m_CollisionPrefab;
            int backslash = cp.LastIndexOf('/');
            if (backslash >= 0)
            {
                m_collisionPrefabs[i].m_CollisionPrefabPath = cp.Substring(0, backslash);
                m_collisionPrefabs[i].m_CollisionPrefab = cp.Substring(backslash + 1);
            }
            else
            {
                m_collisionPrefabs[i].m_CollisionPrefabPath = "";
            }

			m_collisionPrefabs[i].m_handler = ParticleManager.CreatePool(m_collisionPrefabs[i].m_CollisionPrefab,
                                       									 m_collisionPrefabs[i].m_CollisionPrefabPath);
        }

        // Cache
        m_numPos = (int)(4 + m_splits * 2);

        m_whipEnd = transform.Find("WhipEnd").gameObject;
        m_collisionPlane = transform.Find("WhipEnd/collisionPlane").gameObject;
        m_particleList = GetComponentsInChildren<ParticleSystem>();
        
        
        // Instantiate all particles
        List<AddressablesOp> loadingFlames = new List<AddressablesOp>();
        
        int max = m_fireParticles.Count;
        string particleId;
        for (int i = 0; i < max; i++) {
            particleId = m_fireParticles[i].name;
            loadingFlames.Add(HDAddressablesManager.Instance.LoadDependenciesAsync(particleId, ParticleManager.GetVariant(particleId)));                
        }

        max = m_underWaterParticles.Count;
        for (int i = 0; i < max; i++) {
            particleId = m_underWaterParticles[i].name;
            loadingFlames.Add(HDAddressablesManager.Instance.LoadDependenciesAsync(particleId, ParticleManager.GetVariant(particleId)));
        }                

        if ( loadingFlames.Count > 0 )
        {
            m_loadingParticles = true;
            yield return StartCoroutine( WaitToLoadParticles(loadingFlames));
        }
        yield return null;
    }

	
	// Update is called once per frame
	void FixedUpdate () 
	{
        UpdateWhip();

        Vector3 particlePos = m_whipEnd.transform.localPosition;
        float particleDistance = m_distance * (m_effectScale*0.38f);       
        particlePos.x = m_collisionDistance < particleDistance ? m_collisionDistance : particleDistance;
        m_whipEnd.transform.localPosition = particlePos;

        updateMaterialsForDarkEffect();
    }

    void UpdateWhip()
    {
        RaycastHit hit;
        Vector3 whipDirection = transform.right;
        whipDirection.z = 0.0f;
        whipDirection.Normalize();

        Vector3 whipOrigin = transform.position;

        if ( m_showFlame )
        {
        	m_showFlameTimer += Time.deltaTime;	// Scale flame up
        }
        else
        {
        	m_showFlameTimer -= Time.deltaTime * 10;	// Scale Flame down
        }
        m_showFlameTimer = Mathf.Clamp01( m_showFlameTimer );

        float flameAnim = m_FlameAnimation.Evaluate( m_showFlameTimer );
        if (!enableState && m_showFlameTimer <= 0)
        {
        	if (!HasParticleAlive() && !m_loadingParticles)
            	gameObject.active = false;
        }
		float xStep = (flameAnim * m_distance * m_effectScale) / (m_splits + 1);
        m_collisionSplit = (int)m_splits - 1;
		m_collisionDistance = m_collisionMaxDistance;


        bool hitsSomething = false;	// if fire colliding with something
        Vector3 hitPoint = Vector3.zero;	// Closest hit point
        Transform collisionPlaneTransform = null;

		if (Physics.Raycast(transform.position, transform.right, out hit, m_collisionDistance, m_AllLayerMask))
        {
        	hitsSomething = true;
            int hitLayer = 1 << hit.transform.gameObject.layer;

            foreach (CollisionPrefab cp in m_collisionPrefabs)
            {
                if ((cp.m_iCollisionLayerMask & hitLayer) != 0 )
                {
					bool spawn = false;
					switch( cp.m_type )
					{
						case CollisionPrefab.Type.NORMAL:
						{
							spawn = !WaterAreaManager.instance.IsInsideWater( hit.point );
						}break;

						case CollisionPrefab.Type.INSIDE_WATER:
						{
							spawn = WaterAreaManager.instance.IsInsideWater( hit.point );
						}break;

						default:
						{
						}break;
					}
                    if (spawn && Time.time > m_lastTime + cp.m_CollisionDelay)
                    {
						GameObject colFire = cp.m_handler.Spawn(null, hit.point);
                        if (colFire != null)
                        {
                            colFire.transform.rotation = Quaternion.LookRotation(-Vector3.forward, hit.normal);
                        }

                        m_lastTime = Time.time;
                    }
                }
            }

            m_collisionPlane.transform.position = hit.point;
            m_collisionPlane.transform.up = hit.normal;

			collisionPlaneTransform = m_collisionPlane.transform;
            m_collisionDistance = hit.distance;
            hitPoint = hit.point;
        }



        if ( m_insideWater )
        {
        	Vector3 endPos = transform.position + transform.right * m_collisionDistance;
			if (Physics.Raycast(endPos, -transform.right, out hit, m_collisionDistance, m_WaterLayerMask))
			{
				hitsSomething = true;
				int hitLayer = 1 << hit.transform.gameObject.layer;
				foreach (CollisionPrefab cp in m_collisionPrefabs)
	            {
					if ((cp.m_iCollisionLayerMask & hitLayer) != 0 && cp.m_type == CollisionPrefab.Type.WATER_SURFACE)
	                {
						bool spawn = false;
	                    if (spawn && Time.time > m_lastTime + cp.m_CollisionDelay)
	                    {
							GameObject colFire = cp.m_handler.Spawn(null, hit.point);
	                        if (colFire != null)
	                        {
	                            colFire.transform.rotation = Quaternion.LookRotation(-Vector3.forward, hit.normal);
	                        }

	                        m_lastTime = Time.time;
	                    }
	                    break;
	                }
	            }

				m_collisionPlane.transform.position = hit.point;
	            m_collisionPlane.transform.up = -hit.normal;

				collisionPlaneTransform = m_collisionPlane.transform;
				m_collisionDistance = m_collisionDistance - hit.distance;
	            hitPoint = hit.point;
			}
        }


        if ( hitsSomething )
        {
			SetParticleCollisionsPlane(collisionPlaneTransform);
        }
        else
        {
			SetParticleCollisionsPlane(null);
        }

    }


    void SetParticleCollisionsPlane( Transform _tr )
    {
		for( int i = 0; i<m_fireParticlesInstances.Count; i++ )
			m_fireParticlesInstances[i].collision.SetPlane(0, _tr);

		for( int i = 0; i<m_underWaterParticlesInstances.Count; i++ )
			m_underWaterParticlesInstances[i].collision.SetPlane(0, _tr);
    }

    bool HasParticleAlive()
    {
		for( int i = 0; i<m_fireParticlesInstances.Count; i++ )
			if (m_fireParticlesInstances[i].IsAlive())
				return true;

		for( int i = 0; i<m_underWaterParticlesInstances.Count; i++ )
			if (m_underWaterParticlesInstances[i].IsAlive())
				return true;
		return false;
    }

    public void EnableFlame(bool value, bool insideWater = false)
    {
    	if ( value )
    	{
			gameObject.active = true;
			m_showFlame = !insideWater;
    		// Check if inside water!
			for( int i = 0; i<m_fireParticlesInstances.Count; i++ )
			{
				m_fireParticlesInstances[i].gameObject.SetActive(true);
				if (!insideWater)
					m_fireParticlesInstances[i].Play();
			}

			for( int i = 0; i<m_underWaterParticlesInstances.Count; i++ )
			{
				m_underWaterParticlesInstances[i].gameObject.SetActive(true);
				if (insideWater)
					m_underWaterParticlesInstances[i].Play();
			}
    	}
    	else
    	{
			for( int i = 0; i<m_fireParticlesInstances.Count; i++ )
			{
				m_fireParticlesInstances[i].Stop();
				if ( m_fireParticles[i].deactivate ){
					m_fireParticlesInstances[i].gameObject.SetActive(false);	
				}
			}

			for( int i = 0; i<m_underWaterParticlesInstances.Count; i++ )
			{
				m_underWaterParticlesInstances[i].Stop();
				if ( m_underWaterParticles[i].deactivate ){
					m_underWaterParticlesInstances[i].gameObject.SetActive(false);	
				}
			}

			m_showFlame = false;
    	}

        enableState = value;
    }



	public void SwitchToWaterMode()
    {
		m_insideWater = true;
		m_showFlame = false;
		m_waterHeigth = transform.position.y;
		for( int i = 0; i<m_fireParticlesInstances.Count; i++ )
			m_fireParticlesInstances[i].Stop();

		for( int i = 0; i<m_underWaterParticlesInstances.Count; i++ )
			m_underWaterParticlesInstances[i].Play();
    }

    public void SwitchToNormalMode()
    {
		m_insideWater = false;
		m_showFlame = true;
		for( int i = 0; i<m_fireParticlesInstances.Count; i++ )
			m_fireParticlesInstances[i].Play();

		for( int i = 0; i<m_underWaterParticlesInstances.Count; i++ )
			m_underWaterParticlesInstances[i].Stop();
    }
}
