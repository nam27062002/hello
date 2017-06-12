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
	public string dragonSku
	{
		get{ return m_dragonSku; }
		set{ m_dragonSku = value; }
	}
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
		Init();
		// Equip current disguise
		EquipDisguise(UsersManager.currentUser.GetEquipedDisguise(m_dragonSku));
	}

	public void Init()
	{
		// Get assigned dragon sku - from Player for in-game dragons, from DragonPreview for menu dragons
		DragonPlayer player = GetComponent<DragonPlayer>();
		if(player != null && player.data != null) {
			m_dragonSku = player.data.def.sku;
		} else {
			MenuDragonPreview preview = GetComponent<MenuDragonPreview>();
			if ( preview != null )
				m_dragonSku = preview.sku;
		}

		// Store attach points sorted to match AttachPoint enum
		AttachPoint[] points = GetComponentsInChildren<AttachPoint>();
		for(int i = 0; i < points.Length; i++) {
			m_attachPoints[(int)points[i].point] = points[i];
		}

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

		// Remove old body parts
		for( int i = 0; i<m_attachPoints.Length; i++ )
		{
			if ( i > (int) Equipable.AttachPoint.Pet_5 && m_attachPoints[i] != null)
			{
				m_attachPoints[i].Unequip(true);
			}
		}

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

		/*
		// THIS IS JUST A TEST! - DO NOT DELETE FOR THE MOMMENT
		Transform view = transform.FindChild("view");
		if(view != null) {
			SkinnedMeshRenderer skinMesh = view.GetComponentInChildren<SkinnedMeshRenderer>();
			List<MeshFilter> meshFilters = new List<MeshFilter>();
			List<string> meshAnchors = new List<string>();

			for( int i = 0; i<m_attachPoints.Length; i++ )
			{
				if ( m_attachPoints[i] != null && m_attachPoints[i].item != null && m_attachPoints[i].item.attachPoint > Equipable.AttachPoint.Pet_5 && m_attachPoints[i].item.attachPoint < Equipable.AttachPoint.Count)
				{
					// its something!
					MeshFilter mfilter = m_attachPoints[i].item.GetComponentInChildren<MeshFilter>();
					if ( mfilter != null )
					{
						mfilter.gameObject.SetActive(false);
						meshFilters.Add( mfilter );
						meshAnchors.Add( m_attachPoints[i].GetComponent<AutoParenter>().parentName );
					}
				}
			}

			skinMesh.sharedMesh = CustomCombine( skinMesh, meshFilters, meshAnchors);
		}
		*/

	}


	Mesh CustomCombine( SkinnedMeshRenderer skinnedMesh, List<MeshFilter> parts, List<string> bonesAnchors)
	{
			// Bone Weigth
		List<BoneWeight> boneWeights = new List<BoneWeight>();
		boneWeights.AddRange( skinnedMesh.sharedMesh.boneWeights );

			// bind poses
		List<Matrix4x4> bindposes = new List<Matrix4x4>();
		for( int i = 0; i < skinnedMesh.bones.Length; i++ ) {
			bindposes.Add( skinnedMesh.bones[i].worldToLocalMatrix );
        }

        	// uvs
        	/*
		List<Vector2> uvs = new List<Vector2>();
        uvs.AddRange( skinnedMesh.sharedMesh.uv );
        */

        	// Textures
        List<Texture2D> mainTexture = new List<Texture2D>();
        mainTexture.Add( skinnedMesh.material.mainTexture as Texture2D);
		List<Texture2D> normalTexture = new List<Texture2D>();
		normalTexture.Add( skinnedMesh.material.GetTexture("_BumpMap") as Texture2D);
		List<Texture2D> specialTexture = new List<Texture2D>();
		specialTexture.Add( skinnedMesh.material.GetTexture("_DetailTex") as Texture2D );

        Transform[] bones = skinnedMesh.bones;
		for( int i = 0; i<parts.Count; i++ )
		{
			MeshFilter part = parts[i];

			// Check if materials are different
			Renderer r = part.GetComponent<Renderer>();
			mainTexture.Add( r.material.mainTexture as Texture2D );

			// Search proper bone
			bool found = false;
			string boneId = bonesAnchors[i];
			BoneWeight bWeight = new BoneWeight();
			for( int j = 0; j<bones.Length && !found; j++ )
        	{
				if ( bones[j].name.CompareTo( boneId ) == 0)
        		{
        			bWeight.boneIndex0 = j;
        			bWeight.weight0 = 1;
        			found = true;
        		}
        	}
        	// Add weights
			for( int j = 0; j<part.mesh.vertexCount; j++ ) 
			{
	    		boneWeights.Add(bWeight);
			}

			// uvs.AddRange( combine[x].mesh.uv );
		}

		CombineInstance[] combine = new CombineInstance[ parts.Count + 1];
		int[] meshIndex = new int[parts.Count + 1];

		combine[0].mesh = skinnedMesh.sharedMesh;
		combine[0].transform = skinnedMesh.transform.localToWorldMatrix;
		meshIndex[0] = skinnedMesh.sharedMesh.vertexCount;

		for( int i = 0; i<parts.Count; i++ )
		{
			combine[i+1].mesh = parts[i].sharedMesh;
			combine[i+1].transform = parts[i].transform.localToWorldMatrix;
			meshIndex[i+1] = parts[i].sharedMesh.vertexCount;
		}


		Mesh _newMesh = new Mesh();
		_newMesh.CombineMeshes(combine, true, true);
		_newMesh.boneWeights = boneWeights.ToArray();
		_newMesh.bindposes = bindposes.ToArray();
		// _newMesh.uv = uvs.ToArray();

			// Reshape UVs
		Texture2D baseTexture = mainTexture[0];
		Texture2D skinnedMeshAtlas = new Texture2D( baseTexture.width, baseTexture.height, baseTexture.format, baseTexture.mipmapCount > 0);
		Rect[] packingResult = skinnedMeshAtlas.PackTextures( mainTexture.ToArray(), 0 );

		Vector2[] originalUVs = _newMesh.uv;
        Vector2[] atlasUVs = new Vector2[originalUVs.Length];
 
        int rectIndex = 0;
        int vertTracker = 0;
        for( int i = 0; i < atlasUVs.Length; i++ ) {
            atlasUVs[i].x = Mathf.Lerp( packingResult[rectIndex].xMin, packingResult[rectIndex].xMax, originalUVs[i].x );
            atlasUVs[i].y = Mathf.Lerp( packingResult[rectIndex].yMin, packingResult[rectIndex].yMax, originalUVs[i].y );            
 
            if( i >= meshIndex[rectIndex] + vertTracker ) {                
                vertTracker += meshIndex[rectIndex];
                rectIndex++;                
            }
        }
        _newMesh.uv = atlasUVs;
        skinnedMesh.material.mainTexture = skinnedMeshAtlas;

		return _newMesh;
	} 





	/// <summary>
	/// Sets the skin of the dragon. Performs the actual texture swap.
	/// </summary>
	/// <param name="_name">Name of the skin to be applied.</param>
	public void SetSkin(string _name) {

		// Texture change
		if(_name == null || _name.Equals("default") || _name.Equals("")) {
			_name = m_dragonSku + "_0";		// Default skin, all dragons should have it
		}

		m_bodyMaterial = Resources.Load<Material>(SKIN_PATH + m_dragonSku + "/" + _name + "_body");
		m_wingsMaterial = Resources.Load<Material>(SKIN_PATH + m_dragonSku + "/" + _name + "_wings");

		Transform view = transform.FindChild("view");
		if(view != null) {
			Renderer[] renderers = view.GetComponentsInChildren<Renderer>();
			for(int i = 0; i < renderers.Length; i++) {
				Renderer r = renderers[i];
				Material[] mats = Application.isPlaying ? r.materials : r.sharedMaterials;
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
				newInstance.transform.localScale = Vector3.one * player.data.def.GetAsFloat("petScale");
				// newInstance.transform.localScale = Vector3.one * player.data.scale;
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
			CoroutineManager.DelayedCall(
				() => {
					EquipDisguise(UsersManager.currentUser.GetEquipedDisguise(m_dragonSku));
				},
				0.25f, 
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
