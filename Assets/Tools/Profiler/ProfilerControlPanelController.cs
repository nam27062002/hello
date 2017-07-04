using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
                        Transform thisParent = transform;
                        GameObject prefabOption = (GameObject)Instantiate(m_prefabOption);
                        if (prefabOption != null)
                        {
                            prefabOption.transform.SetParent(thisParent);
                            prefabOption.transform.SetLocalScale(1f);
                            prefabOption.SetActive(true);

                            PrefabLogicUnits_SetLabel(prefabOption.transform, pair.Key);

                            //PrefabSlider_Add(pair.Key, pair.Value.logicUnits, prefabOption);                                                        
                            PrefabOptions_Add(pair.Key, pair.Value.logicUnits, prefabOption);
                        }
                    }
                }
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
        Stats_Start();
		FPS_Start();
    }

    void OnEnable()
    {
        Scene_OnEnable();
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
        Village_Witches_Woods,
        Village_Goblins_City,
        Village_TunnelToCastle,

        //---------------------- Castle area ----------------------
        Castle_TunnelToVillage,
        Castle_Castle,
        Castle_Tunnel,
        Castle_Dungeons,
        Castle_Caves,
        Castle_Bridge,

        //---------------------- Dark area ----------------------
        //Dark_Woods
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
        new Vector3(-394, -54, 0f),
        new Vector3(-450, 1.78f, 0f),
        new Vector3(360, 69, 0f),                

        //---------------------- Castle area ----------------------
        new Vector3(414, 68, 0f),
        new Vector3(598, -3, 0f),
        new Vector3(598, -46, 0f),
        new Vector3(566, -62, 0f),
        new Vector3(522, -61, 0f),
        new Vector3(460, -11, 0f),

        //---------------------- Dark area ----------------------
    };

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

    #region stats
    public Toggle m_statsToggle;

    private void Stats_Start()
    {
        if (m_statsToggle != null && ControlPanel.instance != null)
        {
            m_statsToggle.isOn = ControlPanel.instance.IsStatsEnabled;
        }
    }

    public void Stats_OnChangedValue(bool newValue)
    {
        if (ControlPanel.instance != null)
        {
            ControlPanel.instance.IsStatsEnabled = m_statsToggle.isOn;
        }
    }
    #endregion

	#region fps
	public Toggle m_fpsToggle;

	private void FPS_Start()
	{
		if (m_fpsToggle != null && ControlPanel.instance != null)
		{
			m_fpsToggle.isOn = ControlPanel.instance.IsFPSEnabled;
		}
	}

	public void FPS_OnChangedValue(bool newValue)
	{
		if (ControlPanel.instance != null)
		{
			ControlPanel.instance.IsFPSEnabled = m_fpsToggle.isOn;
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

    public void Test_OnToggleDrunkEffect()
    {
        ApplicationManager.instance.Debug_TestToggleDrunk();
    }

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
    #endregion

    #region Scene
    public TMP_Text mSceneGoToMemoryText;

    private void Scene_OnEnable()
    {     
        if (mSceneGoToMemoryText != null)
        {
            /*mSceneGoToMemoryText.transform.parent.gameObject.SetActive(false);
            if (GameSceneManager.nextScene == ProfilerMemoryController.NAME)
            {
                mSceneGoToMemoryText.text = "Go To Menu";
            }
            else
            {
                mSceneGoToMemoryText.text = "Go To Memory Scene";
            }*/
            mSceneGoToMemoryText.text = "Send Notif";
        }
    }

    public void Scene_OnGoToMemorySceneClicked()
    {
        //ApplicationManager.instance.Debug_ToggleProfilerMemoryScene();
        //ApplicationManager.instance.Debug_ToggleProfilerLoadScenesScene();
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
            if (s != null && s.isLoaded)
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
    #endregion
}