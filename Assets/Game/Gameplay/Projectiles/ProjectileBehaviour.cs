using UnityEngine;
using System.Collections;

// [RequireComponent(typeof(PreyMotion))]
public class ProjectileBehaviour : MonoBehaviour {

	[SerializeField] private GameObject m_explosionPrefab = null;
	[SerializeField] private Range m_scaleRange = new Range(1f, 5f);
	[SerializeField] private Range m_rotationRange = new Range(0f, 360f);

	private float m_damage;

	private Vector2 m_targetCenter;
	private PreyMotion m_motion;
	private ProjectileMotion m_pMotion;
	private EdibleBehaviour m_edible;

	// Use this for initialization
	void Start () {		
		if (m_explosionPrefab != null)
			PoolManager.CreatePool(m_explosionPrefab, 5, false);
	}

	public void Shoot(Transform _from, float _damage) {	
		m_targetCenter = InstanceManager.player.transform.position;
		
		m_motion = GetComponent<PreyMotion>();
		m_pMotion = GetComponent<ProjectileMotion>();
		m_edible = GetComponent<EdibleBehaviour>();

		Vector3 p = _from.position;
		p.z = 0;
		transform.position = p;
		transform.rotation = _from.rotation;
				
		Initializable[] components = GetComponents<Initializable>();		
		foreach (Initializable component in components) {
			component.Initialize();
		}

		if ( m_pMotion != null )
		{
			Vector3 pos = InstanceManager.player.transform.position;
			float randomSize = 2.5f;
			pos.x += Random.Range( -randomSize, randomSize );
			pos.y += Random.Range( 0, randomSize );
			pos.z = 0;
			m_pMotion.Shoot( pos );
		}

		m_damage = _damage;
	}

	void Update() {
		// The dragon may eat this projectile, so we disable the explosion if that happens 
		if (!m_edible.isBeingEaten) {
			float distanceToTargetSqr = (m_targetCenter - (Vector2)transform.position).sqrMagnitude;

			if (distanceToTargetSqr <= 0.5f) {
				Explode(false);	
			}
		}
	}
		
	// Update is called once per frame
	void FixedUpdate () {
		if (!m_edible.isBeingEaten) 
		{
			if ( m_motion != null )
				m_motion.Seek(m_targetCenter);
		}
	}

	void OnTriggerEnter(Collider _other) {
		Debug.Log(_other.tag);
		if (!m_edible.isBeingEaten && _other.tag == "Player") 
		{
			Explode(true);
		}
		else if ( _other.gameObject.layer == LayerMask.NameToLayer( "Ground" ) )
		{
			Explode(false);
		}
	}

	private void Explode(bool _hitDragon) {

		if ( m_explosionPrefab != null )
		{
			GameObject explosion = PoolManager.GetInstance(m_explosionPrefab.name);			
			if (explosion) {
				// Random position within range
				explosion.transform.position = transform.position;			
				// Random scale within range
				explosion.transform.localScale = Vector3.one * m_scaleRange.GetRandom();			
				// Random rotation within range
				explosion.transform.Rotate(0, 0, m_rotationRange.GetRandom());
			}
		}

		if (_hitDragon) {
			InstanceManager.player.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage);
		}

		gameObject.SetActive(false);
		PoolManager.ReturnInstance(gameObject);
	}
}
