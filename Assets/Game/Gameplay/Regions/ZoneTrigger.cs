using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
	
    public string m_zoneId;
    public string m_zoneTid;
    private bool m_inside = false;

    void OnTriggerEnter (Collider other)
    {
        if ( other.CompareTag("Player") && !m_inside)
        {
        	m_inside = true;
			Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, true, this);
        }
    }
	
	void OnTriggerExit (Collider other)
    {
		if (other.CompareTag("Player") && m_inside)
        {
        	m_inside = false;
			Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, false, this);
        }
    }
}
