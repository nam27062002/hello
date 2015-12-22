using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PreyMotion))]
public class ProjectileBehaviour : MonoBehaviour {

	[SerializeField] private GameObject m_explosionPrefab = null;
	[SerializeField] private Range m_delayRange = new Range(0f, 0.25f);
	[SerializeField] private Range m_scaleRange = new Range(1f, 5f);
	[SerializeField] private Range m_rotationRange = new Range(0f, 360f);

	private float m_damage;
	private Transform m_from;
	private Collider m_target;
	private PreyMotion m_motion;
	private EdibleBehaviour m_edible;

	// Use this for initialization
	void Start () {		
		PoolManager.CreatePool(m_explosionPrefab, 5, false);
	}

	public void Shoot(Transform _from, float _damage) {
		m_target = InstanceManager.player.GetComponent<SphereCollider>();
		
		m_motion = GetComponent<PreyMotion>();
		m_edible = GetComponent<EdibleBehaviour>();

		m_from = _from;
		transform.position = _from.position;
				
		Initializable[] components = GetComponents<Initializable>();		
		foreach (Initializable component in components) {
			component.Initialize();
		}

		m_damage = _damage;
	}

	void Update() {
		// The dragon may eat this projectile, so we disable the explosion if that happens 
		if (!m_edible.isBeingEaten) {
			float distanceSqr = ((Vector2)m_target.bounds.center - m_motion.position).sqrMagnitude;
			if (distanceSqr <= m_target.bounds.extents.x * m_target.bounds.extents.x) {
				GameObject explosion = PoolManager.GetInstance(m_explosionPrefab.name);			

				if (explosion) {
					// Random position within range
					explosion.transform.position = transform.position;			
					// Random scale within range
					explosion.transform.localScale = Vector3.one * m_scaleRange.GetRandom();			
					// Random rotation within range
					explosion.transform.Rotate(0, 0, m_rotationRange.GetRandom());
				}

				m_target.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage);
				gameObject.SetActive(false);
			}
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!m_edible.isBeingEaten) {
			m_motion.Seek(m_target.bounds.center);
		//	m_motion.ApplySteering();

			// force direction
			//m_motion.direction = m_target.bounds.center - m_from.position;
		}
	}
}
