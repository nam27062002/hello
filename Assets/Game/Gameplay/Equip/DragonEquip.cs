using UnityEngine;
using System.Collections.Generic;

public class DragonEquip : MonoBehaviour {

	void Start() {
		DragonPlayer dragon = GetComponent<DragonPlayer>();

		Dictionary<Equipable.AttachPoint, string> equip = dragon.data.equip;
		AttachPoint[] points = GetComponentsInChildren<AttachPoint>();

		// Equip items and Pets
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

		// Change skin if there is any custom available
		if (equip.ContainsKey(Equipable.AttachPoint.Skin)) {

		}
	}
}
