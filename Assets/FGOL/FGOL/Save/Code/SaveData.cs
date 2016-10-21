using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using FGOL.Common;
using FGOL.Encryption;
using FGOL.ThirdParty;
using FGOL.Save.SaveStates;
using System.Collections.Generic;

namespace FGOL.Save
{
    public class SaveData
    {
    	public const int HeaderVersion = 3;
        public const int ReservedDiskSpace = 1024 * 1024;

        private const string VersionKey = "version";
        private const string PurchasesKey = "purchases";

        private string m_savePath = null;
        private string m_key = null;

        private JSONNestedKeyValueStore m_saveData = null;

        private byte[] m_cryptoKey = null;
        private byte[] m_cryptoIV = null;

        private int m_modifiedTime = 0;
		private string m_deviceName = null;

        public SaveData(string key) : this(SaveUtilities.GetSavePath(key), key) {}

        public SaveData(string savePath, string key)
        {
            m_savePath = savePath;
            m_key = key;

            m_saveData = new JSONNestedKeyValueStore();

            GenerateEncryptionKey();
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
            get { return m_saveData[VersionKey] as string; }
            set { m_saveData[VersionKey] = value; }
        }

        public object Purchases
        {
            get { return m_saveData[PurchasesKey]; }
            set { m_saveData[PurchasesKey] = value; }
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
                return m_saveData[key];
            }
            set
            {
                if (key != VersionKey 
                    && key != PurchasesKey)
                {
                    m_saveData[key] = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(key + " is a reserved key");
                }
            }
        }

        public SaveState Save(bool backUpOnFail = true)
        {
            SaveState saveState = SaveState.DiskSpace;

            string playerPrefKey = "Save." + m_key + ".sav";
            
            //TODO record current platform of save

            m_deviceName = SystemInfo.deviceName;

            //Work around for android devices not returning a valid device name!
            if(m_deviceName == "<unknown>")
            {
                m_deviceName = SystemInfo.deviceModel;
            }

            m_modifiedTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            m_saveData["deviceName"] = m_deviceName;
            m_saveData["modifiedTime"] = m_modifiedTime;

            MemoryStream saveStream = SaveToStream();

            if(saveStream != null)
            {
                byte[] saveBytes = saveStream.ToArray();

                //Log error if we have a bigger save then reserved space!
                if (saveBytes.Length >= (ReservedDiskSpace - sizeof(int)))
                {
                    Debug.LogError("SaveData (Save) :: SaveData has gone over reserved diskspace of: " + ReservedDiskSpace);
                }

                try
                {
                    using(FileStream fs = new FileStream(m_savePath, FileMode.Create, FileAccess.Write))
                    {
                        using(MemoryStream ms = new MemoryStream())
                        {
                            //We will pad out the save file with 0s to ensure we have reserved diskspace
                            byte[] lengthBytes = BitConverter.GetBytes(saveBytes.Length);
                            ms.Write(lengthBytes, 0, lengthBytes.Length);
                            ms.Write(saveBytes, 0, saveBytes.Length);

                            int requiredPadding = ReservedDiskSpace - (lengthBytes.Length + saveBytes.Length);

                            if(requiredPadding > 0)
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

                    saveState = SaveState.OK;
                }
                catch(UnauthorizedAccessException)
                {
                    //TODO log error?
                    saveState = SaveState.PermissionError;
                }
                catch(Exception e)
                {
                    Debug.LogError("SaveData (Save) :: Exception saving to disk");
                    DebugUtils.LogException(e);
                }
                finally
                {
                    if(backUpOnFail && saveState != SaveState.OK)
                    {
                        PlayerPrefs.SetString(playerPrefKey, Convert.ToBase64String(saveBytes));
                    }
                }
            }

            PlayerPrefs.Save();

            return saveState;
        }

        public MemoryStream SaveToStream()
        {
            MemoryStream stream = null;

            string json = m_saveData.ToJSON();

            //Compress the data using LZF compression
            byte[] compressed = null;

            using(MemoryStream compMemStream = new MemoryStream())
            {
                using(StreamWriter writer = new StreamWriter(compMemStream, Encoding.UTF8))
                {
                    writer.Write(json);
                    writer.Close();

                    compressed = CLZF2.Compress(compMemStream.ToArray());
                }
            }

            if(compressed != null)
            {
                byte[] encrypted = AESEncryptor.Encrypt(m_cryptoKey, m_cryptoIV, compressed);

                if(encrypted != null)
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

        public LoadState Load()
        {
            LoadState state = LoadState.NotFound;

            if(File.Exists(m_savePath))
            {
                try
                {
                    using(MemoryStream ms = SaveUtilities.GetUnpaddedSaveData(m_savePath))
                    {
                        state = LoadFromStream(ms);
                    }
                }
                catch(UnauthorizedAccessException e)
                {
                    Debug.LogError("Permissions error when loading file at path: " + m_savePath);
                    DebugUtils.LogException(e);
                    state = LoadState.PermissionError;
                }
                catch(FileNotFoundException)
                {
                    Debug.LogWarning("No save file found at path: " + m_savePath);
                }
                catch(Exception e)
                {
                    Debug.LogError("Exception when loading file at path: " + m_savePath);
                    DebugUtils.LogException(e);
                    state = LoadState.Corrupted;
                }
            }
            else
            {
                Debug.LogWarning("No save file found at path: " + m_savePath);
            }


            //If we can't load from disk try player prefs! Player prefs will be set if we detected an issue when saving!
            if(state != LoadState.OK)
            {
                string playerPrefKey = "Save." + m_key + ".sav";
                string prefSaveString = PlayerPrefs.GetString(playerPrefKey);

                if(!string.IsNullOrEmpty(prefSaveString))
                {
                    try
                    {
                        using(MemoryStream ms = new MemoryStream(Convert.FromBase64String(prefSaveString)))
                        {
                            state = LoadFromStream(ms);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogWarning("Unable to parse save data in PlayerPrefs");
                        Debug.LogWarning(e);
                    }
                }
            }

            return state;
        }

        private void OnLoad()
        {
            if (m_saveData != null)
            {
                object obj = m_saveData["modifiedTime"];
                if (obj != null)
                {
                    m_modifiedTime = Convert.ToInt32(obj);
                }

                obj = m_saveData["deviceName"];
                if (obj != null)
                {
                    m_deviceName = Convert.ToString(obj);
                }
            }
        }

        public LoadState LoadFromString(string data)
        {
            LoadState state = LoadState.Corrupted;

            if (m_saveData.FromJSON(data))
            {
                OnLoad();
                state = LoadState.OK;
            }
            else
            {
                Debug.LogWarning("Trying to load invalid JSON file at path: " + m_savePath);
                state = LoadState.Corrupted;
            }

            return state;
        }

        public LoadState LoadFromStream(Stream stream)
        {
            LoadState state = LoadState.Corrupted;

            try
            {
                byte[] decompressed = null;
                byte[] contentBytes = null;

                bool versionOkay = false;

                //Check version first
                int headerVersion = SaveUtilities.DeserializeVersion(stream);

                if (headerVersion == -1)
                {
                    state = LoadState.Corrupted;
                }
                else if(headerVersion < HeaderVersion)
                {
                    stream = UpgradeFile(stream);

                    if(stream != null)
                    {
                        versionOkay = true;
                    }
                    else
                    {
                        state = LoadState.Corrupted;
                    }
                }
                else if(headerVersion > HeaderVersion)
                {
                    state = LoadState.VersionMismatch;
                }
                else
                {
                    versionOkay = true;
                }

                if (versionOkay)
                {
                    if(IsValidFile(stream, ref contentBytes))
                    {
                        byte[] decrypted = AESEncryptor.Decrypt(m_cryptoKey, m_cryptoIV, contentBytes);

                        if(decrypted != null)
                        {
                            decompressed = CLZF2.Decompress(decrypted);
                        }
                        else
                        {
                            Debug.LogError("SaveData :: Decryption failed!");
                            state = LoadState.Corrupted;
                        }
                    }
                    else
                    {
                        Debug.LogError("SaveData :: File Corrupted!");
                        state = LoadState.Corrupted;
                    }
                }

                if(decompressed != null)
                {
                    using(MemoryStream jsonMemoryStream = new MemoryStream(decompressed))
                    {
                        using(StreamReader reader = new StreamReader(jsonMemoryStream))
                        {
                            if(m_saveData.FromJSON(reader.ReadToEnd()))
                            {
                                OnLoad();
                                state = LoadState.OK;
                            }
                            else
                            {
                                Debug.LogWarning("Trying to load invalid JSON file at path: " + m_savePath);
                                state = LoadState.Corrupted;
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                //TODO determine exception types and see if any are recoverable!
                Debug.LogWarning("Exception when parsing file from stream");
                Debug.LogWarning(e);
            }

            return state;
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

            if(validHeader)
            {
                contentBytes = new byte[contentLength];
                int contentRead = stream.Read(contentBytes, 0, contentLength);

                if(contentRead == contentLength)
                {
                    string contentHash = SaveUtilities.CalculateMD5Hash(contentBytes);

                    if(md5Hash == contentHash)
                    {
                        valid = true;
                    }
                    else
                    {
                        Debug.LogError(string.Format("SaveData :: MD5 Checksum Failed (Got {0} expected {1})", md5Hash, contentHash));
                    }
                }
                else
                {
                    Debug.LogError(string.Format("SaveData :: Content length different to expected (Got {0} expected {1})", contentRead, contentLength));
                }
            }
            else
            {
                Debug.LogError("SaveData :: Invalid header detected!");
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
            Debug.Log("NOT YET IMPLEMENTED");
            return stream;
        }

        public override string ToString()
        {
            return (m_saveData != null) ? m_saveData.ToJSON() : "";
        }

        // [DGR] Method added so the default persistence can be merged with the current one
        public bool Merge(string json)
        {
            return (m_saveData != null) ? m_saveData.Merge(json) : false;            
        }        

#if UNITY_EDITOR
        public Dictionary<string, object> rawSaveData
        {
            get { return m_saveData.rawData; }
        }
#endif        
    }
}
