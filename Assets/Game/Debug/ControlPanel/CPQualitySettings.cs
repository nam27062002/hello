using SimpleJSON;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for handling the tab dedicated to show all settings which values depend on the device quality in the control panel
/// </summary>
public class CPQualitySettings : MonoBehaviour
{
    public GameObject m_prefabOption;

    void Awake()
    {
        SettingsOptions_Setup();
        Profile_Start();
        Device_Start();        
    }    

    void OnEnable()
    {
        // We want the view to be refreshed every time the tab becomes enabled so it will show the latest feature settings since server might have changed after this tab Awake was called
        Setup();
    }

    void Setup()
    {        
        PrefabOptions_Setup();
        Quality_Setup();
        Profile_Setup();
        Device_Setup();
        Audio_Setup();
    }

    void Update()
    {
        if (PrefabSettingsOption_IsDirty)
        {
            PrefabOptions_Update();            
            PrefabSettingsOption_IsDirty = false;
            FeatureSettingsManager.instance.ApplyCurrentFeatureSetting(true);

            Messenger.Broadcast(MessengerEvents.CP_QUALITY_CHANGED);
            WorldSplitter.Manager_SetLevelsLOD(FeatureSettingsManager.instance.LevelsLOD);
        }
    }

    #region prefab_settings_option
    private void PrefabSettingsOption_SetLabel(Transform t, string value)
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

    private bool PrefabSettingsOption_IsDirty { get; set; }
    #endregion

    #region settings_options
    // Key that shouldn't be printed on the console because the user shouldn't interact with them
    private string[] SETTINGS_KEYS_NOT_TO_SHOW = new string[]
    {
        FeatureSettings.KEY_SKU, FeatureSettings.KEY_PROFILE, FeatureSettings.KEY_RATING
    };

    private Dictionary<string, TMP_Dropdown.OptionData> m_settingsOptionsSingleOption;

    private class SettingsRange
    {
        public SettingsRange(float min, float max, float interval)
        {
            Setup(min, max, interval);
        }

        public void Setup(float min, float max, float interval)
        {
            Min = min;
            Max = max;
            Interval = interval;

            if (Values == null)
            {
                Values = new List<float>();
                ValuesAsStrings = new List<string>();
            }
            else
            {
                Values.Clear();
                ValuesAsStrings.Clear();
            }

            for (float i = Min; i < Max; i+= Interval)
            {
                Values.Add(i);
            }

            if (Values[Values.Count - 1] != Max)
            {
                Values.Add(Max);
            }

            int count = Values.Count;
            for (int i = 0; i < count; i++)
            {
                ValuesAsStrings.Add(Values[i] + "");
            }
        }

        public float Min { get; set; }
        public float Max { get; set; }
        public float Interval { get; set; }
        public List<float> Values { get; set; }
        public List<string> ValuesAsStrings { get; set; }
    }
    
    private Dictionary<string, TMP_Dropdown> m_settingsOptionsDropDowns;      

    private void SettingsOptions_Setup()
    {
        if (m_prefabOption != null)
        {
            SettingsOptions_Clear();

            // Show the current value of the feature settings            
            string key;
            Dictionary<string, FeatureSettings.Data> datas = FeatureSettings.Datas;
            foreach (KeyValuePair<string, FeatureSettings.Data> pair in datas)
            {
                key = pair.Key;
				if (SETTINGS_KEYS_NOT_TO_SHOW.IndexOf(key) == -1)
                {
                    Transform thisParent = transform;
                    GameObject prefabOption = (GameObject)Instantiate(m_prefabOption);
                    if (prefabOption != null)
                    {
                        prefabOption.name = "settings_" + pair.Key;
                        prefabOption.transform.parent = thisParent;
                        prefabOption.transform.SetLocalScale(1f);
                        prefabOption.SetActive(true);

                        PrefabSettingsOption_SetLabel(prefabOption.transform, pair.Key);
                        SettingsOptions_Add(pair.Value, prefabOption);
                    }
                }
            }
        }
    }    

    private void SettingsOptions_Clear()
    {
        if (m_settingsOptionsDropDowns != null)
        {
            Transform t;
            string name;
            foreach (KeyValuePair<string, TMP_Dropdown> pair in m_settingsOptionsDropDowns)
            {
                name = "settings_" + pair.Key;
                t = pair.Value.transform;
                while (t.name != name)
                {
                    t = t.parent;
                }

                if (t != null)
                {
                    t.parent = null;
                    Destroy(t.gameObject);
                }                
            }

            m_settingsOptionsDropDowns.Clear();
        }

        if (m_settingsOptionsSingleOption != null)
        {
            m_settingsOptionsSingleOption.Clear();
        }
    }

    private void SettingsOptions_Add(FeatureSettings.Data value, GameObject prefabOption)
    {
        TMP_Dropdown dropDown = prefabOption.GetComponentInChildren<TMP_Dropdown>();
        if (dropDown != null)
        {
            string key = value.Key;
            dropDown.ClearOptions();
           
            TMP_Dropdown.OptionData optionData;
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            List<string> values = FeatureSettings.GetKeyValuesAsString(value.Key);
            if (value.IsAList)
            {                            
                foreach (string v in values)
                {
                    optionData = new TMP_Dropdown.OptionData();
                    optionData.text = v;
                    options.Add(optionData);
                }
            }
            else
            {               
                if (m_settingsOptionsSingleOption == null)
                {
                    m_settingsOptionsSingleOption = new Dictionary<string, TMP_Dropdown.OptionData>();
                }

                // If no values then only the current option has to be shown
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(value.Key);
                options.Add(optionData);

                m_settingsOptionsSingleOption.Add(key, optionData);
            }
            
            dropDown.AddOptions(options);            

            if (m_settingsOptionsDropDowns == null)
            {
                m_settingsOptionsDropDowns = new Dictionary<string, TMP_Dropdown>();
            }

            m_settingsOptionsDropDowns.Add(key, dropDown);

            // Sets the current option
            PrefabOptions_SetupOption(key);
        }
    }
    
    private void PrefabOptions_Setup()
    {
        if (m_settingsOptionsSingleOption != null)
        {
            foreach (KeyValuePair<string, TMP_Dropdown.OptionData> pair in m_settingsOptionsSingleOption)
            {
                pair.Value.text = FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(pair.Key);

                // We need to force the change to make the view change
                m_settingsOptionsDropDowns[pair.Key].options = m_settingsOptionsDropDowns[pair.Key].options;
            }
        }

        if (m_settingsOptionsDropDowns != null)
        {
            foreach (KeyValuePair<string, TMP_Dropdown> pair in m_settingsOptionsDropDowns)
            {                
                PrefabOptions_SetupOption(pair.Key);
            }
        }
    }

    private void PrefabOptions_SetupOption(string key)
    {
        if (m_settingsOptionsDropDowns != null && m_settingsOptionsDropDowns.ContainsKey(key))
        {
            FeatureSettings.Data data = FeatureSettings.Datas[key];            
            if (data.IsAList)                                    
            {
                List<string> values = data.ValuesAsStrings;
                string currentValue = FeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(key);
                m_settingsOptionsDropDowns[key].value = values.IndexOf(currentValue);
            }
            else
            {
                m_settingsOptionsDropDowns[key].value = 0;
            }
        }
    }


    private void PrefabOptions_Update()
    {
        if (m_settingsOptionsDropDowns != null)
        {
            string key;
            List<string> values;
            FeatureSettings settings = FeatureSettingsManager.instance.Device_CurrentFeatureSettings;
            foreach (KeyValuePair<string, TMP_Dropdown> pair in m_settingsOptionsDropDowns)
            {
                key = pair.Key;
                values = FeatureSettings.GetKeyValuesAsString(key);
                if (values != null)                                
                {
                    settings.SetValueFromIndex(key, pair.Value.value);
                }
            }
        }
    }

    public void PrefabOptions_SetValue(int optionId)
    {
        PrefabSettingsOption_IsDirty = true;
    }
    #endregion

    #region quality
    public TextMeshProUGUI m_cpuCores;
    public TextMeshProUGUI m_cpuCoresRating;
    public TextMeshProUGUI m_memory;
    public TextMeshProUGUI m_memoryRating;
    public TextMeshProUGUI m_gfxMemory;
    public TextMeshProUGUI m_gfxMemoryRating;
    public TextMeshProUGUI m_calculatedRating;
    public TextMeshProUGUI m_calculatedProfile;
    public TextMeshProUGUI m_oldNewRating;
    public TextMeshProUGUI m_usingFormula;
    public TextMeshProUGUI m_deviceFrequency;
    public TextMeshProUGUI m_shaderLevel;

    public InputField m_audioVirtualVoices;
    public InputField m_audioRealVoices;

    private void Quality_Setup()
    {
        if (m_cpuCores != null)
        {
            m_cpuCores.text = "" + SystemInfo.processorCount;
        }

        if (m_cpuCoresRating != null)
        {
            m_cpuCoresRating.text = "" + FeatureSettingsManager.instance.Device_CPUCoresRating;
        }

        if (m_memory != null)
        {
            m_memory.text = "" + FeatureSettingsManager.instance.Device_GetSystemMemorySize();
        }

        if (m_memoryRating != null)
        {
            m_memoryRating.text = "" + FeatureSettingsManager.instance.Device_MemoryRating;
        }

        if (m_gfxMemory != null)
        {
            m_gfxMemory.text = "" + FeatureSettingsManager.instance.Device_GetGraphicsMemorySize();
        }

        if (m_gfxMemoryRating != null)
        {
            m_gfxMemoryRating.text = "" + FeatureSettingsManager.instance.Device_GfxMemoryRating;
        }

        if (m_calculatedRating != null)
        {
            m_calculatedRating.text = "" + FeatureSettingsManager.instance.Device_CalculateRating();
        }

        if (m_calculatedProfile != null)
        {
            float rating = FeatureSettingsManager.instance.Device_CalculateRating();
            int processorFrequency = FeatureSettingsManager.instance.Device_GetProcessorFrequency();
            int systemMemorySize = FeatureSettingsManager.instance.Device_GetSystemMemorySize();
            int gfxMemorySize = FeatureSettingsManager.instance.Device_GetGraphicsMemorySize();
            string profileName = FeatureSettingsManager.deviceQualityManager.Profiles_RatingToProfileName(rating, systemMemorySize, gfxMemorySize);

            m_calculatedProfile.text = profileName;
        }

        if (m_oldNewRating != null)
        {
            m_oldNewRating.text = (FeatureSettingsManager.instance.Device_UsingRatingFormula) ?
                            "old:" + FeatureSettingsManager.instance.Device_CalculatedRatingExt.ToString("N3") + " new:" + FeatureSettingsManager.instance.Device_CalculatedRating.ToString("N3"):
                            "old:" + FeatureSettingsManager.instance.Device_CalculatedRating.ToString("N3") + " new:" + FeatureSettingsManager.instance.Device_CalculatedRatingExt.ToString("N3");
        }


        if (m_usingFormula != null)
        {
            m_usingFormula.text = (FeatureSettingsManager.instance.Device_UsingRatingFormula) ? "new" : "old";
        }

        if (m_deviceFrequency != null)
        {
            m_deviceFrequency.text = "" + SystemInfo.processorFrequency;
        }

        if (m_shaderLevel != null)
        {
            m_shaderLevel.text = "" + SystemInfo.graphicsShaderLevel;
        }


    }
    #endregion    

    #region profile
    public TMP_Dropdown m_profileDropDown;

    private void Profile_Start()
    {
        if (m_profileDropDown != null)
        {
            m_profileDropDown.ClearOptions();

            TMP_Dropdown.OptionData optionData;
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            List<string> values = FeatureSettingsManager.instance.Profiles_Names;
            foreach (string v in values)
            {
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = v;
                options.Add(optionData);
            }

            m_profileDropDown.AddOptions(options);            
        }        
    }

    private void Profile_Setup()
    {
        if (m_profileDropDown != null)
        {
            // Sets the current option
            string currentValue = FeatureSettingsManager.instance.Device_CurrentProfile;
            List<string> values = FeatureSettingsManager.instance.Profiles_Names;
            m_profileDropDown.value = values.IndexOf(currentValue);
        }
    }

    public void Profile_SetOption(int option)
    {
        FeatureSettingsManager manager = FeatureSettingsManager.instance;
        if (manager.Device_CurrentProfile != manager.Profiles_Names[m_profileDropDown.value])
        {
            // The model is cleared because we are forcing a profile so the model information is not used anymore
            manager.Device_Model = null;
            manager.Device_CurrentProfile = manager.Profiles_Names[m_profileDropDown.value];

            // The view has to be updated so it will show the configuration for the new profile
            Setup();
        }
    }
    #endregion

    #region device
    public TMP_Dropdown m_deviceDropDown;
    private List<string> m_deviceNames;

    private void Device_Start()
    {
        if (m_deviceDropDown != null)
        {
            m_deviceDropDown.ClearOptions();

            TMP_Dropdown.OptionData optionData;
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            if (m_deviceNames == null)
            {
                Dictionary<string, DefinitionNode> deviceDefs = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.FEATURE_DEVICE_SETTINGS);
                m_deviceNames = new List<string>();
                foreach (KeyValuePair<string, DefinitionNode> pair in deviceDefs)
                {
                    m_deviceNames.Add(pair.Key);
                }
                m_deviceNames.Add("None");
            }

            foreach (string v in m_deviceNames)
            {
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = v;
                options.Add(optionData);
            }

            m_deviceDropDown.AddOptions(options);           
        }
    }

    private void Device_Setup()
    {
        if (m_deviceDropDown != null)
        {
            // Sets the current option
            string currentValue = FeatureSettingsManager.instance.Device_Model;
            // int index = m_deviceNames.IndexOf(currentValue);
			int index = m_deviceNames.Count - 1;;
            if ( !string.IsNullOrEmpty(currentValue) )
            {
	            for( int i = 0 ; i < m_deviceNames.Count; i++ )
	            {
	            	if ( currentValue.Contains( m_deviceNames[i] ) )
	                	index = i;
	            }
            }

            m_deviceDropDown.value = index;
        }
    }

    private void Audio_Setup()
    {
        AudioConfiguration config = AudioSettings.GetConfiguration();
        m_audioVirtualVoices.text = config.numVirtualVoices.ToString();
        m_audioRealVoices.text = config.numRealVoices.ToString();
    }

    public void OnAudioVoicesSetup()
    {
        var config = AudioSettings.GetConfiguration();
        config.numVirtualVoices = int.Parse(m_audioVirtualVoices.text);
        config.numRealVoices = int.Parse( m_audioRealVoices.text );
        AudioSettings.Reset(config);
    }

    public void Device_SetOption(int option)
    {
        FeatureSettingsManager manager = FeatureSettingsManager.instance;
        if (manager.Device_Model != m_deviceNames[m_deviceDropDown.value])
        {
            manager.Device_Model = m_deviceNames[m_deviceDropDown.value];

            // We want the configuration for this device to be used
            manager.RestoreCurrentFeatureSettingsToDevice();
            
            // The view has to be updated so it will show the configuration for the new profile
            Setup();
        }
    }
    #endregion
}