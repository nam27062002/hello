using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZoneTrigger : MonoBehaviour
{
	
    public string m_zoneId;
    public string m_zoneTid;
    public bool m_isStartingArea = false;
    private int m_inside = 0;
    private Coroutine m_checkIntroMovement = null;
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
            // if starting area and we havn't discovered the area we ignore this one
            if ( !m_isStartingArea || UsersManager.currentUser.m_visitedZones.Contains( m_zoneId ) )
            {
                if ( !InstanceManager.player.IsIntroMovement() )
                {
                    if ( m_inside == 0 )
                    {
                        if ( s_enters.ContainsKey(m_zoneId) ){
                            s_enters[ m_zoneId ] = s_enters[ m_zoneId ] + 1;
                        }else{
                            s_enters.Add( m_zoneId, 1 );
                        }
                        // if first then trigger zone
                        if ( s_enters[ m_zoneId ] == 1 )
                            Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, true, this);
                    }
                }
                else
                {
                    // Wait to finish intro movement and if m_inside is bigger than one then we send the event
                    if (m_checkIntroMovement == null)
                        m_checkIntroMovement = StartCoroutine(WaitIntroMovement());
                }
                m_inside++;
            }
            
        }
    }
    
    IEnumerator WaitIntroMovement()
    {
        while( InstanceManager.player.IsIntroMovement() )
        {
            yield return null;
        }
        if ( m_inside > 0 && (!m_isStartingArea || UsersManager.currentUser.m_visitedZones.Contains( m_zoneId )))
        {
            if ( s_enters.ContainsKey(m_zoneId) ){
                s_enters[ m_zoneId ] = s_enters[ m_zoneId ] + 1;
            }else{
                s_enters.Add( m_zoneId, 1 );
            }
            // if first then trigger zone
            if ( s_enters[ m_zoneId ] == 1 )
                Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, true, this);
        }
        m_checkIntroMovement = null;
        yield return null;
    }
	
	void OnTriggerExit (Collider other)
    {
		if (other.CompareTag("Player"))
        {
            if ( m_inside > 0 )
            {
            	m_inside--;
                if (!InstanceManager.player.IsIntroMovement())
                {
                    if (m_inside == 0)
                    {
                        s_enters[m_zoneId] = s_enters[m_zoneId] - 1;
                        // If no one left we are leaving a zone
                        if (s_enters[m_zoneId] == 0)
                            Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, false, this);
                    }
                }
            }
        }
    }
}
