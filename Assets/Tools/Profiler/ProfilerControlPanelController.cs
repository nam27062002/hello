﻿using System.Collections.Generic;
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
                            prefabOption.transform.parent = thisParent;
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
            GameCamera gameCamera = InstanceManager.gameCamera;
            if (gameCamera != null)
            {
                bool enabled = !m_bossCameraAffector.enabled;
                m_bossCameraAffector.enabled = enabled;

                if (enabled)
                {
                    gameCamera.NotifyBoss(m_bossCameraAffector);
                }
                else
                {
                    gameCamera.RemoveBoss(m_bossCameraAffector);
                }                
            }
        }        
    }
    #endregion

}