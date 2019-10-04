// MenuDragonLoader.cs
// Hungry Dragon
//
// Created by Alger Ortín Castellví on 25/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Helper class to easily instantiate dragon models within the menu.
/// </summary>
public class MenuDragonLoader : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Mode {
		CURRENT_DRAGON,		// Automatically loads and updates CURRENT dragon (DragonManager.currentDragon)
		SELECTED_DRAGON,	// Automatically loads and updates SELECTED dragon (MenuSceneController.selectedDragon)
		MANUAL,				// Manual control via the LoadDragon() method and the exposed m_dragonSku parameter
        TOURNAMENT
	}

    //------------------------------------------------------------------//
    // INTERNAL CLASSES 												//
    //------------------------------------------------------------------//

    [System.Serializable]
    public class MenuDragonLoaderEvent : UnityEvent<MenuDragonLoader> { }

    //------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES											//
    //------------------------------------------------------------------//
    // Exposed setup
    [SerializeField] private Mode m_mode = Mode.MANUAL;
	public Mode mode {
		get { return m_mode; }
		set {
			m_mode = value;
			RefreshDragon();
		}
	}
	
	[SkuList(DefinitionsCategory.DRAGONS, true)]
	[SerializeField] private string m_dragonSku = "";
	public string dragonSku {
		get { return m_dragonSku; }
		set {
			m_dragonSku = value;
			RefreshDragon();
		}
	}

	[SerializeField] private string m_disguiseSku = "";
	public string disguiseSku {
		get { return m_disguiseSku; }
		set {
			m_disguiseSku = value;
			RefreshDragon();
		}
	}

	[Space]
	[HideEnumValues(false, true)]
	[SerializeField] private MenuDragonPreview.Anim m_anim = MenuDragonPreview.Anim.IDLE;
	public MenuDragonPreview.Anim anim {
		get { return m_anim; }
		set { m_anim = value; }
	}

	[SerializeField] private bool m_resetDragonScale = true;
	public bool resetDragonScale {
		get { return m_resetDragonScale; }
		set { m_resetDragonScale = value; }
	}

	[SerializeField] private bool m_showPets = false;
	public bool showPets {
		get { return m_showPets; }
		set { m_showPets = value; }
	}

	[SerializeField] private bool m_hideResultsEquipment = false;
	public bool hideResultsEquipment {
		get { return m_hideResultsEquipment; }
		set { m_hideResultsEquipment = value; }
	}

	[SerializeField] private bool m_removeFresnel = false;
	public bool removeFresnel {
		get { return m_removeFresnel; }
		set { m_removeFresnel = value; }
	}

	[SerializeField] private bool m_keepLayers = false;
	public bool keepLayers {
		get { return m_keepLayers; }
		set { m_keepLayers = value; }
	}

	[SerializeField] private bool m_useResultsScreen = false;
	public bool useResultsScreen {
		get { return m_useResultsScreen; }
		set { m_useResultsScreen = value; }
	}

	[SerializeField] private bool m_allowAltAnimations = true;
	public bool allowAltAnimations {
		get { return m_allowAltAnimations; }
		set { m_allowAltAnimations = value; }
	}

	[SerializeField] private int m_altAnimationsMaxLevel = 10;
	public int altAnimationsMaxLevel {
		get { return m_altAnimationsMaxLevel; }
		set { m_altAnimationsMaxLevel = value; }
	}

	public bool m_loadAsync = false;
	private AddressablesOp m_asyncRequest = null;
    private string m_loadingPrefabName;

	private bool m_useShadowMaterial = false;
	public bool useShadowMaterial {
		get { return m_useShadowMaterial; }
		set {
			m_useShadowMaterial = value;
			if (m_dragonInstance)
				RefreshDragon();
		}
	}

    [Space(10)]
    public MenuDragonLoaderEvent OnCompleteLoad;

	// Debug
	[SkuList(DefinitionsCategory.DRAGONS, false)]
	[SerializeField] private string m_placeholderDragonSku = "dragon_classic";	// If the game is not running, we don't have any data on current dragon/skin, so load a placeholder one manually instead

	// Internal
	private MenuDragonPreview m_dragonInstance = null;    
	public MenuDragonPreview dragonInstance {
		get { return m_dragonInstance; }
	}

	public delegate void OnDragonLoaded( MenuDragonLoader loader );
	public OnDragonLoaded onDragonLoaded;

	private bool m_configured = false;
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialiation.
	/// </summary>
	private void Awake() {
		// Try to find out already instantiated previews of the dragon
		m_dragonInstance = this.GetComponentInChildren<MenuDragonPreview>();
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initialize loaded dragon (unless using MANUAL mode)
        if (m_mode != Mode.MANUAL) {         
            RefreshDragon();
        } else {
            if (m_dragonInstance != null) {
                m_dragonInstance.SetAnim(m_anim);
            }
        }
	}


	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Configure the loader with a specific setup.
	/// Doesn't update the current view, if any.
	/// Use either LoadDragon() or RefreshDragon() for that.
	/// </summary>
	/// <param name="_mode">Dragon loading mode.</param>
	/// <param name="_initialAnim">Initial dragon animation.</param>
	/// <param name="_resetScale">Whether to respect dragon's prefab original scale or reset it.</param>
	public void Setup(Mode _mode, MenuDragonPreview.Anim _initialAnim, bool _resetScale) {
		// Store new setup
		m_mode = _mode;
		m_anim = _initialAnim;
		m_resetDragonScale = _resetScale;
	}

	/// <summary>
	/// Load the dragon with the given sku and its default disguise.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be loaded</param>
	public void LoadDragon(string _sku) {
		LoadDragon(_sku, string.Empty);
	}

	/// <summary>
	/// Load the dragon with the given sku.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be loaded</param>
	/// <param name="_disguiseSku">The sku of the disguise to be applied to this dragon.</param>
	public void LoadDragon(string _sku, string _disguiseSku, bool forceSync = false) {

		//Debug.Log("<color=red>Load Dragon: " + _sku + "</color>");
		if (m_dragonInstance != null || m_asyncRequest != null){
			if (_sku == m_dragonSku && _disguiseSku == m_disguiseSku )
			{
				if ( m_asyncRequest == null && !m_configured )
				{
					ConfigureInstance( m_dragonInstance.gameObject);
				}
                if (onDragonLoaded != null)
                    onDragonLoaded(this);
				return;
			}
		}

		// Unload current dragon if any
		UnloadDragon();

		// Update dragon and disguise skus
		m_dragonSku = _sku;
		m_disguiseSku = _disguiseSku;
                
		// Load selected dragon
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _sku);
		if(def != null) {
			string prefabColumn = "menuPrefab";
			if (  m_useResultsScreen )
				prefabColumn = "resultsPrefab";

            m_loadingPrefabName = "";
            if ( def.Get("type") == "special" )
            {
                // TODO: Change this and use a proper tier
                DefinitionNode specialTierDef = DragonDataSpecial.GetBaseDragonTierDef(_sku);
                m_loadingPrefabName = specialTierDef.GetAsString(prefabColumn);
            }
            else
            {
                m_loadingPrefabName = def.GetAsString(prefabColumn);
            }

			if (m_loadAsync && !forceSync && FeatureSettingsManager.MenuDragonsAsyncLoading){
                m_asyncRequest = GetLoadDragonDependenciesOp();              
            }
            else{
				// Instantiate the prefab and add it as child of this object               
				GameObject dragonPrefab = null;

                dragonPrefab = HDAddressablesManager.Instance.LoadAsset<GameObject>(GetPrefabAddressableSku());

                if (dragonPrefab != null) {
					GameObject newInstance = GameObject.Instantiate<GameObject>(dragonPrefab);                
                	ConfigureInstance( newInstance);
					if (onDragonLoaded != null)
						onDragonLoaded(this);
				}
			}
		}
	}

    private string GetPrefabAddressableSku() {
        return (HDAddressablesManager.Instance.ExistsResource(m_loadingPrefabName, null)) ? m_loadingPrefabName: "Game/Dragons/" + m_loadingPrefabName;
    }
    
    private AddressablesOp GetLoadDragonDependenciesOp() {
        string prefabAddressableSku = GetPrefabAddressableSku();
        string acAddressableSku = prefabAddressableSku.Replace("PF_", "AC_");

        List<string> addressableIds = new List<string>() { prefabAddressableSku, acAddressableSku, };
        List<string> dependencyIds = HDAddressablesManager.Instance.GetDependencyIdsList(addressableIds);
        return HDAddressablesManager.Instance.LoadDependencyIdsListAsync(dependencyIds);
    }

	public void Reload( bool forceSync = false ){
        Debug.Log("MenuDragonLoader.Reload(): m_dragonSku = " + m_dragonSku + " m_disguiseSku = " + m_disguiseSku);
		LoadDragon( m_dragonSku, m_disguiseSku, forceSync );
	}

	void Update()
	{
		if ( m_asyncRequest != null && m_asyncRequest.isDone)
		{
            GameObject go = HDAddressablesManager.Instance.LoadAsset<GameObject>(GetPrefabAddressableSku());
            GameObject newInstance = GameObject.Instantiate<GameObject>( go );
                        
            ConfigureInstance( newInstance );

            m_asyncRequest = null;


            if (onDragonLoaded != null)
				onDragonLoaded(this);

            if (OnCompleteLoad != null)
                OnCompleteLoad.Invoke(this);
		}
	}

	public void ConfigureInstance(GameObject newInstance) {

		m_configured = true;

        Animator animator = newInstance.FindComponentRecursive<Animator>();
        if (animator != null) {
            if (animator.runtimeAnimatorController == null) {
                string acName = m_loadingPrefabName.Replace("PF_", "AC_");
                animator.runtimeAnimatorController = HDAddressablesManager.Instance.LoadAsset<RuntimeAnimatorController>(acName);
            }
        }

        newInstance.transform.SetParent(this.transform, false);
		newInstance.transform.localPosition = Vector3.zero;
		newInstance.transform.localRotation = Quaternion.identity;

        Debug.Log("MenuDragonLoader.ConfigureInstance() : GameObject.name = " + newInstance.name + " m_loadingPrefabName = " + m_loadingPrefabName + " animator.runtimeAnimatorController = " + ((animator.runtimeAnimatorController != null) ? animator.runtimeAnimatorController.name: "null"));

		// Keep layers?
		if(!m_keepLayers) {
			newInstance.SetLayerRecursively(this.gameObject.layer);
		}

		// Store dragon preview and launch the default animation
		m_dragonInstance = newInstance.GetComponent<MenuDragonPreview>();
		m_dragonInstance.SetAnim(m_anim);

        // Reset scale if required
        if (m_resetDragonScale) {
			m_dragonInstance.transform.localScale = Vector3.one * m_dragonInstance.scaleModifier;
		}

		// Apply offset modifier
		m_dragonInstance.transform.localPosition += m_dragonInstance.offsetModifier;

		// Apply equipment
		DragonEquip equip = m_dragonInstance.GetComponent<DragonEquip>();
		if(equip != null) {
            if (!Application.isPlaying) {
                equip.Init();
            }

            equip.EquipPets(m_mode == Mode.TOURNAMENT);

            // Apply disguise (if any)
            if (!string.IsNullOrEmpty(m_disguiseSku) && equip.dragonDisguiseSku != m_disguiseSku) {
                equip.EquipDisguise(m_disguiseSku);
            }

            // Toggle pets
            equip.TogglePets(m_showPets, false);

            if (m_hideResultsEquipment)
                equip.HideResultsEquipment();
        }
		
		// Remove fresnel if required
		if(m_removeFresnel) {
			m_dragonInstance.SetFresnelColor(Color.black);
		}

		// Apply shadow material if required
		if(m_useShadowMaterial) {
			m_dragonInstance.equip.EquipDisguiseShadow();
		}

		// Allow alt animations?
		m_dragonInstance.allowAltAnimations = m_allowAltAnimations;
		m_dragonInstance.altAnimationsMaxLevel = m_altAnimationsMaxLevel;

		// Make sure particles are properly scaled as well
		RescaleParticles();
		UbiBCN.CoroutineManager.DelayedCallByFrames(() => { RescaleParticles(); }, 2);	// In case some initial scale transformation is performed during this frame (i.e. child of a UI3DScaler)
	}

	public void RescaleParticles()
	{
		if ( m_dragonInstance )
		{
			ParticleScaler[] scalers = m_dragonInstance.GetComponentsInChildren<ParticleScaler>();
			for( int i = 0;i<scalers.Length; ++i )
			{
				scalers[i].DoScale();
			}
		}
	}

	/// <summary>
	/// Reload dragon preview based on mode.
	/// </summary>
	public void RefreshDragon(bool _force = false) {
		// Force?
		string currentDragonSku = m_dragonSku;
		string currentDisguiseSku = m_disguiseSku;
		if(_force) {
			m_dragonSku = "";
			m_disguiseSku = "";
		}

		// Load different dragons based on mode
		// If the game is not running, we don't have any data on current dragon/skin,
		// so load a placeholder one manually instead
		switch(m_mode) {
			case Mode.CURRENT_DRAGON: {
				if(Application.isPlaying) {
					LoadDragon(DragonManager.CurrentDragon.sku, DragonManager.CurrentDragon.disguise);
				} else {
					LoadDragon(m_placeholderDragonSku, string.Empty, true);
				}
			} break;

			case Mode.SELECTED_DRAGON: {
				if(Application.isPlaying) {
					LoadDragon(InstanceManager.menuSceneController.selectedDragon);
				} else {
					LoadDragon(m_placeholderDragonSku, string.Empty, true);
				}
			} break;

			case Mode.MANUAL: {
				LoadDragon(currentDragonSku, currentDisguiseSku, !Application.isPlaying);
			} break;

            case Mode.TOURNAMENT: {
				if(Application.isPlaying) {
					IDragonData dragonData = HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData;
					LoadDragon(dragonData.sku);
				} else {
					LoadDragon(m_placeholderDragonSku, string.Empty, true);
				}
            } break;
        }
	}

	/// <summary>
	/// Destroy current loaded dragon, if any.
	/// </summary>
	public void UnloadDragon() {
		// Just make sure the object doesn't have anything attached
		m_asyncRequest = null;
        m_dragonInstance = null;
		m_configured = false;
		foreach(Transform child in transform) {
			if(Application.isPlaying) {
				Destroy(child.gameObject);   // Immediate so it can be called from the editor
			} else {
				DestroyImmediate(child.gameObject);
			}
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	public void SetViewPosition( Vector3 position ){
		Transform _viewTransform = m_dragonInstance.transform.Find("view");
		Vector3 diff = position - _viewTransform.position;
		m_dragonInstance.transform.position = m_dragonInstance.transform.position + diff;
	}
}
