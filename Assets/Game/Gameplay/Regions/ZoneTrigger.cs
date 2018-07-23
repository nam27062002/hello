using UnityEngine;
using System.Collections.Generic;

public class ZoneTrigger : MonoBehaviour
{
	
    public string m_zoneId;
    public string m_zoneTid;
    private int m_inside = 0;

    private static Dictionary<string, int> s_enters = new Dictionary<string,int>();
    private static int s_zonetriggers = 0;

    public void Awake()
    {
		s_zonetriggers++;
    }

    public void OnDestroy()
    {
		s_zonetriggers--;
		if ( s_zonetriggers == 0 )
			s_enters.Clear();
    }

    void OnTriggerEnter (Collider other)
    {
        if ( other.CompareTag("Player"))
        {
        	if ( m_inside == 0 )
        	{
        		bool firstTime = false;
        		if ( s_enters.ContainsKey(m_zoneId) ){
					s_enters[ m_zoneId ] = s_enters[ m_zoneId ] + 1;
        		}else{
					s_enters.Add( m_zoneId, 1 );
					firstTime = true;
        		}
				// if first then trigger zone
        		if ( s_enters[ m_zoneId ] == 1 )
					Messenger.Broadcast<bool, ZoneTrigger, bool>(MessengerEvents.MISSION_ZONE, true, this, firstTime);
			}
			m_inside++;
        }
    }
	
	void OnTriggerExit (Collider other)
    {
		if (other.CompareTag("Player"))
        {
        	m_inside--;
        	if ( m_inside == 0 )
        	{
				s_enters[m_zoneId] = s_enters[m_zoneId] - 1;
				// If no one left we are leaving a zone
				if ( s_enters[ m_zoneId ] == 0 )
					Messenger.Broadcast<bool, ZoneTrigger, bool>(MessengerEvents.MISSION_ZONE, false, this, false);
			}
        }
    }
}
