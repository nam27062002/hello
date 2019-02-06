﻿using System;
using UnityEngine;

public class EntityEquip : MonoBehaviour {

    [Serializable]
    private class Item {
        public string equipablePrefabName;
        public GameObject[] toDisableOnEquip = new GameObject[0];

        [Separator]
        [Range(0f, 1f)]public float probability = 1f;
        public string season = "";
    }

    [SerializeField] private Item[] m_inventory = new Item[0];

    private AttachPoint[] m_attachPoints = new AttachPoint[(int)Equipable.AttachPoint.Count];


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
                    GameObject prefabObj = Resources.Load<GameObject>("Game/Equipable/Items/NPC/" + item.equipablePrefabName);

                    GameObject objInstance = Instantiate<GameObject>(prefabObj);
                    Equipable equipable = objInstance.GetComponent<Equipable>();

                    int attackPointIdx = (int)equipable.attachPoint;
                    if (equipable != null && attackPointIdx < m_attachPoints.Length && m_attachPoints[attackPointIdx] != null) {
                        if (m_attachPoints[attackPointIdx].item == null) {
                            m_attachPoints[attackPointIdx].EquipAccessory(equipable);
                            for (int j = 0; j < item.toDisableOnEquip.Length; ++j) {
                                item.toDisableOnEquip[j].SetActive(false);
                            }
                        }
                    }
                }
            }
        }
    }
}
