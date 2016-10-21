using UnityEngine;
using System.Collections;

public class DragonParticleController : MonoBehaviour 
{

	public GameObject m_levelUp;
	public Transform m_levelUpAnchor;
	private ParticleSystem m_levelUpInstance;

	[Space]
	public GameObject m_bubbles;
	public Transform m_bubblesAnchor;
	private ParticleSystem m_bubblesInstance;

	[Space]
	public GameObject m_cloudTrail;
	public Transform m_cloudTrailAnchor;
	private ParticleSystem m_cloudTrailInstance;

	private Transform _transform;
	private bool m_insideWater = false;
	private float m_waterY = 0;
	private float m_waterDepth = 5;
	private const float m_waterDepthIncrease = 2;

	void Start () 
	{
		// Instantiate Particles (at start so we don't feel any framerate drop during gameplay)
		m_levelUpInstance = InitParticles(m_levelUp, m_levelUpAnchor);
		m_bubblesInstance = InitParticles(m_bubbles, m_bubblesAnchor);
		m_cloudTrailInstance = InitParticles(m_cloudTrail, m_cloudTrailAnchor);

		m_waterDepth = InstanceManager.player.data.scale + m_waterDepthIncrease;
		_transform = transform;
	}

	void OnEnable() {
		// Register events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
	}

	void OnDisable()
	{
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
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


	public void OnEnterWater()
	{
		m_waterY = _transform.position.y;
		m_insideWater = true;
	}

	public void OnExitWater()
	{
		m_insideWater = false;
		if ( m_bubblesInstance != null && m_bubblesInstance.isPlaying)
			m_bubblesInstance.Stop();
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
}
