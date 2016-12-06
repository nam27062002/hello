using UnityEngine;
using System.Collections;

public class DragonParticleController : MonoBehaviour 
{

	public GameObject m_levelUp;
	public Transform m_levelUpAnchor;
	private ParticleSystem m_levelUpInstance;

	[Space]
	public GameObject m_revive;
	public Transform m_reviveAnchor;
	private ParticleSystem m_reviveInstance;

	[Space]
	public GameObject m_bubbles;
	public Transform m_bubblesAnchor;
	private ParticleSystem m_bubblesInstance;
	private ParticleSystem.MinMaxCurve m_defaultRate;
	private ParticleSystem.MinMaxCurve m_doubleRate;

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


	private Transform _transform;
	private bool m_insideWater = false;
	private float m_waterY = 0;
	private float m_waterDepth = 5;
	private const float m_waterDepthIncrease = 2;
	private DragonMotion m_dargonMotion;
	private DragonEatBehaviour m_dragonEat;

	void Start () 
	{
		// Instantiate Particles (at start so we don't feel any framerate drop during gameplay)
		m_levelUpInstance = InitParticles(m_levelUp, m_levelUpAnchor);
		m_reviveInstance = InitParticles(m_revive, m_reviveAnchor);
		m_bubblesInstance = InitParticles(m_bubbles, m_bubblesAnchor);
		if ( m_bubblesInstance != null )
		{
			m_defaultRate = m_bubblesInstance.emission.rate;
			m_doubleRate = m_defaultRate;
			m_doubleRate.constantMax *= 10;
			m_doubleRate.constantMin *= 10;
		}

		m_cloudTrailInstance = InitParticles(m_cloudTrail, m_cloudTrailAnchor);
		m_dargonMotion = transform.parent.GetComponent<DragonMotion>();
		m_dragonEat = transform.parent.GetComponent<DragonEatBehaviour>();
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
		

	}

	void OnEnable() {
		// Register events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Messenger.AddListener(GameEvents.PLAYER_REVIVE, OnRevive);
	}

	void OnDisable()
	{
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Messenger.RemoveListener(GameEvents.PLAYER_REVIVE, OnRevive);
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

	void OnRevive()
	{
		m_reviveInstance.Play();
	}


	public bool OnEnterWater( Collider _other )
	{
		m_waterY = _transform.position.y;
		m_insideWater = true;
		if ( m_bubblesInstance != null )
		{
			ParticleSystem.EmissionModule emission = m_bubblesInstance.emission;
			emission.rate = m_defaultRate;
		}

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
	public void DeepLimit()
	{
		if (m_waterAirLimitInstance != null && (m_waterY - m_dragonEat.mouth.position.y) > m_waterDepth)
		{
			m_waterAirLimitInstance.transform.rotation = Quaternion.Euler(90,0,0);
			m_waterAirLimitInstance.Play();
		}

		if ( m_bubblesInstance != null )
		{
			ParticleSystem.EmissionModule emission = m_bubblesInstance.emission;
			emission.rate = m_doubleRate;
		}
	}
}
