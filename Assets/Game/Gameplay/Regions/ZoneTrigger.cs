using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
	
    public string m_zoneId;
    public string m_zoneTid;
    private int m_inside = 0;

    void OnTriggerEnter (Collider other)
    {
        if ( other.CompareTag("Player"))
        {
        	if ( m_inside == 0 )
				Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, true, this);
			m_inside++;
        }
    }
	
	void OnTriggerExit (Collider other)
    {
		if (other.CompareTag("Player"))
        {
        	m_inside--;
        	if ( m_inside == 0 )
				Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, false, this);
        }
    }
}
