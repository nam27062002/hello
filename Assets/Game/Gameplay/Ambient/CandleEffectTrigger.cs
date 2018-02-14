using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleEffectTrigger : MonoBehaviour {

//    public int m_id;
//    public float m_radius;
//    public float m_fallOff;

    [SerializeField] public HUDDarkZoneEffect.CandleData m_candleData;

    void OnTriggerEnter( Collider other)
	{
//        if (other.CompareTag("PlayerGroun"))
//        {
            Messenger.Broadcast<bool, HUDDarkZoneEffect.CandleData>(MessengerEvents.DARK_ZONE_TOGGLE, true, m_candleData);
//        }
	}
    void OnTriggerExit(Collider other)
    {
//        if (other.CompareTag("Player"))
//        {
            Messenger.Broadcast<bool, HUDDarkZoneEffect.CandleData>(MessengerEvents.DARK_ZONE_TOGGLE, false, m_candleData);
//        }
    }

}
