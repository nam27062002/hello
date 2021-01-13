using System.Collections.Generic;
using UnityEngine;

public class CustomParticlesCulling : MonoBehaviour
{
    #region manager
    // This region is responsible for managing all CustomParticlesCulling instances which will be updated just when needed

    // Max amount of items that the manager can manage
    private const int MANAGER_MAX_ITEMS = 200;

    private static BoundingSphere[] Manager_BoundingSpheres { get; set; }   

    private static CullingGroup Manager_CullingGroup;

    private static List<CustomParticlesCulling> Manager_Items { get; set; }

    private static bool smManagerIsEnabled = true;

    public static bool Manager_IsEnabled
    {
        get
        {
            return smManagerIsEnabled;
        }

        set
        {
            smManagerIsEnabled = value;
        }
    }

    public static void Manager_OnDestroy()
    {
        if (Manager_CullingGroup != null)
        {
            Manager_CullingGroup.Dispose();
            Manager_CullingGroup = null;
        }

        Manager_Items = null;
        Manager_BoundingSpheres = null;
    }

    private static void Manager_AddItem(CustomParticlesCulling item)
    {
        if (Manager_IsEnabled)
        {
            if (Manager_Items == null)
            {
                Manager_Items = new List<CustomParticlesCulling>();
            }

            if (Manager_Items.Count >= MANAGER_MAX_ITEMS)
            {                
                Debug.LogError("Too many particle systems to cull");                

                return;
            }

            item.CullingIndex = Manager_Items.Count;
            Manager_Items.Add(item);

            if (Manager_BoundingSpheres == null)
            {
                Manager_BoundingSpheres = new BoundingSphere[MANAGER_MAX_ITEMS];
            }

            Manager_BoundingSpheres[Manager_Items.Count - 1] = item.BoundingSphere;

            if (Manager_CullingGroup == null)
            {
                Manager_CullingGroup = new CullingGroup();
                Manager_CullingGroup.targetCamera = Camera.main;
                Manager_CullingGroup.SetBoundingSpheres(Manager_BoundingSpheres);
                Manager_CullingGroup.onStateChanged += Manager_OnStateChanged;
            }

            Manager_CullingGroup.SetBoundingSphereCount(Manager_Items.Count);

            if (!Manager_IsVisible(item.CullingIndex) && !item.IsPaused)
            {
                item.Pause();
            }
        }
    }

    private static void Manager_RemoveItem(CustomParticlesCulling item)
    {
        if (Manager_Items != null && Manager_Items.Contains(item))
        {
            int index = Manager_Items.IndexOf(item);
            Manager_Items.Remove(item);

            int end = Manager_Items.Count;
            for (int i = index; i < end; i++)
            {
                Manager_Items[i].CullingIndex = i;
                Manager_BoundingSpheres[i] = Manager_BoundingSpheres[i + 1];
            }

            if (Manager_CullingGroup != null)
            {
                Manager_CullingGroup.SetBoundingSphereCount(Manager_Items.Count);
            }
        }
    }    

    private static void Manager_UpdateItem(CustomParticlesCulling item, bool isVisible)
    {
        if (item != null)
        {
            if (isVisible && item.IsPaused)
            {
                item.Resume();
            }
            else if (!isVisible && !item.IsPaused)
            {
                item.Pause();
            }            
        }
    }

    private static void Manager_OnStateChanged(CullingGroupEvent evt)
    {
        if (Manager_Items != null && evt.index < Manager_Items.Count)
        {                       
            if (evt.hasBecomeVisible)
            {
                Manager_UpdateItem(Manager_Items[evt.index], true);
            }
            else if (evt.hasBecomeInvisible)
            {
                Manager_UpdateItem(Manager_Items[evt.index], false);
            }           
        }
    }      

    /// <summary>
    /// Used just for debug purposes
    /// </summary>    
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

    public int CullingIndex { get; set; }

    private BoundingSphere BoundingSphere { get; set; }

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

            BoundingSphere = new BoundingSphere(m_cullingCenter.position, m_cullingRadius);

            Manager_AddItem(this);
        }         
    }

    public bool IsPaused { get; set; }

    public bool IsVisible()
    {
        return Manager_IsVisible(CullingIndex);
    }

    public void Pause()
    {
        if (!IsPaused)
        {
            IsPaused = true;

            int count = m_particleSystems.Count;
            for (int i = 0; i < count; i++)
            {
                // Checks if the particle system is currently active because the inactive ones were registered too just in case 
                // they are enabled after the game started
                if (m_particleSystems[i] != null && m_particleSystems[i].gameObject.activeSelf)
                {
                    m_particleSystems[i].Pause();                    
                }
            }
        }
    }
    
    public void Resume()
    {
        if (IsPaused)
        {
            IsPaused = false;

            int count = m_particleSystems.Count;
            for (int i = 0; i < count; i++)
            {
                // Checks if the particle system is currently active because the inactive ones were registered too just in case 
                // they are enabled after the game started
                if (m_particleSystems[i] != null && m_particleSystems[i].gameObject.activeSelf)
                {
                    m_particleSystems[i].Play();
                }
            }
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
