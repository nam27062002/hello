using SimpleJSON;
using System;
using System.Collections.Generic;

public class GameFeatureSettings : FeatureSettings
{    
    private static Dictionary<EValueType, List<string>> ValueTypes;
    public static Dictionary<string, Data> Datas { get; set; }

    public const string KEY_QUALITY_LEVEL = "qualityLevel";
    public const string KEY_SHADERS_LEVEL = "shadersLevel";
    public const string KEY_GLOW = "glow";
    public const string KEY_ENTITIES_LOD = "entitiesLOD";
    public const string KEY_LEVELS_LOD = "levelsLOD";

    public static void Build()
    {
        if (ValueTypes == null)
        {
            ValueTypes = new Dictionary<EValueType, List<string>>();
            
            ValueTypes.Add(EValueType.Bool, new List<string>(BoolValuesKeys));
            ValueTypes.Add(EValueType.Level2, GetValueTypeValuesAsString<ELevel2Values>());
            ValueTypes.Add(EValueType.Level3, GetValueTypeValuesAsString<ELevel3Values>());
            ValueTypes.Add(EValueType.Level5, GetValueTypeValuesAsString<ELevel5Values>());
            ValueTypes.Add(EValueType.QualityLevel, GetValueTypeValuesAsString<EQualityLevelValues>());            
        }

        if (Datas == null)
        {
            Datas = new Dictionary<string, Data>();

            // qualityLevel: Unity quality settings level
            string key = KEY_QUALITY_LEVEL;
            Data data = new DataInt(key, EValueType.QualityLevel, (int)EQualityLevelValues.very_low);
            Datas.Add(key, data);

            // shadersLevel: Configuration for shaders
            key = KEY_SHADERS_LEVEL;
            data = new DataInt(key, EValueType.Level3, (int)ELevel3Values.low);            
            Datas.Add(key, data);


            // glow: default value is false because glow has caused crashed in several devices so false is a safer value for a device until it's proved that the feature works properly
            key = KEY_GLOW;
            data = new DataInt(key, EValueType.Bool, (int)EBoolValues.FALSE);
            Datas.Add(key, data);

            // entitiesLOD
            key = KEY_ENTITIES_LOD;
            data = new DataInt(key, EValueType.Level2, (int)ELevel2Values.low);            
            Datas.Add(key, data);

            // levelsLOD
            key = KEY_LEVELS_LOD;
            data = new DataInt(key, EValueType.Level3, (int)ELevel3Values.low);            
            Datas.Add(key, data);
        }
    }

    public static List<string> GetValueTypeValuesAsString<T>()
    {
        List<string> returnValue = new List<string>();
        foreach (T v in Enum.GetValues(typeof(T)))
        {
            returnValue.Add(v.ToString());
        }

        return returnValue;
    }

    public static List<string> GetValueTypeValuesAsString(EValueType valueType)
    {
        return (ValueTypes != null && ValueTypes.ContainsKey(valueType)) ? ValueTypes[valueType] : null;
    }    

    public enum EValueType
    {
        Bool,
        Level2,
        Level3,
        Level5,
        QualityLevel
    };

    public enum EBoolValues
    {
        FALSE,
        TRUE
    };

    public static string[] BoolValuesKeys = new string[]
    {
        "false",
        "true"
    };

    public enum ELevel2Values
    {
        low,
        high
    };

    public enum ELevel3Values
    {
        low,
        mid,
        high
    };

    public enum ELevel5Values
    {
        very_low,
        low,
        mid,
        high,
        very_high
    };

    public enum EQualityLevelValues
    {
        very_low,
        low,
        mid,
        high,
        very_high,
        deprecated
    };

    public abstract class Data
    {        
        public Data(string key, EValueType valueType)
        {
            Key = key;
            ValueType = valueType;            
        }

        public string Key { get; set; }
        public EValueType ValueType { get; set; }              
    }    

    public class DataInt : Data
    {
        public DataInt(string key, EValueType type, int defaultValue) : base(key, type)
        {
            DefaultValue = defaultValue;
        }


        public int DefaultValue { get; set; }
    }   

    private Dictionary<string, object> Values;   

    public GameFeatureSettings()
    {
        if (Datas == null)
        {
            Build();
        }

        Values = new Dictionary<string, object>();       
    }

    public bool GetValueAsBool(string key)
    {
        EBoolValues valueAsBool = (Values.ContainsKey(key)) ? (EBoolValues)Values[key] : (EBoolValues)((Datas[key] as DataInt).DefaultValue);
        return valueAsBool == EBoolValues.TRUE;            
    }

    public ELevel2Values GetValueAsLevel2(string key)
    {
        return (Values.ContainsKey(key)) ? (ELevel2Values)Values[key] : (ELevel2Values)((Datas[key] as DataInt).DefaultValue);        
    }

    public ELevel3Values GetValueAsLevel3(string key)
    {
        return (Values.ContainsKey(key)) ? (ELevel3Values)Values[key] : (ELevel3Values)((Datas[key] as DataInt).DefaultValue);
    }

    public ELevel5Values GetValueAsLevel5(string key)
    {
        return (Values.ContainsKey(key)) ? (ELevel5Values)Values[key] : (ELevel5Values)((Datas[key] as DataInt).DefaultValue);
    }

    public EQualityLevelValues GetValueAsQualityLevel(string key)
    {
        return (Values.ContainsKey(key)) ? (EQualityLevelValues)Values[key] : (EQualityLevelValues)((Datas[key] as DataInt).DefaultValue);
    }

    public string GetValueAsString(string key)
    {
        string returnValue = null;
        if (Values != null && Values.ContainsKey(key))
        {
            EValueType type = Datas[key].ValueType;
            int index = (int)Values[key];
            if (ValueTypes.ContainsKey(type))
            {
                returnValue = ValueTypes[type][index];
            }                        
        }

        return returnValue;        
    }    

    public void SetValueFromIndex(string key, int index)
    {
        if (Datas.ContainsKey(key))
        {
            if (Values.ContainsKey(key))
            {
                Values[key] = index;
            }
            else
            {
                Values.Add(key, index);
            }                          
        }        
    }

    public void SetValueFromString(string key, string valueAsString)
    {
        if (Datas.ContainsKey(key))
        {
            List<string> values = ValueTypes[Datas[key].ValueType];
            int valueAsIndex = values.IndexOf(valueAsString);

            SetValueFromIndex(key, valueAsIndex);           
        }
    }

    protected override void ExtendedReset()
    {
        Values.Clear();        
    }

    protected override void ExtendedOverrideFromJSON(JSONNode json)
    {
        if (json != null)
        {
            string key;
            foreach (KeyValuePair<string, Data> pair in Datas)
            {
                key = pair.Key;
                if (json.ContainsKey(key))
                {                    
                    SetValueFromString(key, json[key]);                  
                }
            }
        }
    }

    protected override void ExtendedToJSON(JSONNode json)
    {
        string key;
        foreach (KeyValuePair<string, Data> pair in Datas)
        {
            key = pair.Key;
            json.Add(key, GetValueAsString(key));            
        }
    }
}