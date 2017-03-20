using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonParticleController : MonoBehaviour 
{

	public GameObject m_levelUp;
	public Transform m_levelUpAnchor;
	private ParticleSystem m_levelUpInstance;

	[Space]
	public GameObject m_revive;
	public Transform m_reviveAnchor;
	private ParticleSystem m_reviveInstance;
	public ParticleData m_petRevive;

	[Space]
	public GameObject m_bubbles;
	public Transform m_bubblesAnchor;
	public float m_bubblesDrowningMultiplier = 3;
	private ParticleSystem m_bubblesInstance;
	private float m_defaultRate = 1;
	private float m_rateMultiplier = 0;


	[Space]
	public GameObject m_cloudTrail;
	public Transform m_cloudTrailAnchor;
	private ParticleSystem m_cloudTrailInstance;

	[Space]
	public float m_minSpeedEnterSplash;
	public string m_waterEnterSplash;
	public float m_minSpeedExitSplash;
	public string m_waterExitSplash;
	public string m_waterSplashFolder = "Water";

	[Space]
	public GameObject m_skimmingParticle;
	public Transform m_skimmingAnchor;
	public float m_minSpeedSkimming = 1;
	public float m_skimmingDistance = 1;
	private bool m_skimming = false;
	private ParticleSystem m_skimmingInstance = null;
	private Ray m_skimmingRay;
	private RaycastHit m_rayHit;
	private int m_waterLayer;

	[Space]
	public GameObject m_waterAirLimitParticle;
	private ParticleSystem m_waterAirLimitInstance = null;

	[Space]
	public string m_corpseAsset = "";


	[Space]
	public string m_hiccupParticle;
	public Transform m_hiccupAnchor = null;
	private ParticleSystem m_hiccupInstance;

	[Space]
	public ParticleData m_shieldParticle;
	public Transform m_shieldAnchor = null;
	private ParticleSystem m_shieldInstance;


	private Transform _transform;
	private bool m_insideWater = false;
	private bool m_alive = true;
	private float m_waterY = 0;
	private float m_waterDepth = 5;
	private const float m_waterDepthIncrease = 8;
	private DragonMotion m_dargonMotion;
	private DragonEatBehaviour m_dragonEat;
	private DragonEquip m_dragonEquip;

	[System.Serializable]
	public class BodyParticle
	{
		public bool m_stopInsideWater = false;
		public bool m_stopWhenDead = false;
		public ParticleSystem m_particleReference;
	}

	[Space]
	public List<BodyParticle> m_bodyParticles = new List<BodyParticle>();

	void Start () 
	{
		DragonAnimationEvents dragonAnimEvents = transform.parent.GetComponentInChildren<DragonAnimationEvents>();
		
		// Instantiate Particles (at start so we don't feel any framerate drop during gameplay)
		m_levelUpInstance = InitParticles(m_levelUp, m_levelUpAnchor);
		m_reviveInstance = InitParticles(m_revive, m_reviveAnchor);
		m_bubblesInstance = InitParticles(m_bubbles, m_bubblesAnchor);
		if ( m_bubblesInstance != null )
		{
			m_defaultRate = m_bubblesInstance.emission.rateOverTimeMultiplier;
			m_rateMultiplier = m_defaultRate * m_bubblesDrowningMultiplier;
		}

		m_cloudTrailInstance = InitParticles(m_cloudTrail, m_cloudTrailAnchor);
		m_dargonMotion = transform.parent.GetComponent<DragonMotion>();
		m_dragonEat = transform.parent.GetComponent<DragonEatBehaviour>();
		m_dragonEquip = transform.parent.GetComponent<DragonEquip>();
		m_waterDepth = InstanceManager.player.data.scale + m_waterDepthIncrease;
		_transform = transform;

		if ( !string.IsNullOrEmpty(m_waterEnterSplash) )
			ParticleManager.CreatePool(m_waterEnterSplash, m_waterSplashFolder);
		if ( !string.IsNullOrEmpty(m_waterExitSplash) )
			ParticleManager.CreatePool(m_waterExitSplash, m_waterSplashFolder);


		m_skimmingInstance = InitParticles(m_skimmingParticle, m_skimmingAnchor);

		m_skimmingRay = new Ray();
		m_skimmingRay.direction = Vector3.down;

		m_waterLayer = 1<<LayerMask.NameToLayer("Water");

		if (m_waterAirLimitParticle != null)
			m_waterAirLimitInstance = InitParticles( m_waterAirLimitParticle, m_dragonEat.mouth);

		if (!string.IsNullOrEmpty(m_corpseAsset)) {
			PoolManager.CreatePool(m_corpseAsset, "Game/Corpses/", 1, true, false);
		}
		m_hiccupInstance = InitParticles( m_hiccupParticle, m_hiccupAnchor);
		if (dragonAnimEvents != null)
			dragonAnimEvents.onHiccupEvent += OnHiccup;
	}

	void OnEnable() {
		// Register events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Messenger.AddListener<DamageType>(GameEvents.PLAYER_KO, OnKo);
		Messenger.AddListener(GameEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPreRevive);
		Messenger.AddListener<DragonPlayer.ReviveReason>(GameEvents.PLAYER_REVIVE, OnRevive);
		Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_LOST_SHIELD, OnShieldLost);
	}

	void OnDisable()
	{
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Messenger.RemoveListener<DamageType>(GameEvents.PLAYER_KO, OnKo);
		Messenger.RemoveListener(GameEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPreRevive);
		Messenger.RemoveListener<DragonPlayer.ReviveReason>(GameEvents.PLAYER_REVIVE, OnRevive);
		Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_LOST_SHIELD, OnShieldLost);
	}

	void Update()
	{
		if ( m_insideWater && m_bubblesInstance != null)	
		{
			if ( m_waterY - _transform.position.y > m_waterDepth ){
				// Bubbles should be activated
				if ( !m_bubblesInstance.isPlaying )
					m_bubblesInstance.Play();
			}else{
				// Bubbles should be desactivated
				if ( m_bubblesInstance.isPlaying )
					m_bubblesInstance.Stop();
			}
		}


		// Skimming
		if (m_skimmingParticle != null)
		{
			m_skimmingRay.origin = _transform.position;
			bool speedToSkim = Mathf.Abs(m_dargonMotion.velocity.x) >= m_minSpeedSkimming;
			if ( m_skimming )
			{
				bool stopSkimming = !speedToSkim;
				if ( speedToSkim )
				{
					bool hitsWater = Physics.Raycast(m_skimmingRay, out m_rayHit ,m_skimmingDistance, m_waterLayer);
					if (!hitsWater)
						stopSkimming = true;
				}

				if ( stopSkimming )
				{
					m_skimmingInstance.Stop();
					m_skimming = false;
					m_dargonMotion.EndedSkimming();
				}
			}
			else
			{
				if ( speedToSkim )
				{
					// if speed bigger than min and hitting water -> start
					// bool hitsWater = Physics.Raycast(m_skimmingRay, out m_rayHit ,m_skimmingDistance);
					// bool hitsWater = Physics.Linecast( _transform.position, _transform.position + Vector3.down * m_skimmingDistance, out m_rayHit);
					bool hitsWater = Physics.Raycast(m_skimmingRay, out m_rayHit ,m_skimmingDistance, m_waterLayer);
					if ( hitsWater )
					{
						// Start skimming	
						m_skimmingInstance.Play();
						m_skimming = true;
						m_dargonMotion.StartedSkimming();
					}
				}
			}

			if ( m_skimming )
			{
				m_skimmingInstance.transform.position = m_rayHit.point;
				// Set direction
			}
		}


#if UNITY_EDITOR
		if ( Input.GetKeyDown(KeyCode.E))
		{
			OnShieldLost( DamageType.MINE, null);
		}
#endif

	}

	private ParticleSystem InitParticles(string particle, Transform _anchor)
	{
		ParticleSystem ret = null;
		GameObject go = Resources.Load<GameObject>( "Particles/" + particle );
		if ( go != null )
		{
			 ret = InitParticles( go,  _anchor);
		}
		return ret;
	}

	private ParticleSystem InitParticles(GameObject _prefab, Transform _anchor)
	{
		if(_prefab == null || _anchor == null) return null;

		GameObject go = Instantiate(_prefab);
		ParticleSystem psInstance = go.GetComponent<ParticleSystem>();
		if(psInstance != null) {
			psInstance.transform.SetParentAndReset(_anchor);
			psInstance.Stop();
		}
		return psInstance;
	}

	void OnLevelUp( DragonData data )
	{
		m_levelUpInstance.Play();
		m_waterDepth = data.scale + m_waterDepthIncrease;
	}

	void OnKo( DamageType type )
	{
		m_alive = false;	
		CheckBodyParts();
		if ( type == DamageType.MINE )
		{
			SpawnCorpse();
		}
	}

	void SpawnCorpse()
	{
		if (!string.IsNullOrEmpty(m_corpseAsset)) {
			// spawn corpse
			GameObject corpse = PoolManager.GetInstance(m_corpseAsset, true);
			corpse.transform.CopyFrom(transform);
			Corpse c = corpse.GetComponent<Corpse>();
			c.Spawn(false, false);
			c.SwitchDragonTextures(m_dragonEquip.bodyMaterial.mainTexture, m_dragonEquip.wingsMaterial.mainTexture);

		}
	}

	void OnPreRevive()
	{
		// Instantiate particle
		GameObject instance = m_petRevive.CreateInstance();
		instance.transform.position = m_reviveAnchor.position + m_petRevive.offset;
	}

	void OnRevive(DragonPlayer.ReviveReason reason)
	{
		switch( reason )
		{
			default:
			{
				m_reviveInstance.Play();
			}break;
			case DragonPlayer.ReviveReason.FREE_REVIVE_PET:
			{
				
			}break;
		}
		m_alive = true;
		CheckBodyParts();
	}

	public void OnShieldLost( DamageType _damageType, Transform _tr)
	{
		GameObject instance =  m_shieldParticle.CreateInstance();
		instance.transform.parent = m_shieldAnchor;
		instance.transform.localPosition = m_shieldParticle.offset;
		if ( _tr != null )
		{
			instance.transform.LookAt( _tr.position );
		}
	}


	public bool OnEnterWater( Collider _other )
	{
		m_waterY = _transform.position.y;
		m_insideWater = true;
		if ( m_bubblesInstance != null )
		{
			ParticleSystem.EmissionModule emission = m_bubblesInstance.emission;
			emission.rateOverTimeMultiplier = m_defaultRate;
		}

		CheckBodyParts();

		if ( m_dargonMotion != null && Mathf.Abs(m_dargonMotion.velocity.y) >= m_minSpeedEnterSplash )
		{
			CreateSplash(_other, m_waterEnterSplash);
			return true;
		}
		return false;
	}

	public bool OnExitWater( Collider _other )
	{
		m_insideWater = false;
		if ( m_bubblesInstance != null && m_bubblesInstance.isPlaying)
			m_bubblesInstance.Stop();

		CheckBodyParts();

		if ( m_dargonMotion != null && Mathf.Abs(m_dargonMotion.velocity.y) >= m_minSpeedExitSplash )
		{
			CreateSplash(_other, m_waterExitSplash);
			return true;
		}
		return false;
	}

	private void CreateSplash( Collider _other, string particleName )
	{
		Vector3 pos = _transform.position;
		float waterY = _other.bounds.center.y + _other.bounds.extents.y;
		pos.y = waterY;

		ParticleManager.Spawn(particleName, pos, m_waterSplashFolder);
	}

	public void OnEnterOuterSpace() {
		// Launch cloud trail, will stop automatically
		if(m_cloudTrailInstance != null) {
			m_cloudTrailInstance.Play();
		}
	}

	public void OnExitOuterSpace() {
		// Launch cloud trail again!
		if(m_cloudTrailInstance != null) {
			m_cloudTrailInstance.Play();
		}
	}

	/// <summary>
	/// Function call when Dragon Motion is forced to go up inside water
	/// </summary>
	public void OnNoAirBubbles()
	{
		if ( m_insideWater )
		{
			if (m_waterAirLimitInstance != null)
			{
				m_waterAirLimitInstance.transform.rotation = Quaternion.Euler(90,0,0);
				m_waterAirLimitInstance.Play();
			}

			if ( m_bubblesInstance != null )
			{
				ParticleSystem.EmissionModule emission = m_bubblesInstance.emission;
				emission.rateOverTimeMultiplier = m_rateMultiplier;
			}
		}
	}

	public bool ShouldShowDeepLimitParticles()
	{
		if (m_waterAirLimitInstance != null && (m_waterY - m_dragonEat.mouth.position.y) > m_waterDepth)
			return true;
		return false;
	}


	public void CheckBodyParts()
	{
		for( int i = 0; i<m_bodyParticles.Count; i++ )
		{
			if ( (m_insideWater && m_bodyParticles[i].m_stopInsideWater)
				|| (!m_alive && m_bodyParticles[i].m_stopWhenDead)
			)
			{
				if (m_bodyParticles[i].m_particleReference.isPlaying)
					m_bodyParticles[i].m_particleReference.Stop();
			}
			else
			{
				if (!m_bodyParticles[i].m_particleReference.isPlaying)
					m_bodyParticles[i].m_particleReference.Play();
			}
		}
	}

	private void OnHiccup()
	{
		if ( m_hiccupInstance != null )
			m_hiccupInstance.Play();
	}
}
