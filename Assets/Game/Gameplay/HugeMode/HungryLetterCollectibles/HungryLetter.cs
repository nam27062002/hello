using FGOL;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class HungryLetter : MonoBehaviour
{
	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	private HungryLettersManager.CollectibleLetters m_letter = HungryLettersManager.CollectibleLetters.H;
	[SerializeField]
	private GameObject m_mesh;
	public GameObject mesh {
		get { return m_mesh; }
	}

	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private Collider m_collider;
	public Collider collider {
		get { return m_collider; }
	}

	private HungryLettersManager m_letterManager;
	private ParticleSystem m_particle;

	private HungryLetterMapMarker m_mapMarker;
	public HungryLetterMapMarker mapMarker {
		get { return m_mapMarker; }
	}

	//------------------------------------------------------------
	// Public Properties:
	//------------------------------------------------------------

	public HungryLettersManager.CollectibleLetters letter { get { return m_letter; } }
	public Transform cachedTransform { get; private set; }

	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	protected void Awake()
	{
		cachedTransform = transform;
		m_collider = GetComponent<Collider>();
		Assert.Fatal(m_collider != null);
		m_particle = GetComponent<ParticleSystem>();
		m_mapMarker = GetComponentInChildren<HungryLetterMapMarker>();
	}

	protected void OnCollisionEnter(Collision coll)
	{
		// check if the collision happened with the player.
		DragonPlayer player = InstanceManager.player;
		// use rigidbody as a shortcut to the player's root, in case player obj has colliders on sub objects.
		if((player == null) || coll.rigidbody == null || coll.rigidbody.gameObject != player.gameObject)
		{
			return;
		}
		if (m_letterManager.IsLetterCollected(m_letter))
		{
			return;
		}
		OnLetterCollected();
    }

	protected void OnTriggerEnter(Collider coll)
	{
		DragonPlayer player = InstanceManager.player;
		// use rigidbody as a shortcut to the player's root, in case player obj has colliders on sub objects.
		if((player == null) || coll.attachedRigidbody == null || coll.attachedRigidbody.gameObject != player.gameObject)
		{
			return;
		}
		if (m_letterManager.IsLetterCollected(m_letter))
		{
			return;
		}
		OnLetterCollected();
	}

	public void OnLetterCollected()
	{
		// notify that this letter has been collected.
		m_letterManager.LetterCollected(this);
		// disable the collider.
		m_collider.enabled = false;
		// disable this component.
		enabled = false;
		// disable particle and map marker.
		// TODO: Recover
		// m_mapMarker.HideOnMap = true;
		m_particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // ParticleSystem.EmissionModule em = m_particle.emission;
        // em.enabled = false;
	}

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	public void Init(HungryLettersManager manager, Transform container)
	{
		m_letterManager = manager;
		cachedTransform.parent = container;
		cachedTransform.localPosition = Vector3.zero;
		if ( m_mapMarker != null)
		{
			m_mapMarker.OnUpdateMarkerStatus();
		}

		m_particle.Play();
		// ParticleSystem.EmissionModule em = m_particle.emission;
        // em.enabled = true;

		enabled = true;
		m_collider.enabled = true;
		ChangeLayersBack();
		gameObject.transform.localScale = Vector3.one;
	}

	public void ChangeLayers()
	{
		// change the layers.
		// We want to keep particles and everyhing in the default layer, move only the actual letter!
		gameObject.layer = LayerMask.NameToLayer("Default");
		m_mesh.layer = LayerMask.NameToLayer("UI");
	}

	public void ChangeLayersBack()
	{
		gameObject.layer = LayerMask.NameToLayer("Triggers");
		m_mesh.layer = LayerMask.NameToLayer("Default");
	}


	public HungryLettersManager GetHungryLettersManager( ) {
		return m_letterManager;
	}
}