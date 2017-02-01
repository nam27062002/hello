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

	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private Collider m_collider;
	private HungryLettersManager m_letterManager;
	private ParticleSystem m_particle;
	private MapMarker m_mapMarker;

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
		m_mapMarker = GetComponent<MapMarker>();
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
		// notify that this letter has been collected.
		m_letterManager.LetterCollected(this);
		// disable the collider.
		m_collider.enabled = false;
		// disable this component.
		enabled = false;
		// disable particle and map marker.
		// TODO: Recover
		// m_mapMarker.HideOnMap = true;
		m_particle.Stop();

#if UNITY_5_3_OR_NEWER
        ParticleSystem.EmissionModule em = m_particle.emission;
        em.enabled = false;
#else
        m_particle.enableEmission = false;
#endif
    }

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	public void Init(HungryLettersManager manager, Transform container)
	{
		m_letterManager = manager;
		cachedTransform.parent = container;
		cachedTransform.localPosition = Vector3.zero;
	}

	public void ChangeLayers()
	{
		// change the layers.
		gameObject.layer = LayerMask.NameToLayer("Default");
		m_mesh.layer = LayerMask.NameToLayer("UI");
	}

	public HungryLettersManager GetHungryLettersManager( ) {
		return m_letterManager;
	}
}