﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

[ExecuteInEditMode]
public class MemoryEntitiesController : MonoBehaviour, IBroadcastListener {   
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
    private AssetMemoryProfiler m_profiler;

    //---------------------------------------

    // Use this for initialization
    private void Start() {
        m_profiler = new AssetMemoryProfiler();
       
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
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);        
    }

    /// <summary>
    /// Component disabled.
    /// </summary>
    private void OnDisable() {
        // Unsubscribe from external events
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);        
    }
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
                OnLevelLoaded();
            }break;
        }
    }

    private void Update() {
        m_timer -= Time.deltaTime;
        if (m_timer <= 0f) {
            OnReset();
            GetPrefabsFromScene();
        }

        m_timerUI.value = m_timer;
        

        /*if (Input.GetKeyDown(KeyCode.A)) {
            OnReset();
            GetPrefabsFromScene();
        }*/
    }

	private void OnReset() {
		m_memory = 0f;
		m_meshes = 0f;
		m_textures = 0f;
		m_animations = 0f;

		UpdateUI();

		m_timer = 10f;
		m_timerUI.value = m_timer;

        m_profiler.Reset();
    }

	private void GetPrefabsFromScene() {
		Entity[] gos;

		if (Application.isPlaying) {
			gos = PoolManager.instance.GetComponentsInChildren<Entity>(true);
		} else {
			gos = GameObjectExt.FindObjectsOfType<Entity>(true).ToArray();
		}

        GameObject go;
		for (int i = 0; i < gos.Length; i++) {
            go = (gos[i] as Entity).gameObject;
            m_profiler.AddGo(go, "NPCs");           
		}               
                
        float mesh = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Mesh));
        float animation = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Animation));
        float texture = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Texture));
        float other = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Other));
        float total = BytesToMegaBytes(m_profiler.GetSize());                

        m_memory += total;
        m_meshes += mesh;
        m_animations += animation;
        m_textures += texture;

        UpdateUI();
	}

	private void UpdateUI() {
		m_memoryAll.value = m_memory;
		m_memoryMesh.value = m_meshes;
		m_memoryTextures.value = m_textures;
		m_memoryAnimations.value = m_animations;

		m_memoryAllText.text = m_memory.ToString("F2") + "/" + MAX_MEMORY + " MB";
		m_memoryMeshText.text = m_meshes.ToString("F2") + " MB";
		m_memoryTexturesText.text = m_textures.ToString("F2") + " MB";
		m_memoryAnimationsText.text = m_animations.ToString("F2") + " MB";
	}

	private void OnLevelLoaded() {
		OnReset();
    }

    private float BytesToMegaBytes(long bytes) {
        return bytes / (1024f * 1024f);
    }
}
