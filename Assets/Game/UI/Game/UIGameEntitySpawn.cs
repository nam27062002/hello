using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameEntitySpawn : MonoBehaviour{

    public string m_prefab;
    private GameObject m_instance;
    public GameObject instance { get { return m_instance; } }
    public bool m_startDeactivated = false;
    public Transform m_follow;
    
    public void Start()
    {
        // look for the ui reference
        // InstanceManager.gameHUD.m_miscGroup
        // spawn the prefab
        GameObject go = Resources.Load<GameObject>( "UI/" + m_prefab);
        if ( go != null )
        {
			if(InstanceManager.gameHUD != null) {
				m_instance = Instantiate(go, InstanceManager.gameHUD.m_miscGroup.transform) as GameObject;
				if(m_instance != null && m_startDeactivated) {
					m_instance.SetActive(false);
				}
				UIGameEntitySignaler signaler = m_instance.GetComponent<UIGameEntitySignaler>();
				if(signaler != null) {
					signaler.m_following = m_follow;
				}
			}
        }
    }
}
