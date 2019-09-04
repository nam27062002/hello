using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


/// <summary>
/// This class is responsible for handling the column dedicated to the spawner tool in the control panel
/// </summary>
public class ProfilerControlPanelController : MonoBehaviour
{
    [SerializeField]
    public TMP_Dropdown m_numEntities;

    [SerializeField]
    public TMP_Dropdown m_prefabToSpawn;
    private List<string> m_prefabNames;

    private const int MAX_NUM_ENTITIES = 200;
    private const int NUM_ENTITIES_PERIOD = 10;

	[Space]
    public GameObject m_prefabOption;

    void Awake()
    {
        if (!ProfilerSettingsManager.IsLoaded)
        {
            ProfilerSettingsManager.Load(null);
        }
    }

    void Start()
    {
        List<TMP_Dropdown.OptionData> options = null;

        // Options for the num entities drop down are created
        if (m_numEntities != null)
        {
            m_numEntities.ClearOptions();

            TMP_Dropdown.OptionData optionData;
            options = new List<TMP_Dropdown.OptionData>();
            for (int i = NUM_ENTITIES_PERIOD; i <= MAX_NUM_ENTITIES; i += NUM_ENTITIES_PERIOD)
            {
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = i + "";
                options.Add(optionData);
            }

            m_numEntities.AddOptions(options);

            int value = (ProfilerSettingsManager.Spawner_NumEntities / NUM_ENTITIES_PERIOD) - 1;
            if (value >= MAX_NUM_ENTITIES / NUM_ENTITIES_PERIOD)
            {
                value = MAX_NUM_ENTITIES / NUM_ENTITIES_PERIOD;
            }

            m_numEntities.value = value;
            options = null;
        }

        // Options for the prefab drop down have to be created        
        if (m_prefabToSpawn != null)
        {
            m_prefabToSpawn.ClearOptions();
            options = new List<TMP_Dropdown.OptionData>();
        }

        ProfilerSettings settings = ProfilerSettingsManager.SettingsCached;
        if (settings != null)
        {
            Dictionary<string, ProfilerSettings.SpawnerData> spawnerDatas = settings.SpawnerDatas;
            if (spawnerDatas != null)
            {
                m_prefabNames = new List<string>();

                TMP_Dropdown.OptionData optionData;
                foreach (KeyValuePair<string, ProfilerSettings.SpawnerData> pair in spawnerDatas)
                {
                    m_prefabNames.Add(pair.Key);

                    if (options != null)
                    {
                        optionData = new TMP_Dropdown.OptionData();
                        optionData.text = pair.Key;
                        options.Add(optionData);
                    }

                    if (m_prefabOption != null)
                    {
                        GameObject prefabOption = (GameObject)Instantiate(m_prefabOption);
                        if (prefabOption != null)
                        {
                            prefabOption.transform.SetParent(m_prefabOption.transform.parent);
                            prefabOption.transform.SetLocalScale(1f);
                            prefabOption.SetActive(true);

                            PrefabLogicUnits_SetLabel(prefabOption.transform, pair.Key);

                            //PrefabSlider_Add(pair.Key, pair.Value.logicUnits, prefabOption);                                                        
                            PrefabOptions_Add(pair.Key, pair.Value.logicUnits, prefabOption);
                        }
                    }
                }

				if(m_prefabOption != null) m_prefabOption.SetActive(false);
            }
        }

        if (m_prefabToSpawn != null)
        {
            m_prefabToSpawn.AddOptions(options);
        }

        if (m_prefabNames != null)
        {
            string prefabToSpawn = ProfilerSettingsManager.Spawner_Prefab;

            // If there's no prefab to spawn assigned yet then the first of the list should be used
            if (string.IsNullOrEmpty(prefabToSpawn))
            {
                if (m_prefabNames.Count > 0)
                {
                    SetPrefabToSpawn(0);
                }
            }
            else
            {
                int index = m_prefabNames.IndexOf(ProfilerSettingsManager.Spawner_Prefab);
                if (index > -1)
                {
                    m_prefabToSpawn.value = index;
                }
            }
        }

        Checkpoints_Start();
    }

    void OnEnable()
    {
        Scene_OnEnable();
        Layers_Init();
    }

    void Update()
    {
        if (PrefabLogicUnits_IsDirty)
        {
            ProfilerSettings settings = ProfilerSettingsManager.SettingsCached;
            if (settings != null)
            {
                //PrefabSliders_Update(settings);
                PrefabOptions_Update(settings);

                if (settings.isDirty)
                {
                    ProfilerSettingsManager.SaveToCache();
                }
            }

            PrefabLogicUnits_IsDirty = false;
        }

        Layers_Update();
    }

    public void SetNumEntities(int optionId)
    {
        ProfilerSettingsManager.Spawner_NumEntities = (optionId + 1) * NUM_ENTITIES_PERIOD;
    }

    public void SetPrefabToSpawn(int optionId)
    {
        if (m_prefabNames != null && optionId > -1 && optionId < m_prefabNames.Count)
        {
            ProfilerSettingsManager.Spawner_Prefab = m_prefabNames[optionId];
        }
    }

    #region prefab_logic_units
    private void PrefabLogicUnits_SetLabel(Transform t, string value)
    {
        if (t != null)
        {
            TextMeshProUGUI[] labels = t.GetComponentsInChildren<TextMeshProUGUI>();
            if (labels != null)
            {
                int count = labels.Length;
                for (int i = 0; i < count; i++)
                {
                    if (labels[i].name == "Label")
                    {
                        labels[i].text = value;
                        break;
                    }
                }
            }
        }
    }

    private bool PrefabLogicUnits_IsDirty { get; set; }
    #endregion

    #region prefab_sliders
    private Dictionary<string, Slider> m_prefabSliders;

    private void PrefabSlider_Add(string prefabName, float logicUnits, GameObject prefabOption)
    {
        Slider slider = prefabOption.GetComponentInChildren<Slider>();
        if (slider != null)
        {

            slider.value = logicUnits;

            if (m_prefabSliders == null)
            {
                m_prefabSliders = new Dictionary<string, Slider>();
            }

            m_prefabSliders.Add(prefabName, slider);
        }
    }

    private void PrefabSliders_Update(ProfilerSettings settings)
    {
        if (m_prefabSliders != null && settings != null)
        {
            foreach (KeyValuePair<string, Slider> pair in m_prefabSliders)
            {
                settings.SetLogicUnits(pair.Key, pair.Value.value);
            }
        }
    }

    public void PrefabSliders_SetLogicUnits()
    {
        PrefabLogicUnits_IsDirty = true;
    }
    #endregion

    #region prefab_options
    private const int MAX_LOGIC_UNITS = 10;
    private const float LOGIC_UNITS_PERIOD = 0.5f;

    private Dictionary<string, TMP_Dropdown> m_prefabDropDowns;

    private void PrefabOptions_Add(string prefabName, float logicUnits, GameObject prefabOption)
    {
        TMP_Dropdown dropDown = prefabOption.GetComponentInChildren<TMP_Dropdown>();
        if (dropDown != null)
        {
            dropDown.ClearOptions();

            float min = float.MaxValue;
            float diff;
            float value = LOGIC_UNITS_PERIOD;

            TMP_Dropdown.OptionData optionData;
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            for (float i = LOGIC_UNITS_PERIOD; i <= MAX_LOGIC_UNITS; i += LOGIC_UNITS_PERIOD)
            {
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = i + "";
                options.Add(optionData);
                diff = Mathf.Abs(logicUnits - i);
                if (diff < min)
                {
                    min = diff;
                    value = i;
                }
            }

            dropDown.AddOptions(options);
            dropDown.value = ((int)(value / LOGIC_UNITS_PERIOD)) - 1;

            if (m_prefabDropDowns == null)
            {
                m_prefabDropDowns = new Dictionary<string, TMP_Dropdown>();
            }

            m_prefabDropDowns.Add(prefabName, dropDown);
        }
    }

    private void PrefabOptions_Update(ProfilerSettings settings)
    {
        if (m_prefabDropDowns != null && settings != null)
        {
            foreach (KeyValuePair<string, TMP_Dropdown> pair in m_prefabDropDowns)
            {
                settings.SetLogicUnits(pair.Key, (pair.Value.value + 1) * LOGIC_UNITS_PERIOD);
            }
        }
    }

    public void PrefabOptions_SetLogicUnits(int optionId)
    {
        PrefabLogicUnits_IsDirty = true;
    }
    #endregion

    #region checkpoints
    private enum ECheckpoint
    {
        //---------------------- Village area ----------------------
        Village_StartingPoint,
        Village_FloatingRock,
        Village_HangingBridge,
        Village_VineBigLeaf,
        Village_WoodsCabins,
        Village_WoodsLeaves,
        Village_Ruins1,
        Village_Ruins2,
        Village_Witches_Woods,
        Village_Goblins_City,
		Village_Village,
		Village_BigTurret1,
		Village_BigTurret2,
		Village_BigTurret3,
        Village_TunnelToCastleThruWater,
        Village_TunnelToCastle,

        //---------------------- Castle area ----------------------
        Castle_TunnelToVillage,
        Castle_Wheel,
        Castle_Castle,
        Castle_Tunnel,
        Castle_Dungeons,
        Castle_Caves,
        Castle_Bridge,
        Castle_Goblin_Mines,
        Castle_Goblin_Mines_Test,
		Castle_Indoors,

        //---------------------- Dark area ----------------------
        Village_TunnelToDark,
		Village_TwisterCaveToDark,
        Dark_TunnelToVillage,
		Dark_ToTwisterCave,
        Dark_WitchesTree
	}

    private Vector3[] m_checkpointsPositions = new Vector3[]
    {
        //---------------------- Village area ----------------------
		new Vector3(-180, 119, 0f),
        new Vector3(-174, 75, 0f),
        new Vector3(-132, -21, 0f),
        new Vector3(-285, 45, 0f),
        new Vector3(137, 51, 0f),
        new Vector3(280, 42, 0f),
        new Vector3(132, 232, 0f),
        new Vector3(280, 190, 0f),
        new Vector3(-394, -54, 0f),
        new Vector3(-450, 1.78f, 0f),
		new Vector3(-102, 47, 0f),
		new Vector3(17, 168, 0f),
		new Vector3(371, 250, 0f),
		new Vector3(744, 182, 0f),
        new Vector3(-60, -129, 0f),
        new Vector3(370, 81, 0f),                         

        //---------------------- Castle area ----------------------
        new Vector3(414, 68, 0f),
        new Vector3(555, -177, 0f),
        new Vector3(598, -3, 0f),
        new Vector3(598, -46, 0f),
        new Vector3(566, -62, 0f),
        new Vector3(522, -61, 0f),
        new Vector3(460, -11, 0f),
        new Vector3(422, -239, 0f),
        new Vector3(554, -227, 0f),
		new Vector3(775, 55, 0f),        

        //---------------------- Dark area ----------------------
        new Vector3(-625, 100, 0f),
		new Vector3(-665, -105, 0f),
        new Vector3(-663, 92, 0f),
		new Vector3(-729, -103, 0f),
		new Vector3(-736, 107),
    };

	[Space]
    public TMP_Dropdown m_checkpoints;

    private void Checkpoints_Start()
    {
        if (m_checkpoints != null)
        {
            List<TMP_Dropdown.OptionData> options = null;
            m_checkpoints.ClearOptions();

            TMP_Dropdown.OptionData optionData;
            options = new List<TMP_Dropdown.OptionData>();
            int count = m_checkpointsPositions.Length;
            for (int i = 0; i < count; i++)
            {
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = ((ECheckpoint)i).ToString();
                options.Add(optionData);
            }

            m_checkpoints.AddOptions(options);
        }
    }

    public void Checkpoints_SetOptionId(int optionId)
    {
        if (InstanceManager.player != null)
        {
            InstanceManager.player.transform.position = m_checkpointsPositions[optionId];
        }
    }
    #endregion

    #region occlusion
    public void Occlusion_OnChangedValue(bool newValue)
    {
        if (InstanceManager.gameCamera != null)
        {
            Camera camera = InstanceManager.gameCamera.GetComponent<Camera>();
            if (camera != null)
            {
                camera.useOcclusionCulling = newValue;
            }
        }
    }
    #endregion

    #region entities_visibility
    public void EntitiesVisibility_OnChangedValue(bool newValue)
    {
        if (EntityManager.instance != null)
        {
            EntityManager.instance.Debug_EntitiesVisibility = newValue;
        }
    }
    #endregion

    #region particles
    public void Particles_VisibilityOnChangedValue(bool newValue)
    {
        if (ApplicationManager.instance != null)
        {
            ApplicationManager.instance.Debug_ParticlesVisibility = newValue;
        }
    }

    public void Particles_StateOnChangedValue(int option)
    {
        ApplicationManager.instance.Debug_SetParticlesState(option);
    }

    public void Particles_PlayerVisibilityOnChangedValue(bool newValue)
    {
        if (ApplicationManager.instance != null)
        {
            ApplicationManager.instance.Debug_PlayerParticlesVisibility = newValue;
        }
    }

    public void Particles_CustomCullingIsEnabledOnChangedValue(bool newValue)
    {
        CustomParticlesCulling.Manager_IsEnabled = newValue;
    }
    #endregion

    #region ground
    public void Ground_VisibleOnChangedValue(bool newValue)
    {
        if (ApplicationManager.instance != null)
        {
            ApplicationManager.instance.Debug_GroundVisibility = newValue;
        }
    }
    #endregion

    #region test
    public BossCameraAffector m_bossCameraAffector;


    public void Test_OnToggleFrameColorEffect()
    {
        ApplicationManager.instance.Debug_TestToggleFrameColor();
    }

    public void Test_OnToggleBossCameraEffect()
    {
        if (m_bossCameraAffector != null)
        {
            ApplicationManager.instance.Debug_OnToggleBossCameraEffect(m_bossCameraAffector);
        }
    }

    public void Test_ToggleBakedLights(bool value)
    {
        ApplicationManager.instance.Debug_DisableBakedLights(value);
    }
    public void Test_ToggleColliders(bool value)
    {
        ApplicationManager.instance.Debug_DisableColliders(value);
    }

    #endregion

    #region Scene
    public TMP_Text mSceneGoToMemoryText;

    private void Scene_OnEnable()
    {
        if (mSceneGoToMemoryText != null)
        {
            mSceneGoToMemoryText.transform.parent.gameObject.SetActive(false);
            if (GameSceneManager.nextScene == ProfilerMemoryController.NAME)
            {
                mSceneGoToMemoryText.text = "Go To Menu";
            }
            else
            {
                mSceneGoToMemoryText.text = "Go To Memory Scene";
            }
        }
    }

    public void Scene_OnGoToMemorySceneClicked()
    {
        ApplicationManager.instance.Debug_ToggleProfilerMemoryScene();
        ApplicationManager.instance.Debug_ToggleProfilerLoadScenesScene();
    }

	public void OnSendNotification() {
		ApplicationManager.instance.Debug_ScheduleNotification();
	}
    #endregion

    #region game_scene
    private class SceneState
    {
        private Dictionary<GameObject, bool> State;

        public void Toggle(string sceneName)
        {
            UnityEngine.SceneManagement.Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (s.isLoaded)
            {
                bool isActive = State == null;
                if (isActive)
                {
                    State = new Dictionary<GameObject, bool>();
                }
                else
                {
                    State = null;
                }

                GameObject go;
                GameObject[] allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    go = allGameObjects[j];

                    if (State != null)
                    {
                        State.Add(go, go.activeSelf);
                    }

                    go.SetActive(!isActive);
                }
            }
        }
    }

    private static Dictionary<string, SceneState> GameScene_States;

    public static void GameScene_Toggle(string sceneName)
    {
        if (GameScene_States == null)
        {
            GameScene_States = new Dictionary<string, SceneState>();
        }

        if (!GameScene_States.ContainsKey(sceneName))
        {
            GameScene_States.Add(sceneName, new SceneState());
        }

        SceneState state = GameScene_States[sceneName];
        state.Toggle(sceneName);
    }

    public void GameScene_BgSkyVisibleOnChangedValue(bool newValue)
    {
        GameScene_Toggle("ART_Levels_Background_Skies");
    }

    public void GameScene_BgAreaVisibleOnChangedValue(bool newValue)
    {
        GameScene_Toggle("ART_L1_Background_Village");
    }

    public void GameScene_FortressVisibleOnChangedValue(bool newValue)
    {
        GameScene_Toggle("ART_L1_Village_Fortress");
    }

	public void GameScene_HideMeshesAtDistance(float _distance) {
		if (ApplicationManager.instance != null)
		{
			ApplicationManager.instance.Debug_DisableMeshesAt(_distance);
		}
	}
    #endregion

    #region layers

	[Space]
    public TMP_Dropdown m_layersHoldersDropdown;
    private List<ProfilerLayers> m_layersHolders;
    public GameObject m_layerPrefab;

    private List<GameObject> m_layersGOs;
    private List<Toggle> m_layersGOsToggles;

    private void Layers_Init()
    {
        Layers_Destroy();

        if (m_layersHoldersDropdown != null)
        {
            m_layersHoldersDropdown.ClearOptions();

            m_layersHolders = GameObjectExt.FindObjectsOfType<ProfilerLayers>(true);
            if (m_layersHolders != null)
            {
                List<TMP_Dropdown.OptionData> options = null;                

                TMP_Dropdown.OptionData optionData;
                options = new List<TMP_Dropdown.OptionData>();
                int count = m_layersHolders.Count;
                for (int i = 0; i < count; i++)
                {
                    optionData = new TMP_Dropdown.OptionData();
                    optionData.text = m_layersHolders[i].gameObject.scene.name;
                    options.Add(optionData);
                }

                m_layersHoldersDropdown.AddOptions(options);

                if (count > 0)
                {
                    Layers_SetOptionId(0);
                }
            }
        }
    }
    
    private void Layers_Destroy()
    {
        m_layersHolders = null;
        Layers_DestroyLayersUI();
        Layers_IsDirty = false;
    }

    public void Layers_SetOptionId(int optionId)
    {
       if (m_layersHolders != null)
       {
            optionId = m_layersHoldersDropdown.value;
            ProfilerLayers profilerLayers = m_layersHolders[optionId];
            Layers_CreateLayersUI(profilerLayers);
       }
    }

    private void Layers_CreateLayersUI(ProfilerLayers profilerLayers)
    {
        // We need to destroy the previous one
        Layers_DestroyLayersUI();        
        
        if (profilerLayers != null && profilerLayers.m_layers != null)
        {
            if (m_layersGOs == null)
            {
                m_layersGOs = new List<GameObject>();
            }
            else
            {
                m_layersGOs.Clear();
            }

            if (m_layersGOsToggles == null)
            {
                m_layersGOsToggles = new List<Toggle>();
            }
            else
            {
                m_layersGOsToggles.Clear();
            }

            Transform thisParent = m_layerPrefab.transform.parent;
            int count = profilerLayers.m_layers.Count;
            GameObject go;
            for (int i = 0; i < count; i++)
            {
                go = Instantiate(m_layerPrefab, thisParent);
                go.SetActive(true);
                go.name = profilerLayers.m_layers[i].name;

                TextMeshProUGUI label = go.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = go.name;
                }

                Toggle toggle = go.GetComponentInChildren<Toggle>();
                if (toggle != null)
                {
                    toggle.isOn = profilerLayers.m_layers[i].activeSelf;                    
                }

                m_layersGOs.Add(go);
                m_layersGOsToggles.Add(toggle);
            }
			m_layerPrefab.SetActive(false);

		}
    }

    private void Layers_DestroyLayersUI()
    {
        if (m_layersGOs != null)
        {
            int count = m_layersGOs.Count;
            for (int i = 0; i < count; i++)
            {
                Destroy(m_layersGOs[i]);
            }

            m_layersGOs = null;
        }

        m_layersGOsToggles = null;        
    }

    public void Layers_OnLayerValueChanged(bool newValue)
    {
        Layers_IsDirty = true;
    }

    private bool Layers_IsDirty { get; set; }

    private void Layers_Update()
    {
        if (m_layersGOs != null && Layers_IsDirty)
        {
            Layers_IsDirty = false;

            ProfilerLayers layersHolder = m_layersHolders[m_layersHoldersDropdown.value];
            if (layersHolder != null)
            {
                int count = m_layersGOs.Count;
                Toggle toggle;
                for (int i = 0; i < count; i++)
                {
                    toggle = m_layersGOsToggles[i];
                    if (toggle != null)
                    {
                        layersHolder.m_layers[i].SetActive(toggle.isOn);                        
                    }
                }
            }
        }
    }
    #endregion

    #region render_queues

    private GameObject[] getBackgroundGameObjects(bool foreground)
    {
        List<GameObject> result = new List<GameObject>();

        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (foreground)
            {
                if (scene.name.Contains("ART") && !scene.name.Contains("ART_Levels_Background_Skies") && !scene.name.Contains("ART_L1_Background_Village") && !scene.name.Contains("ART_L1_Background_Castle") && !scene.name.Contains("ART_L1_Background_Dark"))
                {
                    if (scene.isLoaded)
                    {
                        GameObject[] goRoots = scene.GetRootGameObjects();
                        result.AddRange(goRoots);
                    }
                }
            }
            else
            {
                if (scene.name.Contains("ART_Levels_Background_Skies") || scene.name.Contains("ART_L1_Background_Village") || scene.name.Contains("ART_L1_Background_Castle") || scene.name.Contains("ART_L1_Background_Dark"))
                {
                    if (scene.isLoaded)
                    {
                        GameObject[] goRoots = scene.GetRootGameObjects();
                        result.AddRange(goRoots);
                    }
                }
            }
        }

        return result.ToArray();
    }

    private Renderer[] getBackgroundRenderers(bool foreground)
    {
        List<Renderer> result = new List<Renderer>();

        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (foreground)
            {
                if (scene.name.Contains("ART") && !scene.name.Contains("ART_Levels_Background_Skies") && !scene.name.Contains("ART_L1_Background_Village") && !scene.name.Contains("ART_L1_Background_Castle") && !scene.name.Contains("ART_L1_Background_Dark"))
                {
                    if (scene.isLoaded)
                    {
                        GameObject[] goRoots = scene.GetRootGameObjects();
                        for (int c = 0; c < goRoots.Length; c++)
                        {
                            result.AddRange(goRoots[c].GetComponentsInChildren<Renderer>(false));
                        }
                    }
                }
            }
            else
            {
                if (scene.name.Contains("ART_Levels_Background_Skies") || scene.name.Contains("ART_L1_Background_Village") || scene.name.Contains("ART_L1_Background_Castle") || scene.name.Contains("ART_L1_Background_Dark"))
                {
                    if (scene.isLoaded)
                    {
                        GameObject[] goRoots = scene.GetRootGameObjects();
                        for (int c = 0; c < goRoots.Length; c++)
                        {
                            result.AddRange(goRoots[c].GetComponentsInChildren<Renderer>(false));
                        }
                    }
                }
            }
        }

        return result.ToArray();
    }


    public void Background_OnBackgroundValueChanged(bool newValue)
    {
        GameObject[] go = getBackgroundGameObjects(false);
        for (int c = 0; c < go.Length; c++)
        {
//            if (renderers[c].material.shader.name.Contains("Hungry Dragon/Scenary/Scenary Standard"))
                go[c].SetActive(newValue);
        }
    }
    public void Foreground_OnForegroundValueChanged(bool newValue)
    {
        GameObject[] go = getBackgroundGameObjects(true);
        for (int c = 0; c < go.Length; c++)
        {
//            if (renderers[c].material.shader.name.Contains("Hungry Dragon/Scenary/Scenary Standard"))
                go[c].SetActive(newValue);
        }
    }

    private struct QueueBackup
    {
        public bool isNPC;
        public int oldQueue;
    };

    private QueueBackup[] m_oldQueues = null;
    private int[] m_backgroundQueues = null;
    private int[] m_foregroundQueues = null;

    public void Queues_OnQueuesValueChanged(bool newValue)
    {
        int changed = 0;

        List<Renderer> renderer = GameObjectExt.FindObjectsOfType<Renderer>(false);

        if (!newValue)
        {
            m_oldQueues = new QueueBackup[renderer.Count];
        }

        for (int c = 0; c < renderer.Count; c++)
        {
            Material mat = renderer[c].material;
            if (mat.shader.name.Contains("Hungry Dragon/Dragon/Dragon standard") && mat.GetTag("RenderType", false).Contains("Opaque"))
            {
                if (newValue)
                {
                    if (m_oldQueues != null)
                    {
                        mat.renderQueue = m_oldQueues[c].oldQueue;
                        changed++;
                    }
                }
                else
                {
                    m_oldQueues[c].oldQueue = mat.renderQueue;
                    m_oldQueues[c].isNPC = false;
                }
            }
            else if (mat.shader.name.Contains("NPC") && mat.GetTag("RenderType", false).Contains("Opaque"))
            {
                if (newValue)
                {
                    if (m_oldQueues != null)
                    {
                        mat.renderQueue = m_oldQueues[c].oldQueue;
                        changed++;
                    }
                }
                else
                {
                    m_oldQueues[c].oldQueue = mat.renderQueue;
                    m_oldQueues[c].isNPC = true;
                }
            }
            else
            {
                if (!newValue)
                {
                    m_oldQueues[c].oldQueue = -1;
                }
            }
        }

        if (newValue)
        {
            m_oldQueues = null;
        }
        else
        {            
            for (int c = 0; c < renderer.Count; c++)
            {
                if (c < m_oldQueues.Length && m_oldQueues[c].oldQueue != -1)
                {
                    if (m_oldQueues[c].isNPC)
                    {
                        renderer[c].material.renderQueue = 1000;
                    }
                    else
                    {
                        renderer[c].material.renderQueue = 500;
                    }
                    changed++;
                }
            }
        }

        renderer.Clear();
        renderer.AddRange(getBackgroundRenderers(false));
        if (!newValue)
        {
            m_backgroundQueues = new int[renderer.Count];
        }

        for (int c = 0; c < renderer.Count; c++)
        {
            string renderType = renderer[c].material.GetTag("RenderType", false);
            if (renderType.Contains("Opaque") || renderType.Contains("TransparentCutout"))
            {
                if (newValue)
                {
                    if (m_backgroundQueues != null)
                    {
                        renderer[c].material.renderQueue = m_backgroundQueues[c];
                        changed++;
                    }
                }
                else
                {
                    m_backgroundQueues[c] = renderer[c].material.renderQueue;
                }
            }
        }

        if (newValue)
        {
            m_backgroundQueues = null;
        }
        else
        {
            for (int c = 0; c < renderer.Count; c++)
            {
                string renderType = renderer[c].material.GetTag("RenderType", false);
                if (renderType.Contains("Opaque") || renderType.Contains("TransparentCutout"))
                {
                    renderer[c].material.renderQueue = 2500;
                    changed++;
                }
            }
        }


        renderer.Clear();
        renderer.AddRange(getBackgroundRenderers(true));
        if (!newValue)
        {
            m_foregroundQueues = new int[renderer.Count];
        }

        for (int c = 0; c < renderer.Count; c++)
        {
            string renderType = renderer[c].material.GetTag("RenderType", false);
            if (renderType.Contains("Opaque") || renderType.Contains("TransparentCutout"))
            {
                if (newValue)
                {
                    if (m_foregroundQueues != null)
                    {
                        renderer[c].material.renderQueue = m_foregroundQueues[c];
                        changed++;
                    }
                }
                else
                {
                    m_foregroundQueues[c] = renderer[c].material.renderQueue;
                }
            }
        }

        if (newValue)
        {
            m_foregroundQueues = null;
        }
        else
        {
            for (int c = 0; c < renderer.Count; c++)
            {
                string renderType = renderer[c].material.GetTag("RenderType", false);
                if (renderType.Contains("Opaque") || renderType.Contains("TransparentCutout"))
                {
                    renderer[c].material.renderQueue = 2000;
                    changed++;
                }
            }
        }

        Debug.Log("Materials changed: " + changed);
    }
    #endregion


    public void OnDisableGoldenMaterial(bool newValue) {
        ViewControl.sm_allowGoldenMaterial = !newValue;
    }
}