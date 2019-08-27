// FontManager.cs
// 
// Created by Alger Ortín Castellví on 07/03/2018
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global font manager to be in charge of font assets loading/unloading logic.
/// Requires LocalizationManager to be initialized.
/// Subscribe to FONT_CHANGE_STARTED and FONT_CHANGE_FINISHED events.
/// </summary>
public class FontManager : UbiBCN.SingletonMonoBehaviour<FontManager>, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum State {
		IDLE,

		LOOSING_REFERENCES,
		UNLOADING_ASSETS,
		GC,
		LOADING_FONT,
		FINISH
	}

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[Tooltip("Resources folder where the fonts are allocated. Must follow the structure \"Resources/.../FNT_Name/FNT_Name.asset\".")]
	[SerializeField] private string m_fontsDir = "";

	// Dummy font to be used while unloading/loading a new font
	[SerializeField] private TMP_FontAsset m_dummyFont = null;
	public TMP_FontAsset dummyFont {
		get { return m_dummyFont; }
	}

	// Current font group
	public static string currentFontGroupSku {
		get { return instance.m_currentFontGroup == null ? "" : instance.m_currentFontGroup.sku; }
	}

	private FontGroup m_currentFontGroup = null;
	public FontGroup currentFontGroup {
		get { return instance.m_currentFontGroup; }
	}

	// [fontName] = fontAsset
	// Will be cleared every time a new font group is loaded
	private Dictionary<string, TMP_FontAsset> m_fontsCache = new Dictionary<string, TMP_FontAsset>();

	// Materials cache
	// [fontName][materialID] = sharedMaterial
	// Shared materials will be loaded on demand and stored for future requests
	// They will be cleared every time a new font group is loaded
	private Dictionary<string, Dictionary<string, Material>> m_sharedMaterialsCache = new Dictionary<string, Dictionary<string, Material>>();

	// [fontName][materialID] = dummyMaterial
	// Dummy materials are materials with no atlas assigned, just properties
	// Persistent, will be used to fallback styles when a specific material is not defined for a specific font
	private Dictionary<string, Dictionary<string, Material>> m_dummyMaterialsCache = new Dictionary<string, Dictionary<string, Material>>();

	// Font defs
	private Dictionary<string, FontGroup> m_fontGroups = new Dictionary<string, FontGroup>();

	// Initialization
	private bool m_ready = false;
	public bool isReady {
		get { return m_ready; }
	}

	// Internal members
	private State m_state = State.IDLE;
	private float m_timer = 0f;

	// Aux
	private StringBuilder m_sb = new StringBuilder();	// Optimize string composition

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake () {
		
	}

	/// <summary>
	/// 
	/// </summary>
	public void Init() {
		// Don't do anything if already initialized
		if(m_ready) return;

		// Load font groups
		List<DefinitionNode> fontGroupsDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.FONT_GROUPS);
		m_fontGroups.Clear();
		for(int i = 0; i < fontGroupsDefs.Count; ++i) {
			m_fontGroups[fontGroupsDefs[i].sku] = new FontGroup(fontGroupsDefs[i]);
		}

		// Initialize fonts cache
		// Create an entry for each font
		m_fontsCache.Clear();
		foreach(KeyValuePair<string, FontGroup> kvp in m_fontGroups) {
			for(int i = 0; i < kvp.Value.fontAssets.Length; ++i) {
				m_fontsCache[kvp.Value.fontAssets[i]] = null;
			}
		}

		// Load default font
		m_currentFontGroup = null;
		DefinitionNode langDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LOCALIZATION, LocalizationManager.SharedInstance.GetCurrentLanguageSKU());
		if(langDef != null) {
			m_currentFontGroup = GetFontGroup(langDef.GetAsString("fontGroup"));
			LoadFontAssets(m_currentFontGroup);
		}

		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);

		// Initial state
		ChangeState(State.IDLE);

		// Done!
		m_ready = true;

		// Simulate font load events to properly initialize any existing FontReplacer component
		Broadcaster.Broadcast(BroadcastEventType.FONT_CHANGE_STARTED);
		Broadcaster.Broadcast(BroadcastEventType.FONT_CHANGE_FINISHED);
	}

	/// <summary>
	/// 
	/// </summary>
	public void Start() {
		
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnDestroy() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}
    
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.LANGUAGE_CHANGED:
            {
                OnLanguageChanged();
            }break;
        }
    }
    

	/// <summary>
	/// 
	/// </summary>
	public void Update() {
		// Update timer
		if(m_timer > 0f) {
			m_timer -= Time.deltaTime;
		}

		// State logic
		switch(m_state) {
			case State.LOOSING_REFERENCES: {
				// Wait for timer to end
				if(m_timer <= 0f) ChangeState(State.UNLOADING_ASSETS);
			} break;

			case State.UNLOADING_ASSETS: {
				// Wait for timer to end
				if(m_timer <= 0f) ChangeState(State.GC);
			} break;

			case State.GC: {
				// Wait for timer to end
				if(m_timer <= 0f) ChangeState(State.LOADING_FONT);
			} break;

			case State.LOADING_FONT: {
				// Nothing to do
				ChangeState(State.FINISH);
			} break;

			case State.FINISH: {
				// Nothing to do
				ChangeState(State.IDLE);
			} break;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_newState">New state.</param>
	private void ChangeState(State _newState) {
		Debug.Log("<color=cyan>" + this.GetType().Name + "</color> " + "Changing state from <color=red>" + m_state + "</color> to <color=green>" + _newState + "</color>");

		switch(_newState) {
			case State.LOOSING_REFERENCES: {
				// Notify game
				Broadcaster.Broadcast(BroadcastEventType.FONT_CHANGE_STARTED);

				// Clear old font reference
				ClearFontAssets(m_currentFontGroup);
				m_currentFontGroup = null;

				// Reset timer
				m_timer = 0.3f;
			} break;

			case State.UNLOADING_ASSETS: {
				// Do it! The old font assets shouldn't be referenced by anyone at this point
				Resources.UnloadUnusedAssets();

				// Reset timer
				m_timer = 0.1f;
			} break;

			case State.GC: {
				// Just in case, the UnloadUnusedAssets() already performs a GC.Collect()
				GC.Collect();

				// Reset timer
				m_timer = 0.1f;
			} break;

			case State.LOADING_FONT: {
				// Get the font group corresponding to the current language
				DefinitionNode langDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LOCALIZATION, LocalizationManager.SharedInstance.GetCurrentLanguageSKU());
				m_currentFontGroup = GetFontGroup(langDef.GetAsString("fontGroup"));

				// Load font assets!
				LoadFontAssets(m_currentFontGroup);
			} break;

			case State.FINISH: {
				// Notify game
				Broadcaster.Broadcast(BroadcastEventType.FONT_CHANGE_FINISHED);
			} break;
		}

		// Save new state
		m_state = _newState;
	}

	/// <summary>
	/// Initialize the font and materials caches with the assets of a given group.
	/// </summary>
	private void LoadFontAssets(FontGroup _fontGroup) {
		Debug.Log("<color=orange>Loading font assets for group: </color>" + (_fontGroup == null ? "NULL" : _fontGroup.sku));

		if(_fontGroup == null) return;

		TMP_FontAsset fnt = null;
		Dictionary<string, Material> sharedMaterialsCache = null;
		for(int i = 0; i < _fontGroup.fontAssets.Length; ++i) {
			string fontName = _fontGroup.fontAssets[i];

			// Register new font to the assets cache
			fnt = Resources.Load<TMP_FontAsset>(GetFontAssetPath(fontName));
			m_fontsCache[fontName] = fnt;

			// Create new shared materials cache for this font if it doesn't exist
			if(!m_sharedMaterialsCache.TryGetValue(fontName, out sharedMaterialsCache)) {
				// Create a new cache for this font asset!
				sharedMaterialsCache = new Dictionary<string, Material>();
				m_sharedMaterialsCache[fontName] = sharedMaterialsCache;
			}

            if (fnt != null) {
                // Register default material to the shared materials cache
                string materialID = GetMaterialIDFromName(fontName, fnt.material.name);
                sharedMaterialsCache[materialID] = fnt.material;
            } else  {
                // TODO: Notify metrics/ crashlytics fontName is null becaue it's not supposed to   
                Debug.LogWarning("Font " + fontName + " is null but it's not supposed to");
            }
		}
	}

	/// <summary>
	/// Clear the font and materials caches.
	/// </summary>
	private void ClearFontAssets(FontGroup _fontGroup) {
		Debug.Log("<color=orange>Clearing font assets for group: </color>" + (_fontGroup == null ? "NULL" : _fontGroup.sku));

		if(_fontGroup == null) return;

		Dictionary<string, Material> sharedMaterialsCache = null;
		for(int i = 0; i < _fontGroup.fontAssets.Length; ++i) {
			// Font assets cache
			m_fontsCache[_fontGroup.fontAssets[i]] = null;

			// Shared materials cache
			if(m_sharedMaterialsCache.TryGetValue(_fontGroup.fontAssets[i], out sharedMaterialsCache)) {
				sharedMaterialsCache.Clear();
			}
		}
	}

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Given a font group sku, return the matching font group data.
	/// </summary>
	/// <returns>The font group. <c>null</c> if none could be found with the given sku.</returns>
	/// <param name="_fontGroupSku">Font group sku to be found.</param>
	public FontGroup GetFontGroup(string _fontGroupSku) {
		FontGroup res = null;
		m_fontGroups.TryGetValue(_fontGroupSku, out res);
		return res;
	}

	/// <summary>
	/// If target font asset is not compatible with current font group, default
	/// asset (the one at index 0) for the current font group will be used instead.
	/// </summary>
	/// <returns>The font asset.</returns>
	/// <param name="_fontAssetName">Font asset name.</param>
	public TMP_FontAsset GetFontAsset(string _fontAssetName) {
		// Clean up font name
		_fontAssetName = GetNonEditorName(_fontAssetName);

		// Is target font asset compatible with the current font group?
		if(currentFontGroup != null) {
			bool isCompatible = false;
			for(int i = 0; i < currentFontGroup.fontAssets.Length; ++i) {
				if(currentFontGroup.fontAssets[i] == _fontAssetName) {
					isCompatible = true;
					break;
				}
			}

			// Font asset not compatible with current font group
			if(!isCompatible) {
				// Use default font asset for current font group
				_fontAssetName = currentFontGroup.defaultFont;
			}
		}

		// Retrieve target font asset from the cache
		TMP_FontAsset res = null;
		m_fontsCache.TryGetValue(_fontAssetName, out res);

		// If no valid asset is loaded, use dummy font
		if(res == null) res = m_dummyFont;

		return res;
	}

	/// <summary>
	/// Applies a specific material to a textfield.
	/// The material will be looked up in the shared materials cache and in the Resources folder.
	/// If not found in neither places, the current material instance of the textfield will be modified
	/// to match the looks of the requested material ID in the given fallback font.
	/// If no fallback font is provided or material ID can't be found, nothing will happen.
	/// </summary>
	/// <param name="_target">Target.</param>
	/// <param name="_materialID">Material ID.</param>
	/// <param name="_fontName">Font name.</param>
	/// <param name="_fallbackFontName">Fallback font name.</param>
	public void ApplyFontMaterial(ref TMP_Text _target, string _materialID, string _fontName, string _fallbackFontName) {
		// Textfield must be valid!
		if(_target == null) return;

		// Clean up font and material names
		_fontName = GetNonEditorName(_fontName);
		_materialID = GetNonEditorName(_materialID);

		// Is material cached?
		Material m = TryGetSharedMaterial(_fontName, _materialID);

		// Do we have a matching material for this font and id?
		if(m != null) {
			// Yes! Apply it to the textfield
			_target.fontSharedMaterial = m;
		} else {
			// No! Fallback to dummy material
			m = TryGetDummyMaterial(_fallbackFontName, _materialID);
			if(m != null) {
				// Apply the original material's properties to the current font material
				// [AOC] Luckily for us TMP already has a tool for this!
				_target.fontMaterial.shader = m.shader;
				TMP_MaterialManager.CopyMaterialPresetProperties(m, _target.fontMaterial);
				_target.SetMaterialDirty();
				_target.UpdateMeshPadding();
			}
		}
	}

	/// <summary>
	/// Try to get a shared material for a specific font.
	/// Look first at the materials cache. If not found, it will try to load it 
	/// from Resources (once).
	/// </summary>
	/// <returns>The requested shared material. <c>null</c> if no shared material exists for requested font and ID.</returns>
	/// <param name="_fontName">Target font name.</param>
	/// <param name="_materialID">Material ID.</param>
	public Material TryGetSharedMaterial(string _fontName, string _materialID) {
		// Clean up font name and material names
		_fontName = GetNonEditorName(_fontName);
		_materialID = GetNonEditorName(_materialID);

		// Create a new cache entry for this font name if not yet created
		Dictionary<string, Material> fontSharedMaterials = null;
		if(!m_sharedMaterialsCache.TryGetValue(_fontName, out fontSharedMaterials)) {
			fontSharedMaterials = new Dictionary<string, Material>();
			m_sharedMaterialsCache[_fontName] = fontSharedMaterials;
		}

		// Is material cached?
		Material m = null;
		if(!fontSharedMaterials.TryGetValue(_materialID, out m)) {
			// Material is not cached, try to load it
			m = Resources.Load<Material>(GetMaterialPath(_fontName, _materialID));
			fontSharedMaterials[_materialID] = m;	// Might be null, store it anyways to avoid trying to load it again
		}

		// Done!
		return m;
	}

	/// <summary>
	/// Create a new dummy material to the cache for the given font-materialID pair.
	/// It will be a copy of the source material without any reference to the texture atlas.
	/// Nothing happens if a dummy material for this pair already exists.
	/// </summary>
	/// <param name="_fontName">Font name.</param>
	/// <param name="_materialID">Material ID.</param>
	/// <param name="_sourceMaterial">Reference material from where the new material will be initialized.</param>
	public void RegisterDummyMaterial(string _fontName, string _materialID, Material _sourceMaterial) {
		// Clean up font name and material names
		_fontName = GetNonEditorName(_fontName);
		_materialID = GetNonEditorName(_materialID);

		// Create a new cache entry for this font name if not yet created
		Dictionary<string, Material> fontDummyMaterials = null;
		if(!m_dummyMaterialsCache.TryGetValue(_fontName, out fontDummyMaterials)) {
			fontDummyMaterials = new Dictionary<string, Material>();
			m_dummyMaterialsCache[_fontName] = fontDummyMaterials;
		}

		// Do we have a dummy material?
		Material m = null;
		if(!fontDummyMaterials.TryGetValue(_materialID, out m)) {
			// No dummy material exists, create it!
			m = new Material(_sourceMaterial);
			m.SetTexture(ShaderUtilities.ID_MainTex, null);	// Don't keep atlas in memory!!
			m.name = GetNonEditorName(_sourceMaterial.name) + "_DUMMY";

			// Store new dummy material
			fontDummyMaterials[_materialID] = m;
		}
	}

	/// <summary>
	/// Try to get a dummy material for the given font-materialID pair.
	/// </summary>
	/// <returns>The dummy material corresponding to the given pair. <c>null</c> if no dummy material exists for that pair.</returns>
	/// <param name="_fontName">Font name.</param>
	/// <param name="_materialID">Material ID.</param>
	public Material TryGetDummyMaterial(string _fontName, string _materialID) {
		// Clean up font name and material names
		_fontName = GetNonEditorName(_fontName);
		_materialID = GetNonEditorName(_materialID);

		// Do we have any dummy materials for this font?
		Dictionary<string, Material> fontDummyMaterials = null;
		if(!m_dummyMaterialsCache.TryGetValue(_fontName, out fontDummyMaterials)) {
			return null;
		}

		// Do we have the requested dummy material?
		Material m = null;
		fontDummyMaterials.TryGetValue(_materialID, out m);
		return m;
	}

	//------------------------------------------------------------------------//
	// UTILS 																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Given a material name and the font it belongs to, figure out material ID.
	/// <example>(FNT_Bold, FNT_Bold_Stroke) => _Stroke</example>
	/// </summary>
	/// <returns>The material identifier.</returns>
	/// <param name="_fontName">Font name.</param>
	/// <param name="_materialName">Material name.</param>
	public string GetMaterialIDFromName(string _fontName, string _materialName) {
		// Clean up font name
		_fontName = GetNonEditorName(_fontName);

		// Strip font name from the material name
		return _materialName.Replace(_fontName, "");	// (FNT_Bold, FNT_Bold_Stroke) => _Stroke
	}

	/// <summary>
	/// Given a material ID and a font, compose the actual name for that material-font pair.
	/// <example>(FNT_Bold, _Stroke) => FNT_Bold_Stroke</example>
	/// </summary>
	/// <returns>The material name corresponding to the target font.</returns>
	/// <param name="_fontName">Font name.</param>
	/// <param name="_materialID">Material ID.</param>
	public string GetMaterialNameFromID(string _fontName, string _materialID) {
		// Clean up font name and material names
		_fontName = GetNonEditorName(_fontName);
		_materialID = GetNonEditorName(_materialID);

		m_sb.Length = 0;
		m_sb.Append(_fontName).Append(_materialID);		// (FNT_Bold, _Stroke) => FNT_Bold_Stroke
		return m_sb.ToString();
	}

	/// <summary>
	/// Gets the name of the non-editor name matching the given font/material name.
	/// </summary>
	/// <returns>The non editor name.</returns>
	/// <param name="_fontName">Font/material name to be cleaned.</param>
	public string GetNonEditorName(string _fontName) {
		return _fontName.Replace("_Editor", "");	// (FNT_Bold_Editor) => FNT_Bold
	}

	/// <summary>
	/// Given a font name, compose the full path to its asset in Resources.
	/// <example>(FNT_Bold) => Fonts/FNT_Bold/FNT_Bold</example>
	/// </summary>
	/// <returns>The font asset path.</returns>
	/// <param name="_fontName">Font name.</param>
	public string GetFontAssetPath(string _fontName) {
		m_sb.Length = 0;
		m_sb.Append(m_fontsDir).Append("/").Append(_fontName).Append("/").Append(_fontName);		// (FNT_Bold) => Fonts/FNT_Bold/FNT_Bold
		return m_sb.ToString();
	}

	/// <summary>
	/// Given a font and material pair, compose the full path to the material's asset in Resources.
	/// <example>(FNT_Bold, _Stroke) => Fonts/FNT_Bold/FNT_Bold_Stroke</example>
	/// </summary>
	/// <returns>The material path.</returns>
	/// <param name="_fontName">Font name.</param>
	/// <param name="_materialID">Material ID.</param>
	public string GetMaterialPath(string _fontName, string _materialID) {
		// Figure out material name
		string materialName = GetMaterialNameFromID(_fontName, _materialID);

		// Material should be in the font's folder
		m_sb.Length = 0;
		m_sb.Append(m_fontsDir).Append("/").Append(_fontName).Append("/").Append(materialName);		// (FNT_Bold, _Stroke) => Fonts/FNT_Bold/FNT_Bold_Stroke
		return m_sb.ToString();
	}

	/// <summary>
	/// Dump to console output.
	/// </summary>
	public void Dump() {
		m_sb.Length = 0;

		// Header
		m_sb.AppendLine("<color=orange>FONT MANAGER DUMP</color>");

		// Font Cache
		m_sb.AppendLine("Fonts Cache:");
		foreach(KeyValuePair<string, TMP_FontAsset> kvp in m_fontsCache) {
			m_sb.Append("\t").Append(kvp.Key).Append(": ")
				.Append(kvp.Value == null ? "NULL" : kvp.Value.ToString()).AppendLine();
		}

		// Shared Materials Cache
		m_sb.AppendLine();
		m_sb.AppendLine("Shared Materials Cache:");
		foreach(KeyValuePair<string, Dictionary<string, Material>> kvp1 in m_sharedMaterialsCache) {
			m_sb.Append("\t").Append(kvp1.Key).Append(": ").AppendLine();
			foreach(KeyValuePair<string, Material> kvp2 in kvp1.Value) {
				m_sb.Append("\t\t").Append(kvp2.Key).Append(": ")
					.Append(kvp2.Value == null ? "NULL" : kvp2.Value.ToString()).AppendLine();
			}
		}

		// Dummy Materials Cache
		m_sb.AppendLine();
		m_sb.AppendLine("Dummy Materials Cache:");
		foreach(KeyValuePair<string, Dictionary<string, Material>> kvp1 in m_dummyMaterialsCache) {
			m_sb.Append("\t").Append(kvp1.Key).Append(": ").AppendLine();
			foreach(KeyValuePair<string, Material> kvp2 in kvp1.Value) {
				m_sb.Append("\t\t").Append(kvp2.Key).Append(": ")
					.Append(kvp2.Value == null ? "NULL" : kvp2.Value.ToString()).AppendLine();

				if(kvp2.Value.GetTexture(ShaderUtilities.ID_MainTex) != null) {
					m_sb.AppendLine("<color=red>DUMMY MATERIAL REFERENCING TEXTURE!!!!</color>");
				}
			}
		}

		// Log!
		Debug.Log(m_sb.ToString());
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Current language has been changed in the LocalizationManager.
	/// </summary>
	private void OnLanguageChanged() {
		// If current language requires a different font than the current one, start the sequence.
		// Otherwise we have nothing to do!
		Debug.Log("<color=orange>FontManager.OnLanguageChanged!</color> " + LocalizationManager.SharedInstance.GetCurrentLanguageSKU());
		DefinitionNode langDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LOCALIZATION, LocalizationManager.SharedInstance.GetCurrentLanguageSKU());
		if(m_currentFontGroup == null || langDef.GetAsString("fontGroup") != m_currentFontGroup.sku) {
			ChangeState(State.LOOSING_REFERENCES);
		}
	}
}
