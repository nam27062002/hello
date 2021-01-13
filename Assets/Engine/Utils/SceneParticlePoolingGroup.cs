using System.Collections.Generic;
using UnityEngine;

public class SceneParticlePoolingGroup : MonoBehaviour
{
    #region manager
	// This region is responsible for managing all SceneParticleCulling instances which will be updated just when needed

    // Max amount of items that the manager can manage
    private const int MANAGER_MAX_ITEMS = 200;

    private static BoundingSphere[] Manager_BoundingSpheres { get; set; }   

    private static CullingGroup Manager_CullingGroup;

	private static List<SceneParticlePoolingGroup> Manager_Items { get; set; }

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

	private static void Manager_AddItem(SceneParticlePoolingGroup item)
    {
        if (Manager_IsEnabled)
        {
            if (Manager_Items == null)
            {
				Manager_Items = new List<SceneParticlePoolingGroup>();
            }

            if (Manager_Items.Count >= MANAGER_MAX_ITEMS)
            {                
                Debug.LogError("Too many particle systems to cull");                

                return;
            }

            if (!Manager_Items.Contains(item))
            {
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
                    Manager_CullingGroup.SetBoundingSpheres(Manager_BoundingSpheres);
                    Manager_CullingGroup.onStateChanged += Manager_OnStateChanged;
                }
				Manager_CullingGroup.targetCamera = Camera.main;

                Manager_CullingGroup.SetBoundingSphereCount(Manager_Items.Count);

                if (!Manager_IsVisible(item.CullingIndex) && !item.IsPaused)
                {
                    item.Pause();
                }
                else if (Manager_IsVisible(item.CullingIndex) && item.IsPaused)
                {
                    item.Resume();
                }
            }
        }
    }

	private static void Manager_RemoveItem(SceneParticlePoolingGroup item)
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

	private static void Manager_UpdateItem(SceneParticlePoolingGroup item, bool isVisible)
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
    public int CullingIndex { get; set; }
    private BoundingSphere BoundingSphere { get; set; }
    private bool IsInitialized { get; set; }
    private ParticleDataPlace[] m_particles;


    private void Init()
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            CullingIndex = -1;
            if (m_cullingCenter == null)
            {
                m_cullingCenter = transform;
            }
            IsPaused = true;
            BoundingSphere = new BoundingSphere(m_cullingCenter.position, m_cullingRadius);

			m_particles = GetParticleDataPlaces();	
        }

		for( int i = 0; i<m_particles.Length; ++i )
		{
			m_particles[i].Init();
		}
    }

    public ParticleDataPlace[] GetParticleDataPlaces()
    {
    	GameObject go = m_parent;
		if (go == null)
        {
            go = gameObject;
        }
		return go.GetComponentsInChildren<ParticleDataPlace>(true);;
    }

    void Start()
    {
        Init();
        Manager_AddItem(this);
    }

    public bool IsPaused { get; set; }

    public bool IsVisible()
    {
        return Manager_IsVisible(CullingIndex);
    }

    void OnEnable()
    {
        Init();
        Manager_AddItem(this);
    }


    void OnDisable()
    {
        if ( ApplicationManager.IsAlive )
        { 
    	    Pause();
		    Manager_RemoveItem(this);
        }
    }

    public void Pause()
    {
        if (!IsPaused)
        {
            IsPaused = true;
            for( int i = 0; i<m_particles.Length; ++i )
            {
				m_particles[i].HideParticle();
            }
        }
    }
    
    public void Resume()
    {
        if (IsPaused)
        {
            IsPaused = false;
			for( int i = 0; i<m_particles.Length; ++i )
            {
            	m_particles[i].ShowParticle();
            }
        }
    }

    void OnDestroy()
    {       
		if ( ApplicationManager.IsAlive )
        { 
    	    Pause();
		    Manager_RemoveItem(this);
        }
    }

    void OnDrawGizmos()
    {
        Transform t = (m_cullingCenter == null) ? transform : m_cullingCenter;
        // Draw gizmos to show the culling sphere.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(t.position, m_cullingRadius);
    }
}
