using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityEquip : MonoBehaviour {   
    //------------------------------------------------------------
    [Serializable]
    public class WeightedItem {
        public string equipablePrefabName;
        [Range(0f, 1f)] public float probability = 1f;

        public List<GameObject> toDisableOnEquip = new List<GameObject>();

        [Separator]
        public Vector3 position = GameConstants.Vector3.zero;
        public Vector3 scale = GameConstants.Vector3.one;
        public Vector3 rotation = GameConstants.Vector3.zero;
    }
    public class CompareWeightedItem : IComparer<WeightedItem> {
        public int Compare(WeightedItem _l, WeightedItem _r) {
            if (_l.probability > _r.probability) {
                return 1;
            } else if (_l.probability < _r.probability) {
                return -1;
            }

            return 0;
        }
    }

    [Serializable]
    public class WeightedGroup {
        public string name;
        [Range(0f, 1f)] public float probability = 1f;
        public List<WeightedItem> items = new List<WeightedItem>();
    }
    public class CompareWeightedGroup : IComparer<WeightedGroup> {
        public int Compare(WeightedGroup _l, WeightedGroup _r) {
            if (_l.probability > _r.probability) {
                return 1;
            } else if (_l.probability < _r.probability) {
                return -1;
            }

            return 0;
        }
    }

    [Serializable]
    public class Seasons {
        [SeasonList] public string season;
        public List<WeightedGroup> groups = new List<WeightedGroup>();
    }

    [SerializeField] private List<Seasons> m_seasonalItems = new List<Seasons>();
    public List<Seasons> seasonalItems { get { return m_seasonalItems; } set { m_seasonalItems = value; } }
    //------------------------------------------------------------


    private AttachPoint[] m_attachPoints = new AttachPoint[(int)Equipable.AttachPoint.Count];
    private List<string> m_equippedSkus = new List<string>();

    void Awake() {
        // Store attach points sorted to match AttachPoint enum
        AttachPoint[] points = GetComponentsInChildren<AttachPoint>();
        for (int i = 0; i < points.Length; i++) {
            m_attachPoints[(int)points[i].point] = points[i];
        }

        foreach (Seasons season in m_seasonalItems) {
            if (season.season.Equals(SeasonManager.activeSeason)) {                
                foreach (WeightedGroup group in season.groups) {
                    float groupRND = UnityEngine.Random.Range(0f, 1f);
                    if (groupRND <= group.probability) {
                        float itemProbability = 0f;
                        float itemRND = UnityEngine.Random.Range(0f, 1f);
                        foreach (WeightedItem item in group.items) {
                            itemProbability += item.probability;
                            if (itemRND <= itemProbability) {
                                //this entity must wear this item!
                                GameObject prefabObj = HDAddressablesManager.Instance.LoadAsset<GameObject>(item.equipablePrefabName);

                                GameObject objInstance = Instantiate<GameObject>(prefabObj);
                                Equipable equipable = objInstance.GetComponent<Equipable>();

                                int attackPointIdx = (int)equipable.attachPoint;
                                if (equipable != null && attackPointIdx < m_attachPoints.Length && m_attachPoints[attackPointIdx] != null) {
                                    if (m_attachPoints[attackPointIdx].item == null) {
                                        m_attachPoints[attackPointIdx].EquipAccessory(equipable, item.position, item.scale, item.rotation);
                                        m_equippedSkus.Add(equipable.sku);
                                        for (int j = 0; j < item.toDisableOnEquip.Count; ++j) {
                                            item.toDisableOnEquip[j].SetActive(false);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
                break;
            }
        }
    }

    public bool HasSomethingEquiped() {
        return m_equippedSkus.Count > 0;
    }

    public bool HasEquipped(string _sku) {
        for (int i = 0; i < m_equippedSkus.Count; ++i) {
            if (m_equippedSkus[i].Equals(_sku)) {
                return true;
            }
        }
        return false;
    }
}
