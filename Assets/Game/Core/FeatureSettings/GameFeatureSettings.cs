using SimpleJSON;
using System;
using System.Collections.Generic;

public class GameFeatureSettings
{    
    public class ValueTypeData
    {
        public ValueTypeData(EValueType valueType)
        {
            ValueType = valueType;
        }

        public ValueTypeData(EValueType valueType, List<string> valueTypesAsString)
        {
            ValueType = valueType;
            ValuesAsStrings = valueTypesAsString;
        }        

        public EValueType ValueType { get; set; }        
        public List<string> ValuesAsStrings { get; set; }       
    }    

    private static Dictionary<EValueType, List<string>> ValueTypeDatas;
    public static Dictionary<string, Data> Datas { get; set; }

    public const string KEY_RATING = "rating";
    public const string KEY_PROFILE = "profile";
    public const string KEY_QUALITY_LEVEL = "qualityLevel";
    public const string KEY_SHADERS_LEVEL = "shadersLevel";
    public const string KEY_GLOW = "glow";
    public const string KEY_ENTITIES_LOD = "entitiesLOD";
    public const string KEY_LEVELS_LOD = "levelsLOD";
    
    // Examples of how to use different type datas
    public const string KEY_INT_TEST = "intTest";    
    public const string KEY_FLOAT_TEST = "floatTest";
    public const string KEY_STRING_TEST = "stringTest";
    public const string KEY_INT_RANGE_TEST = "intRangeTest";
    public const string KEY_FLOAT_RANGE_TEST = "floatRangeTest";

    public static void Build()
    {
        if (ValueTypeDatas == null)
        {
            ValueTypeDatas = new Dictionary<EValueType, List<string>>();
            
            ValueTypeDatas.Add(EValueType.Bool, new List<string>(BoolValuesKeys));            
            ValueTypeDatas.Add(EValueType.Int, null);
            ValueTypeDatas.Add(EValueType.Float, null);
            ValueTypeDatas.Add(EValueType.String, null);            
            ValueTypeDatas.Add(EValueType.Level2, GetValueTypeValuesAsString<ELevel2Values>());            
            ValueTypeDatas.Add(EValueType.Level3, GetValueTypeValuesAsString<ELevel3Values>());
            ValueTypeDatas.Add(EValueType.Level5, GetValueTypeValuesAsString<ELevel5Values>());            
            ValueTypeDatas.Add(EValueType.QualityLevel, GetValueTypeValuesAsString<EQualityLevelValues>());            
        }
       
        if (Datas == null)
        {
            Datas = new Dictionary<string, Data>();

            // profile
            string key = KEY_PROFILE;
            Data data = new DataString(key, EValueType.String, "very_low");
            Datas.Add(key, data);

            key = KEY_RATING;
            data = new DataFloat(key, EValueType.Float, 0f);
            Datas.Add(key, data);

            // qualityLevel: Unity quality settings level
            key = KEY_QUALITY_LEVEL;
            data = new DataInt(key, EValueType.QualityLevel, (int)EQualityLevelValues.very_low);
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

            key = KEY_FLOAT_RANGE_TEST;           
            float minAsFloat = 3f;
            float maxAsFloat = 5f;
            float intervalAsFloat = 0.5f;
            data = new DataRangeFloat(key, EValueType.Float, minAsFloat, minAsFloat, maxAsFloat, intervalAsFloat);
            Datas.Add(key, data);

            key = KEY_INT_RANGE_TEST;
            int minAsInt = 3;
            int maxAsInt = 5;
            int intervalAsInt = -1;
            data = new DataRangeInt(key, EValueType.Int, minAsInt, minAsInt, maxAsInt, intervalAsInt);
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

    public static List<string> GetKeyValuesAsString(string key)
    {
        return (Datas.ContainsKey(key)) ? Datas[key].ValuesAsStrings : null;
    }

    public enum EValueType
    {
        Bool,
        Int,
        Float,
        String,
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
        public static bool ParseValueInt(string value, out int outValue)
        {
            bool returnValue = int.TryParse(value, out outValue);
            if (!returnValue)
            {
                LogError(value + " is not an int value");
            }

            return returnValue;
        }

        public static bool ParseValueFloat(string value, out float outValue)
        {
            bool returnValue = float.TryParse(value, out outValue);
            if (!returnValue)
            {
                LogError(value + " is not a float value");
            }

            return returnValue;
        }

        public Data(string key, EValueType valueType, object defaultValue, string defaultValueAsString)
        {
            Key = key;
            ValueType = valueType;
            DefaultValue = defaultValue;
            DefaultValueAsString = defaultValueAsString;
            ValuesAsStrings = ValueTypeDatas[valueType];
        }

        public string Key { get; set; }
        public EValueType ValueType { get; set; }      
        public object DefaultValue { get; set; }
        public string DefaultValueAsString { get; set; }
        public int DefaultValueAsInt
        {
            get
            {
                return (int)DefaultValue;
            }
        }

        public float DefaultValueAsFloat
        {
            get
            {
                return (float)DefaultValue;
            }
        }

        public bool ParseValue(string value)
        {
            bool returnValue = false;
            if (IsAList)
            {
                returnValue = ValuesAsStrings.IndexOf(value) > -1;                
                if (!returnValue)
                {
                    LogError(value + " is not among the valid values: " + GetValuesAsStringsList() + " for key = " + Key);
                }                                                
            } 
            else
            {
                returnValue = ParseSingleValue(value);
            }

            return returnValue;
        }

        protected abstract bool ParseSingleValue(string value);

        public List<string> ValuesAsStrings { get; set; }
        public string GetValuesAsStringsList()
        {
            
            string returnValue = "[";
            if (ValuesAsStrings != null)
            {
                int count = ValuesAsStrings.Count;
                for (int i = 0; i < count; i++)
                {
                    returnValue += ValuesAsStrings[i];
                    if (i < count - 1)
                    {
                        returnValue += ",";
                    }
                }
            }
            returnValue += "]";

            return returnValue;
        }

        public abstract string GetDataAsString(object data);
        public abstract object GetStringAsData(string data);        

        public bool IsAList
        {
            get
            {
                return ValuesAsStrings != null;
            }
        }

        /// <summary>
        /// Returns the value associated to an index
        /// </summary>        
        public virtual object GetValueFromListIndex(int index)
        {
            // By default it returns the index in the list
            return index;
        }
    }    

    public class DataInt : Data
    {
        public DataInt(string key, EValueType type, int defaultValue) : base(key, type, defaultValue, defaultValue + "")
        {
        }

        protected override bool ParseSingleValue(string value)
        {            
            int valueAsInt;
            return ParseValueInt(value, out valueAsInt);            
        }

        public override string GetDataAsString(object data)
        {
            string returnValue = null;
            int intValue = (int)data;
            if (ValueType == EValueType.Int)
            {
                returnValue = intValue + "";
            }
            else
            {
                // Enum value                
                returnValue = ValuesAsStrings[intValue];                
            }

            return returnValue;             
        }

        public override object GetStringAsData(string data)
        {
            return int.Parse(data);
        }        
    }

    public class DataFloat : Data
    {
        public DataFloat(string key, EValueType type, float defaultValue) : base(key, type, defaultValue, defaultValue + "")
        {         
        }

        protected override bool ParseSingleValue(string value)
        {            
            float valueAsFloat;
            return ParseValueFloat(value, out valueAsFloat);            
        }

        public override string GetDataAsString(object data)
        {
            return ((float)data) + "";            
        }        

        public override object GetStringAsData(string data)
        {
            return float.Parse(data);
        }       
    }    
    
    public class DataString : Data
    {
        public DataString(string key, EValueType type, string defaultValue) : base(key, type, defaultValue, defaultValue)
        {                       
        }

        protected override bool ParseSingleValue(string value)
        {
            return true;
        }

        public override string GetDataAsString(object data)
        {
            return (string)data;
        }

        public override object GetStringAsData(string data)
        {
            return data;
        }        
    }

    public class DataRangeFloat : Data
    {
        public DataRangeFloat(string key, EValueType type, float defaultValue, float min, float max, float interval=-1f) : base(key, type, defaultValue, null)
        {            
            DefaultValueAsString = GetDataAsString(defaultValue);

            Min = min;
            Max = max;
                       
            if (interval > -1)
            {
                Setup();
                
                for (float i = min; i <= max; i += interval)
                {
                    Values.Add(i);
                }

                if ((float)Values[Values.Count - 1] != max)
                {
                    Values.Add(max);
                }

                SetupStrings();
            }
        }

        private List<float> Values { get; set; }

        private float Min { get; set; }
        private float Max { get; set; }

        private void Setup()
        {            
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
        }

        private void SetupStrings()
        {
            int count = Values.Count;
            for (int i = 0; i < count; i++)
            {
                ValuesAsStrings.Add(Values[i] + "");
            }
        }

        protected override bool ParseSingleValue(string value)
        {
            bool returnValue = false;
            float valueAsFloat;
            bool isFloat = ParseValueFloat(value, out valueAsFloat);
            if (isFloat)
            {                
                if ((valueAsFloat < Min || valueAsFloat > Max))
                {
                    LogError(value + " for key " + Key + " must belong to [" + Min + "," + Max + "]");
                }
                else
                {
                    returnValue = true;
                }
            }            

            return returnValue;
        }       

        public override string GetDataAsString(object data)
        {
            return ((float)data) + "";            
        }

        /// <summary>
        /// Returns the value associated to an index
        /// </summary>        
        public override object GetValueFromListIndex(int index)
        {
            return Values[index];
        }

        public override object GetStringAsData(string data)
        {
            return float.Parse(data);            
        }        
    }

    public class DataRangeInt : Data
    {       
        public DataRangeInt(string key, EValueType type, int defaultValue, int min, int max, int interval = -1) : base(key, type, defaultValue, null)
        {
            DefaultValueAsString = GetDataAsString(defaultValue);

            Min = min;
            Max = max;

            if (interval > -1)
            {
                Setup();

                for (int i = min; i <= max; i += interval)
                {
                    Values.Add(i);
                }

                if ((int)Values[Values.Count - 1] != max)
                {
                    Values.Add(max);
                }

                SetupStrings();
            }
        }

        private List<int> Values { get; set; }
        private int Min { get; set; }
        private int Max { get; set; }

        private void Setup()
        {
            if (Values == null)
            {
                Values = new List<int>();
                ValuesAsStrings = new List<string>();
            }
            else
            {
                Values.Clear();
                ValuesAsStrings.Clear();
            }
        }

        private void SetupStrings()
        {
            int count = Values.Count;
            for (int i = 0; i < count; i++)
            {
                ValuesAsStrings.Add(Values[i] + "");
            }
        }        

        protected override bool ParseSingleValue(string value)
        {
            bool returnValue = false;
            int valueAsInt;
            bool isInt = ParseValueInt(value, out valueAsInt);
            if (isInt)
            {
                if (valueAsInt < Min || valueAsInt > Max)
                {
                    LogError(value + " for key " + Key + " must belong to [" + Min + "," + Max + "]");
                }
                else
                {
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public override string GetDataAsString(object data)
        {
            return ((int)data) + "";            
        }

        /// <summary>
        /// Returns the value associated to an index
        /// </summary>        
        public override object GetValueFromListIndex(int index)
        {
            return Values[index];
        }

        public override object GetStringAsData(string data)
        {
            return int.Parse(data);            
        }
    }

    private Dictionary<string, object> Values { get; set; }

    public GameFeatureSettings()
    {
        if (Datas == null)
        {
            Build();
        }

        Values = new Dictionary<string, object>();       
    }

    public float Rating
    {
        get
        {
            return GetValueAsFloat(KEY_RATING);
        }

        set
        {
            string key = KEY_RATING;
            if (Values.ContainsKey(key))
            {
                Values[key] = value;
            }
            else
            {
                Values.Add(key, value);
            }
        }
    }

    public string Profile
    {
        get
        {
            return GetValueAsString(KEY_PROFILE);
        }

        set
        {
            string key = KEY_PROFILE;
            if (Values.ContainsKey(key))
            {
                Values[key] = value;
            }
            else
            {
                Values.Add(key, value);
            }
        }
    }

    public bool GetValueAsBool(string key)
    {
        EBoolValues valueAsBool = (Values.ContainsKey(key)) ? (EBoolValues)Values[key] : (EBoolValues)((Datas[key] as DataInt).DefaultValue);
        return valueAsBool == EBoolValues.TRUE;            
    }

    public int GetValueAsInt(string key)
    {        
        return (Values.ContainsKey(key)) ? (int)Values[key] : Datas[key].DefaultValueAsInt;
    }

    public float GetValueAsFloat(string key)
    {
        return (Values.ContainsKey(key)) ? (float)Values[key] : Datas[key].DefaultValueAsFloat;
    }

    public ELevel2Values GetValueAsLevel2(string key)
    {
        return (Values.ContainsKey(key)) ? (ELevel2Values)Values[key] : (ELevel2Values)(Datas[key].DefaultValueAsInt);        
    }

    public ELevel3Values GetValueAsLevel3(string key)
    {
        return (Values.ContainsKey(key)) ? (ELevel3Values)Values[key] : (ELevel3Values)(Datas[key].DefaultValueAsInt);
    }

    public ELevel5Values GetValueAsLevel5(string key)
    {
        return (Values.ContainsKey(key)) ? (ELevel5Values)Values[key] : (ELevel5Values)(Datas[key].DefaultValueAsInt);
    }

    public EQualityLevelValues GetValueAsQualityLevel(string key)
    {
        return (Values.ContainsKey(key)) ? (EQualityLevelValues)Values[key] : (EQualityLevelValues)(Datas[key].DefaultValueAsInt);
    }

    public string GetValueAsString(string key)
    {
        // The stored value is returned as string if it exists, otherwise the default value is returned
        return (Values.ContainsKey(key)) ? Datas[key].GetDataAsString(Values[key]) : Datas[key].DefaultValueAsString;        
    }    

    public void SetValueFromIndex(string key, int index)
    {
        if (Datas.ContainsKey(key) && Datas[key].IsAList)
        {            
            object value = Datas[key].GetValueFromListIndex(index);
            if (Values.ContainsKey(key))
            {
                Values[key] = value;
            }
            else
            {
                Values.Add(key, value);
            }                                                   
        }        
    }

    public void SetValueFromString(string key, string valueAsString)
    {
        if (Datas.ContainsKey(key))
        {
            if (Datas[key].IsAList)
            {
                int valueAsIndex = Datas[key].ValuesAsStrings.IndexOf(valueAsString);
                SetValueFromIndex(key, valueAsIndex);
            }
            else
            {
                object data = Datas[key].GetStringAsData(valueAsString);
                if (Values.ContainsKey(key))
                {
                    Values[key] = data;
                }
                else
                {
                    Values.Add(key, data);
                }                
            }                     
        }
    }    

    public void Reset()
    {
        Values.Clear();
    }

    /// <summary>
    /// Loads this object with the data defined by <c>json</c>. Default values are set for the keys that are not defined in <c>json</c>.
    /// </summary>    
    public void FromJSON(JSONNode json)
    {
        Reset();
        OverrideFromJSON(json);
    }

    public JSONNode ParseJSON(JSONNode json)
    {
        JSONNode returnValue = new JSONClass();
        if (json != null)
        {
            JSONClass jsonClass = json as JSONClass;
            foreach (KeyValuePair<string, JSONNode> pair in jsonClass.m_Dict)
            {
                if (Datas.ContainsKey(pair.Key))
                {
                    if (ParseValue(pair.Key, pair.Value))
                    {
                        returnValue.Add(pair.Key, pair.Value);
                    }
                }
                else
                {
                    LogError("Key " + pair.Key + " not valid");
                }
            }
        }

        return returnValue;
    }

    protected bool ParseValue(string key, string valueAsString)
    {
        bool returnValue = false;
        if (Datas.ContainsKey(key))
        {
            returnValue = Datas[key].ParseValue(valueAsString);
        }

        return returnValue;
    }

    /// <summary>
    /// Overrides the current data of this object with the data defined in <c>json</c>. Only the keys defined in <c>json</c> are overriden.
    /// </summary>    
    public void OverrideFromJSON(JSONNode json)
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

    public SimpleJSON.JSONNode ToJSON()
    {
        SimpleJSON.JSONClass returnValue = new SimpleJSON.JSONClass();
        string key;
        foreach (KeyValuePair<string, object> pair in Values)
        {
            key = pair.Key;
            returnValue.Add(key, GetValueAsString(key));
        }

        return returnValue;
    }    

    #region debug
    private const string DEBUG_CHANNEL = "[GameFeatureSettings] ";
    public static void Log(string msg)
    {
        Debug.Log(DEBUG_CHANNEL + msg);
    }

    public static void LogWarning(string msg)
    {
        Debug.LogWarning(DEBUG_CHANNEL + msg);
    }

    public static void LogError(string msg)
    {
        Debug.LogError(DEBUG_CHANNEL + msg);
    }
    #endregion  
}