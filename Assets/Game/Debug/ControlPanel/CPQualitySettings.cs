using SimpleJSON;
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

    void Start()
    {
        if (m_prefabOption != null)
        {
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
                    prefabOption.transform.parent = thisParent;
                    prefabOption.transform.SetLocalScale(1f);
                    prefabOption.SetActive(true);

                    PrefabSettingsOption_SetLabel(prefabOption.transform, pair.Key);                    
                    SettingsOptions_Add(pair.Value, prefabOption);
                }
            }           
        }

        Setup();
    }

    void Setup()
    {
        PrefabOptions_Setup();
        Quality_Setup();
        Profile_Setup();
    }

    void Update()
    {
        if (PrefabSettingsOption_IsDirty)
        {
            PrefabOptions_Update();
            Messenger.Broadcast(GameEvents.CP_QUALITY_CHANGED);
            PrefabSettingsOption_IsDirty = false;
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
    private Dictionary<string, TMP_Dropdown> m_settingsOptionsDropDowns;

    private void SettingsOptions_Add(GameFeatureSettings.Data value, GameObject prefabOption)
    {
        TMP_Dropdown dropDown = prefabOption.GetComponentInChildren<TMP_Dropdown>();
        if (dropDown != null)
        {
            dropDown.ClearOptions();
           
            TMP_Dropdown.OptionData optionData;
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            List<string> values = GameFeatureSettings.GetValueTypeValuesAsString(value.ValueType);
            foreach (string v in values)
            {
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = v;
                options.Add(optionData);                
            }
            
            dropDown.AddOptions(options);            

            if (m_settingsOptionsDropDowns == null)
            {
                m_settingsOptionsDropDowns = new Dictionary<string, TMP_Dropdown>();
            }

            m_settingsOptionsDropDowns.Add(value.Key, dropDown);

            // Sets the current option
            PrefabOptions_SetupOption(value.Key);
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
            List<string> values = GameFeatureSettings.GetValueTypeValuesAsString(data.ValueType);
            string currentValue = GameFeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(key);
            m_settingsOptionsDropDowns[key].value = values.IndexOf(currentValue);
        }
    }


    private void PrefabOptions_Update()
    {
        if (m_settingsOptionsDropDowns != null)
        {
            GameFeatureSettings settings = GameFeatureSettingsManager.instance.Device_CurrentFeatureSettings;
            foreach (KeyValuePair<string, TMP_Dropdown> pair in m_settingsOptionsDropDowns)
            {                
                settings.SetValueFromIndex(pair.Key, pair.Value.value);
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

    private void Profile_Setup()
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

            // Sets the current option
            string currentValue = GameFeatureSettingsManager.instance.Device_CurrentProfile;
            m_profileDropDown.value = values.IndexOf(currentValue);

            if (m_settingsOptionsDropDowns == null)
            {
                m_settingsOptionsDropDowns = new Dictionary<string, TMP_Dropdown>();
            }            
        }
    }

    public void Profile_SetOption(int option)
    {        
        GameFeatureSettingsManager manager = GameFeatureSettingsManager.instance;
        manager.Device_CurrentProfile = manager.Profiles_Names[m_profileDropDown.value];

        // The view has to be updated so it will show the configuration for the new profile
        Setup();
    }
    #endregion
}