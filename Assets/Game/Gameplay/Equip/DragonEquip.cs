using UnityEngine;
using System.Collections.Generic;

public class DragonEquip : MonoBehaviour {
	private static readonly string DISGUISE_CHANGE_PS = "PS_DisguiseChange";
	private static readonly string DISGUISE_CHANGE_PS_FOLDER = "Menu";

	private string m_dragonSku;

	public static int m_numPets = 1;

	void Awake() {
		DragonPlayer player = GetComponent<DragonPlayer>();

		if (player != null) {
			m_dragonSku = player.data.def.sku;
		} else {
			MenuDragonPreview preview = GetComponent<MenuDragonPreview>();
			m_dragonSku = preview.sku;
		}

		EquipDisguise(UsersManager.currentUser.GetEquipedDisguise(m_dragonSku));




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

	void Start()
	{

		Dictionary<Equipable.AttachPoint, string> equip = new Dictionary<Equipable.AttachPoint, string>();
		for( int i = 0; i<m_numPets; i++ )
		{
			if ( i == 0 )
				equip.Add(Equipable.AttachPoint.Pet_1+i, "PF_PetMachine");	
			else if ( i == 1 )
				equip.Add(Equipable.AttachPoint.Pet_1+i, "PF_PetGhostBuster");	
			else
				equip.Add(Equipable.AttachPoint.Pet_1+i, "PF_PetArmored");	
		}

		AttachPoint[] points = GetComponentsInChildren<AttachPoint>();
		for (int i = 0; i < points.Length; i++) {
			Equipable.AttachPoint point = points[i].point;
			if (equip.ContainsKey(point)) {
				string item = equip[point];

				string pet = "Game/Equipable/Pets/" + item;
				GameObject prefabObj = Resources.Load<GameObject>(pet);
				GameObject equipable = Instantiate<GameObject>(prefabObj);

				// get equipable object!
				points[i].Equip(equipable.GetComponent<Equipable>());
			}
		}

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
			// Show some FX and cool animation!
			// https://youtu.be/RFqw3xiuSvQ?t=8m45s
			ParticleManager.Spawn(DISGUISE_CHANGE_PS, transform.position + new Vector3(0f, 1.5f, -4f), DISGUISE_CHANGE_PS_FOLDER);	// [AOC] Hardcoded offset! :(
			EquipDisguise(UsersManager.currentUser.GetEquipedDisguise(m_dragonSku));
		}
	}
	
	private void EquipDisguise(string _disguise) {		
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _disguise);

		if (def != null) {
			def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES_EQUIP, def.GetAsString("equipSet"));
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

		// [AOC] HACK!! Older dragons still don't have the proper materials ----
		// 		 To be removed
		if(m_dragonSku != "dragon_baby") {
			Renderer renderer = transform.FindChild("view").GetComponentInChildren<Renderer>();
			Material[] materials = renderer.materials;
			if(materials.Length > 0) materials[0] = bodyMat;
			if(materials.Length > 1) materials[1] = wingsMat;
			renderer.materials = materials;
		}
		// ---------------------------------------------------------------------

		Transform view = transform.FindChild("view");
		if ( view != null )
		{
			Renderer[] renderers = view.GetComponentsInChildren<Renderer>();
			for( int i = 0; i<renderers.Length; i++ )
			{
				Renderer r = renderers[i];
				Material[] mats = r.materials;
				for( int j = 0;j<mats.Length; j++ )
				{
					if ( mats[j].shader.name.Contains("Wings") )
					{
						mats[j] = wingsMat;
					}
					else if (mats[j].shader.name.Contains("Dragon"))
					{
						mats[j] = bodyMat;
					}
				}
				r.materials = mats;
			}
		}
	}
}
