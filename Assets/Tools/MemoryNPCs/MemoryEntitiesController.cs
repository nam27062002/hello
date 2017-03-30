using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

[ExecuteInEditMode]
public class MemoryEntitiesController : MonoBehaviour {   
	//---------------------------------------
	private struct AssetMemory {		
		public float size;
		public int refs;
	}

	//---------------------------------------
	private const float MAX_MEMORY = 40f;
    
	//---------------------------------------
	[SerializeField] private Slider m_timerUI;
	[SeparatorAttribute]
	[SerializeField] private Slider m_memoryAll;
	[SerializeField] private TMP_Text m_memoryAllText;
	[SeparatorAttribute]
	[SerializeField] private Slider m_memoryMesh;
	[SerializeField] private TMP_Text m_memoryMeshText;
	[SeparatorAttribute]
	[SerializeField] private Slider m_memoryTextures;
	[SerializeField] private TMP_Text m_memoryTexturesText;
	[SeparatorAttribute]
	[SerializeField] private Slider m_memoryAnimations;
	[SerializeField] private TMP_Text m_memoryAnimationsText;

	//---------------------------------------
	private List<string> m_prefabNames;

	private float m_memory;
	private float m_meshes;
	private float m_textures;
	private float m_animations;

	private float m_timer;

	//---------------------------------------

	// Use this for initialization
	private void Start() {
		m_timerUI.minValue = 0f;
		m_memoryAll.minValue = 0f;
		m_memoryMesh.minValue = 0f;
		m_memoryTextures.minValue = 0f;
		m_memoryAnimations.minValue = 0f;

		m_timerUI.maxValue = 10f;
		m_memoryAll.maxValue = MAX_MEMORY;
		m_memoryMesh.maxValue = MAX_MEMORY;
		m_memoryTextures.maxValue = MAX_MEMORY;
		m_memoryAnimations.maxValue = MAX_MEMORY;

		m_timer = 10f;
	}

    private void OnEnable() {
        // Subscribe to external events
        Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);        
    }

    /// <summary>
    /// Component disabled.
    /// </summary>
    private void OnDisable() {
        // Unsubscribe from external events
        Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);        
    }

	private void Update() {
		m_timer -= Time.deltaTime;
		if (m_timer <= 0f) {
			OnReset();
			GetPrefabsFromScene();
		}

		m_timerUI.value = m_timer;
	}

	private void OnReset() {
		m_memory = 0f;
		m_meshes = 0f;
		m_textures = 0f;
		m_animations = 0f;

		UpdateUI();

		m_timer = 10f;
		m_timerUI.value = m_timer;
	}

	private void GetPrefabsFromScene() {
		Object[] gos = GameObject.FindObjectsOfType(typeof(Entity));

		for (int i = 0; i < gos.Length; i++) {
			//gos[i]
			float mem = 2f;
			float mesh = 0.5f;
			float animation = 1f;
			float texture = 0.5f;

			m_memory += mem;
			m_meshes += mesh;
			m_animations += animation;
			m_textures += texture;
		}

		UpdateUI();
	}

	private void UpdateUI() {
		m_memoryAll.value = m_memory;
		m_memoryMesh.value = m_meshes;
		m_memoryTextures.value = m_textures;
		m_memoryAnimations.value = m_animations;

		m_memoryAllText.text = m_memory + "/" + MAX_MEMORY + " MB";
		m_memoryMeshText.text = m_meshes + " MB";
		m_memoryTexturesText.text = m_textures + " MB";
		m_memoryAnimationsText.text = m_animations + " MB";
	}

	private void OnLevelLoaded() {
		OnReset();
    }
}
