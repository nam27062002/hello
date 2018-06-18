using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPModifiers : MonoBehaviour {
	//------------------------------------------------------------------------//
	private static List<ModifierDragon> sm_dragonMods = new List<ModifierDragon>();

	public static void CreateDragonMod(ModifierDragon _mod) { sm_dragonMods.Add(_mod); }
	public static void DestroyDragonMod(ModifierDragon _mod) { sm_dragonMods.Remove(_mod); }

	public static void ApplyDragonMods() {
		for (int i = 0; i < sm_dragonMods.Count; ++i) {
			sm_dragonMods[i].Apply();
		}
	}
	public static void RemoveDragonMods() {
		for (int i = 0; i < sm_dragonMods.Count; ++i) {
			sm_dragonMods[i].Remove();
		}
	}
	//------------------------------------------------------------------------//



	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private GameObject m_modPillPrefab;
	[SerializeField] private RectTransform m_container;

	private bool m_initialized = false;



	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	void OnEnable() {
		if (!m_initialized) {
			List<DefinitionNode> mods = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.LIVE_EVENTS_MODIFIERS);
			if (mods.Count > 0) {
				for (int i = 0; i < mods.Count; ++i) {
					GameObject go = GameObject.Instantiate<GameObject>(m_modPillPrefab, m_container, false);
					CPMod mod = go.GetComponent<CPMod>();
					mod.Init(mods[i]);
				}
				m_initialized = true;
			}
		}
	}
}
