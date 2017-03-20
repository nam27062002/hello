using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class DragonEquip : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string SKIN_PATH = "Game/Equipable/Skins/";
	private const string PET_PREFAB_PATH_GAME = "Game/Equipable/Pets/";
	private const string PET_PREFAB_PATH_MENU = "UI/Menu/Pets/";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private bool m_menuMode = false;

	// Internal
	private string m_dragonSku;
	private AttachPoint[] m_attachPoints = new AttachPoint[(int)Equipable.AttachPoint.Count];
	private bool m_showPets = true;

	// Skins
	private Material m_bodyMaterial;
	public Material bodyMaterial {
		get { return m_bodyMaterial; }
	}

	private Material m_wingsMaterial;
	public Material wingsMaterial {
		get { return m_wingsMaterial; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get assigned dragon sku - from Player for in-game dragons, from DragonPreview for menu dragons
		DragonPlayer player = GetComponent<DragonPlayer>();
		if(player != null) {
			m_dragonSku = player.data.def.sku;
		} else {
			MenuDragonPreview preview = GetComponent<MenuDragonPreview>();
			m_dragonSku = preview.sku;
		}

		// Store attach points sorted to match AttachPoint enum
		AttachPoint[] points = GetComponentsInChildren<AttachPoint>();
		for(int i = 0; i < points.Length; i++) {
			m_attachPoints[(int)points[i].point] = points[i];
		}

		// Equip current disguise
		EquipDisguise(UsersManager.currentUser.GetEquipedDisguise(m_dragonSku));


	}

	private void Start()
	{
		// Equip current pets loadout
		List<string> pets = UsersManager.currentUser.GetEquipedPets(m_dragonSku);
		for(int i = 0; i < pets.Count; i++) {
			EquipPet(pets[i], i);
		}
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_DISGUISE_CHANGE, OnDisguiseChanged);
		Messenger.AddListener<string, int , string>(GameEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_DISGUISE_CHANGE, OnDisguiseChanged);
		Messenger.RemoveListener<string, int, string>(GameEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Equip the disguise with the given sku.
	/// </summary>
	/// <param name="_disguiseSku">The disguise to be equipped.</param>
	public void EquipDisguise(string _disguiseSku) {		
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _disguiseSku);
		if ( def == null)
		{
			def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, m_dragonSku + "_0");	
			if(def == null) return;
		}
		SetSkin( def.Get("skin") );

		// Now body parts!
		List<string> bodyParts = def.GetAsList<string>("body_parts");
		for( int i = 0; i<bodyParts.Count; i++ )
		{
			if ( !string.IsNullOrEmpty(bodyParts[i]) )
			{
				GameObject prefabObj = Resources.Load<GameObject>("Game/Equipable/Items/" + m_dragonSku + "/" + bodyParts[i]);
				if ( prefabObj != null )
				{
					GameObject objInstance = Instantiate<GameObject>(prefabObj);
					Equipable equipable = objInstance.GetComponent<Equipable>();
					int attackPointIdx = (int)equipable.attachPoint;
					if ( equipable != null && attackPointIdx < m_attachPoints.Length )
					{
						if (m_attachPoints[attackPointIdx] != null )
							m_attachPoints[attackPointIdx].EquipAccessory( equipable );
					}
				}
			}
		}

	}

	/// <summary>
	/// Sets the skin of the dragon. Performs the actual texture swap.
	/// </summary>
	/// <param name="_name">Name of the skin to be applied.</param>
	private void SetSkin(string _name) {

		// Texture change
		if(_name == null || _name.Equals("default") || _name.Equals("")) {
			_name = m_dragonSku + "_0";		// Default skin, all dragons should have it
		}

		m_bodyMaterial = Resources.Load<Material>(SKIN_PATH + m_dragonSku + "/" + _name + "_body");
		m_wingsMaterial = Resources.Load<Material>(SKIN_PATH + m_dragonSku + "/" + _name + "_wings");

		// [AOC] HACK!! Older dragons still don't have the proper materials ----
		// 		 To be removed
		if(m_dragonSku != "dragon_baby" && m_dragonSku != "dragon_classic") {
			Renderer renderer = transform.FindChild("view").GetComponentInChildren<Renderer>();
			Material[] materials = renderer.materials;
			if(materials.Length > 0) materials[0] = m_bodyMaterial;
			if(materials.Length > 1) materials[1] = m_wingsMaterial;
			renderer.materials = materials;
		}
		// ---------------------------------------------------------------------

		Transform view = transform.FindChild("view");
		if(view != null) {
			Renderer[] renderers = view.GetComponentsInChildren<Renderer>();
			for(int i = 0; i < renderers.Length; i++) {
				Renderer r = renderers[i];
				Material[] mats = r.materials;
				for(int j = 0; j < mats.Length; j++) {
					if(mats[j].shader.name.Contains("Dragon/Wings")) {
						mats[j] = m_wingsMaterial;
					}
					else if(mats[j].shader.name.Contains("Dragon/Body")) {
						mats[j] = m_bodyMaterial;
					}
				}
				r.materials = mats;
			}
		}

	}

	/// <summary>
	/// Equip a single pet.
	/// </summary>
	/// <param name="_petSku">Pet sku. Use empty string to unequip.</param>
	/// <param name="_slot">Slot index.</param>
	public void EquipPet(string _petSku, int _slot) {
		// Check slot
		int attachPointIdx = (int)Equipable.AttachPoint.Pet_1 + _slot;
		if(attachPointIdx < (int)Equipable.AttachPoint.Pet_1 || attachPointIdx > (int)Equipable.AttachPoint.Pet_5) return;	// [AOC] MAGIC NUMBERS!! Figure out a better way!
		if(m_attachPoints[attachPointIdx] == null) return;

		// Equip or unequip?
		if(string.IsNullOrEmpty(_petSku)) {
			// In the menu, trigger the animation
			if(m_menuMode) {
				// Launch out animation
				if(m_attachPoints[attachPointIdx].item != null) {
					MenuPetPreview pet = m_attachPoints[attachPointIdx].item.GetComponent<MenuPetPreview>();
					pet.SetAnim(MenuPetPreview.Anim.OUT);

					// Program a delayed destruction of the item (to give some time to see the anim)
					GameObject.Destroy(m_attachPoints[attachPointIdx].item.gameObject, 0.3f);	// [AOC] MAGIC NUMBERS!! More or less synced with the animation
				}

				// Unequip
				m_attachPoints[attachPointIdx].Unequip(false);
			} else {
				// Unequip
				m_attachPoints[attachPointIdx].Unequip(true);
			}
		} else {
			// Equip!
			// Get pet definition
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _petSku);
			if(petDef == null) return;

			// Load prefab and instantiate
			string pet = null;
			if(m_menuMode) {
				pet = PET_PREFAB_PATH_MENU + petDef.Get("menuPrefab");
			} else {
				pet = PET_PREFAB_PATH_GAME + petDef.Get("gamePrefab");
			}
			GameObject prefabObj = Resources.Load<GameObject>(pet);
			GameObject newInstance = Instantiate<GameObject>(prefabObj);

			// Adjust scale and parenting
			if(m_menuMode) {
				// In menu mode, make it a child of the dragon so it inherits scale factor
				newInstance.transform.SetParent(m_attachPoints[attachPointIdx].transform, true);	// [AOC] Compensate scale factor with the dragon using the worldPositionStays parameter
				newInstance.transform.localPosition = Vector3.zero;
				newInstance.transform.localRotation = Quaternion.identity;

				// Initialize preview and launch intro animation
				MenuPetPreview petPreview = newInstance.GetComponent<MenuPetPreview>();
				petPreview.sku = _petSku;
				petPreview.SetAnim(MenuPetPreview.Anim.IN);
			} else {
				// In game mode, adjust to dragon's scale factor
				DragonPlayer player = GetComponent<DragonPlayer>();
				newInstance.transform.localScale = Vector3.one * player.data.scale;
			}

			// Get equipable object!
			m_attachPoints[attachPointIdx].EquipPet(newInstance.GetComponent<Equipable>());

			// Apply current pets visibility
			m_attachPoints[attachPointIdx].item.gameObject.SetActive(m_showPets);
		}
	}

	/// <summary>
	/// Show/Hide the pets. Useful specially on the menus.
	/// </summary>
	/// <param name="_show">Whether to show or not the pets.</param>
	/// <param name="_animate">Animation or instant?</param>
	public void TogglePets(bool _show, bool _animate) {
		// Store value
		m_showPets = _show;

		// Iterate through all pet attach points and activate/deactivate them
		for(int i = (int)Equipable.AttachPoint.Pet_1; i < (int)Equipable.AttachPoint.Pet_5; i++) {
			if(m_attachPoints[i] != null) {
				if(m_attachPoints[i].item != null) {
					GameObject item = m_attachPoints[i].gameObject;
					if(_animate) {
						// Launch the right animation
						/*MenuPetPreview pet = m_attachPoints[i].item.GetComponent<MenuPetPreview>();
						if(_show) {
							m_attachPoints[i].item.gameObject.SetActive(true);
							pet.SetAnim(MenuPetPreview.Anim.IN);
						} else {
							// Program a delayed disable of the item (to give some time to see the anim)
							pet.SetAnim(MenuPetPreview.Anim.OUT);
							DOVirtual.DelayedCall(0.3f, () => { pet.gameObject.SetActive(false); }, false);
						}*/

						// Animation is too slow, better do it with a tween
						// [AOC] TODO!! Add some particles?
						DOTween.Kill(item, true);
						Sequence seq = DOTween.Sequence();
						if(_show) {
							item.SetActive(true);
							item.transform.localScale = Vector3.zero;
							seq.Append(item.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
						} else {
							seq.Append(item.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack));
							seq.AppendCallback(() => item.SetActive(false));
						}
						seq.SetTarget(item);
						seq.Play();
					} else {
						item.SetActive(m_showPets);
					}
				}
			}
		}
	}

	/// <summary>
	/// Get one of the attach points for this dragon equip.
	/// </summary>
	/// <returns>The requested attach point, <c>null</c> if not found.</returns>
	/// <param name="_point">The attach point to be found.</param>
	public AttachPoint GetAttachPoint(Equipable.AttachPoint _point) {
		// Array is always initialized to enum's size and properly initialized if Awake has been called
		return m_attachPoints[(int)_point];
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The selected disguise has been changed in the menu.
	/// </summary>
	/// <param name="_sku">The new disguise to be equipped.</param>
	private void OnDisguiseChanged(string _sku) {
		// Is it meant for this dragon?
		if(m_dragonSku == _sku) {
			// Do it with some delay to sync with FX
			// DOTween allows us to do it in a super-easy way
			DOVirtual.DelayedCall(
				0.25f, 
				() => {
					EquipDisguise(UsersManager.currentUser.GetEquipedDisguise(m_dragonSku));
				},
				false
			);
		}
	}

	/// <summary>
	/// The pets loadout has changed in the menu.
	/// </summary>
	/// <param name="_dragonSku">The dragon whose assigned pets have changed.</param>
	/// <param name="_slotIdx">Slot that has been changed.</param>
	/// <param name="_newPetSku">New pet assigned to the slot. Empty string for unequip.</param>
	public void OnPetChanged(string _dragonSku, int _slotIdx, string _newPetSku) {
		// Is it meant for this dragon?
		if(_dragonSku == m_dragonSku) {
			// [AOC] TODO!! Make it look good!
			EquipPet(_newPetSku, _slotIdx);
		}
	}
}
