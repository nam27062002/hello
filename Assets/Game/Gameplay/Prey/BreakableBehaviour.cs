using UnityEngine;
using System.Collections;

public class BreakableBehaviour : MonoBehaviour 
{	
	[SerializeField] private DragonTier m_tierWithTurboBreak = 0;
	[SerializeField] private DragonTier m_tierNoTurboBreak = 0;

	[SerializeField] private int m_hitCount = 1;
	[SerializeField] private bool m_destroyOnBreak = true;

	[SerializeField] private string m_onBreakParticle;
	[SerializeField] private string m_onBreakAudio;

	[SerializeField] Transform m_view;

	//----------------------------------------------------------------------

	private int m_remainingHits;


	private Wobbler m_wobbler;
	private Vector3 m_initialViewPos;


	//----------------------------------------------------------------------

	void Start() {
		if (m_view == null)
			m_view = transform.FindChild("view");
		
		m_initialViewPos = m_view.localPosition;
	}

	void OnEnable() {
		m_remainingHits = m_hitCount;

		if (m_wobbler == null)
			m_wobbler = GetComponent<Wobbler>();

		m_wobbler.enabled = false;
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.transform.CompareTag("Player")) {
			DragonPlayer player = collision.transform.gameObject.GetComponent<DragonPlayer>();
			DragonTier tier = player.GetTierWhenBreaking();

			float value = Mathf.Max(0.1f, Vector3.Dot( collision.contacts[0].normal, player.dragonMotion.direction));

			Vector3 pushVector = Vector3.zero;

			if (tier >= m_tierNoTurboBreak) {
				m_remainingHits--;
			} else if (tier >= m_tierWithTurboBreak) {
				DragonBoostBehaviour boost = collision.transform.gameObject.GetComponent<DragonBoostBehaviour>();	

				if (boost.IsBoostActive())	{
					m_remainingHits--;
					pushVector = -collision.contacts[0].normal * value;
				} else {
					// Message : You need boost!
					Messenger.Broadcast(GameEvents.BREAK_OBJECT_NEED_TURBO);

				}
			} else {
				// Message: You need a bigger dragon
				Messenger.Broadcast(GameEvents.BREAK_OBJECT_BIGGER_DRAGON);
				value *= 0.5f;
			}

			if (m_remainingHits <= 0) 
			{
				Break(pushVector);
			}
			else
			{
				Shake();
			}
		}
	}

	void Break(Vector3 pushVector) {
		// Spawn particle
		GameObject prefab = Resources.Load("Particles/" + m_onBreakParticle) as GameObject;
		if (prefab != null) {
			GameObject go = Instantiate(prefab) as GameObject;
			if (go != null) {
				go.transform.position = transform.position;
				go.transform.rotation = transform.rotation;
			}
		}

		AudioController.Play(m_onBreakAudio);

		DragonMotion dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
		if (pushVector != Vector3.zero) {
			pushVector *= Mathf.Log(Mathf.Max(dragonMotion.velocity.magnitude, 2f));
			dragonMotion.AddForce( pushVector );
		}
		else {
			dragonMotion.NoDamageImpact();
		}

		// Destroy
		gameObject.SetActive(false);
		if (m_destroyOnBreak) {			
			Destroy(gameObject);
		}
	}

	public void Shake() {
		if (m_wobbler != null) {
			m_wobbler.enabled = true;
			m_wobbler.StartWobbling(m_view, m_initialViewPos);
		}
	}
}
