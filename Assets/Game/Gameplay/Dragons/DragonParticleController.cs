using UnityEngine;
using System.Collections;

public class DragonParticleController : MonoBehaviour 
{

	public GameObject m_levelUp;
	public Transform m_levelUpAnchor;
	public GameObject m_bubbles;
	public Transform m_bubblesAnchor;

	private ParticleSystem m_levelUpInstance;
	private ParticleSystem m_bubblesInstance;

	// Use this for initialization
	void Start () 
	{
		GameObject go;
		// Instantiate Particles
		go = Instantiate( m_levelUp );
		m_levelUpInstance = go.GetComponent<ParticleSystem>();
		SetupParticlePosition( go, m_levelUpAnchor );
		m_levelUpInstance.Stop();

		go = Instantiate( m_bubbles );
		m_bubblesInstance = go.GetComponent<ParticleSystem>();
		SetupParticlePosition( go, m_bubblesAnchor );
		m_bubblesInstance.Stop();

		// Register events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
	}


	void OnDestroy()
	{
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
	}

	private void SetupParticlePosition( GameObject _instance, Transform _transform )
	{
		_instance.transform.parent = _transform;
		_instance.transform.localPosition = Vector3.zero;
		_instance.transform.localRotation = Quaternion.identity;
		_instance.transform.localScale = Vector3.one;
	}

	void OnLevelUp( DragonData data )
	{
		m_levelUpInstance.Play();
	}


	public void OnInsideWater()
	{
		if ( m_bubblesInstance != null )
			m_bubblesInstance.Play();
	}

	public void OnExitWater()
	{
		if ( m_bubblesInstance != null )
			m_bubblesInstance.Stop();
	}

}
