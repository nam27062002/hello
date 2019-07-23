using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EntityEquip))]
public class EntityEquipEditor : Editor {

	EntityEquip m_target = null;
	string[] m_items;
	AttachPoint[] m_attachPoints;
	GameObject m_itemInstance;
	int m_selectedItem;

	public void Awake() {		
		m_target = (EntityEquip)target;

		List<string> items = new List<string>();
		foreach (EntityEquip.Item item in m_target.inventory) {
			items.Add(item.equipablePrefabName);
		}

		m_attachPoints = GetAttachPoints();
		m_items = items.ToArray();
		m_itemInstance = null;
		m_selectedItem = 0;
	}

	private void OnDestroy() {
		ClearItems();
	}

	
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		EditorGUILayout.Space();
		EditorGUILayoutExt.Separator("Test items");
		
		m_selectedItem = EditorGUILayout.Popup("Item", m_selectedItem, m_items);
		if(GUILayout.Button("Equip Item"))  {	
			ClearItems();

			GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/3D/Gameplay/Equipable/Items/NPC/" + m_items[m_selectedItem] + ".prefab");

			m_itemInstance = Instantiate<GameObject>(prefabObj);
			m_itemInstance.hideFlags = HideFlags.HideAndDontSave;

			Equipable equipable = m_itemInstance.GetComponent<Equipable>();
			
			int attackPointIdx = (int)equipable.attachPoint;
			if (equipable != null && attackPointIdx < m_attachPoints.Length && m_attachPoints[attackPointIdx] != null) {
				if (m_attachPoints[attackPointIdx].item == null) {
					m_attachPoints[attackPointIdx].EquipAccessory(equipable, 
																m_target.inventory[m_selectedItem].position,
																m_target.inventory[m_selectedItem].scale,
																m_target.inventory[m_selectedItem].rotation);
				}
			}
		}

		if (GUILayout.Button("Clear Items")) {
			ClearItems();
		}

		if (m_itemInstance != null) {
			m_itemInstance.transform.localPosition = m_target.inventory[m_selectedItem].position;
			m_itemInstance.transform.localScale = m_target.inventory[m_selectedItem].scale;
			m_itemInstance.transform.localRotation = Quaternion.Euler(m_target.inventory[m_selectedItem].rotation);
		}
    }

	private void ClearItems() {
		foreach(AttachPoint ap in m_attachPoints) {
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

}
