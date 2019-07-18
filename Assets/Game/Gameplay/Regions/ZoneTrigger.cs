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
    private static List<ZoneTrigger> s_activeZones = new List<ZoneTrigger>();
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
        {
            s_enters.Clear();
            s_activeZones.Clear();
        }
			
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
                        {
                           OnEnterNewZone(this);
                        }
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
            {
                OnEnterNewZone(this);
            }
        }
        m_checkIntroMovement = null;
        yield return null;
    }

    void OnEnterNewZone( ZoneTrigger zone )
    {
        if ( s_activeZones.Count > 0)   // The first one saved form an id
        {
            // Exit previous zone
            Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, false, s_activeZones[ s_activeZones.Count - 1 ]);
        }
        // Enter new zone
        s_activeZones.Add( this );
        Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, true, this);
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
                        {
                            OnExitZone(this);
                        }
                    }
                }
            }
        }
    }

    void OnExitZone(ZoneTrigger zone)
    {
        bool isCurrent = false;
        isCurrent = s_activeZones.Last().m_zoneId == zone.m_zoneId;
        // Check if zone is last to exit
        if ( isCurrent )
        {
            Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, false, this);
        }

        // Remove all instances of this zone id
        int length = s_activeZones.Count;
        for (int i = length-1; i >= 0; i--)
        {
            if ( s_activeZones[i].m_zoneId == zone.m_zoneId )
            {
                s_activeZones.RemoveAt(i);
            }
        }

        // if this id was the current zone, we deleted it, if we still have active zones we entered the new one
        if ( isCurrent && s_activeZones.Count > 0 )
        {
            Messenger.Broadcast<bool, ZoneTrigger>(MessengerEvents.MISSION_ZONE, true, s_activeZones.Last());
        }
                            
    }
}
