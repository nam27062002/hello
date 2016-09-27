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
			m_waterTrail = ParticleManager.Spawn("PS_Skimming", pos, "Water");
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
		if ( _other.tag == "Player" )
			CreateSplash(m_player.velocity.y, _other.transform);
		else if ( _other.tag == "Pet" ){
			AI.Machine m = _other.GetComponent<AI.Machine>();
			CreateSplash(m.velocity.y, _other.transform);
		}
	}

	void OnTriggerExit (Collider _other) {
		if ( _other.tag == "Player" )
			CreateSplash( m_player.velocity.y, _other.transform );
		else if ( _other.tag == "Pet" ){
			AI.Machine m = _other.GetComponent<AI.Machine>();
			CreateSplash(m.velocity.y, _other.transform);
		}
	}

	void CreateSplash (float yVelocity, Transform _transform) {
		yVelocity = Mathf.Abs(yVelocity);
		if (yVelocity > 1f) {
			Vector3 pos = _transform.position;
			float waterY = transform.position.y;
			pos.y = waterY;

			if (yVelocity > 10f) 	 ParticleManager.Spawn("PS_Dive", pos, "Water");
			else if (yVelocity > 5f) ParticleManager.Spawn("PS_Dive", pos, "Water");
			else  					 ParticleManager.Spawn("PS_Dive", pos, "Water");
		}
	}
}
