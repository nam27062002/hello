using System.Collections.Generic;
using UnityEngine;

public class CustomParticlesCulling : MonoBehaviour
{
    #region manager
    // This region is responsible for managing all CustomParticlesCulling instances which will be updated just when needed
    
    private static CullingGroup Manager_CullingGroup;

    private static List<CustomParticlesCulling> Manager_Items { get; set; }

    public static void Manager_OnDestroy()
    {
        if (Manager_CullingGroup != null)
        {
            Manager_CullingGroup.Dispose();
        }
    }

    private static void Manager_AddItem(CustomParticlesCulling item)
    {
        if (Manager_Items == null)
        {
            Manager_Items = new List<CustomParticlesCulling>();
        }

        Manager_Items.Add(item);       
    }

    private static void Manager_RemoveItem(CustomParticlesCulling item)
    {
        if (Manager_Items != null && Manager_Items.Contains(item))
        {
            Manager_Items.Remove(item);
        }
    }    

    private static void Manager_UpdateItem(CustomParticlesCulling item, EEvent e)
    {
        if (item != null && e != EEvent.None)
        {
            switch (e)
            {
                case EEvent.HasBecomeVisible:
                    item.UpdateState(true, false);                   
                    break;

                case EEvent.HasBecomeInvisible:
                    item.UpdateState(false, true);                    
                    break;
            }
        }
    }

    private static void Manager_OnStateChanged(CullingGroupEvent evt)
    {
        if (Manager_Items != null && evt.index < Manager_Items.Count)
        {
            EEvent newEvent = EEvent.None;
            if (evt.hasBecomeVisible)
            {
                newEvent = EEvent.HasBecomeVisible;
            }
            else if (evt.hasBecomeInvisible)
            {
                newEvent = EEvent.HasBecomeInvisible;
            }

            if (newEvent != EEvent.None)
            {                
                Manager_UpdateItem(Manager_Items[evt.index], newEvent);                                
            }
        }
    }

    public static void Manager_NotifyGameStarted()
    {
        int count = 0;
        if (Manager_Items != null && Manager_Items.Count > 0)
        {
            count = Manager_Items.Count;
        }

        // Checks if it hasn't been initialized yet
        if (Manager_CullingGroup == null)
        {
            if (count > 0)
            {
                Manager_CullingGroup = new CullingGroup();
                Manager_CullingGroup.targetCamera = Camera.main;
                
                BoundingSphere[] bSpheres = new BoundingSphere[count];
                for (int i = 0; i < count; i++)
                {
                    Manager_Items[i].CullingIndex = i;
                    bSpheres[i] = new BoundingSphere(Manager_Items[i].m_cullingCenter.position, Manager_Items[i].m_cullingRadius);
                }

                Manager_CullingGroup.SetBoundingSpheres(bSpheres);
                Manager_CullingGroup.SetBoundingSphereCount(count);
                Manager_CullingGroup.onStateChanged += Manager_OnStateChanged;
            }
        }
        
        for (int i = 0; i < count; i++)
        {
            if (Manager_CullingGroup.IsVisible(i))
            {                
                Manager_UpdateItem(Manager_Items[i], EEvent.HasBecomeVisible);
            }
            else
            {                
                Manager_UpdateItem(Manager_Items[i], EEvent.HasBecomeInvisible);
            }
        }
    }  

    public static void Manager_SimulateForAll(bool hasBecomeVisible, bool hasBecomeInvisible)
    {
        if (Manager_Items != null)
        {
            int count = Manager_Items.Count;
            for (int i = 0; i < count; i++)
            {
                Manager_Items[i].UpdateState(hasBecomeVisible, hasBecomeInvisible);                
            }
        }
    }

    public static bool Manager_IsVisible(int cullingIndex)
    {
        bool returnValue = true;
        if (Manager_CullingGroup != null && Manager_Items != null && cullingIndex > -1 && cullingIndex < Manager_Items.Count)
        {
            returnValue = Manager_CullingGroup.IsVisible(cullingIndex);
        }

        return returnValue;
    }    
    #endregion

    public Transform m_cullingCenter;
    public float m_cullingRadius = 10;

    /// <summary>
    /// Parent of all particle systems. If null then this.gameObject is used instead
    /// </summary>
    public GameObject m_parent;
    private List<ParticleSystem> m_particleSystems;

    private enum EEvent
    {
        None,
        HasBecomeVisible,
        HasBecomeInvisible
    };       

    public int CullingIndex { get; set; }

    void Start()
    {
        CullingIndex = -1;        

        if (m_parent == null)
        {
            m_parent = gameObject;
        }

        if (m_cullingCenter == null)
        {
            m_cullingCenter = transform;
        }

        // We also want to register the inactive particle systems in order to take them into consideration if they are enabled later on
        ParticleSystem[] systems = m_parent.GetComponentsInChildren<ParticleSystem>(true);
        if ((systems != null && systems.Length > 0))
        {
            m_particleSystems = new List<ParticleSystem>();           

            int count = systems.Length;            
            for (int i = 0; i < count; i++)
            {
                m_particleSystems.Add(systems[i]);               
            }
                        
            Manager_AddItem(this);
        }         
    }         

    public void UpdateState(bool hasBecomeVisible, bool hasBecomeInvisible)
    {
        int count = m_particleSystems.Count;
        for (int i = 0; i < count; i++)
        {
            // Checks if the particle system is currently active because the inactive ones were registered too just in case 
            // they are enabled after the game started
            if (m_particleSystems[i] != null && m_particleSystems[i].gameObject.activeSelf)
            {
                if (hasBecomeVisible)
                {
                    // We could simulate forward a little here to hide that the system was not updated off-screen.
                    //if (m_particleSystems[i].isPaused)
                    {
                        m_particleSystems[i].Play(true);
                    }
                }
                else if (hasBecomeInvisible)
                {
                    if (m_particleSystems[i].isPlaying)
                    {
                        m_particleSystems[i].Pause();
                        //m_particleSystems[i].Stop();
                    }
                }
            }
        }
    }

    public bool IsVisible()
    {
        return Manager_IsVisible(CullingIndex);
    }

    void OnDestroy()
    {       
        Manager_RemoveItem(this);
    }

    void OnDrawGizmos()
    {
        Transform t = (m_cullingCenter == null) ? transform : m_cullingCenter;
        // Draw gizmos to show the culling sphere.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(t.position, m_cullingRadius);
    }
}
