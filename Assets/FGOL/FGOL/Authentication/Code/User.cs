using FGOL.ThirdParty.MiniJSON;
using FGOL.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FGOL.Authentication
{
    public class User
    {
        public enum LoginType
        {
            FGOL,
            Facebook,
            GooglePlus,
            GameCenter,
            Weibo,
            Default
        }

        public class LoginCredentials
        {
            public string socialID = null;
            public string accessToken = null;
            public int expiry = -1;
            public PermissionType[] permissions = new PermissionType[0];
            
            public string accountName = null;

            public bool isAccessTokenValid
            {
                get { return !string.IsNullOrEmpty(accessToken) && expiry != -1; }
            }

            public LoginCredentials(string socialID, string accessToken = null, int expiry = -1, PermissionType[] permissions = null, string accountName = null)
            {
                this.socialID = socialID;

                if(accessToken != null)
                {
                    this.accessToken = accessToken;
                }

                if(expiry != -1)
                {
                    this.expiry = expiry;
                }

                if(permissions != null)
                {
                    this.permissions = permissions;
                }
                
                if (accountName != null)
                {
                    this.accountName = accountName;
                }
            }

			public void Invalidate( ) 
			{
				accessToken = null;
				expiry = -1;
				permissions = new PermissionType[0];
			}

            public Dictionary<string, string> ToDictionary()
            {
                return new Dictionary<string, string>
                {
                    { "socialID", socialID },
                    { "accessToken", accessToken }
                };
            }
        }

        private string m_userID = null;
        private string m_saveID = null;
        private string m_sessionToken = null;
        private int m_sessionExpiry = -1;

        private CloudCredentials m_cloudCredentials = new CloudCredentials();
        private string m_cloudSavePath = null;
        private string m_cloudSaveBucket = null;

        private Dictionary<LoginType, LoginCredentials> m_loginCredentials = new Dictionary<LoginType, LoginCredentials>();

        private bool m_cloudSaveAvailable = false;

        private bool m_loaded = false;

        public string ID
        {
            get { return m_userID; }
            set { m_userID = value; }
        }

        public string saveID
        {
            get { return m_saveID; }
            set { m_saveID = value; }
        }

        public Dictionary<LoginType, LoginCredentials> loginCredentials
        {
            get { return m_loginCredentials; }
        }

        public CloudCredentials cloudCredentials
        {
            get { return m_cloudCredentials; }
        }

        public string sessionToken
        {
            get { return m_sessionToken; }
            set { m_sessionToken = value; }
        }

        public int sessionExpiry
        {
            get { return m_sessionExpiry; }
            set { m_sessionExpiry = value; }
        }

        public string cloudSaveLocation
        {
            get { return m_cloudSavePath; }
            set { m_cloudSavePath = value; }
        }

        public string cloudSaveBucket
        {
            get { return m_cloudSaveBucket; }
            set { m_cloudSaveBucket = value; }
        }

        public bool cloudSaveAvailable
        {
            get { return m_cloudSaveAvailable; }
            set { m_cloudSaveAvailable = value; }
        }

        public User()
        {

        }

        public bool IsSessionValid()
        {
            return !string.IsNullOrEmpty(m_sessionToken) && m_sessionExpiry != -1;
        }

        public void InvalidateSession()
        {
            m_sessionToken = null;
            m_sessionExpiry = -1;
        }

        public void Load()
        {
            if(!m_loaded)
            {
                string userID = PlayerPrefs.GetString("LocalUserID");

                if(!string.IsNullOrEmpty(userID))
                {
                    m_userID = userID;
                }

                string saveID = PlayerPrefs.GetString("LocalSaveID");

                if(!string.IsNullOrEmpty(saveID))
                {
                    m_saveID = saveID;
                }

                m_cloudSaveAvailable = PlayerPrefs.GetInt("LocalUserCloudSaveAvailable", 0) == 1;

                string socialNetworksJson = PlayerPrefs.GetString("LocalUserAssociatedNetworks");

                if(!string.IsNullOrEmpty(socialNetworksJson))
                {
                    Dictionary<string, object> socialNetworks = Json.Deserialize(socialNetworksJson) as Dictionary<string, object>;

                    if(socialNetworks != null)
                    {
                        foreach(KeyValuePair<string, object> pair in socialNetworks)
                        {
                            Dictionary<string, object> credentials = pair.Value as Dictionary<string, object>;
                            
                            LoginCredentials loginCredentials = new LoginCredentials(credentials["socialID"] as string);
                            loginCredentials.accountName = credentials["accountName"] as string;

                            LoginType type = (LoginType)Enum.Parse(typeof(LoginType), pair.Key);

                            m_loginCredentials[type] = loginCredentials;
                        }
                    }
                }

                m_loaded = true;
            }
        }

        public void Save()
        {
            PlayerPrefs.SetString("LocalUserID", m_userID);
            PlayerPrefs.SetString("LocalSaveID", m_saveID);
            PlayerPrefs.SetInt("LocalUserCloudSaveAvailable", m_cloudSaveAvailable ? 1 : 0);

            Dictionary<string, Dictionary<string, string>> socialNetworks = new Dictionary<string, Dictionary<string, string>>();

            foreach(KeyValuePair<LoginType, LoginCredentials> pair in m_loginCredentials)
            {
                socialNetworks.Add(pair.Key.ToString(), new Dictionary<string, string> { { "socialID", pair.Value.socialID }, { "accountName", pair.Value.accountName } });
            }

            PlayerPrefs.SetString("LocalUserAssociatedNetworks", Json.Serialize(socialNetworks));

            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey("LocalUserID");
            PlayerPrefs.DeleteKey("LocalSaveID");
            PlayerPrefs.DeleteKey("LocalUserAssociatedNetworks");
            PlayerPrefs.DeleteKey("LocalUserCloudSaveAvailable");
            PlayerPrefs.Save();
        }
    }
}
