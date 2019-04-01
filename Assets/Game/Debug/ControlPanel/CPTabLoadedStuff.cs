using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CPTabLoadedStuff : MonoBehaviour
{
    public class ItemListView
    {        
        private List<ItemView> m_itemViews = new List<ItemView>();        
        private GameObject m_itemGO;
        private Transform m_itemRoot;
        
        public void Setup(GameObject itemListGO, GameObject itemGO)
        {                      
            m_itemRoot = itemListGO.FindComponentRecursive<Transform>("ItemsRoot");
            m_itemGO = itemGO;
        }

        public void SetItemNames(List<string> itemNames)
        {
            int currentCount = m_itemViews.Count;
            int newCount = (itemNames== null) ? 0 : itemNames.Count;            

            int diff = newCount - currentCount;
            if (diff > 0)
            {
                ItemView itemView;
                GameObject itemGO;
                for (int i = 0; i < diff; i++)
                {
                    itemGO = Instantiate(m_itemGO, m_itemRoot);
                    itemGO.SetActive(true);
                    itemView = new ItemView();
                    itemView.Setup(itemGO);
                    m_itemViews.Add(itemView);
                }
            }
            else if (diff < 0)
            {
                diff = -diff;
                for (int i = 0; i < diff; i++)
                {
                    Destroy(m_itemViews[0].GO);
                    m_itemViews.RemoveAt(0);
                }
            }
            
            for (int i = 0; i < newCount; i++)
            {
                m_itemViews[i].SetName(itemNames[i]);
            }
        }                       
    }
    
    public class ItemView
    {
        private TMP_Text m_nameText;
        public GameObject GO { get; set; }

        public void Setup(GameObject go)
        {
            m_nameText = go.FindComponentRecursive<TMP_Text>("NameText");
            GO = go;
        }

        public void Setup(GameObject go, string name)
        {
            Setup(go);
            SetName(name);
        }

        public void SetName(string value)
        {
            m_nameText.text = value;
        }               
    }
            
    [SerializeField]
    private GameObject m_itemViewPrefab;

    [SerializeField]
    private GameObject m_assetBundlesRoot;
    private ItemListView m_assetBundlesListView;

    [SerializeField]
    private GameObject m_scenesRoot;
    private ItemListView m_scenesView;

    private List<string> m_assetBundleIdsLoaded;
    private List<string> m_sceneNamesLoaded;

    private float m_latestUpdateAt = 0f;

    void Start()
    {        
        m_assetBundlesListView = new ItemListView();
        m_assetBundlesListView.Setup(m_assetBundlesRoot, m_itemViewPrefab);

        m_scenesView = new ItemListView();
        m_scenesView.Setup(m_scenesRoot, m_itemViewPrefab);

        m_assetBundleIdsLoaded = new List<string>();
        m_sceneNamesLoaded = new List<string>();

        UpdateLists();
    }        

    void UpdateAssetBundleIdsList()
    {
        m_assetBundleIdsLoaded.Clear();        
        AssetBundlesManager.Instance.FillWithLoadedAssetBundleIdList(m_assetBundleIdsLoaded);

        m_assetBundlesListView.SetItemNames(m_assetBundleIdsLoaded);
    }

    void UpdateSceneNamesList()
    {
        m_sceneNamesLoaded.Clear();

        Scene scene;
        int count = SceneManager.sceneCount;
        for (int i = 0; i < count; i++)
        {
            scene = SceneManager.GetSceneAt(i);
            m_sceneNamesLoaded.Add(scene.name);
        }

        m_scenesView.SetItemNames(m_sceneNamesLoaded);  
    }

    void UpdateLists()
    {
        UpdateAssetBundleIdsList();
        UpdateSceneNamesList();
        m_latestUpdateAt = Time.realtimeSinceStartup;
    }

    void Update()
    {
        if (Time.realtimeSinceStartup - m_latestUpdateAt >= 5f)
        {
            UpdateLists();
        }
    }
}
