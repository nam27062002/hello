using SimpleJSON;
using System;
using System.Collections.Generic;

public class GameFeatureSettings : FeatureSettings
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

    public const string KEY_QUALITY_LEVEL = "qualityLevel";
    public const string KEY_SHADERS_LEVEL = "shadersLevel";
    public const string KEY_GLOW = "glow";
    public const string KEY_ENTITIES_LOD = "entitiesLOD";
    public const string KEY_LEVELS_LOD = "levelsLOD";
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

            key = KEY_FLOAT_RANGE_TEST;           
            float minAsFloat = 3f;
            float maxAsFloat = 5f;
            float intervalAsFloat = 1f;
            data = new DataRange(key, EValueType.Float, minAsFloat, minAsFloat, maxAsFloat, intervalAsFloat);
            Datas.Add(key, data);

            key = KEY_INT_RANGE_TEST;
            int minAsInt = 3;
            int maxAsInt = 5;
            int intervalAsInt = 1;
            data = new DataRange(key, EValueType.Int, minAsInt, minAsInt, maxAsInt, intervalAsInt);
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

        public List<string> ValuesAsStrings { get; set; }
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

        public override string GetDataAsString(object data)
        {
            return (string)data;
        }

        public override object GetStringAsData(string data)
        {
            return data;
        }        
    }

    public class DataRange : Data
    {
        public DataRange(string key, EValueType type, float defaultValue, float min, float max, float interval=-1f) : base(key, type, defaultValue, null)
        {            
            DefaultValueAsString = GetDataAsString(defaultValue);

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

        public DataRange(string key, EValueType type, int defaultValue, int min, int max, int interval=-1) : base(key, type, defaultValue, null)
        {
            DefaultValueAsString = GetDataAsString(defaultValue);

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

        private void Setup()
        {            
            if (Values == null)
            {
                Values = new List<object>();
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
       
        private List<object> Values { get; set; }        

        public override string GetDataAsString(object data)
        {            
            string returnValue = null;
            switch (ValueType)
            {
                case EValueType.Int:
                    returnValue = ((int)data) +"";
                    break;

                case EValueType.Float:
                    returnValue = ((float)data) + "";
                    break;
            }
            return returnValue;
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
            float originalData = float.Parse(data);
            object returnValue = null;
            switch (ValueType)
            {
                case EValueType.Int:
                    returnValue = (int)originalData;
                    break;

                case EValueType.Float:
                    returnValue = originalData;
                    break;
            }

            return returnValue;
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