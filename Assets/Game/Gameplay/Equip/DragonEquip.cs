using UnityEngine;
using System.Collections.Generic;

public class DragonEquip : MonoBehaviour {
	private DragonPlayer m_dragon;

	void Start() {
		m_dragon = GetComponent<DragonPlayer>();
		Dictionary<Equipable.AttachPoint, string> equip = m_dragon.data.equip;

		// Change skin if there is any custom available
		if (equip.ContainsKey(Equipable.AttachPoint.Skin)) {
			SetSkin(equip[Equipable.AttachPoint.Skin]);
		} else {
			SetSkin(null);
		}
			
		// Equip items and Pets
		AttachPoint[] points = GetComponentsInChildren<AttachPoint>();
		for (int i = 0; i < points.Length; i++) {
			Equipable.AttachPoint point = points[i].point;
			if (equip.ContainsKey(point)) {
				string item = equip[point];

				GameObject prefabObj = Resources.Load<GameObject>(item);
				GameObject equipable = Instantiate<GameObject>(prefabObj);

				// get equipable object!
				points[i].Equip(equipable.GetComponent<Equipable>());
			}
		}
	}

	void SetSkin(string _name) {
		Material bodyMat;
		Material wingsMat;

		if (_name == null) {
			_name = m_dragon.data.def.sku;
			bodyMat  = Resources.Load("Game/Assets/Dragons/" + _name + "/Materials/" + _name + "_Body", typeof(Material)) as Material;
			wingsMat = Resources.Load("Game/Assets/Dragons/" + _name + "/Materials/" + _name + "_Wings", typeof(Material)) as Material;
		} else {
			bodyMat  = Resources.Load("Game/Equipable/Skins/" + _name + "/" + _name + "_Body", typeof(Material)) as Material;
			wingsMat = Resources.Load("Game/Equipable/Skins/" + _name + "/" + _name + "_Wings", typeof(Material)) as Material;
		}

		Renderer renderer = GetComponentInChildren<Renderer>();
		Material[] materials = renderer.materials;
		materials[0] = bodyMat;
		materials[1] = wingsMat;
		renderer.materials = materials;
	}
}
