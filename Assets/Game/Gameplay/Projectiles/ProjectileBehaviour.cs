using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PreyMotion))]
public class ProjectileBehaviour : MonoBehaviour {

	[SerializeField] private float m_damageAreaRadius = 1f;
	[SerializeField] private GameObject m_explosionPrefab = null;
	[SerializeField] private Range m_scaleRange = new Range(1f, 5f);
	[SerializeField] private Range m_rotationRange = new Range(0f, 360f);

	private float m_damage;
	private float m_damageAreaRadiusSqr;

	private DragonHealthBehaviour m_dragon;
	private Vector2 m_targetCenter;
	private float m_targetRadiusSqr;
	private PreyMotion m_motion;
	private EdibleBehaviour m_edible;

	// Use this for initialization
	void Start () {		
		PoolManager.CreatePool(m_explosionPrefab, 5, false);
	}

	public void Shoot(Transform _from, float _damage) {
		
		SphereCollider collider = InstanceManager.player.GetComponent<SphereCollider>();
		m_dragon = collider.GetComponent<DragonHealthBehaviour>();
		m_targetCenter = collider.transform.position;
		m_targetRadiusSqr = collider.radius * collider.radius;
		
		m_motion = GetComponent<PreyMotion>();
		m_edible = GetComponent<EdibleBehaviour>();

		transform.position = _from.position;
				
		Initializable[] components = GetComponents<Initializable>();		
		foreach (Initializable component in components) {
			component.Initialize();
		}

		m_damage = _damage;
		m_damageAreaRadiusSqr = m_damageAreaRadius * m_damageAreaRadius;
	}

	void Update() {
		// The dragon may eat this projectile, so we disable the explosion if that happens 
		if (!m_edible.isBeingEaten) {
			float distanceToTargetSqr = (m_targetCenter - m_motion.position).sqrMagnitude;
			float distanceToDragonSqr = ((Vector2)m_dragon.transform.position - m_motion.position).sqrMagnitude;

			bool hitDragon = (distanceToDragonSqr <= (m_targetRadiusSqr + m_damageAreaRadiusSqr));
			bool explode = hitDragon || (distanceToTargetSqr <= m_targetRadiusSqr);

			if (explode) {
				GameObject explosion = PoolManager.GetInstance(m_explosionPrefab.name);			

				if (explosion) {
					// Random position within range
					explosion.transform.position = transform.position;			
					// Random scale within range
					explosion.transform.localScale = Vector3.one * m_scaleRange.GetRandom();			
					// Random rotation within range
					explosion.transform.Rotate(0, 0, m_rotationRange.GetRandom());
				}

				if (hitDragon) {
					m_dragon.ReceiveDamage(m_damage);
				}

				gameObject.SetActive(false);
			}
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!m_edible.isBeingEaten) {
			m_motion.Seek(m_targetCenter);
		}
	}
}
