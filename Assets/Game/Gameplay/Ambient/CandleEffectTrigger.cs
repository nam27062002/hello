using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleEffectTrigger : MonoBehaviour {
    public enum EffectStatus {
        enter,
        exit
    };

    public EffectStatus m_status;

	void OnTriggerEnter( Collider other)
	{
        if (other.CompareTag("Player"))
        {
            Messenger.Broadcast<bool>(GameEvents.DARK_ZONE_TOGGLE, m_status == EffectStatus.enter ? true : false);
        }
	}
}
