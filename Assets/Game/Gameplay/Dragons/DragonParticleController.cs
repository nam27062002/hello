﻿using UnityEngine;
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
	public ParticleData m_spaceEnterSplash = new ParticleData("PS_DiveIn", "Water" , Vector3.zero);
	public ParticleData m_spaceExitSplash = new ParticleData("PS_DiveOut", "Water" , Vector3.zero);

	[Space]
	public float m_minSpeedEnterSplash;
	public string m_waterEnterSplash;
	private ParticleHandler m_waterEnterSplashHandler;

	public float m_minSpeedExitSplash;
	public string m_waterExitSplash;
	private ParticleHandler m_waterExitSplashHandler;

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
	private ParticleHandler m_corpseHandler;

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

	[Space]
	// trails stuff
	bool m_trailsActive = false;
	bool m_playinTrails = false;
	ParticleSystem m_trailsInstance;
	public ParticleData m_trailsParticle;
	public Transform m_trailsAnchor;

	[Space]
	public ParticleData m_landingParticle;
	ParticleSystem m_landingInstance;

	private List<ParticleSystem> m_toDeactivate = new List<ParticleSystem>();


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
			m_waterEnterSplashHandler = ParticleManager.CreatePool(m_waterEnterSplash, m_waterSplashFolder);
		if ( !string.IsNullOrEmpty(m_waterExitSplash) )
			m_waterExitSplashHandler = ParticleManager.CreatePool(m_waterExitSplash, m_waterSplashFolder);


		m_skimmingInstance = InitParticles(m_skimmingParticle, m_skimmingAnchor);

		m_skimmingRay = new Ray();
		m_skimmingRay.direction = Vector3.down;

		m_waterLayer = 1<<LayerMask.NameToLayer("Water");

		if (m_waterAirLimitParticle != null)
			m_waterAirLimitInstance = InitParticles( m_waterAirLimitParticle, m_dragonEat.mouth);

		if (!string.IsNullOrEmpty(m_corpseAsset)) {
			m_corpseHandler = ParticleManager.CreatePool(m_corpseAsset, "Corpses/");
		}
		m_hiccupInstance = InitParticles( m_hiccupParticle, m_hiccupAnchor);

		if (dragonAnimEvents != null)
			dragonAnimEvents.onHiccupEvent += OnHiccup;

		if ( m_trailsParticle.IsValid() )
		{
			GameObject go = m_trailsParticle.CreateInstance();
			go.transform.parent = m_trailsAnchor;
			go.transform.localPosition = Vector3.zero + m_trailsParticle.offset;
			go.transform.localScale = Vector3.one;
			go.transform.localRotation = Quaternion.identity;
			m_trailsInstance = go.GetComponent<ParticleSystem>();
			m_trailsInstance.gameObject.SetActive(false);
		}

		if ( m_landingParticle.IsValid() )
		{
			GameObject go = m_landingParticle.CreateInstance();
			m_landingInstance = go.GetComponent<ParticleSystem>();
			m_landingInstance.gameObject.SetActive(false);
		}

		if ( m_spaceEnterSplash.IsValid() )
			m_spaceEnterSplash.CreatePool();
		if ( m_spaceExitSplash.IsValid() )
			m_spaceExitSplash.CreatePool();
	}

	void OnEnable() {
		// Register events
		Messenger.AddListener<DragonData>(MessengerEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnKo);
		Messenger.AddListener(MessengerEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPreRevive);
		Messenger.AddListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnRevive);
		Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_LOST_SHIELD, OnShieldLost);
		Messenger.AddListener(MessengerEvents.GAME_AREA_ENTER, OnGameAreaEnter);
	}

	void OnDisable()
	{
		Messenger.RemoveListener<DragonData>(MessengerEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnKo);
		Messenger.RemoveListener(MessengerEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPreRevive);
		Messenger.RemoveListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnRevive);
		Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_LOST_SHIELD, OnShieldLost);
		Messenger.RemoveListener(MessengerEvents.GAME_AREA_ENTER, OnGameAreaEnter);
	}

	void OnGameAreaEnter()
	{
		if (!string.IsNullOrEmpty(m_corpseAsset)) {
			m_corpseHandler = ParticleManager.CreatePool(m_corpseAsset, "Corpses/");
		}
	}

	void Update()
	{
		if ( m_insideWater && m_bubblesInstance != null)	
		{
			if ( m_waterY - _transform.position.y > m_waterDepth ){
				// Bubbles should be activated
				if ( !m_bubblesInstance.isPlaying )
				{
					m_bubblesInstance.gameObject.SetActive(true);
					m_bubblesInstance.Play();

				}
			}else{
				// Bubbles should be desactivated
				if ( m_bubblesInstance.isPlaying )
				{
					m_bubblesInstance.Stop();
					m_toDeactivate.Add(m_bubblesInstance);
				}
			}
		}


		// Skimming
		if (m_skimmingParticle != null)
		{
			m_skimmingRay.origin = m_skimmingAnchor.position;
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
					m_toDeactivate.Add( m_skimmingInstance );
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
						m_skimmingInstance.gameObject.SetActive(true);
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

		if ( m_trailsActive )
		{
			if (!m_playinTrails)
			{
				if (!m_insideWater && m_dargonMotion.howFast > 0.85f )	// Check speed
				{
					PlayTrails();
				}
			}
			else if ( m_insideWater )
			{
				// Stop
				StopTrails();
			}
		}
		else
		{
			if ( m_playinTrails )
			{
				// Stop
				StopTrails();
			}
		}

		int size = m_toDeactivate.Count;
		for( int i = size-1; i>= 0; --i )
		{
			if ( !m_toDeactivate[i].IsAlive() )	
			{
				m_toDeactivate[i].gameObject.SetActive(false);
				m_toDeactivate.RemoveAt(i);
			}
		}


#if UNITY_EDITOR
		if ( Input.GetKeyDown(KeyCode.E))
		{
			OnShieldLost( DamageType.MINE, null);
		}

		if ( Input.GetKeyDown(KeyCode.T))
		{
			SpawnCorpse();
		}

#endif

	}

	private ParticleSystem InitParticles(string particle, Transform _anchor)
	{
		ParticleSystem ret = null;
		GameObject go = Resources.Load<GameObject>( "Particles/Master/" + particle );
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
			go.SetActive(false);
		}
		return psInstance;
	}

	void OnLevelUp( DragonData data )
	{
		m_levelUpInstance.gameObject.SetActive(true);
		m_levelUpInstance.Play();
		m_toDeactivate.Add(m_levelUpInstance);
		m_waterDepth = data.scale + m_waterDepthIncrease;
	}

	void OnKo( DamageType type , Transform _source)
	{
		m_alive = false;	
		CheckBodyParts();
		if ( type == DamageType.MINE || type == DamageType.BIG_DAMAGE )
		{
			SpawnCorpse();
		}
	}

	void SpawnCorpse()
	{
		if (!string.IsNullOrEmpty(m_corpseAsset)) {
			// spawn corpse
			GameObject corpse = m_corpseHandler.Spawn(null);
			if ( corpse != null )
			{
				Transform tr = m_dragonEquip.transform.Find("view");
				if ( tr == null )
					tr = transform;
				corpse.transform.CopyFrom(tr);
				corpse.transform.localScale = m_dragonEquip.transform.localScale;


				DragonCorpse equip = corpse.GetComponent<DragonCorpse>();
				if ( equip != null ){
					equip.EquipDisguise( m_dragonEquip.dragonSku,m_dragonEquip.dragonDisguiseSku );
					equip.Spawn(false);
				}

			}
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
				m_reviveInstance.gameObject.SetActive(true);
				m_reviveInstance.Play();
				m_toDeactivate.Add( m_reviveInstance );
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
		Transform t = instance.transform;
		t.parent = m_shieldAnchor;
		t.localScale = Vector3.one * InstanceManager.player.data.scale;
		t.localPosition = m_shieldParticle.offset;
		if ( _tr != null )
		{
			t.LookAt( _tr.position );
			t.Rotate( -Vector3.forward * 90, Space.Self);
			t.Rotate( -Vector3.up * 90, Space.Self);
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
			CreateSplash(_other, m_waterEnterSplashHandler);
			return true;
		}
		return false;
	}

	public bool OnExitWater( Collider _other )
	{
		m_insideWater = false;
		if ( m_bubblesInstance != null && m_bubblesInstance.isPlaying)
		{
			m_bubblesInstance.Stop();
			m_toDeactivate.Add( m_bubblesInstance );
		}

		CheckBodyParts();

		if ( m_dargonMotion != null && Mathf.Abs(m_dargonMotion.velocity.y) >= m_minSpeedExitSplash )
		{
			CreateSplash(_other, m_waterExitSplashHandler);
			return true;
		}
		return false;
	}

	private void CreateSplash( Collider _other, ParticleHandler _handler )
	{
		if (_handler != null) {
			Vector3 pos = _transform.position;
			float waterY = _other.bounds.center.y + _other.bounds.extents.y;
			pos.y = waterY;

			_handler.Spawn(null, pos);
		}
	}

	public void OnEnterOuterSpace( Collider _spaceCollider, bool fast ) {
		// Launch cloud trail, will stop automatically
		if(fast && m_cloudTrailInstance != null) 
		{
			m_cloudTrailInstance.gameObject.SetActive(true);
			m_cloudTrailInstance.Play();
			m_toDeactivate.Add( m_cloudTrailInstance );
		}
		if ( m_spaceEnterSplash.IsValid() )
		{
			Vector3 pos = _transform.position;
			float spaceY = _spaceCollider.bounds.center.y - _spaceCollider.bounds.extents.y;
			pos.y = spaceY;
			m_spaceEnterSplash.Spawn(pos);
		}
			
	}

	public void OnExitOuterSpace(Collider _spaceCollider, bool fast) {
		// Launch cloud trail again!
		if(fast && m_cloudTrailInstance != null) 
		{
			m_cloudTrailInstance.gameObject.SetActive(true);
			m_cloudTrailInstance.Play();
			m_toDeactivate.Add( m_cloudTrailInstance );
		}
		if ( m_spaceExitSplash.IsValid() )
		{
			Vector3 pos = _transform.position;
			float spaceY = _spaceCollider.bounds.center.y - _spaceCollider.bounds.extents.y;
			pos.y = spaceY;
			m_spaceExitSplash.Spawn( pos );
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
				m_waterAirLimitInstance.gameObject.SetActive(true);
				m_waterAirLimitInstance.transform.rotation = Quaternion.Euler(90,0,0);
				m_waterAirLimitInstance.Play();
				m_toDeactivate.Add(m_waterAirLimitInstance);
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
		{
			m_hiccupInstance.gameObject.SetActive(true);
			m_hiccupInstance.Play();
			m_toDeactivate.Add( m_hiccupInstance );
		}
	}

	#region boost_trails
	void PlayTrails()
	{
		if (m_trailsInstance)
		{
			m_trailsInstance.gameObject.SetActive(true);
			m_trailsInstance.Play();
		}
		m_playinTrails = true;
	}

	void StopTrails()
	{
		if (m_trailsInstance)
		{
			m_trailsInstance.Stop();
			m_toDeactivate.Add( m_trailsInstance );
		}
		m_playinTrails = false;
	}

	public void ActivateTrails()
	{
		m_trailsActive = true;
	}

	public void DeactivateTrails()
	{
		m_trailsActive = false;
	}

	#endregion

	public void WingsEvent()
	{
		if ( m_dargonMotion.height <= 2 && m_landingInstance)
		{
			Vector3 disp = transform.rotation * m_landingParticle.offset;
			m_landingInstance.transform.position = m_dargonMotion.lastGroundHit + disp;
			m_landingInstance.gameObject.SetActive(true);
			m_landingInstance.Play();
			m_toDeactivate.Add(m_landingInstance);
		}
	}

}
