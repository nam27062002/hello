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

	// Use this for initialization
	void Start () 
	{
		GameObject go;

		// Instantiate Particles (at start so we don't feel any framerate drop during gameplay)
		m_levelUpInstance = InitParticles(m_levelUp, m_levelUpAnchor);
		m_bubblesInstance = InitParticles(m_bubbles, m_bubblesAnchor);
		m_cloudTrailInstance = InitParticles(m_cloudTrail, m_cloudTrailAnchor);

		// Register events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
	}


	void OnDestroy()
	{
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
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
	}


	public void OnEnterWater()
	{
		if ( m_bubblesInstance != null )
			m_bubblesInstance.Play();
	}

	public void OnExitWater()
	{
		if ( m_bubblesInstance != null )
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
