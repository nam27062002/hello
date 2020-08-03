using UnityEngine;
using System.Collections;

public class EmberController : MonoBehaviour, IBroadcastListener
{

    ParticleSystem m_emberParticle;

    // Use this for initialization
    void Start()
    {
        m_emberParticle = GetComponent<ParticleSystem>();
        m_emberParticle.Stop();

        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
    }

    void OnDisable()
    {
        Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.FURY_RUSH_TOGGLED:
            {
                FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                OnFuryToggled(furyRushToggled.activated, furyRushToggled.type); 
            }break;
        }
    }

	private void OnFuryToggled(bool _value, DragonBreathBehaviour.Type _type) {
		if (_value) {
			m_emberParticle.Clear();
			m_emberParticle.Play();
		} else {
			m_emberParticle.Stop();
		}
	}
}
