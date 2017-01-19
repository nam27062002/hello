using SimpleJSON;
using System;
using System.Collections.Generic;

public class GameFeatureSettings : FeatureSettings
{
    public static Dictionary<string, Data> Datas { get; set; }

    public const string KEY_GLOW = "glow";
    public const string KEY_ENTITIES_LOD = "entitiesLOD";
    public const string KEY_LEVELS_LOD = "levelsLOD";

    public static void Build()
    {
        if (Datas == null)
        {
            Datas = new Dictionary<string, Data>();

            // Glow: default value is false because glow has caused crashed in several devices so false is a safer value for a device until it's proved that the feature works properly
            string key = KEY_GLOW;
            Data data = new DataBool(key, EValueType.Bool, false);
            Datas.Add(key, data);

            // entitiesLOD
            key = KEY_ENTITIES_LOD;
            data = new DataLevel2(key, EValueType.Level2, ELevel2Values.low);
            Datas.Add(key, data);

            // levelsLOD
            key = KEY_LEVELS_LOD;
            data = new DataLevel3(key, EValueType.Level3, ELevel3Values.low);
            Datas.Add(key, data);
        }
    }

    public static List<string> GetValueTypeValuesAsString(EValueType valueType)
    {
        List<string> returnValue = new List<string>();
        switch (valueType)
        {
            case EValueType.Bool:
                foreach (EBoolValues v in Enum.GetValues(typeof(EBoolValues)))
                {
                    returnValue.Add(v.ToString());
                }
                break;

            case EValueType.Level2:
                foreach (ELevel2Values v in Enum.GetValues(typeof(ELevel2Values)))
                {
                    returnValue.Add(v.ToString());
                }
                break;

            case EValueType.Level3:
                foreach (ELevel3Values v in Enum.GetValues(typeof(ELevel3Values)))
                {
                    returnValue.Add(v.ToString());
                }
                break;
        }

        return returnValue;
    }

    private static int GetEnumIndexFromString<T>(string value)
    {
        int returnValue = 0;
        Array values = Enum.GetValues(typeof(T));
        foreach (T v in values)
        {
            if (v.ToString() == value)
            {
                break;
            }
            else
            {
                returnValue++;
            }
        }

        if (returnValue >= values.Length)
        {
            DeviceQualityManager.LogError("No enum value found in " + typeof(T).ToString() + " for string " + value);
        }

        return returnValue;
    }

    public enum EValueType
    {
        Bool,
        Level2,
        Level3
    };

    public enum EBoolValues
    {
        FALSE,
        TRUE
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

    public class DataBool : Data
    {
        public DataBool(string key, EValueType valueType, bool defaultValue) : base(key, valueType)
        {
            DefaultValue = defaultValue;
        }

        public bool DefaultValue { get; set; }
    }

    public class DataLevel2 : Data
    {
        public DataLevel2(string key, EValueType valueType, ELevel2Values defaultValue) : base(key, valueType)
        {
            DefaultValue = defaultValue;
        }

        public ELevel2Values DefaultValue { get; set; }
    }

    public class DataLevel3 : Data
    {
        public DataLevel3(string key, EValueType valueType, ELevel3Values defaultValue) : base(key, valueType)
        {
            DefaultValue = defaultValue;
        }

        public ELevel3Values DefaultValue { get; set; }
    }

    private Dictionary<string, bool> BoolValues;
    private Dictionary<string, ELevel2Values> Level2Values;
    private Dictionary<string, ELevel3Values> Level3Values;

    public GameFeatureSettings()
    {
        if (Datas == null)
        {
            Build();
        }

        BoolValues = new Dictionary<string, bool>();
        Level2Values = new Dictionary<string, ELevel2Values>();
        Level3Values = new Dictionary<string, ELevel3Values>();
    }

    public bool GetValueAsBool(string key)
    {
        return (BoolValues.ContainsKey(key)) ? BoolValues[key] : (Datas[key] as DataBool).DefaultValue;
    }

    public ELevel2Values GetValueAsLevel2(string key)
    {
        return (Level2Values.ContainsKey(key)) ? Level2Values[key] : (Datas[key] as DataLevel2).DefaultValue;
    }

    public ELevel3Values GetValueAsLevel3(string key)
    {
        return (Level3Values.ContainsKey(key)) ? Level3Values[key] : (Datas[key] as DataLevel3).DefaultValue;
    }

    public string GetValueAsString(string key)
    {
        string returnValue = null;
        if (Datas.ContainsKey(key))
        {
            switch (Datas[key].ValueType)
            {
                case EValueType.Bool:
                    returnValue = GetValueAsBool(key) + "";
                    returnValue = returnValue.ToUpper();
                    break;

                case EValueType.Level2:
                    returnValue = GetValueAsLevel2(key).ToString();
                    break;

                case EValueType.Level3:
                    returnValue = GetValueAsLevel3(key).ToString();
                    break;
            }
        }

        return returnValue;
    }

    public void SetValueFromIndex(string key, int index)
    {
        if (Datas.ContainsKey(key))
        {
            switch (Datas[key].ValueType)
            {
                case EValueType.Bool:
                    BoolValues[key] = (EBoolValues)index == EBoolValues.TRUE;
                    break;
            }
        }
    }

    protected override void ExtendedReset()
    {
        BoolValues.Clear();
        Level2Values.Clear();
        Level3Values.Clear();
    }

    protected override void ExtendedFromJSON(JSONNode json)
    {
        if (json != null)
        {
            string key;
            foreach (KeyValuePair<string, Data> pair in Datas)
            {
                key = pair.Key;

                if (json.ContainsKey(key))
                {
                    switch (pair.Value.ValueType)
                    {
                        case EValueType.Bool:
                            {
                                bool value = json[key].AsBool;
                                if (BoolValues.ContainsKey(key))
                                {
                                    BoolValues[key] = value;
                                }
                                else
                                {
                                    BoolValues.Add(key, value);
                                }
                            }
                            break;

                        case EValueType.Level2:
                            {
                                string valueString = json[key];
                                ELevel2Values value = (ELevel2Values)GetEnumIndexFromString<ELevel2Values>(valueString);
                                if (Level2Values.ContainsKey(key))
                                {
                                    Level2Values[key] = value;
                                }
                                else
                                {
                                    Level2Values.Add(key, value);
                                }
                            }
                            break;

                        case EValueType.Level3:
                            {
                                string valueString = json[key];
                                ELevel3Values value = (ELevel3Values)GetEnumIndexFromString<ELevel3Values>(valueString);
                                if (Level3Values.ContainsKey(key))
                                {
                                    Level3Values[key] = value;
                                }
                                else
                                {
                                    Level3Values.Add(key, value);
                                }
                            }
                            break;
                    }
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
            switch (pair.Value.ValueType)
            {
                case EValueType.Bool:
                    if (BoolValues.ContainsKey(key))
                    {
                        json.Add(key, BoolValues[key].ToString());
                    }
                    break;

                case EValueType.Level2:
                    if (Level2Values.ContainsKey(key))
                    {
                        json.Add(key, Level2Values[key].ToString());
                    }
                    break;

                case EValueType.Level3:
                    if (Level3Values.ContainsKey(key))
                    {
                        json.Add(key, Level3Values[key].ToString());
                    }
                    break;
            }
        }
    }
}