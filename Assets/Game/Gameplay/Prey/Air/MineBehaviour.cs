using UnityEngine;
using System.Collections;

public class MineBehaviour : Initializable {

	[SerializeField] private float m_damage;
	[SerializeField] private float m_forceStrength;
	[SerializeField] private float m_radius;

	[Header("Explosion")]
	[SerializeField] private GameObject m_explosionPrefab = null;
	[SerializeField] private Range m_delayRange = new Range(0f, 0.25f);
	[SerializeField] private Range m_scaleRange = new Range(1f, 5f);
	[SerializeField] private Range m_rotationRange = new Range(0f, 360f);


	private float m_timer;
	private DragonHealthBehaviour m_dragon;


	// Use this for initialization
	void Start() {
	
		PoolManager.CreatePool(m_explosionPrefab, 5, false);

		m_dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();

		m_timer = 0;

	}

	public override void Initialize() {
		
		EdibleBehaviour edible = GetComponent<EdibleBehaviour>();
		if (edible != null) {
			if (edible.edibleFromTier <= InstanceManager.player.data.def.tier) {
				enabled = false;
			}
		}
	}

	void OnEnable() {
		MeshRenderer renderer = transform.FindChild("view").GetComponent<MeshRenderer>();
		renderer.enabled = true;
		m_timer = 0;
	}

	void Update() {

		if (m_timer > 0) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {

				m_timer = 0;
				GameObject explosion = PoolManager.GetInstance(m_explosionPrefab.name);

				// Random position within range
				explosion.transform.position = transform.position;

				// Random scale within range
				explosion.transform.localScale = Vector3.one * m_scaleRange.GetRandom();
				
				// Random rotation within range
				explosion.transform.Rotate(0, 0, m_rotationRange.GetRandom());

				gameObject.SetActive(false);
			}
		} else if (m_dragon.enabled) {
			Vector2 v = (m_dragon.transform.position - transform.position);
			float distanceSqr = v.sqrMagnitude;
			if (distanceSqr <= m_radius * m_radius) {
				m_dragon.ReceiveDamage(m_damage, this.transform);
				DragonMotion motion = m_dragon.GetComponent<DragonMotion>();
				motion.AddForce(v.normalized * m_forceStrength);
				Explode();
			}
		}
	}

	private void Explode() {

		// Hide mesh and destroy object after all explosions have been triggered
		MeshRenderer renderer = transform.FindChild("view").GetComponent<MeshRenderer>();
		renderer.enabled = false;

		m_timer = m_delayRange.GetRandom();
	}

	void OnDrawGizmos() {
		Color color = Color.red;
		color.a = 0.25f;

		Gizmos.color = color;
		Gizmos.DrawWireSphere(transform.position, m_radius);
	}
}
