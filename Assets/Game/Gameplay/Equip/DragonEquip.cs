using UnityEngine;
using System.Collections.Generic;

public class DragonEquip : MonoBehaviour {

	private string m_dragonSku;

	void Awake() {
		DragonPlayer player = GetComponent<DragonPlayer>();

		if (player != null) {
			m_dragonSku = player.data.def.sku;
		} else {
			MenuDragonPreview preview = GetComponent<MenuDragonPreview>();
			m_dragonSku = preview.sku;
		}

		EquipDisguise(Wardrobe.GetEquipedDisguise(m_dragonSku));

		/* TODO: refractor full equip function
		 Dictionary<Equipable.AttachPoint, string> equip = dragon.data.equip;
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
		}*/
	}

	public void PreviewDisguise(string _disguise) {
		EquipDisguise(_disguise);
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_DISGUISE_CHANGE, OnDisguiseChanged);
	}
	
	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_DISGUISE_CHANGE, OnDisguiseChanged);
	}

	private void OnDisguiseChanged(string _sku) {
		if (m_dragonSku == _sku) {
			EquipDisguise(Wardrobe.GetEquipedDisguise(m_dragonSku));
		}
	}
	
	private void EquipDisguise(string _disguise) {		
		DefinitionNode def = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES, _disguise);

		if (def != null) {
			def = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES_EQUIP, def.GetAsString("equipSet"));
		}

		if (def != null)  {
			SetSkin(def.GetAsString("skin"));
		} else {
			SetSkin(null);
		}
	}

	void SetSkin(string _name) {
		Material bodyMat;
		Material wingsMat;

		if (_name == null || _name.Equals("default") || _name.Equals("")) {
			_name = m_dragonSku;
		}

		bodyMat  = Resources.Load<Material>("Game/Equipable/Skins/" + m_dragonSku + "/" + _name + "_body");
		wingsMat = Resources.Load<Material>("Game/Equipable/Skins/" + m_dragonSku + "/" + _name + "_wings");

		Renderer renderer = GetComponentInChildren<Renderer>();
		Material[] materials = renderer.materials;
		materials[0] = bodyMat;
		// materials[1] = wingsMat;	// TODO(miguel): set it back when propertly done
		renderer.materials = materials;
	}
}
