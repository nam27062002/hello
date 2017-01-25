﻿using SimpleJSON;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// This class is responsible for handling the tab dedicated to show all settings which values depend on the device quality in the control panel
/// </summary>
public class CPQualitySettings : MonoBehaviour
{
    public GameObject m_prefabOption;

    void Awake()
    {        
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
        SettingsOptions_Setup();
        PrefabOptions_Setup();
        Quality_Setup();
        Profile_Setup();
        Device_Setup();
    }

    void Update()
    {
        if (PrefabSettingsOption_IsDirty)
        {
            PrefabOptions_Update();
            Messenger.Broadcast(GameEvents.CP_QUALITY_CHANGED);
            PrefabSettingsOption_IsDirty = false;
            GameFeatureSettingsManager.instance.ApplyCurrentFeatureSetting();
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
            Dictionary<string, GameFeatureSettings.Data> datas = GameFeatureSettings.Datas;
            foreach (KeyValuePair<string, GameFeatureSettings.Data> pair in datas)
            {
                key = pair.Key;
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
    }

    private void SettingsOptions_Add(GameFeatureSettings.Data value, GameObject prefabOption)
    {
        TMP_Dropdown dropDown = prefabOption.GetComponentInChildren<TMP_Dropdown>();
        if (dropDown != null)
        {
            string key = value.Key;
            dropDown.ClearOptions();
           
            TMP_Dropdown.OptionData optionData;
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            List<string> values = GameFeatureSettings.GetKeyValuesAsString(value.Key);
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
                // If no values then only the current option has to be shown
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = GameFeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(value.Key);
                options.Add(optionData);                
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
            GameFeatureSettings.Data data = GameFeatureSettings.Datas[key];            
            if (data.IsAList)                                    
            {
                List<string> values = data.ValuesAsStrings;
                string currentValue = GameFeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(key);
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
            GameFeatureSettings settings = GameFeatureSettingsManager.instance.Device_CurrentFeatureSettings;
            foreach (KeyValuePair<string, TMP_Dropdown> pair in m_settingsOptionsDropDowns)
            {
                key = pair.Key;
                values = GameFeatureSettings.GetKeyValuesAsString(key);
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
    public TextMeshProUGUI m_calculatedRating;
    public TextMeshProUGUI m_rating;

    private void Quality_Setup()
    {
        if (m_calculatedRating != null)
        {
            m_calculatedRating.text = "" + GameFeatureSettingsManager.instance.Device_CalculatedRating;
        }

        if (m_rating != null)
        {
            m_rating.text = "" + GameFeatureSettingsManager.instance.Device_CurrentRating;
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
            List<string> values = GameFeatureSettingsManager.instance.Profiles_Names;
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
            string currentValue = GameFeatureSettingsManager.instance.Device_CurrentProfile;
            List<string> values = GameFeatureSettingsManager.instance.Profiles_Names;
            m_profileDropDown.value = values.IndexOf(currentValue);
        }
    }

    public void Profile_SetOption(int option)
    {
        GameFeatureSettingsManager manager = GameFeatureSettingsManager.instance;
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
            string currentValue = GameFeatureSettingsManager.instance.Device_Model;
            int index = m_deviceNames.IndexOf(currentValue);
            if (index == -1)
            {
                index = m_deviceNames.Count - 1;
            }

            m_deviceDropDown.value = index;
        }
    }

    public void Device_SetOption(int option)
    {
        GameFeatureSettingsManager manager = GameFeatureSettingsManager.instance;
        if (manager.Device_Model != m_deviceNames[m_deviceDropDown.value])
        {
            manager.Device_Model = m_deviceNames[m_deviceDropDown.value];

            // We want the configuration for this device to be used
            manager.SetupCurrentFeatureSettings(null);

            // The view has to be updated so it will show the configuration for the new profile
            Setup();
        }
    }
    #endregion
}