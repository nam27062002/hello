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
    public const string KEY_INT_TEST = "intTest";    
    public const string KEY_FLOAT_TEST = "floatTest";
    public const string KEY_STRING_TEST = "stringTest";
    public const string KEY_INT_RANGE_TEST = "intRangeTest";

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
            ValueTypes.Add(EValueType.Int, null);
            ValueTypes.Add(EValueType.String, null);
            ValueTypes.Add(EValueType.Float, null);
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

            // intTest
            key = KEY_INT_TEST;
            data = new DataInt(key, EValueType.Int, 0);
            Datas.Add(key, data);

            // floatTest
            key = KEY_FLOAT_TEST;
            data = new DataFloat(key, EValueType.Float, 0f);
            Datas.Add(key, data);

            // stringTest
            key = KEY_STRING_TEST;
            data = new DataString(key, EValueType.String, "");
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
        QualityLevel,
        Int,
        Float,
        String
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
        public Data(string key, EValueType valueType, string defaultValue)
        {
            Key = key;
            ValueType = valueType;
            DefaultValueAsString = defaultValue;        
        }

        public string Key { get; set; }
        public EValueType ValueType { get; set; }      
        public string DefaultValueAsString { get; set; }        
    }    

    public class DataInt : Data
    {
        public DataInt(string key, EValueType type, int defaultValue) : base(key, type, defaultValue + "")
        {
            DefaultValue = defaultValue;
        }

        public int DefaultValue { get; set; }
    }

    public class DataFloat : Data
    {
        public DataFloat(string key, EValueType type, float defaultValue) : base(key, type, defaultValue + "")
        {
            DefaultValue = defaultValue;
        }

        public float DefaultValue { get; set; }
    }    
    
    public class DataString : Data
    {
        public DataString(string key, EValueType type, string defaultValue) : base(key, type, defaultValue)
        {                       
        }

        public string DefaultValue
        {
            get
            {
                return DefaultValueAsString;
            }
            
            set
            {
                DefaultValueAsString = value;
            }
        }
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

    public int GetValueAsInt(string key)
    {
        return (Values.ContainsKey(key)) ? (int)Values[key] : (int)((Datas[key] as DataInt).DefaultValue);
    }

    public float GetValueAsFloat(string key)
    {
        return (Values.ContainsKey(key)) ? (float)Values[key] : (float)((Datas[key] as DataFloat).DefaultValue);
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
        if (Values.ContainsKey(key))
        {
            EValueType type = Datas[key].ValueType;
            switch (type)
            {
                case EValueType.Int:
                    returnValue = (int)Values[key] + "";
                    break;

                case EValueType.Float:
                    returnValue = (float)Values[key] + "";
                    break;

                case EValueType.String:
                    returnValue = (string)Values[key];
                    break;

                default:
                    // Enum value
                    int index = (int)Values[key];
                    if (ValueTypes.ContainsKey(type))
                    {
                        returnValue = ValueTypes[type][index];
                    }
                    break;
            }            
        }
        else
        {
            returnValue = Datas[key].DefaultValueAsString;
        }

        return returnValue;        
    }    

    public void SetValueFromIndex(string key, int index)
    {
        if (Datas.ContainsKey(key))
        {
            List<string> values = ValueTypes[Datas[key].ValueType];
            if (values != null)
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
    }

    public void SetValueFromString(string key, string valueAsString)
    {
        if (Datas.ContainsKey(key))
        {
            List<string> values = ValueTypes[Datas[key].ValueType];
            if (values == null)
            {
                switch (Datas[key].ValueType)
                {
                    case EValueType.Int:
                        int valueAsInt = int.Parse(valueAsString);
                        if (Values.ContainsKey(key))
                        {
                            Values[key] = valueAsInt;
                        }
                        else
                        {
                            Values.Add(key, valueAsInt);
                        }
                        break;

                    case EValueType.Float:
                        float valueAsFloat = float.Parse(valueAsString);
                        if (Values.ContainsKey(key))
                        {
                            Values[key] = valueAsFloat;
                        }
                        else
                        {
                            Values.Add(key, valueAsFloat);
                        }
                        break;

                    case EValueType.String:
                        if (Values.ContainsKey(key))
                        {
                            Values[key] = valueAsString;
                        }
                        else
                        {
                            Values.Add(key, valueAsString);
                        }
                        break;
                }               
            }
            else
            {
                int valueAsIndex = values.IndexOf(valueAsString);
                SetValueFromIndex(key, valueAsIndex);
            }
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
        foreach (KeyValuePair<string, object> pair in Values)            
        {
            key = pair.Key;
            json.Add(key, GetValueAsString(key));            
        }
    }
}