#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//using UnityEditor;


public class OTA_NPCSceneController : MonoBehaviour {
    public enum Log {
        Subsets = 0,
        Assets
    }

    [SerializeField] private string m_areaPrefix = "";
    [SerializeField] private GameObject[] m_area;
	#if UNITY_EDITOR
    AssetBundleSubsets assetBundleSubsets;
	#endif

    public void Build(Log _logType) {
#if UNITY_EDITOR
        if (m_area.Length > 0) {
            assetBundleSubsets = new AssetBundleSubsets(m_area.Length);
            assetBundleSubsets.ChangeSubsetPrefix(m_areaPrefix);

            for (int a = 0; a < m_area.Length; ++a) {
                GameObject go = m_area[a];

                assetBundleSubsets.ChangeSubsetName(a, go.name);

                List<ISpawner> spawners = new List<ISpawner>();
                FindISpawner(go.transform, ref spawners);

                for (int s = 0; s < spawners.Count; ++s) {
                    List<string> prefabs = spawners[s].GetPrefabList();
                    if (prefabs != null) {
                        for (int j = 0; j < prefabs.Count; ++j) {
                            assetBundleSubsets.AddAssetName(a, prefabs[j]);
                        }
                    }
                }
            }

            assetBundleSubsets.BuildSubsets();

            switch(_logType) {
                case Log.Subsets: assetBundleSubsets.LogSubsets(false, true); break;
                case Log.Assets: assetBundleSubsets.LogAssets(false, true); break;
            }
        }
#endif
    }

    private static void FindISpawner(Transform _t, ref List<ISpawner> _list) {
        ISpawner c = _t.GetComponent<ISpawner>();
        if (c != null) {
            _list.Add(c);
        }
        // Not found, iterate children transforms
        foreach (Transform t in _t) {
            FindISpawner(t, ref _list);
        }
    }
}
#endif
