using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using FGOL.Common;
using FGOL.Encryption;
using FGOL.ThirdParty;
using FGOL.Save;
using System.Collections.Generic;

public class PersistenceData
{
    public const int HeaderVersion = 3;
    public const int ReservedDiskSpace = 1024 * 1024;

    private const string VersionKey = "version";
    private const string PurchasesKey = "purchases";

    private string m_savePath = null;
    private string m_key = null;

    private JSONNestedKeyValueStore m_data = null;

    private byte[] m_cryptoKey = null;
    private byte[] m_cryptoIV = null;

    private int m_modifiedTime = 0;
    private string m_deviceName = null;

    public PersistenceData(string key) : this(SaveUtilities.GetSavePath(key), key) { }

	public List<PersistenceSystem> Systems { get; set; }

    public PersistenceData(string savePath, string key)
    {
        m_savePath = savePath;
        m_key = key;

		Reset ();
    }

	public void Reset ()
	{
		m_data = new JSONNestedKeyValueStore ();

		LoadState = PersistenceStates.LoadState.OK;
		SaveState = PersistenceStates.SaveState.OK;

		GenerateEncryptionKey ();

		if (Systems != null)
		{
			Systems_Reset();
			Systems.Clear();
		}
	}

    public string Key
    {
        get { return m_key; }
        set
        {
            m_key = value;
            m_savePath = SaveUtilities.GetSavePath(m_key);
        }
    }

    public string Version
    {
        get { return m_data[VersionKey] as string; }
        set { m_data[VersionKey] = value; }
    }

    public object Purchases
    {
        get { return m_data[PurchasesKey]; }
        set { m_data[PurchasesKey] = value; }
    }

    public int Timestamp
    {
        get { return m_modifiedTime; }
    }

    public string DeviceName
    {
        get { return m_deviceName; }
    }

    public object this[string key]
    {
        get
        {
            return m_data[key];
        }
        set
        {
            if (key != VersionKey
                && key != PurchasesKey)
            {
                m_data[key] = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(key + " is a reserved key");
            }
        }
    }

	private PersistenceStates.LoadState mLoadState;
    public PersistenceStates.LoadState LoadState 
	{ 
		get 
		{
			return mLoadState;
		}

		set
		{
			mLoadState = value;
		}
	}

    public PersistenceStates.SaveState SaveState { get; set; }

    public PersistenceStates.SaveState Save(bool backUpOnFail = true)
	{
		Systems_Save();

        SaveState = PersistenceStates.SaveState.DiskSpace;

        string playerPrefKey = "Save." + m_key + ".sav";

        //TODO record current platform of save

        m_deviceName = SystemInfo.deviceName;

        //Work around for android devices not returning a valid device name!
        if (m_deviceName == "<unknown>")
        {
            m_deviceName = SystemInfo.deviceModel;
        }

        m_modifiedTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        m_data["deviceName"] = m_deviceName;
        m_data["modifiedTime"] = m_modifiedTime;

        MemoryStream saveStream = SaveToStream();

        if (saveStream != null)
        {
            byte[] saveBytes = saveStream.ToArray();

            //Log error if we have a bigger save then reserved space!
            if (saveBytes.Length >= (ReservedDiskSpace - sizeof(int)))
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    PersistenceFacade.LogError("PersistenceData (Save) :: PersistenceData has gone over reserved diskspace of: " + ReservedDiskSpace);
                }
            }

            try
            {
                using (FileStream fs = new FileStream(m_savePath, FileMode.Create, FileAccess.Write))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //We will pad out the save file with 0s to ensure we have reserved diskspace
                        byte[] lengthBytes = BitConverter.GetBytes(saveBytes.Length);
                        ms.Write(lengthBytes, 0, lengthBytes.Length);
                        ms.Write(saveBytes, 0, saveBytes.Length);

                        int requiredPadding = ReservedDiskSpace - (lengthBytes.Length + saveBytes.Length);

						if (SaveUtilities.UseDebugMode)
						    requiredPadding = 0;

                        if (requiredPadding > 0)
                        {
                            byte[] padding = new byte[requiredPadding];

                            ms.Write(padding, 0, padding.Length);
                        }

                        ms.WriteTo(fs);
                    }
                }

#if UNITY_IPHONE && !UNITY_EDITOR
                //Mark file as not being a file to backup
                UnityEngine.iOS.Device.SetNoBackupFlag(m_savePath);
#endif

                //If we successfully wrote to disk delete playerprefs backup
                PlayerPrefs.DeleteKey(playerPrefKey);

                SaveState = PersistenceStates.SaveState.OK;
            }
            catch (UnauthorizedAccessException)
            {
                //TODO log error?
                SaveState = PersistenceStates.SaveState.PermissionError;
            }
            catch (Exception e)
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    PersistenceFacade.LogError("PersistenceDataData (Save) :: Exception saving to disk");
                    DebugUtils.LogException(e);
                }
            }
            finally
            {
                if (backUpOnFail && SaveState != PersistenceStates.SaveState.OK)
                {
                    PlayerPrefs.SetString(playerPrefKey, Convert.ToBase64String(saveBytes));
                }
            }
        }

        PlayerPrefs.Save();

        return SaveState;
    }

    /// <summary>
    /// Returns whether or not the data are stored clear.
    /// </summary>
    /// <returns></returns>
    public bool IsStoredClear()
    {
#if UNITY_EDITOR
        return true;
#else
        // It should always return false except when debugging that can be changed to true
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            return true;
        }
        else
        {
            return false;
        }      
#endif
    }    

    public MemoryStream SaveToStream()
    {
        MemoryStream stream = null;

        string json = m_data.ToJSON();

        //Compress the data using LZF compression
        byte[] compressed = null;

        using (MemoryStream compMemStream = new MemoryStream())
        {
            using (StreamWriter writer = new StreamWriter(compMemStream, Encoding.UTF8))
            {
                writer.Write(json);
                writer.Close();

                if (IsStoredClear())
                {
                    compressed = compMemStream.ToArray();
                }
                else
                {
                    compressed = CLZF2.Compress(compMemStream.ToArray());
                }
            }
        }

        if (compressed != null)
        {
            byte[] encrypted;
            if (IsStoredClear())
            {
                encrypted = compressed;
            }
            else
            {
                encrypted = AESEncryptor.Encrypt(m_cryptoKey, m_cryptoIV, compressed);
            }

            if (encrypted != null)
            {
                stream = new MemoryStream();

                //Write version and headers!
                byte[] version = SaveUtilities.SerializeVersion(HeaderVersion);
                stream.Write(version, 0, version.Length);

                byte[] header = SaveUtilities.SerializeHeader(/*m_modifiedTime, m_deviceName, */SaveUtilities.CalculateMD5Hash(encrypted), encrypted.Length);
                stream.Write(header, 0, header.Length);

                //Write encrypted and compressed data to stream
                stream.Write(encrypted, 0, encrypted.Length);
            }
        }

        return stream;
    }

    public PersistenceStates.LoadState Load()
    {
        LoadState = PersistenceStates.LoadState.NotFound;

        if (File.Exists(m_savePath))
        {
            try
            {
                using (MemoryStream ms = SaveUtilities.GetUnpaddedSaveData(m_savePath))
                {
                    LoadState = LoadFromStream(ms);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    PersistenceFacade.LogError("Permissions error when loading file at path: " + m_savePath);
                    DebugUtils.LogException(e);
                }

                LoadState = PersistenceStates.LoadState.PermissionError;
            }
            catch (FileNotFoundException)
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    PersistenceFacade.LogWarning("No save file found at path: " + m_savePath);
            }
            catch (Exception e)
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    PersistenceFacade.LogError("Exception when loading file at path: " + m_savePath);
                    DebugUtils.LogException(e);
                }

                LoadState = PersistenceStates.LoadState.Corrupted;
            }
        }
        else if (FeatureSettingsManager.IsDebugEnabled)
        {
            PersistenceFacade.LogWarning("No save file found at path: " + m_savePath);
        }


        //If we can't load from disk try player prefs! Player prefs will be set if we detected an issue when saving!
        if (LoadState != PersistenceStates.LoadState.OK)
        {
            string playerPrefKey = "Save." + m_key + ".sav";
            string prefSaveString = PlayerPrefs.GetString(playerPrefKey);

            if (!string.IsNullOrEmpty(prefSaveString))
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(prefSaveString)))
                    {
                        LoadState = LoadFromStream(ms);
                    }
                }
                catch (Exception e)
                {
                    if (FeatureSettingsManager.IsDebugEnabled)
                    {
                        Debug.LogWarning("Unable to parse save data in PlayerPrefs");
                        Debug.LogWarning(e);
                    }
                }
            }
        }

        return LoadState;
    }

    private void OnLoad()
    {
        if (m_data != null)
        {
            object obj = m_data["modifiedTime"];
            if (obj != null)
            {
                m_modifiedTime = Convert.ToInt32(obj);
            }

            obj = m_data["deviceName"];
            if (obj != null)
            {
                m_deviceName = Convert.ToString(obj);
            }
        }
    }

    public PersistenceStates.LoadState LoadFromString(string data)
    {
        LoadState = PersistenceStates.LoadState.Corrupted;

        if (m_data.FromJSON(data))
        {
            OnLoad();
            LoadState = PersistenceStates.LoadState.OK;
			Systems_Load();
        }
        else
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                PersistenceFacade.LogWarning("Trying to load invalid JSON file at path: " + m_savePath);

            LoadState = PersistenceStates.LoadState.Corrupted;
        }

        return LoadState;
    }

    public PersistenceStates.LoadState LoadFromStream(Stream stream)
    {
        LoadState = PersistenceStates.LoadState.Corrupted;

        try
        {
            byte[] decompressed = null;
            byte[] contentBytes = null;

            bool versionOkay = false;

            //Check version first
            int headerVersion = SaveUtilities.DeserializeVersion(stream);

            if (headerVersion == -1)
            {
                LoadState = PersistenceStates.LoadState.Corrupted;
            }
            else if (headerVersion < HeaderVersion)
            {
                stream = UpgradeFile(stream);

                if (stream != null)
                {
                    versionOkay = true;
                }
                else
                {
                    LoadState = PersistenceStates.LoadState.Corrupted;
                }
            }
            /*
            // [DGR] TODO
            else if (headerVersion > HeaderVersion)
            {
                LoadState = PersistenceStates.LoadState.VersionMismatch;
            }
            */
            else
            {
                versionOkay = true;
            }

            if (versionOkay)
            {                
                if (IsValidFile(stream, ref contentBytes))                
                {
                    if (IsStoredClear())
                    {
                        decompressed = contentBytes;
                    }
                    else
                    {
                        byte[] decrypted = AESEncryptor.Decrypt(m_cryptoKey, m_cryptoIV, contentBytes);
                        if (decrypted != null)
                        {
                            decompressed = CLZF2.Decompress(decrypted);
                        }
                        else
                        {
                            if (FeatureSettingsManager.IsDebugEnabled)
                                PersistenceFacade.LogError("PersistenceData :: Decryption failed!");

                            LoadState = PersistenceStates.LoadState.Corrupted;
                        }
                    }
                }
                else
                {
                    if (FeatureSettingsManager.IsDebugEnabled)
                        PersistenceFacade.LogError("PersistenceData :: File Corrupted!");

                    LoadState = PersistenceStates.LoadState.Corrupted;
                }
            }

            if (decompressed != null)
            {
                using (MemoryStream jsonMemoryStream = new MemoryStream(decompressed))
                {
                    using (StreamReader reader = new StreamReader(jsonMemoryStream))
                    {
						string text = reader.ReadToEnd();
                        if (m_data.FromJSON(text))
                        {
                            OnLoad();
                            LoadState = PersistenceStates.LoadState.OK;
							Systems_Load();
                        }
                        else
                        {
                            if (FeatureSettingsManager.IsDebugEnabled)
                                Debug.LogWarning("Trying to load invalid JSON file at path: " + m_savePath);

                            LoadState = PersistenceStates.LoadState.Corrupted;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            //TODO determine exception types and see if any are recoverable!
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Debug.LogWarning("Exception when parsing file from stream");
                Debug.LogWarning(e);
            }
        }

        return LoadState;
    }

    public void UpdateSavePathAndKey(string savePath, string newKey)
    {
        m_savePath = savePath;
        m_key = newKey;
        GenerateEncryptionKey();
    }

    private bool IsValidFile(Stream stream, ref byte[] contentBytes)
    {
        bool valid = false;

        string md5Hash = null;
        int headerLength = 0;
        int contentLength = 0;
        bool validHeader = SaveUtilities.DeserializeHeader(stream, ref headerLength/*, ref m_modifiedTime, ref m_deviceName*/, ref md5Hash, ref contentLength);

        if (validHeader)
        {            
            contentBytes = new byte[contentLength];
            int contentRead = stream.Read(contentBytes, 0, contentLength);

            if (SaveUtilities.UseDebugMode)
            {
                valid = true;
            }
            else
            {                
                if (contentRead == contentLength)
                {
                    string contentHash = SaveUtilities.CalculateMD5Hash(contentBytes);

                    if (md5Hash == contentHash)
                    {
                        valid = true;
                    }
                    else if (FeatureSettingsManager.IsDebugEnabled)
                    {
                        PersistenceFacade.LogError(string.Format("PersistenceData :: MD5 Checksum Failed (Got {0} expected {1})", md5Hash, contentHash));
                    }
                }
                else if (FeatureSettingsManager.IsDebugEnabled)
                {
                    PersistenceFacade.LogError(string.Format("PersistenceData :: Content length different to expected (Got {0} expected {1})", contentRead, contentLength));
                }
            }
        }
        else if (FeatureSettingsManager.IsDebugEnabled)
        {
            PersistenceFacade.LogError("PersistenceData :: Invalid header detected!");
        }                

        return valid;
    }

    private void GenerateEncryptionKey()
    {
        //Generate the Salt, with any custom logic and using the user's ID
        string salt = String.Format("{0},{0},{0},{0},{0},{0},{0},{0}", m_key.Length);

        Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(m_key), Encoding.UTF8.GetBytes(salt), 100);
        m_cryptoKey = pwdGen.GetBytes(AESEncryptor.KeySize / 8);
        m_cryptoIV = pwdGen.GetBytes(AESEncryptor.KeySize / 8);
    }

    private Stream UpgradeFile(Stream stream)
    {
        //TODO upgrade between versions
        if (FeatureSettingsManager.IsDebugEnabled)
            PersistenceFacade.Log("NOT YET IMPLEMENTED");

        return stream;
    }

    public override string ToString()
    {
        return (m_data != null) ? m_data.ToJSON() : "";
    }

    // [DGR] Method added so the default persistence can be merged with the current one
    public bool Merge(string json)
    {
        bool returnValue = false;
        if (m_data == null)
        {
            LoadState = PersistenceStates.LoadState.NotFound;            
        }
        else
        {
            returnValue = m_data.Merge(json);
            if (returnValue)
            {
                LoadState = PersistenceStates.LoadState.OK;
				Systems_Load();
            }
            else
            {
                LoadState = PersistenceStates.LoadState.Corrupted;
            }
        }

        return returnValue;    
    }

#region systems
	private void Systems_Reset()
	{
		if (Systems != null)
		{
			int count = Systems.Count;
			for (int i = 0; i < count; i++)
			{
				Systems[i].Reset();
			}
		}
	}

	private void Systems_Load()
	{
		if (Systems != null && LoadState == PersistenceStates.LoadState.OK)
        {
            try
            {
                Systems_Reset();
                
				int count = Systems.Count;
				for (int i = 0; i < count; i++)
				{
					Systems[i].Load();
				}
            }
            catch (FGOL.Server.CorruptedSaveException)
            {
                LoadState = PersistenceStates.LoadState.Corrupted;
            }	
        }      
	}

	private void Systems_Save()
	{
		if (Systems != null)
		{
			int count = Systems.Count;
			for (int i = 0; i < count; i++)
			{
				Systems[i].Save();
			}
		}
	}

	public void Systems_RegisterSystem(PersistenceSystem system)
	{
		if (Systems == null)
		{
			Systems = new List<PersistenceSystem> ();
		}

		if (!Systems.Contains(system))
		{
			system.data = this;
			Systems.Add(system);
		}
	}

	public void Systems_UnregisterSystem(PersistenceSystem system)
	{
		if (Systems != null && Systems.Contains(system))
		{
			Systems.Remove(system);
		}
	}
#endregion

#if UNITY_EDITOR
    public Dictionary<string, object> rawSaveData
    {
        get { return m_data.rawData; }
    }
#endif
    
}
