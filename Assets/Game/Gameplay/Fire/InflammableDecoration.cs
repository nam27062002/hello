using UnityEngine;
using System.Collections.Generic;

public class InflammableDecoration : Initializable {

	[CommentAttribute("Add an explosion effect when this object is burned out.")]
	[SerializeField] private string m_explosionParticle = "";

	private GameObject m_view;
	private GameObject m_viewBurned;

	private FireNode[] m_fireNodes;
	private bool m_burned;
	private DeltaTimer m_timer = new DeltaTimer();

	private AutoSpawnBehaviour m_autoSpawner;
	public bool m_crumble = false;
	private Vector3 m_startPosition;

	// Use this for initialization
	void Start () {
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_view = transform.FindChild("view").gameObject;
		m_viewBurned = transform.FindChild("view_burned").gameObject;
		m_fireNodes = transform.GetComponentsInChildren<FireNode>();
		m_burned = false;

		int coins = 0;

		if (GetComponent<Entity>() != null) {
			coins = GetComponent<Entity>().def.reward.coins;
		}

		int coinsPerNode = coins / m_fireNodes.Length;

		for (int i = 0; i < m_fireNodes.Length - 1; i++) {
			m_fireNodes[i].Init(coinsPerNode);
		}

		m_fireNodes[m_fireNodes.Length - 1].Init(coins - (coinsPerNode * (m_fireNodes.Length - 1)));

		m_startPosition = transform.position;
	}

	public override void Initialize() {
		m_view.SetActive(true);
		m_viewBurned.SetActive(false);
		transform.localScale = Vector3.one;
		m_burned = false;

		for (int i = 0; i < m_fireNodes.Length; i++) {
			m_fireNodes[i].Reset();
		}

		enabled = true;

		transform.position = m_startPosition;
	}

	// Update is called once per frame
	void Update() {	

		if ( m_autoSpawner.state == AutoSpawnBehaviour.State.Respawning )	// if respawning we wait
			return;

		if (m_burned) 
		{
			// Wait burn animation to end
			Vector3 scale = transform.localScale;
			scale.y = 0.5f + (0.5f - (m_timer.GetDelta() * 0.5f));
			transform.localScale = scale;

			Vector3 pos = m_startPosition;
			float size = 0.1f;
			pos.x += Random.Range( -size, size );
			pos.y += Random.Range( -size, size );
			pos.z += Random.Range( -size, size );
			transform.position = pos;

			if ( m_timer.Finished() )
			{
				m_autoSpawner.Respawn();
			}
		} 
		else 
		{
			m_burned = true;
			for (int i = 0; i < m_fireNodes.Length && m_burned; i++) {
				m_burned = m_burned && m_fireNodes[i].IsBurned();
			}

			if (m_burned && m_explosionParticle != "") {
				ParticleManager.Spawn(m_explosionParticle, transform.position + Vector3.back * 3f);
			}

			if ( m_burned )
			{
				for (int i = 0; i < m_fireNodes.Length && m_burned; i++) {
					m_fireNodes[i].StartSmoke();
				}
				m_view.SetActive(false);
				m_viewBurned.SetActive(true);
				if (m_crumble)
				{
					m_timer.Start( 2f );
				}
				else
				{
					m_autoSpawner.Respawn();
				}
			}
		}
	}
}
