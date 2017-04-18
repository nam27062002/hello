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

        private Slider particlesSlider;
        private TMP_Text particlesText;

        private Slider audioSlider;
        private TMP_Text audioText;

        private Slider otherSlider;
        private TMP_Text otherText;

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

            particlesSlider = root.FindComponentRecursive<Slider>("ParticlesMemSlider");
            particlesText = root.FindComponentRecursive<TMP_Text>("ParticlesMemText");

            audioSlider = root.FindComponentRecursive<Slider>("AudioMemSlider");
            audioText = root.FindComponentRecursive<TMP_Text>("AudioMemText");

            otherSlider = root.FindComponentRecursive<Slider>("OtherMemSlider");
            otherText = root.FindComponentRecursive<TMP_Text>("OtherMemText");

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

            if (particlesSlider != null) {
                particlesSlider.minValue = 0f;
                particlesSlider.maxValue = m_maxMem;
            }

            if (audioSlider != null) {
                audioSlider.minValue = 0f;
                audioSlider.maxValue = m_maxMem;
            }

            if (otherSlider != null) {
                otherSlider.minValue = 0f;
                otherSlider.maxValue = m_maxMem;
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

            if (particlesSlider != null) {
                particlesSlider.value = _data.particles;
                particlesText.text = _data.particles.ToString("F2") + " MB";
            }

            if (audioSlider != null) {
                audioSlider.value = _data.audio;
                audioText.text = _data.audio.ToString("F2") + " MB";
            }

            if (otherSlider != null) {
                otherSlider.value = _data.other;
                otherText.text = _data.other.ToString("F2") + " MB";
            }
        }
	}

	public class MemoryData {
        public string label;
		public float all;
		public float mesh;
		public float texture;
		public float animations;
        public float particles;
        public float audio;
        public float other;

        public MemoryData(string label) {
            this.label = label;
        }

		public void Reset() {
			all = 0f;
			mesh = 0f;
			texture = 0f;
			animations = 0f;
            particles = 0f;
            audio = 0f;
            other = 0f;
		}

        public override string ToString() {
            return "Label: " + label + " mesh: " + mesh + " texture = " + texture + 
                   " animations = " + animations + " particles = " + particles + 
                   " audio = " + audio + " other = " + other + 
                   " TOTAL = " + all;
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
	private MemoryData m_particlesMemory;

	private float m_timer;

    //---------------------------------------
    private AssetMemoryProfiler m_profiler;
    private AssetMemoryProfilerPrinter m_profilerPrinter;
    private MemoryProfiler m_memoryProfilerExtra;


    //---------------------------------------
    private const string LABEL_HUD = "Hud";
    private const string LABEL_DRAGON = "Dragon";
    private const string LABEL_PETS = "Pets";
    private const string LABEL_NPCS = "NPCS";
    private const string LABEL_WORLD = "World";
    private const string LABEL_SOUND = "Sound";
    private const string LABEL_PARTICLES = "Particles";


    // Use this for initialization
    private void Start() {
        m_profiler = new AssetMemoryProfiler();
        m_profilerPrinter = new AssetMemoryProfilerPrinter(m_profiler);
        m_memoryProfilerExtra = new MemoryProfiler();

        m_timerUI.minValue = 0f;
		m_timerUI.maxValue = 10f;

		m_memory = new MemoryData(null);
		m_dragonMemory = new MemoryData(LABEL_DRAGON);
		m_petsMemory = new MemoryData(LABEL_PETS);
		m_npcsMemory = new MemoryData(LABEL_NPCS);
		m_worldMemory = new MemoryData(LABEL_WORLD);
		//m_decorationsMemory = new MemoryData();
		m_particlesMemory = new MemoryData(LABEL_PARTICLES);

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
        if (FlowManager.IsInGameScene()) {
            m_timer -= Time.deltaTime;
            if (m_timer <= 0f) {
                OnReset();

                //AnalizePerConcept();
                //AnalizePerType();
                m_memoryProfilerExtra.TakeASample();

                SumAllMem();

                UpdateUI();
            }

            m_timerUI.value = m_timer;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            //OnReset();
            //GetPrefabsFromScene();
            m_profilerPrinter.Print("AssetMemoryAnalysis", AssetMemoryPrinterFormatType.XML, AssetMemoryPrinterSortType.HighToLow, true);
        }
    }

    private void AnalizePerType()
    {
        //List<GameObject> all = new List<GameObject>();
        List<GameObject> all = GameObjectExt.FindAllObjectsInScene(true);
        /*for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {                
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {                    
                    all.Add(allGameObjects[j]);
                }
            }
        }*/

        /*Entity[] gos = PoolManager.instance.GetComponentsInChildren<Entity>(true);
        if (gos != null) {
            int count = gos.Length;
            GameObject go;
            for (int i = 0; i < count; i++) {
                go = (gos[i] as Entity).gameObject;
                all.Add(go);
            }
        }*/

        //all.Add(PoolManager.instance.gameObject);
        //all.Add(ParticleManager.instance.gameObject);

        AnalizeWorld(all);
        ProfilerToData(m_worldMemory);

        long total = 0;        
        Dictionary<string, long> textures = m_profiler.GetDetailedSizePerType(AssetMemoryGlobals.EAssetType.Texture);
        List<AssetMemoryGlobals.AssetMemoryRawData> data = new List<AssetMemoryGlobals.AssetMemoryRawData>();
        foreach (KeyValuePair<string, long> pair in textures)
        {
            total += pair.Value;
            data.Add(new AssetMemoryGlobals.AssetMemoryRawData(pair.Key, pair.Value));            
        }

        string texturesFile = "AssetMemoryAnalysis_Textures";
        AssetMemoryProfilerPrinter.Print(texturesFile, data, AssetMemoryPrinterSortType.HighToLow);

        /*Texture[] texturesResources = Resources.FindObjectsOfTypeAll(typeof(Texture)) as Texture[];
        int count = texturesResources.Length;
        for (int i = 0; i < count; i++)
        {
            if (texturesResources[i])
            // Resources.FindObjectsOfTypeAll() also returns internal stuff so we need to be extra careful about the game ojects returned by this
            // function
            if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                continue;
        }

        Debug.Log("Count = " + texturesResources.Length);
        */
    }

    private void AnalizePerConcept()
    {
        List<GameObject> dragon = new List<GameObject>();
        List<GameObject> pets = new List<GameObject>();
        List<GameObject> npcs = new List<GameObject>();
        List<GameObject> hud = new List<GameObject>();
        List<GameObject> world = new List<GameObject>();
        List<GameObject> particles = new List<GameObject>();

        string name;
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    name = allGameObjects[j].name;
                    if (name == "UIRoot" || name == "ResultsScene")
                    {
                        hud.Add(allGameObjects[j]);
                    }
                    else if (name == "Player")
                    {
                        dragon.Add(allGameObjects[j]);
                    }
                    else
                    {
                        world.Add(allGameObjects[j]);
                    }
                }
            }
        }

        List<Entity> entities = new List<Entity>();
        List<Decoration> decorations = new List<Decoration>();
        List<ParticleSystem> psystems = new List<ParticleSystem>();

        List<GameObject> gos = GameObjectExt.FindAllObjectsInScene(true);
        if (gos != null)
        {
            int count = gos.Count;
            for (int i = 0; i < count; i++)
            {
                entities.AddRange(gos[i].GetComponentsInChildren<Entity>(true));
                decorations.AddRange(gos[i].GetComponentsInChildren<Decoration>(true));
                psystems.AddRange(gos[i].GetComponentsInChildren<ParticleSystem>(true));
            }

            count = entities.Count;
            for (int i = 0; i < count; i++)
            {
                npcs.Add(entities[i].gameObject);
            }

            count = decorations.Count;
            for (int i = 0; i < count; i++)
            {
                if (!npcs.Contains(decorations[i].gameObject))
                    npcs.Add(decorations[i].gameObject);
            }

            count = psystems.Count;
            for (int i = 0; i < count; i++)
            {
                particles.Add(psystems[i].gameObject);
            }
        }

        AnalizeWorld(world);
        Analize(dragon, LABEL_DRAGON);
        Analize(pets, LABEL_PETS);
        Analize(npcs, LABEL_NPCS);
        Analize(particles, LABEL_PARTICLES);

        ProfilerToData(m_worldMemory);
        ProfilerToData(m_dragonMemory);
        ProfilerToData(m_petsMemory);
        ProfilerToData(m_npcsMemory);
        ProfilerToData(m_particlesMemory);
    }

    private void RemoveGoAndChildren(List<GameObject> allGos, GameObject goToRemove)
    {
        if (allGos != null && goToRemove != null)
        {

        }
    }

	private void OnReset() {
		m_memory.Reset();	
		m_dragonMemory.Reset();
		m_petsMemory.Reset();
		m_npcsMemory.Reset();
		m_worldMemory.Reset();
		//m_decorationsMemory.Reset();
		m_particlesMemory.Reset();

		UpdateUI();

		m_timer = 10f;
		m_timerUI.value = m_timer;

        m_profiler.Reset();        
    }

	private void SumAllMem() {
		m_memory.all += m_dragonMemory.all + m_petsMemory.all + m_npcsMemory.all + m_worldMemory.all /*+ m_decorationsMemory.all*/ + m_particlesMemory.all;
	}	

    private void Analize(List<GameObject> gos, string label) {
        if (gos != null) {
            int count = gos.Count;
            for (int i = 0; i < count; i++) {
                m_profiler.MoveGoToLabel(gos[i], label);
            }
        }        
    }

    private void AnalizeWorld(List<GameObject> gos) {       
        string label = LABEL_WORLD;

        FogManager fogManager = InstanceManager.fogManager;
        if (fogManager != null) {
            FogManager.FogAttributes attributes = fogManager.m_defaultAreaFog;
            if (attributes != null) {
                m_profiler.AddTexture(null, attributes.texture, label, "FogAreaTexture");                
            }

            List<FogManager.FogAttributes> attributesList = fogManager.m_generatedAttributes;
            if (attributesList != null) {
                int count = attributesList.Count;
                for (int i = 0; i < count; i++) {
                    m_profiler.AddTexture(null, attributesList[i].texture, label, "FogAreaTexture");
                }
            }
        }       

        if (gos != null) {
            int count = gos.Count;
            for (int i = 0; i < count; i++) {
                m_profiler.AddGo(gos[i], LABEL_WORLD);
            }
        }        
    }

    private void ProfilerToData(MemoryData _data) {
        string label = _data.label;
		float mesh = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Mesh, label));
		float animation = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Animation, label));
		float texture = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Texture, label));
		float particles = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.ParticleSystem, label));
        float other = BytesToMegaBytes(m_profiler.GetSizePerType(AssetMemoryGlobals.EAssetType.Other, label));
        float total = BytesToMegaBytes(m_profiler.GetSize(label));                

		_data.all += total; //should we add the "other" value?
		_data.mesh += mesh;
		_data.texture += texture;
		_data.animations += animation;
        _data.particles += particles;
        _data.other += other;
    }

	private void UpdateUI() {
		m_memoryUI.Update(m_memory);
		m_dragonUI.Update(m_dragonMemory);
		m_petsUI.Update(m_petsMemory);
		m_npcsUI.Update(m_npcsMemory);
		m_worldUI.Update(m_worldMemory);
		//m_decorationsUI.Update(m_decorationsMemory);
		m_particleSystemsUI.Update(m_particlesMemory);

        Debug.Log("Memory: " + m_memory.ToString());
        Debug.Log("Dragon: " + m_dragonMemory.ToString());
        Debug.Log("Pets: " + m_petsMemory.ToString());
        Debug.Log("Npcs: " + m_npcsMemory.ToString());
        Debug.Log("World: " + m_worldMemory.ToString());
        //Debug.Log("Decorations: " + m_decorationsMemory.ToString());
        Debug.Log("Particles: " + m_particleSystemsUI.ToString());
    }

	private void OnLevelLoaded() {
		OnReset();
    }

    private float BytesToMegaBytes(long bytes) {
        return bytes / (1024f * 1024f);
    }

    private float BytesToKiloBytes(long bytes) {
        return bytes / (1024f);
    }
}
