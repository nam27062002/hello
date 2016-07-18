using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineBehaviour : Initializable {

	[SerializeField] private float m_damage;
	[SerializeField] private float m_forceStrength;

	[Header("Explosion")]
	[SerializeField] private GameObject m_explosionPrefab = null;
	[SerializeField] private List<string> m_explosionSounds = new List<string>();
	[SerializeField] private Range m_delayRange = new Range(0f, 0.25f);
	[SerializeField] private Range m_scaleRange = new Range(1f, 5f);
	[SerializeField] private Range m_rotationRange = new Range(0f, 360f);


	private float m_timer;
	private DragonHealthBehaviour m_dragon;

	private GameCameraController m_camera;

	// Use this for initialization
	void Start() {
		m_camera = GameObject.Find("PF_GameCamera").GetComponent<GameCameraController>();
	
		PoolManager.CreatePool(m_explosionPrefab, 5, false);

		m_dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();

		m_timer = 0;

	}

	public override void Initialize() {		
		Entity_Old entity = GetComponent<Entity_Old>();
		if(entity != null && entity.isEdible && entity.edibleFromTier <= InstanceManager.player.data.tier) {
			enabled = false;
		}

		GetComponent<Collider>().enabled = true;
	}

	void OnEnable() {
		Renderer renderer = transform.FindChild("view").GetComponentInChildren<Renderer>();
		renderer.enabled = true;
		m_timer = 0;
	}

	void Update() {
		if (m_timer > 0) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {

				m_timer = 0;
				GameObject explosion = PoolManager.GetInstance(m_explosionPrefab.name);
				if(explosion != null) {
					// Random position within range
					explosion.transform.position = transform.position;

					// Random scale within range
					explosion.transform.localScale = Vector3.one * m_scaleRange.GetRandom();
					
					// Random rotation within range
					explosion.transform.Rotate(0, 0, m_rotationRange.GetRandom());
				}
				SpawnBehaviour sp = GetComponent<SpawnBehaviour>();
				sp.EatOrBurn();
				gameObject.SetActive(false);
			}
		}
	}

	void OnCollisionEnter(Collision _collision) {
		if (m_dragon != null && m_dragon.enabled) {
			DragonMotion motion = _collision.gameObject.GetComponent<DragonMotion>();
			if (motion != null) { // the dragon Collided with the mine
				// Check if dragon has shield!
				DragonPlayer dp = _collision.gameObject.GetComponent<DragonPlayer>();
				if ( dp.HasMineShield() )
				{
					dp.LoseMineShield();
				}
				else
				{
					m_dragon.ReceiveDamage(m_damage, this.transform);
				}
				motion.AddForce(_collision.impulse.normalized * m_forceStrength);
				Explode();
			}
		}
	}

	private void Explode() {

		// Hide mesh and destroy object after all explosions have been triggered
		Renderer renderer = transform.FindChild("view").GetComponentInChildren<Renderer>();
		renderer.enabled = false;

		m_camera.Shake(0.75f, new Vector3(0.75f, 0.75f, 0));

		m_timer = m_delayRange.GetRandom();

		if ( m_explosionSounds.Count > 0 )
		{
			string soundName = m_explosionSounds[ Random.Range(0, m_explosionSounds.Count) ];
			if (!string.IsNullOrEmpty( soundName ))
			{
				AudioManager.instance.PlayClip( soundName );
			}
		}


		GetComponent<Collider>().enabled = false;
	}
}
