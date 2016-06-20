using UnityEngine;
using System.Collections;

public class WaterController : MonoBehaviour {
		
	private DragonMotion m_player;
	private BoxCollider m_bounds;
	private GameObject m_waterTrail;


	// Use this for initialization
	void Start () {
		m_player = InstanceManager.player.GetComponent<DragonMotion>();
		m_bounds = GetComponent<BoxCollider>();
	}
	
	// Update is called once per frame
	void Update () {
		if(m_player == null) return;

		bool activeTrail = false;
		Vector3 pos = m_player.transform.position;
		Vector3 waterPos = transform.position;

		if (pos.x > waterPos.x - (m_bounds.size.x * 0.5f)
		&&  pos.x < waterPos.x + (m_bounds.size.x * 0.5f)) {		
			if (pos.y > waterPos.y - 1 && pos.y < waterPos.y + 1) {
				pos.y = waterPos.y;
				activeTrail = Mathf.Abs(m_player.rbody.velocity.x) > 1.5f;
				if (m_waterTrail != null) {
					m_waterTrail.transform.position = pos;
				}
			}
		}

		if (activeTrail && m_waterTrail == null) {
			m_waterTrail = ParticleManager.Spawn("PF_WaterTrail", pos);
			if ( m_waterTrail != null )
			{
				m_waterTrail.GetComponent<ParticleSystem>().loop = true;
				m_waterTrail.GetComponent<DisableInSeconds>().enabled = false;
			}
		} else if (!activeTrail && m_waterTrail != null) {
			m_waterTrail.GetComponent<ParticleSystem>().Stop();
			m_waterTrail.GetComponent<DisableInSeconds>().enabled = true;
			m_waterTrail = null;
		}
	}

	void OnTriggerEnter (Collider _other) {
		CreateSplash();
	}

	void OnTriggerExit (Collider _other) {
		CreateSplash();
	}

	void CreateSplash () {
		float yVelocity = Mathf.Abs(m_player.velocity.y);
		Debug.Log(yVelocity);
		if (yVelocity > 1f) {
			Vector3 pos = m_player.transform.position;
			float waterY = transform.position.y;
			pos.y = waterY;

			if (yVelocity > 10f) 	 ParticleManager.Spawn("PF_WaterSplash_L", pos);
			else if (yVelocity > 5f) ParticleManager.Spawn("PF_WaterSplash_M", pos);
			else  					 ParticleManager.Spawn("PF_WaterSplash_S", pos);
		}
	}
}
