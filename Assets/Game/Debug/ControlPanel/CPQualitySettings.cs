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

    #region prefab_logic_units
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

            // Sets the current option
            string currentValue = GameFeatureSettingsManager.instance.Device_CurrentFeatureSettings.GetValueAsString(value.Key);
            dropDown.value = values.IndexOf(currentValue);

            if (m_settingsOptionsDropDowns == null)
            {
                m_settingsOptionsDropDowns = new Dictionary<string, TMP_Dropdown>();
            }

            m_settingsOptionsDropDowns.Add(value.Key, dropDown);
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
}