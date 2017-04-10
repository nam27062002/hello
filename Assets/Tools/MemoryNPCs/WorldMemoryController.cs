using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

public class WorldMemoryController : MonoBehaviour { 
	//---------------------------------------
	private const float MAX_MEMORY_BUDGET = 40f;
	private const float MAX_MEMORY_DRAGON = 15f;
	private const float MAX_MEMORY_PETS = 10f;
	private const float MAX_MEMORY_NPCS = 35f;
	private const float MAX_MEMORY_WORLD = 45f;
	private const float MAX_MEMORY_DECORATIONS = 5f;
	private const float MAX_MEMORY_PARTICLES = 25f;

	//---------------------------------------
	[System.Serializable]
	public class UIContainers {
		[SerializeField] private GameObject root;

		private Slider allSlider;
		private TMP_Text allText;

		private Slider meshSlider;
		private TMP_Text meshText;

		private Slider textureSlider;
		private TMP_Text textureText;

		private Slider animationsSlider;
		private TMP_Text animationsText;

		//---------------------------------------
		private float m_maxMem;

		//---------------------------------------
		public void Setup(float _maxValue) {
			m_maxMem = _maxValue;

			allSlider = root.FindComponentRecursive<Slider>("AllMemSlider");
			allText = root.FindComponentRecursive<TMP_Text>("AllMemText");

			meshSlider = root.FindComponentRecursive<Slider>("MeshMemSlider");
			meshText = root.FindComponentRecursive<TMP_Text>("MeshMemText");

			textureSlider = root.FindComponentRecursive<Slider>("TextureMemSlider");
			textureText = root.FindComponentRecursive<TMP_Text>("TextureMemText");

			animationsSlider = root.FindComponentRecursive<Slider>("AnimationMemSlider");
			animationsText = root.FindComponentRecursive<TMP_Text>("AnimationMemText");

			allSlider.minValue = 0f;
			allSlider.maxValue = _maxValue;

			if (meshSlider != null) {
				meshSlider.minValue = 0f;
				meshSlider.maxValue = m_maxMem;
			}

			if (textureSlider != null) {
				textureSlider.minValue = 0f;
				textureSlider.maxValue = m_maxMem;
			}

			if (animationsSlider != null) {
				animationsSlider.minValue = 0f;
				animationsSlider.maxValue = m_maxMem;
			}
		}

		public void Update(MemoryData _data) {
			allSlider.value = _data.all;
			allText.text = _data.all.ToString("F2") + "/" + m_maxMem + " MB";

			if (meshSlider != null) {
				meshSlider.value = _data.mesh;
				meshText.text = _data.mesh.ToString("F2") + " MB";
			}

			if (textureSlider != null) {
				textureSlider.value = _data.texture;
				textureText.text = _data.texture.ToString("F2") + " MB";
			}

			if (animationsSlider != null) {
				animationsSlider.value = _data.animations;
				animationsText.text = _data.animations.ToString("F2") + " MB";
			}
		}
	}

	public class MemoryData {
		public float all;
		public float mesh;
		public float texture;
		public float animations;

		public void Reset() {
			all = 0f;
			mesh = 0f;
			texture = 0f;
			animations = 0f;
		}
	}
	    
	//---------------------------------------
	[SerializeField] private Slider m_timerUI;
	[SeparatorAttribute]
	[SerializeField] private UIContainers m_memoryUI;
	[SerializeField] private UIContainers m_dragonUI;
	[SerializeField] private UIContainers m_petsUI;
	[SerializeField] private UIContainers m_npcsUI;
	[SerializeField] private UIContainers m_worldUI;
	[SerializeField] private UIContainers m_decorationsUI;
	[SerializeField] private UIContainers m_particleSystemsUI;

	//---------------------------------------
	private List<string> m_prefabNames;

	private MemoryData m_memory;
	private MemoryData m_dragonMemory;
	private MemoryData m_petsMemory;
	private MemoryData m_npcsMemory;
	private MemoryData m_worldMemory;
	private MemoryData m_decorationsMemory;
	private MemoryData m_particleSystemsMemory;

	private float m_timer;

    //---------------------------------------
    private AssetMemoryProfiler m_profiler;


    //---------------------------------------
    // Use this for initialization
    private void Start() {
        m_profiler = new AssetMemoryProfiler();
       
        m_timerUI.minValue = 0f;
		m_timerUI.maxValue = 10f;

		m_memory = new MemoryData();
		m_dragonMemory = new MemoryData();
		m_petsMemory = new MemoryData();
		m_npcsMemory = new MemoryData();
		m_worldMemory = new MemoryData();
		m_decorationsMemory = new MemoryData();
		m_particleSystemsMemory = new MemoryData();

		//TODO setup all containers
		m_memoryUI.Setup(MAX_MEMORY_BUDGET);
		m_dragonUI.Setup(MAX_MEMORY_DRAGON);
		m_petsUI.Setup(MAX_MEMORY_PETS);
		m_npcsUI.Setup(MAX_MEMORY_NPCS);
		m_worldUI.Setup(MAX_MEMORY_WORLD);
		m_decorationsUI.Setup(MAX_MEMORY_DECORATIONS);
		m_particleSystemsUI.Setup(MAX_MEMORY_PARTICLES);

		OnReset();
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

			AnalizeDragon();
			AnalizePets();
			AnalizeNPCs();
            AnalizeWorld();
            SumAllMem();

			UpdateUI();
        }

        m_timerUI.value = m_timer;
	}

	private void OnReset() {
		m_memory.Reset();	
		m_dragonMemory.Reset();
		m_petsMemory.Reset();
		m_npcsMemory.Reset();
		m_worldMemory.Reset();
		m_decorationsMemory.Reset();
		m_particleSystemsMemory.Reset();

		UpdateUI();

		m_timer = 10f;
		m_timerUI.value = m_timer;

        m_profiler.Reset();
    }

	private void SumAllMem() {
		m_memory.all += m_dragonMemory.all + m_petsMemory.all + m_npcsMemory.all + m_worldMemory.all + m_decorationsMemory.all + m_particleSystemsMemory.all;
	}

	private void AnalizeDragon() {
		m_profiler.Reset(); //???

        if (InstanceManager.player != null) m_profiler.AddGo(InstanceManager.player.gameObject, "Dragon"); 

		ProfilerToData(m_dragonMemory);
	}

	private void AnalizePets() {
		m_profiler.Reset(); //???
	}

	private void AnalizeNPCs() {
		Entity[] gos;

		if (Application.isPlaying) 	gos = PoolManager.instance.GetComponentsInChildren<Entity>(true);
		else						gos = GameObjectExt.FindObjectsOfType<Entity>(true).ToArray();

		m_profiler.Reset(); //???

		GameObject go;
		for (int i = 0; i < gos.Length; i++) {
			go = (gos[i] as Entity).gameObject;
			m_profiler.AddGo(go, "NPCs");           
		}               

		ProfilerToData(m_npcsMemory);
	}

    private void AnalizeWorld() {
        string label = "World";

        m_profiler.Reset();

        FogManager fogManager = InstanceManager.fogManager;
        if (fogManager != null) {
            FogManager.FogAttributes attributes = fogManager.m_defaultAreaFog;
            if (attributes != null) {
                m_profiler.AddTexture(attributes.texture, label, "FogAreaTexture");                
            }

            List<FogManager.FogAttributes> attributesList = fogManager.m_generatedAttributes;
            if (attributesList != null) {
                int count = attributesList.Count;
                for (int i = 0; i < count; i++) {
                    m_profiler.AddTexture(attributesList[i].texture, label, "FogAreaTexture");
                }
            }
        }       

        ProfilerToData(m_worldMemory);
    }

    private void ProfilerToData(MemoryData _data) {
		float mesh = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Mesh));
		float animation = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Animation));
		float texture = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Texture));
		float other = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Other));
		float total = BytesToMegaBytes(m_profiler.GetSize());                

		_data.all += total + other; //should we add the "other" value?
		_data.mesh += mesh;
		_data.texture += texture;
		_data.animations += animation;
	}

	private void UpdateUI() {
		m_memoryUI.Update(m_memory);
		m_dragonUI.Update(m_dragonMemory);
		m_petsUI.Update(m_petsMemory);
		m_npcsUI.Update(m_npcsMemory);
		m_worldUI.Update(m_worldMemory);
		m_decorationsUI.Update(m_decorationsMemory);
		m_particleSystemsUI.Update(m_particleSystemsMemory);
	}

	private void OnLevelLoaded() {
		OnReset();
    }

    private float BytesToMegaBytes(long bytes) {
        return bytes / (1024f * 1024f);
    }
}
