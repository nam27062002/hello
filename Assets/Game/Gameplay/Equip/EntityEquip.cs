using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityEquip : MonoBehaviour {

    [Serializable]
    public class Item {
        public string equipablePrefabName;
        public GameObject[] toDisableOnEquip = new GameObject[0];

        [Separator]
        [Range(0f, 1f)]public float probability = 1f;
        [SeasonList]
        public string season = "";

        [Separator]
        public Vector3 position = GameConstants.Vector3.zero;
        public Vector3 scale = GameConstants.Vector3.one;
        public Vector3 rotation = GameConstants.Vector3.zero;
    }

    [SerializeField] private Item[] m_inventory = new Item[0];
    public Item[] inventory { get { return m_inventory; } }

    private AttachPoint[] m_attachPoints = new AttachPoint[(int)Equipable.AttachPoint.Count];
    private List<string> m_equippedSkus = new List<string>();

    void Awake() {
        // Store attach points sorted to match AttachPoint enum
        AttachPoint[] points = GetComponentsInChildren<AttachPoint>();
        for (int i = 0; i < points.Length; i++) {
            m_attachPoints[(int)points[i].point] = points[i];
        }

        //This should happen before all the other scripts
        //Equip time
        for (int i = 0; i < m_inventory.Length; ++i) {
            Item item = m_inventory[i];

            float rnd = UnityEngine.Random.Range(0f, 1f);
            if (rnd <= item.probability) {
                if (string.IsNullOrEmpty(item.season) || item.season.Equals(SeasonManager.activeSeason)) {
                    //this entity must wear this item!
                    GameObject prefabObj = HDAddressablesManager.Instance.LoadAsset<GameObject>(item.equipablePrefabName);

                    GameObject objInstance = Instantiate<GameObject>(prefabObj);
                    Equipable equipable = objInstance.GetComponent<Equipable>();

                    int attackPointIdx = (int)equipable.attachPoint;
                    if (equipable != null && attackPointIdx < m_attachPoints.Length && m_attachPoints[attackPointIdx] != null) {
                        if (m_attachPoints[attackPointIdx].item == null) {
                            m_attachPoints[attackPointIdx].EquipAccessory(equipable, item.position, item.scale, item.rotation);
                            m_equippedSkus.Add(equipable.sku);
                            for (int j = 0; j < item.toDisableOnEquip.Length; ++j) {
                                item.toDisableOnEquip[j].SetActive(false);
                            }
                        }
                    }
                }
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
