using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilerMemoryController : MonoBehaviour
{
    public const string NAME = "SC_ProfilerMemory";

    private double Time_To_GC = 0f;      

    void Awake()
    {
        Quality_ApplyIndex(0);
    }

    // Update is called once per frame
    void Update ()
    {
        Time_To_GC -= Time.deltaTime;

        if (Time_To_GC <= 0f)
        {
            Time_To_GC = 1f;

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        TouchState touchState = GameInput.CheckTouchState(0);
        //Debug.Log("Got touchState 0 = " + touchState.ToString()); 		// NO TOUCH STATE IS BEING RECEIVED AFTER APP COMES BACK
        if (touchState == TouchState.pressed)
        {
            if (Gos_Go != null)
            {
                Gos_Unload();
            }
            else
            {
                Prefabs_AdvanceCurrentIndex();
                Gos_Load(sm_prefabsCatalog[Prefabs_CurrentIndex]);
            }
        }   

		if(MemoryProfiler_NeedsToTakeASample) 
		{
			MemoryProfiler_TakeASample();
			MemoryProfiler_NeedsToTakeASample = false;
		}
	}

    #region quality
    private int Quality_CurrentIndex = -1;

    private void Quality_ApplyIndex(int qualityIndex)
    {
        /*if (qualityIndex != Quality_CurrentIndex)
        {
            Quality_CurrentIndex = qualityIndex;
            QualitySettings.SetQualityLevel(Quality_CurrentIndex);
            Debug.Log(">> qualityIndex = " + qualityIndex);
        }*/
    }
    #endregion

    #region prefabs
    private static string[] sm_prefabsCatalog = new string[]
    {                
        "Game/Entities/NewEntites/Air/PF_Canary01_Flock",
        "Game/Entities/NewEntites/Air/PF_BatSmall01_Flock",
        "Game/Entities/NewEntites/Air/PF_BatSmall02_Flock",
        "Game/Entities/NewEntites/Air/PF_BatBig_Flock",
        "Game/Entities/NewEntites/Air/PF_Ghost01",
        "Game/Entities/NewEntites/Air/PF_MineSmall",
        "Game/Entities/NewEntites/Air/PF_MineMedium",
        "Game/Entities/NewEntites/Air/PF_MineBig",
        "Game/Entities/NewEntites/Air/PF_Witch",
        "Game/Entities/NewEntites/Air/PF_EnemyTier0",
        "Game/Entities/NewEntites/Air/PF_EnemyTier1",
        "Game/Entities/NewEntites/Air/PF_EnemyTier2",
        "Game/Entities/NewEntites/Cage/PF_Cage",
        "Game/Entities/NewEntites/Cage/PF_HangingCage",
        "Game/Entities/NewEntites/Goblin/PF_Kamikaze",
        "Game/Entities/NewEntites/Goblin/PF_Spartakus",
        "Game/Entities/NewEntites/Goblin/PF_Worker01",
        "Game/Entities/NewEntites/Goblin/PF_Worker02",
        "Game/Entities/NewEntites/Goblin/PF_WorkerWife",        
        "Game/Entities/NewEntites/Monster/PF_Troll",
        "Game/Entities/NewEntites/Surface/PF_Archer01",
        "Game/Entities/NewEntites/Surface/PF_Richman",
        "Game/Entities/NewEntites/Surface/PF_Soldier01",
        "Game/Entities/NewEntites/Surface/PF_Villager01",
        "Game/Entities/NewEntites/Surface/PF_Cow",
        "Game/Entities/NewEntites/Surface/PF_Horse",
        "Game/Entities/NewEntites/Surface/PF_SpiderRed",
        "Game/Entities/NewEntites/Water/PF_Crocodile",
        "Game/Entities/NewEntites/Water/PF_fish01_Generic",
        "Game/Entities/NewEntites/Water/PF_Piranha",
        "Game/Entities/NewEntites/Water/PF_Shark",
        "Game/Dragons/PF_DragonBaby",
        "Game/Dragons/PF_DragonClassic",
        "Game/Equipable/Pets/PF_PetFroggy",
    };

    private static int sm_prefabsCatalogCount = sm_prefabsCatalog.Length;

    private int mPrefabsCurrentIndex = -1;
    private int Prefabs_CurrentIndex
    {
        get
        {
            return mPrefabsCurrentIndex;
        }

        set
        {
            mPrefabsCurrentIndex = value;
        }
    }

    private void Prefabs_AdvanceCurrentIndex()
    {
        Prefabs_CurrentIndex++;

        if (Prefabs_CurrentIndex >= sm_prefabsCatalogCount)
        {
            Prefabs_CurrentIndex %= sm_prefabsCatalogCount;

            int nextIndex = (Quality_CurrentIndex + 1) % 5;
            Quality_ApplyIndex(nextIndex);            
        }
    }
    #endregion

    #region gos
    private GameObject Gos_Go;

    private void Gos_Load(string prefabPath)
    {
        Object o = Resources.Load(prefabPath);
        if (o != null)
        {
            Gos_Go = (GameObject)GameObject.Instantiate(o, transform);
            if (Gos_Go != null)
            {
                Gos_Go.transform.position = Vector3.zero;

                MonoBehaviour[] components = Gos_Go.GetComponentsInChildren<MonoBehaviour>(true);
                if (components != null)
                {
                    int count = components.Length;
                    for (int i = 0; i < count; i++)
                    {
                        Destroy(components[i]);
                    }
                }

				MemoryProfiler_NeedsToTakeASample = true;
            }
        }
    }

    private void Gos_Unload()
    {
        if (Gos_Go != null)
        {
            Destroy(Gos_Go);
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
    #endregion

	#region memory_profiler
	private AssetMemoryProfiler m_memoryProfiler;
    private MemoryProfiler m_memoryProfilerExtra;

	private bool MemoryProfiler_NeedsToTakeASample { get; set; }

	private void MemoryProfiler_TakeASample() 
	{
		/*if(m_memoryProfiler == null) 
		{
			m_memoryProfiler = new AssetMemoryProfiler();
		} 
		else 
		{
			m_memoryProfiler.Reset();
		}

		m_memoryProfiler.AddGo(Gos_Go, "Npc");
		long textures = m_memoryProfiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Texture);
		long animations = m_memoryProfiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Animation);
		long meshes = m_memoryProfiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Mesh);
		long other = m_memoryProfiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Other);
		long total = m_memoryProfiler.GetSize();

		Debug.Log("");
		Debug.Log("--------------------------------");
		Debug.Log("Go: " + Gos_Go.name);
		Debug.Log("textures: " + BytesToMegaBytes(textures));
		Debug.Log("animations: " + BytesToMegaBytes(animations));
		Debug.Log("meshes: " + BytesToMegaBytes(meshes));
		Debug.Log("other: " + BytesToMegaBytes(other));
		Debug.Log("TOTAL: " + BytesToMegaBytes(total));
		Debug.Log("--------------------------------");
        */

        if (m_memoryProfilerExtra == null)
        {
            m_memoryProfilerExtra = new MemoryProfiler();
        }

       // m_memoryProfilerExtra.TakeASample();
	}

	private float BytesToMegaBytes(long bytes) 
	{
		return bytes / (1024f * 1024f);
	}
	#endregion
}
