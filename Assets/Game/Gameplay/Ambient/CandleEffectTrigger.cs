using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleEffectTrigger : MonoBehaviour {

	public enum TriggerEvent {
		Enter,
		Exit,
	};
	public TriggerEvent m_event = TriggerEvent.Enter;

	void OnTriggerEnter( Collider other)
	{
        Messenger.Broadcast<bool>(GameEvents.DARK_ZONE_TOGGLE, (m_event == TriggerEvent.Enter) ? true : false);
	}
}
