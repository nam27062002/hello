#if UNITY_EDITOR
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetBundleSubsets {
    private List<List<int>> m_combinations;
    private HashSet<string>[] m_sets;
    private HashSet<string>[] m_subsets;

    private string   m_subsetPrefix;
    private string[] m_setNames;

    private Dictionary<string, string> m_assetLODsMap;



    public AssetBundleSubsets(int _sets) {
        m_sets = new HashSet<string>[_sets];
        m_setNames = new string[_sets];

        for (int i = 0; i < _sets; ++i) {
            m_sets[i] = new HashSet<string>();

            char c = (char)('A' + i);
            m_setNames[i] = c.ToString();
        }
        m_subsetPrefix = "auto_bundle_";

        m_combinations = new List<List<int>>();
        BuildCombinations();

        m_subsets = new HashSet<string>[m_combinations.Count];
        for (int i = 0; i < m_subsets.Length; ++i) {
            m_subsets[i] = new HashSet<string>();
        }

        m_assetLODsMap = new Dictionary<string, string>();
    }

    public void ChangeSubsetName(int _set, string _name) {
        m_setNames[_set] = _name;
    }

    public void ChangeSubsetPrefix(string _prefix) {
        m_subsetPrefix = _prefix;
    }

    public void AddAssetName(int _set, string _name) {
        m_sets[_set].Add(_name);
    }

    public void AddAssetName(int _set, string _name, string _lods) {
        m_sets[_set].Add(_name);

        if (!m_assetLODsMap.ContainsKey(_name)) {
            m_assetLODsMap.Add(_name, _lods);
        }
    }

    public void BuildSubsets() {
        for (int set = 0; set < m_sets.Length; ++set) {
            foreach (string asset in m_sets[set]) {
                bool storeInSet = true;
                for (int combination = m_combinations.Count - 1; combination >= m_sets.Length; --combination) {
                    if (CombinationContainsAsset(asset, set, m_combinations[combination])) {
                        m_subsets[combination].Add(asset);
                        storeInSet = false;
                        break;
                    }
                }
                if (storeInSet) {
                    m_subsets[set].Add(asset);
                }
            }
        }

        int assetCount = 0;
        for (int subset = 0; subset < m_subsets.Length; subset++) {
            List<string> sorted = m_subsets[subset].ToList();
            sorted.Sort();

            string output = "-++[" + GetSubSetName(subset) + "]++++-----------------------\n";
            foreach (string asset in sorted) {
                output += "    " + asset;
                if (m_assetLODsMap.ContainsKey(asset)) {
                    output += " [" + m_assetLODsMap[asset] + "]";
                }
                output += "\n";
                assetCount++;
            }
            output += "-++----";

            Debug.LogWarning(output);
        }
    }

    public void AssignBundles() {
        for (int subset = 0; subset < m_subsets.Length; subset++) {
            List<string> sorted = m_subsets[subset].ToList();
            sorted.Sort();

            foreach (string prefab in sorted) {
                string[] path = AssetDatabase.FindAssets("t:prefab " + prefab);
                for (int i = 0; i < path.Length; ++i) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(path[i]);
                    AssetImporter ai = AssetImporter.GetAtPath(assetPath);
                    ai.SetAssetBundleNameAndVariant(GetSubSetName(subset), "");
                }
            }
        }
    }

    private bool CombinationContainsAsset(string _asset, int _sourceSet, List<int> _combination) {
        bool contains = true;
        foreach (int set in _combination) {
            if (set != _sourceSet) {
                contains &= m_sets[set].Contains(_asset);
            }
        }
        return contains;
    }

    private void BuildCombinations() {
        for (int combinationsOf = 1; combinationsOf <= m_sets.Length; ++combinationsOf) {
            int[] combination = new int[combinationsOf];
            Combinations(combinationsOf, 0, combination);
        }
    }

    private void Combinations(int _len, int _startPosition, int[] _combination) {
        if (_len == 0) {
            m_combinations.Add(_combination.ToList());
            return;
        }

        for (int i = _startPosition; i <= m_sets.Length - _len; ++i) {
            _combination[_combination.Length - _len] = i;
            Combinations(_len - 1, i + 1, _combination);
        }
    }

    private string GetSubSetName(int _set) {
        string name = m_subsetPrefix;

        for (int i = 0; i < m_combinations[_set].Count; ++i) {
            if (i > 0) name += "_";
            name += m_setNames[m_combinations[_set][i]];
        }

        return name;
    }
}
#endif