using System;
using System.Collections;
using System.Collections.Generic;
using FGOL.Server;

public abstract class PersistenceSystem
{
    protected string m_systemName = string.Empty;
    protected PersistenceData m_persistenceData;  
    private Stack<string> m_keyStack = new Stack<string>();

    private bool mIsDirty;
    public bool IsDirty
    {
        get { return mIsDirty; }
        set
        {
            mIsDirty = value;
        }
    }

    #region cache
    protected abstract class CacheData
    {
        public string Key { get; set; }

        public abstract void Reset();
        public abstract void Load(PersistenceSystem system);
        public abstract void Save(PersistenceSystem system);
    }

    protected class CacheDataString : CacheData
    {
        public CacheDataString(string key, string defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
            Reset();
        }

        public string DefaultValue { get; set; }
        public string Value { get; set; }

        public override void Reset()
        {
            Value = DefaultValue;
        }

        public override void Load(PersistenceSystem system)
        {
            Value = system.GetString(Key, DefaultValue);
        }

        public override void Save(PersistenceSystem system)
        {
            system.SetString(Key, Value);
        }
    }

    protected class CacheDataInt : CacheData
    {
        public CacheDataInt(string key, int defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
            Value = DefaultValue;
        }

        public int DefaultValue { get; set; }
        public int Value { get; set; }

        public override void Reset()
        {
            Value = DefaultValue;
        }

        public override void Load(PersistenceSystem saveSystem)
        {
            Value = saveSystem.GetInt(Key, DefaultValue);
        }

        public override void Save(PersistenceSystem saveSystem)
        {
            saveSystem.SetInt(Key, Value);
        }
    }

	protected class CacheDataFloat : CacheData
	{
		public CacheDataFloat(string key, float defaultValue)
		{
			Key = key;
			DefaultValue = defaultValue;
			Value = DefaultValue;
		}

		public float DefaultValue { get; set; }
		public float Value { get; set; }

		public override void Reset()
		{
			Value = DefaultValue;
		}

		public override void Load(PersistenceSystem saveSystem)
		{
			Value = saveSystem.GetFloat(Key, DefaultValue);
		}

		public override void Save(PersistenceSystem saveSystem)
		{
			saveSystem.SetFloat(Key, Value);
		}
	}

    protected class CacheDataLong : CacheData
    {
        public CacheDataLong(string key, long defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
            Value = DefaultValue;
        }

        public long DefaultValue { get; set; }
        public long Value { get; set; }

        public override void Reset()
        {
            Value = DefaultValue;
        }

        public override void Load(PersistenceSystem system)
        {
            Value = system.GetLong(Key, DefaultValue);
        }

        public override void Save(PersistenceSystem system)
        {
            system.SetLong(Key, Value);
        }
    }

    protected class CacheDataBool : CacheData
    {
        public CacheDataBool(string key, bool defaultValue)
        {
            Key = key;
            DefaultValue = defaultValue;
            Value = DefaultValue;
        }

        public bool DefaultValue { get; set; }
        public bool Value { get; set; }

        public override void Reset()
        {
            Value = DefaultValue;
        }

        public override void Load(PersistenceSystem system)
        {
            Value = system.GetBool(Key, DefaultValue);
        }

        public override void Save(PersistenceSystem system)
        {
            system.SetBool(Key, Value);
        }
    }

    private Dictionary<string, CacheData> m_cacheData;

    protected void Cache_AddData(string key, CacheData data)
    {
        if (m_cacheData == null)
        {
            m_cacheData = new Dictionary<string, CacheData>();
        }

        if (m_cacheData.ContainsKey(key))
        {
            m_cacheData[key] = data;
        }
        else
        {
            m_cacheData.Add(key, data);
        }
    }

    protected int Cache_GetInt(string key)
    {
        int returnValue = 0;
        if (m_cacheData.ContainsKey(key))
        {
            returnValue = ((CacheDataInt)m_cacheData[key]).Value;
        }

        return returnValue;
    }

    protected void Cache_SetInt(string key, int value)
    {
        if (m_cacheData != null && m_cacheData.ContainsKey(key))
        {
            CacheDataInt dataInt = ((CacheDataInt)m_cacheData[key]);
            if (dataInt != null && dataInt.Value != value)
            {
                dataInt.Value = value;
                IsDirty = true;
            }
        }
    }

	protected float Cache_GetFloat(string key)
	{
		float returnValue = 0f;
		if (m_cacheData.ContainsKey(key))
		{
			returnValue = ((CacheDataFloat)m_cacheData[key]).Value;
		}

		return returnValue;
	}

	protected void Cache_SetFloat(string key, float value)
	{
		if (m_cacheData != null && m_cacheData.ContainsKey(key))
		{
			CacheDataFloat dataFloat = ((CacheDataFloat)m_cacheData[key]);
			if (dataFloat != null && dataFloat.Value != value)
			{
				dataFloat.Value = value;
				IsDirty = true;
			}
		}
	}

    protected long Cache_GetLong(string key)
    {
        long returnValue = 0;
        if (m_cacheData.ContainsKey(key))
        {
            returnValue = ((CacheDataLong)m_cacheData[key]).Value;
        }

        return returnValue;
    }

    protected void Cache_SetLong(string key, long value)
    {
        if (m_cacheData != null && m_cacheData.ContainsKey(key))
        {
            CacheDataLong dataLong = ((CacheDataLong)m_cacheData[key]);
            if (dataLong != null && dataLong.Value != value)
            {
                dataLong.Value = value;
                IsDirty = true;
            }
        }
    }

    protected string Cache_GetString(string key)
    {
        string returnValue = null;
        if (m_cacheData.ContainsKey(key))
        {
            returnValue = ((CacheDataString)m_cacheData[key]).Value;
        }

        return returnValue;
    }

    protected void Cache_SetString(string key, string value)
    {
        if (m_cacheData != null && m_cacheData.ContainsKey(key))
        {
            CacheDataString dataString = ((CacheDataString)m_cacheData[key]);
            if (dataString != null && dataString.Value != value)
            {
                dataString.Value = value;
                IsDirty = true;
            }
        }
    }

    protected bool Cache_GetBool(string key)
    {
        bool returnValue = false;
        if (m_cacheData.ContainsKey(key))
        {
            returnValue = ((CacheDataBool)m_cacheData[key]).Value;
        }

        return returnValue;
    }

    protected void Cache_SetBool(string key, bool value)
    {
        if (m_cacheData != null && m_cacheData.ContainsKey(key))
        {
            CacheDataBool dataBool = ((CacheDataBool)m_cacheData[key]);
            if (dataBool != null && dataBool.Value != value)
            {
                dataBool.Value = value;
                IsDirty = true;
            }
        }
    }

    protected void Cache_Reset()
    {
        if (m_cacheData != null)
        {
            foreach (KeyValuePair<string, CacheData> pair in m_cacheData)
            {
                pair.Value.Reset();
            }

            IsDirty = false;
        }
    }

    protected void Cache_Load()
    {
        if (m_cacheData != null)
        {
            try
            {
                bool isDirty = IsDirty;
                foreach (KeyValuePair<string, CacheData> pair in m_cacheData)
                {
                    pair.Value.Load(this);
                }

                // Prevents it from being marked as dirty because of data that have just been loaded
                IsDirty = isDirty;
            }
            catch (Exception e)
            {				
                PersistenceFacade.LogError("SaveSystem (Load) :: Exception - " + e);
                throw new CorruptedSaveException(e);
            }
        }
    }

    protected void Cache_Save()
    {
        if (m_cacheData != null)
        {
            foreach (KeyValuePair<string, CacheData> pair in m_cacheData)
            {
                pair.Value.Save(this);
            }
        }
    }
    #endregion

    #region Public Properties
    public string name
    {
        get { return m_systemName; }
    }

    public PersistenceData data
    {
        set
        {
            m_persistenceData = value;
        }
    }

    public string version
    {
        get { return m_persistenceData.Version; }
    }
    #endregion

    #region Protected Properties
    /*
    //[DGR] RULES: No support added yet
    protected GameDB gameDB
    {
        get { return GameDataManager.Instance.gameDB; }
    }
    */
    #endregion

    #region Abstract Interface
    public abstract void Reset();

    public abstract void Load();

    public abstract void Save();

    public abstract bool Upgrade();    

    public SimpleJSON.JSONNode ToJSON()
    {
        SimpleJSON.JSONNode returnValue = null;
        if (m_persistenceData != null)
        {
            Save();
            returnValue = SimpleJSON.JSON.Parse(m_persistenceData.ToString());
            returnValue = returnValue[name];
        }

        return returnValue;
    }
    #endregion

    #region Scoping
    protected void PushKey(string key)
    {
        m_keyStack.Push(key);
    }

    protected void PopKey()
    {
        m_keyStack.Pop();
    }
    #endregion

    #region Getters
    protected bool KeyExists(string key, bool platformSpecific = false)
    {
        return GetObject(key, platformSpecific) != null;
    }

    protected int GetInt(string key, int defaultValue = 0, bool platformSpecific = false)
    {
        object intObj = GetObject(key, platformSpecific);

        return intObj != null ? Convert.ToInt32(intObj) : defaultValue;
    }

    protected long GetLong(string key, long defaultValue = 0, bool platformSpecific = false)
    {
        object intObj = GetObject(key, platformSpecific);

        return intObj != null ? Convert.ToInt64(intObj) : defaultValue;
    }

    protected string GetString(string key, string defaultValue = "", bool platformSpecific = false)
    {
        object stringObj = GetObject(key, platformSpecific);

        return stringObj != null ? Convert.ToString(stringObj) : defaultValue;
    }

    protected float GetFloat(string key, float defaultValue = 0, bool platformSpecific = false)
    {
        object floatObj = GetObject(key, platformSpecific);

        return floatObj != null ? Convert.ToSingle(floatObj) : defaultValue;
    }

    protected bool GetBool(string key, bool defaultValue = false, bool platformSpecific = false)
    {
        object boolObj = GetObject(key, platformSpecific);

        return boolObj != null ? Convert.ToBoolean(boolObj) : defaultValue;
    }

    protected int[] GetIntArray(string key, bool platformSpecific = false)
    {
        IEnumerable iter = GetObject(key, platformSpecific) as IEnumerable;

        List<int> array = new List<int>();

        if (iter != null)
        {
            foreach (object intObj in iter)
            {
                array.Add(Convert.ToInt32(intObj));
            }
        }

        return array.ToArray();
    }

    protected float[] GetFloatArray(string key, bool platformSpecific = false)
    {
        IEnumerable iter = GetObject(key, platformSpecific) as IEnumerable;

        List<float> array = new List<float>();

        if (iter != null)
        {
            foreach (object floatObj in iter)
            {
                array.Add(Convert.ToSingle(floatObj));
            }
        }

        return array.ToArray();
    }

    protected bool[] GetBoolArray(string key, bool platformSpecific = false)
    {
        IEnumerable iter = GetObject(key, platformSpecific) as IEnumerable;

        List<bool> array = new List<bool>();

        if (iter != null)
        {
            foreach (object boolObj in iter)
            {
                array.Add(Convert.ToBoolean(boolObj));
            }
        }

        return array.ToArray();
    }

    protected string[] GetStringArray(string key, bool platformSpecific = false)
    {
        IEnumerable iter = GetObject(key, platformSpecific) as IEnumerable;

        List<string> array = new List<string>();

        if (iter != null)
        {
            foreach (object strObj in iter)
            {
                array.Add(Convert.ToString(strObj));
            }
        }

        return array.ToArray();
    }

    #endregion

    #region Setters
    protected void SetInt(string key, int value, bool platformSpecific = false)
    {
        SetObject(key, value, platformSpecific);
    }

    protected void SetLong(string key, long value, bool platformSpecific = false)
    {
        SetObject(key, value, platformSpecific);
    }

    protected void SetFloat(string key, float value, bool platformSpecific = false)
    {
        SetObject(key, value, platformSpecific);
    }

    protected void SetBool(string key, bool value, bool platformSpecific = false)
    {
        SetObject(key, value, platformSpecific);
    }

    protected void SetString(string key, string value, bool platformSpecific = false)
    {
        SetObject(key, value, platformSpecific);
    }

    //Be very careful using the below setters this will completely replace all entries in an array with the new array and can lose data
    protected void SetStringArray(string key, string[] array, bool platformSpecific = false, bool assertOnLengthDecrease = true)
    {
        if (assertOnLengthDecrease)
        {
            string[] currentArray = GetStringArray(key, platformSpecific);

            FGOL.Assert.Fatal(currentArray == null || array.Length >= currentArray.Length, "Array decrease should never happen in this mode!");
        }

        SetObject(key, array, platformSpecific);
    }

    protected void SetIntArray(string key, int[] array, bool platformSpecific = false, bool assertOnLengthDecrease = true)
    {
        if (assertOnLengthDecrease)
        {
            int[] currentArray = GetIntArray(key, platformSpecific);

            FGOL.Assert.Fatal(currentArray == null || array.Length >= currentArray.Length, "Array decrease should never happen in this mode!");
        }

        SetObject(key, array, platformSpecific);
    }

    protected void SetFloatArray(string key, float[] array, bool platformSpecific = false, bool assertOnLengthDecrease = true)
    {
        if (assertOnLengthDecrease)
        {
            float[] currentArray = GetFloatArray(key, platformSpecific);

            FGOL.Assert.Fatal(currentArray == null || array.Length >= currentArray.Length, "Array decrease should never happen in this mode!");
        }

        SetObject(key, array, platformSpecific);
    }

    protected void SetBoolArray(string key, bool[] array, bool platformSpecific = false, bool assertOnLengthDecrease = true)
    {
        if (assertOnLengthDecrease)
        {
            bool[] currentArray = GetBoolArray(key, platformSpecific);

            FGOL.Assert.Fatal(currentArray == null || array.Length >= currentArray.Length, "Array decrease should never happen in this mode!");
        }

        SetObject(key, array, platformSpecific);
    }
    #endregion

    #region Private Methods
    private string GetKey(string key, bool platformSpecific)
    {
        m_keyStack.Push(key);

        string[] keys = m_keyStack.ToArray();

        Array.Reverse(keys);

        string saveKey = "";

        if (!string.IsNullOrEmpty(m_systemName))
        {
            saveKey += m_systemName + ".";
        }

        saveKey += (platformSpecific ? Globals.GetPlatform().ToString() + "." : "") + string.Join(".", keys);

        m_keyStack.Pop();

        return saveKey;
    }

    public object GetObject(string key, bool platformSpecific = false)
    {
        return m_persistenceData != null ? m_persistenceData[GetKey(key, platformSpecific)] : null;
    }

    public void SetObject(string key, object value, bool platformSpecific = false)
    {
        if (m_persistenceData != null)
        {
            m_persistenceData[GetKey(key, platformSpecific)] = value;
        }
        else 
        {
            PersistenceFacade.LogError("SaveSystem (Set) :: Trying to set data before saveData available");
        }
    }

    private void SetObjectAtIndex(string key, int index, object value, bool platformSpecific = false)
    {
        object valueAtKey = GetObject(key, platformSpecific);
        List<object> list = valueAtKey as List<object>;

        if (valueAtKey == null || list != null)
        {
            if (list == null)
            {
                list = new List<object>();
                SetObject(key, list, platformSpecific);
            }

            //Fill array to be the size required to set index
            if ((list.Count - 1) < index)
            {
                for (int i = list.Count - 1; i <= index; i++)
                {
                    list.Add(null);
                }
            }

            list[index] = value;
        }
        else
        {
            PersistenceFacade.LogWarning("SaveSystem (SetAtIndex) :: Unable to set value at index as key is not an array");
        }

    }
    #endregion
}
