using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfilerLoadScenesController : MonoBehaviour
{  
    public List<string> m_scenesAreaToLoad;

    public List<string> m_ScenesCommonToLoad;
    
    void Awake()
    {
        ScenesAreLoaded = false;
        if (!ContentManager.ready) ContentManager.InitContent(true);
    }

    // Use this for initialization
    void Update()
    {
        if (ContentManager.ready && !ScenesAreLoaded)
        {
            ScenesAreLoaded = true;

            int count = m_scenesAreaToLoad.Count;
            for (int i = 0; i < count; i++)
            {
                SceneManager.LoadSceneAsync(m_scenesAreaToLoad[i], LoadSceneMode.Additive);
            }

            count = m_ScenesCommonToLoad.Count;
            for (int i = 0; i < count; i++)
            {
                SceneManager.LoadSceneAsync(m_ScenesCommonToLoad[i], LoadSceneMode.Additive);
            }
        }
    }	

    private bool ScenesAreLoaded { get; set; }
}
