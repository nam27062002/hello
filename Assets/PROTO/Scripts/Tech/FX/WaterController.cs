using UnityEngine;
using System.Collections;

public class WaterController : MonoBehaviour {

	
	public GameObject m_waterTrailPrefab;

	public GameObject m_waterSplashSmallPrefab;
	public GameObject m_waterSplashMediumPrefab;
	public GameObject m_waterSplashLargePrefab;

	private DragonMotion m_player;
	private BoxCollider m_bounds;

	private GameObject m_waterTrail;


	// Use this for initialization
	void Start () {
	
		m_player = GameObject.Find ("Player").GetComponent<DragonMotion>();
		m_bounds = GetComponent<BoxCollider>();

		m_waterTrail = (GameObject)Object.Instantiate(m_waterTrailPrefab, Vector3.zero, Quaternion.identity);
		m_waterTrail.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
		bool activeTrail = false;

		Vector3 pos = m_player.transform.position;
		Vector3 waterPos = transform.position;

		if (pos.x > waterPos.x - (m_bounds.size.x * 0.5f)
		&&  pos.x < waterPos.x + (m_bounds.size.x * 0.5f)) {
		
			if (pos.y > waterPos.y - 200 && pos.y < waterPos.y + 50) {

				pos.y = waterPos.y;
				if (Mathf.Abs(m_player.m_rbody.velocity.x) > 150) {
					//CreateParticles(pos);
					activeTrail = true;
					m_waterTrail.transform.position = pos;
				}
			}
		}

		if (activeTrail && !m_waterTrail.activeInHierarchy) {

			m_waterTrail.SetActive(true);
			m_waterTrail.GetComponent<ParticleSystem>().Play();
		} else if (!activeTrail && m_waterTrail.activeInHierarchy) {

			m_waterTrail.SetActive(false);
			m_waterTrail.GetComponent<ParticleSystem>().Stop();
		}
	}

	void OnTriggerEnter (Collider _other) {
		CreateSplash();
	}

	void OnTriggerExit (Collider _other) {
		CreateSplash();
	}

	void CreateSplash () {
		float yVelocity = Mathf.Abs(m_player.m_rbody.velocity.y);

		if (yVelocity > 100) {
			Vector3 pos = m_player.transform.position;
			float waterY = transform.position.y;
			
			if (Mathf.Abs(pos.y - waterY) < 50) {
				pos.y = waterY;

				if (yVelocity > 1000) 		Object.Instantiate(m_waterSplashLargePrefab,  pos, Quaternion.identity);
				else if (yVelocity > 500) 	Object.Instantiate(m_waterSplashMediumPrefab, pos, Quaternion.identity);
				else 						Object.Instantiate(m_waterSplashSmallPrefab,  pos, Quaternion.identity);
			}
		}
	}
}
