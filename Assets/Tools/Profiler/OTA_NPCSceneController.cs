using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using UnityEditor;


public class OTA_NPCSceneController : MonoBehaviour {

	public string area1Scene;
    public string area2Scene;
    public string area3Scene;

    private HashSet<string> m_area1_NPCs = new HashSet<string>();
    private HashSet<string> m_area2_NPCs = new HashSet<string>();
    private HashSet<string> m_area3_NPCs = new HashSet<string>();

    private HashSet<string> m_npcs_area1 = new HashSet<string>();
    private HashSet<string> m_npcs_area2 = new HashSet<string>();
    private HashSet<string> m_npcs_area3 = new HashSet<string>();
    private HashSet<string> m_npcs_area1_area2 = new HashSet<string>();
    private HashSet<string> m_npcs_area1_area3 = new HashSet<string>();
    private HashSet<string> m_npcs_area2_area3 = new HashSet<string>();
    private HashSet<string> m_npcs_area1_area2_area3 = new HashSet<string>();

    StreamWriter m_sw;



    public void Build(int _area, List<ISpawner> _spawners) {
        HashSet<string> targetSet = m_area1_NPCs;

        switch(_area) {
            case 1: targetSet = m_area1_NPCs; break;
            case 2: targetSet = m_area2_NPCs; break;
            case 3: targetSet = m_area3_NPCs; break;
        }

        targetSet.Clear();

        //lets instantiate one of each NPC			 
        for (int i = 0; i < _spawners.Count; ++i) {
            ISpawner sp = _spawners[i];

            if (sp.GetType() == typeof(Spawner) || sp.GetType() == typeof(SpawnerCage)) {
                Spawner.EntityPrefab[] prefabs = (sp as Spawner).m_entityPrefabList;
                for (int j = 0; j < prefabs.Length; ++j) {
                    targetSet.Add(prefabs[j].name);
                }
            } else if (sp.GetType() == typeof(SeasonalSpawner)) {
                List<SeasonalSpawner.SeasonalConfig> seasonals = (sp as SeasonalSpawner).m_spawnConfigs;
                for (int s = 0; s < seasonals.Count; ++s) {
                    for (int j = 0; j < seasonals[s].m_spawners.Length; ++j) {
                        targetSet.Add(seasonals[s].m_spawners[j]);
                    }
                }
            } else if (sp.GetType() == typeof(SpawnerRoulette)) {
                SpawnerRoulette.EntityPrefab[] prefabs = (sp as SpawnerRoulette).m_entityPrefabList;
                for (int j = 0; j < prefabs.Length; ++j) {
                    targetSet.Add(prefabs[j].name);
                }
            } else if (sp.GetType() == typeof(SpawnerWagon)) {
                SpawnerWagon.EntityPrefab[] prefabs = (sp as SpawnerWagon).m_entityPrefabList;
                for (int j = 0; j < prefabs.Length; ++j) {
                    targetSet.Add(prefabs[j].name);
                }
            } else if (sp.GetType() == typeof(SpawnerStar)) {
                targetSet.Add((sp as SpawnerStar).entityPrefab);
            } else if (sp.GetType() == typeof(SpawnerBg)) {
                targetSet.Add((sp as SpawnerBg).m_entityPrefabStr);
            }
        }
    }

    public void CompareSets() {
        m_npcs_area1.Clear();
        m_npcs_area2.Clear();
        m_npcs_area3.Clear();

        m_npcs_area1_area2.Clear();
        m_npcs_area1_area3.Clear();
        m_npcs_area2_area3.Clear();

        m_npcs_area1_area2_area3.Clear();


        CompareSets(m_area1_NPCs, m_area2_NPCs, m_area3_NPCs,
                    m_npcs_area1, m_npcs_area1_area2, m_npcs_area1_area3,
                    m_npcs_area1_area2_area3);

        CompareSets(m_area2_NPCs, m_area1_NPCs, m_area3_NPCs,
                    m_npcs_area2, m_npcs_area1_area2, m_npcs_area2_area3,
                    m_npcs_area1_area2_area3);

        CompareSets(m_area3_NPCs, m_area1_NPCs, m_area2_NPCs,
                    m_npcs_area3, m_npcs_area1_area3, m_npcs_area2_area3,
                    m_npcs_area1_area2_area3);


        WriteHashSet("npcs_area1", m_npcs_area1);
        WriteHashSet("npcs_area2", m_npcs_area2);
        WriteHashSet("npcs_area3", m_npcs_area3);

        WriteHashSet("npcs_area1_area2", m_npcs_area1_area2);
        WriteHashSet("npcs_area1_area3", m_npcs_area1_area3);
        WriteHashSet("npcs_area2_area3", m_npcs_area2_area3);

        WriteHashSet("npcs_area1_area2_area3", m_npcs_area1_area2_area3);
    }

    private void CompareSets(HashSet<string> _sourceA, HashSet<string> _sourceB, HashSet<string> _sourceC, 
                             HashSet<string> _destA, HashSet<string> _destB, HashSet<string> _destC,
                             HashSet<string> _destAll) {

        foreach (string npc in _sourceA) {
            bool dupInB = _sourceB.Contains(npc);
            bool dupInC = _sourceC.Contains(npc);

            if (dupInB && dupInC)   _destAll.Add(npc);
            else if (dupInC)        _destC.Add(npc);
            else if (dupInB)        _destB.Add(npc);
            else                    _destA.Add(npc);
        }
    }

    private void WriteHashSet(string _fileName, HashSet<string> _set) {
        List<string> sorted = _set.ToList();
        sorted.Sort();

        foreach (string npc in sorted) {
            string path = "Assets/Resources/" + IEntity.ENTITY_PREFABS_PATH + npc + ".prefab";
            AssetImporter ai = AssetImporter.GetAtPath(path);
            ai.SetAssetBundleNameAndVariant(_fileName, "");
        }
        /*
        m_sw = new StreamWriter(_fileName+".txt", false);
        m_sw.AutoFlush = true;



        foreach (string npc in sorted) {
            m_sw.WriteLine(npc);
        }
        m_sw.Close();*/
    }
}
