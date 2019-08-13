using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EntityEquip))]
public class EntityEquipEditor : Editor {
    EntityEquip m_target = null;
	EntityEquip.WeightedItem[] m_items;
    string[] m_itemSkus;
	AttachPoint[] m_attachPoints;
	GameObject m_itemInstance;
	int m_selectedItem;
    int m_attachPointIndex;

	public void Awake() { 
		m_target = (EntityEquip)target;
        
        //UpdateDATA();

        List<string> itemSkus = new List<string>();
        List<EntityEquip.WeightedItem> items = new List<EntityEquip.WeightedItem>();
		foreach (EntityEquip.Seasons seasons in m_target.seasonalItems) {
			foreach (EntityEquip.WeightedGroup groups in seasons.groups) {
                foreach (EntityEquip.WeightedItem item in groups.items) {
                    items.Add(item);
                    itemSkus.Add(seasons.season + "/" + item.equipablePrefabName);
                }
            }
		}

		m_attachPoints = GetAttachPoints();
		m_items = items.ToArray();
        m_itemSkus = itemSkus.ToArray();
        m_itemInstance = null;
		m_selectedItem = 0;
    }

    private void OnDisable() {
        if (!Application.isPlaying) {

            for (int s = 0; s < m_target.seasonalItems.Count; ++s) {
                // sort groups
                List<EntityEquip.WeightedGroup> groups = m_target.seasonalItems[s].groups;
                groups.Sort(new EntityEquip.CompareWeightedGroup());
              
                for (int g = 0; g < groups.Count; ++g) {
                    List<EntityEquip.WeightedItem> items = groups[g].items;

                    float probFactor = 0;
                    for (int i = 0; i < items.Count; ++i) {
                        probFactor += items[i].probability;
                    }

                    if (probFactor > 0f) {
                        probFactor = 1f / probFactor;
                        for (int i = 0; i < items.Count; ++i) {
                            items[i].probability *= probFactor;
                        }

                        items.Sort(new EntityEquip.CompareWeightedItem());
                    }
                }
            }
        }
    }

    private void OnDestroy() {
		ClearItems();
	}
    	
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();        

        EditorGUILayout.Space();
		EditorGUILayoutExt.Separator("Test items");

        if (GUILayout.Button("Update Item List")) {
            ClearItems();

            List<string> itemSkus = new List<string>();
            List<EntityEquip.WeightedItem> items = new List<EntityEquip.WeightedItem>();
            foreach (EntityEquip.Seasons seasons in m_target.seasonalItems) {
                foreach (EntityEquip.WeightedGroup groups in seasons.groups) {
                    foreach (EntityEquip.WeightedItem item in groups.items) {
                        items.Add(item);
                        itemSkus.Add(seasons.season + "/" + item.equipablePrefabName);
                    }
                }
            }

            m_items = items.ToArray();
            m_itemSkus = itemSkus.ToArray();                        
        }

        EditorGUILayout.Space();

        m_selectedItem = EditorGUILayout.Popup("Item", m_selectedItem, m_itemSkus);
		if(GUILayout.Button("Equip Item"))  {
			ClearItems();

			GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/3D/Gameplay/Equipable/Items/NPC/" + m_items[m_selectedItem].equipablePrefabName + ".prefab");

			m_itemInstance = Instantiate<GameObject>(prefabObj);
			m_itemInstance.hideFlags = HideFlags.HideAndDontSave;

			Equipable equipable = m_itemInstance.GetComponent<Equipable>();

            m_attachPointIndex = (int)equipable.attachPoint;
			if (equipable != null && m_attachPointIndex < m_attachPoints.Length && m_attachPoints[m_attachPointIndex] != null) {
				if (m_attachPoints[m_attachPointIndex].item == null) {
                    foreach(GameObject go in m_items[m_selectedItem].toDisableOnEquip) {
                        go.SetActive(false);
                    }

					m_attachPoints[m_attachPointIndex].EquipAccessory(equipable,
                                                                m_items[m_selectedItem].position,
                                                                m_items[m_selectedItem].scale,
                                                                m_items[m_selectedItem].rotation);
				}
			}
		}
        
        if (GUILayout.Button("Clear Items")) {
            ClearItems();
        }
    }

    void OnSceneGUI() {
        if (m_itemInstance != null) {
            Vector3 position = m_attachPoints[m_attachPointIndex].transform.TransformPoint(m_items[m_selectedItem].position);

            switch (Tools.current) {
                case Tool.Move: position = Handles.PositionHandle(position, m_attachPoints[m_attachPointIndex].transform.rotation); break;
                case Tool.Scale: m_items[m_selectedItem].scale = Handles.ScaleHandle(m_items[m_selectedItem].scale, position, m_itemInstance.transform.rotation, 0.25f); break;
                case Tool.Rotate: m_items[m_selectedItem].rotation = Handles.RotationHandle(Quaternion.Euler(m_items[m_selectedItem].rotation), position).eulerAngles; break;
            }
                        
            m_itemInstance.transform.localPosition = m_attachPoints[m_attachPointIndex].transform.InverseTransformPoint(position);
            m_itemInstance.transform.localScale = m_items[m_selectedItem].scale;
            m_itemInstance.transform.localRotation = Quaternion.Euler(m_items[m_selectedItem].rotation);

            m_items[m_selectedItem].position = m_itemInstance.transform.localPosition;
        }
    }

    private void ClearItems() {
        foreach (GameObject go in m_items[m_selectedItem].toDisableOnEquip) {
            go.SetActive(true);
        }
        foreach (AttachPoint ap in m_attachPoints) {
			if (ap != null) {
				ap.Unequip(true);
			}
		}
		m_itemInstance = null;
	}

	private AttachPoint[] GetAttachPoints() {
		AttachPoint[] attachPoints = new AttachPoint[(int)Equipable.AttachPoint.Count];
		AttachPoint[] points = m_target.GetComponentsInChildren<AttachPoint>();
		for (int i = 0; i < points.Length; i++) {
			attachPoints[(int)points[i].point] = points[i];
		}
		return attachPoints;
	}

    /*
    private void UpdateDATA() {
        if (m_target.inventory.Length > 0
        && (m_target.seasonalItems == null || m_target.seasonalItems.Count == 0)) {
            // move old inventory to new system.
            Dictionary<string, List<EntityEquip.WeightedGroup>> itemsPerSeason = new Dictionary<string, List<EntityEquip.WeightedGroup>>();
            foreach (EntityEquip.Item item in m_target.inventory) {
                List<EntityEquip.WeightedGroup> groups;
                EntityEquip.WeightedGroup group;
                if (itemsPerSeason.ContainsKey(item.season)) {
                    groups = itemsPerSeason[item.season];
                    group = groups[0];
                } else {
                    groups = new List<EntityEquip.WeightedGroup>();
                    group = new EntityEquip.WeightedGroup {
                        name = "import"
                    };
                    groups.Add(group);
                    itemsPerSeason[item.season] = groups;
                }

                EntityEquip.WeightedItem wItem = new EntityEquip.WeightedItem();

                wItem.equipablePrefabName = item.equipablePrefabName;
                wItem.probability = item.probability;
                foreach (GameObject go in item.toDisableOnEquip) {
                    wItem.toDisableOnEquip.Add(go);
                }

                wItem.position = item.position;
                wItem.rotation = item.rotation;
                wItem.scale = item.scale;

                group.items.Add(wItem);
            }

            List<EntityEquip.Seasons> seasonalItems = new List<EntityEquip.Seasons>();
            foreach (string key in itemsPerSeason.Keys) {
                EntityEquip.Seasons item = new EntityEquip.Seasons();
                item.season = key;
                item.groups = itemsPerSeason[key];
                seasonalItems.Add(item);
            }
            m_target.seasonalItems = seasonalItems;
        }
    }*/
}
