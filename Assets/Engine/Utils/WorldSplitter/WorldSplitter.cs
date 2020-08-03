using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Hungry Dragon/Add World Splitter")]
public class WorldSplitter : MonoBehaviour
{
    #region manager
    // This region is responsible for manager all WorldSplitter objects for dubug purposes

    private static List<WorldSplitter> Manager_WorldSplitters { get; set; }

    private static void Manager_RegisterWorldSplitter(WorldSplitter ws)
    {
        if (Manager_WorldSplitters == null)
        {
            Manager_WorldSplitters = new List<WorldSplitter>();
        }

        Manager_WorldSplitters.Add(ws);
    }

    private static void Manager_UnregisterWorldSplitter(WorldSplitter ws)
    {
        if (Manager_WorldSplitters != null && Manager_WorldSplitters.Contains(ws))
        {
            Manager_WorldSplitters.Remove(ws);
        }
    }

    public static void Manager_SetLevelsLOD(FeatureSettings.ELevel4Values level)
    {
        if (Manager_WorldSplitters != null)
        {
            int count = Manager_WorldSplitters.Count;            
            for (int i = 0; i < count; i++)
            {                
                Manager_WorldSplitters[i].SetLevelsLOD(level);
            }
        }
    }    
    #endregion

    [SerializeField]
	public bool Low = true;

	[SerializeField]
	public bool Medium = true;

	[SerializeField]
	public bool High = true;

    void Awake()
    {
        Manager_RegisterWorldSplitter(this);
        if (FeatureSettingsManager.IsReady())
        {
            SetLevelsLOD(FeatureSettingsManager.instance.LevelsLOD);
        }
    }

    void OnDestroy()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Manager_UnregisterWorldSplitter(this);
        }
    }   

    private void SetLevelsLOD(FeatureSettings.ELevel4Values level)
    {
        // very_low is ignored
        bool active = (level == FeatureSettings.ELevel4Values.low && Low) ||
                      (level == FeatureSettings.ELevel4Values.mid && Medium) ||
                      (level == FeatureSettings.ELevel4Values.high && High);

        //gameObject.SetActive(active);

        if (active) {
            gameObject.SetActive(true);
        } else {
            Destroy(gameObject);
        }
    }
}