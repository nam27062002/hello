using UnityEngine;
using System.Collections;
using FGOL.Events;

public class MissionAreaTrigger : MonoBehaviour
{
    [SerializeField]
    private string m_areaOasisKey;
    public string OasisKey { get { return m_areaOasisKey; } set { m_areaOasisKey = value; } }

    void OnTriggerEnter (Collider other)
    {
        DragonPlayer player = InstanceManager.player;
        if ((player != null) && (other.gameObject == player.gameObject))            
        {
            if ( !player.IsFuryOn() && !player.IsMegaFuryOn() )
            {
            	// TODO: MALH
                // EventManager.Instance.TriggerEvent(Events.DisplayMissionAreaName, true, m_areaOasisKey);
            }
        }
    }
	
	void OnTriggerExit (Collider other)
    {
		DragonPlayer player = InstanceManager.player;
        if ((player != null) && (other.gameObject == player.gameObject))            
        {
			if ( !player.IsFuryOn() && !player.IsMegaFuryOn() )
            {
            	// TODO: MALH
                // EventManager.Instance.TriggerEvent(Events.DisplayMissionAreaName, false, m_areaOasisKey);
            }
        }
    }
}
