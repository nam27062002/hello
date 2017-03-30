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

	private float m_timer;

	//---------------------------------------

	// Use this for initialization
	private void Start() {
		m_memoryAll.minValue = 0f;
		m_memoryMesh.minValue = 0f;
		m_memoryTextures.minValue = 0f;
		m_memoryAnimations.minValue = 0f;

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
		}
	}

	private void OnReset() {
		m_memoryAll.value = 0f;
		m_memoryMesh.value = 0f;
		m_memoryTextures.value = 0f;
		m_memoryAnimations.value = 0f;

		m_memoryAllText.text = 0f + "/" + MAX_MEMORY + " MB";
		m_memoryMeshText.text = 0f + " MB";
		m_memoryTexturesText.text = 0f + " MB";
		m_memoryAnimationsText.text = 0f + " MB";

		m_timer = 10f;
	}

	private void OnLevelLoaded() {
		OnReset();
    }
}
