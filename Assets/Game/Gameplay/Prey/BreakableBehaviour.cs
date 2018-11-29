using UnityEngine;
using System.Collections;

public class BreakableBehaviour : MonoBehaviour, IBroadcastListener 
{	
	[SerializeField] private bool m_isBlocker = false;
	[SerializeField] private bool m_unbreakableBlocker = false;

	[SerializeField] private DragonTier m_tierWithTurboBreak = 0;
	[SerializeField] private DragonTier m_tierNoTurboBreak = 0;

	[SerializeField] private int m_hitCount = 1;
	[SerializeField] private bool m_disableOnBreak = true;
	[SerializeField] private bool m_destroyOnBreak = true;

	[SerializeField] private ParticleData m_onBreakParticle;
	[SerializeField] private string m_corpseAsset;
	[SerializeField] private string m_onBreakAudio;

	[SerializeField] private Transform m_view;
	[SerializeField] private GameObject m_activateOnDestroy;

	//----------------------------------------------------------------------

	private int m_remainingHits;

	private ParticleHandler m_corpseHandler;

	private Wobbler m_wobbler;
	private Collider m_collider;
	private Vector3 m_initialViewPos;



	//----------------------------------------------------------------------
	void Awake()
	{
		if (m_view == null)
			m_view = transform.Find("view");

		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}

	void OnDestroy() {
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                CreatePool();
            }break;
        }
    }
    

	void CreatePool() {
		m_onBreakParticle.CreatePool();
		if (!string.IsNullOrEmpty(m_corpseAsset)) {
			m_corpseHandler = ParticleManager.CreatePool(m_corpseAsset, "Corpses/");
		}
	}

	void Start() {		
		CreatePool();			
		m_initialViewPos = m_view.localPosition;
	}

	void OnEnable() {
		m_remainingHits = m_hitCount;

		if (m_wobbler == null)
			m_wobbler = GetComponent<Wobbler>();		
		m_wobbler.enabled = false;

		if (m_collider == null)
			m_collider = GetComponent<Collider>();
		m_collider.isTrigger = false;

		m_view.gameObject.SetActive(true);

		if (m_activateOnDestroy != null)
			m_activateOnDestroy.SetActive(false);
	}


	void OnCollisionEnter(Collision collision) {
		if (collision.transform.CompareTag("Player")) {
			if (m_unbreakableBlocker) {
				Messenger.Broadcast(MessengerEvents.BREAK_OBJECT_SHALL_NOT_PASS);
			} else {
				DragonPlayer player = collision.transform.gameObject.GetComponent<DragonPlayer>();
				if ( player.changingArea )
				{
					float value = Mathf.Max(0.1f, Vector3.Dot( collision.contacts[0].normal, player.dragonMotion.direction));
					Vector3 pushVector = -collision.contacts[0].normal * value;
					Break( pushVector );
				}
				else
				{
					DragonTier tier = player.GetTierWhenBreaking();

					float value = Mathf.Max(0.1f, Vector3.Dot( collision.contacts[0].normal, player.dragonMotion.direction));

					Vector3 pushVector = Vector3.zero;

					if (tier >= m_tierNoTurboBreak) {
						m_remainingHits--;
					} else if (tier >= m_tierWithTurboBreak) {
						if (player.IsBreakingMovement())	{
							m_remainingHits--;
							pushVector = -collision.contacts[0].normal * value;
						} else {
							// Message : You need boost!
							Messenger.Broadcast(MessengerEvents.BREAK_OBJECT_NEED_TURBO);

						}
					} else {
						// Message: You need a bigger dragon
						Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, m_tierWithTurboBreak, "");
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
		}
	}

	void Break(Vector3 pushVector) {

		// Spawn particle
		GameObject go = m_onBreakParticle.Spawn(transform.position);
		if (go != null)
		{
			go.transform.rotation = transform.rotation;	
			ParticleScaler scaler = go.GetComponentInChildren<ParticleScaler>();
			if ( scaler != null )
			{
				scaler.m_scale = transform.lossyScale.x;
				scaler.DoScale();
			}
		}

		// spawn corpse
		if (m_corpseHandler != null) {
			// spawn corpse
			GameObject corpse = m_corpseHandler.Spawn(null);
			if (corpse != null) {
				corpse.transform.CopyFrom(transform);
				corpse.GetComponent<Corpse>().Spawn(false, true);
			}
		}
	
		AudioController.Play(m_onBreakAudio);

		DragonMotion dragonMotion = InstanceManager.player.GetComponent<DragonMotion>();
		if (pushVector != Vector3.zero) {
			pushVector *= Mathf.Log(Mathf.Max(dragonMotion.velocity.magnitude, 2f));
			dragonMotion.AddForce( pushVector, false );
		}
		dragonMotion.NoDamageImpact();

		// don't destroy them yet, first change the collider to trigger to throw the "on collision exit message"
		m_collider.isTrigger = true;
		m_view.gameObject.SetActive(false);

		if (m_activateOnDestroy != null)
			m_activateOnDestroy.SetActive(true);

		if (m_isBlocker) {			
			Messenger.Broadcast(MessengerEvents.BLOCKER_DESTROYED);

			Messenger.Broadcast<float, float>(MessengerEvents.CAMERA_SHAKE, 1f, 1f);
		}
        InstanceManager.timeScaleController.HitStop();

		// Destroy
		StartCoroutine(DestroyCountdown(0.15f));
	}

	public void Shake() {
		if (m_wobbler != null) {
			m_wobbler.enabled = true;
			m_wobbler.StartWobbling(m_view, m_initialViewPos);
		}
	}

	private IEnumerator DestroyCountdown(float _waitTime) {
		yield return new WaitForSeconds(_waitTime);
		if (m_disableOnBreak) {
			gameObject.SetActive(false);
		}
		if (m_destroyOnBreak) {			
			Destroy(gameObject);
		}
	}
}
