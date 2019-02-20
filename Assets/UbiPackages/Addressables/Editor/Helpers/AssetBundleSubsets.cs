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
    private string[] m_subsetNames;





    public AssetBundleSubsets(int _sets) {
        m_sets = new HashSet<string>[_sets];
        m_subsetNames = new string[_sets];

        for (int i = 0; i < _sets; ++i) {
            char c = (char)('A' + i);
            m_subsetNames[i] = c.ToString();
        }
        m_subsetPrefix = "auto_bundle_";

        m_combinations = new List<List<int>>();
        BuildCombinations();

        m_subsets = new HashSet<string>[m_combinations.Count];
    }

    public void ChangeSubsetName(int _set, string _name) {
        m_subsetNames[_set] = _name;
    }

    public void ChangeSubsetPrefix(string _prefix) {
        m_subsetPrefix = _prefix;
    }

    public void AddAssetName(int _set, string _name) {
        m_sets[_set].Add(_name);
    }

    public void BuildSubsets() {
        for (int set = 0; set < m_sets.Length; ++set) {
            foreach (string asset in m_sets[set]) {
                for (int combination = m_sets.Length; combination < m_combinations.Count; ++combination) {
                    if (CombinationContainsAsset(asset, set, m_combinations[combination])) {
                        m_subsets[combination].Add(asset);
                    }
                }
            }
        }

    }

    public bool CombinationContainsAsset(string _asset, int _sourceSet, List<int> _combination) {
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

    /*
    public void SetBundlesNames() {
        for (int i = 0; i < m_subsets.Length; ++i) {
            SetBundleNames(i);
        }
    }

    private void SetBundleNames(int _subset) {
        List<string> sorted = m_subsets[_subset].ToList();
        sorted.Sort();

        foreach (string prefab in sorted) {
            string[] path = AssetDatabase.FindAssets(prefab + ".prefab");
            if (path.Length > 0) {
                AssetImporter ai = AssetImporter.GetAtPath(path[0]);
                ai.SetAssetBundleNameAndVariant(m_assetBundleNames[_subset], "");
            }
        }
    }*/
}
#endif