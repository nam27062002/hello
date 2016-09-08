using UnityEngine;
using FGOL.ThirdParty.MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;

public class WeiboTokenSaver
{
    private string m_token = null;
    private string m_refreshToken = null;
    private string m_userID = null;
    private string m_tokenExpires = null;

    public string Token { get { return m_token; } }
    public string RefreshToken { get { return m_refreshToken; } }
    public string UserID { get { return m_userID; } }
    public string TokenExpiry { get { return m_tokenExpires; } }

    public WeiboTokenSaver()
    {
        LoadWeiboAccessTokenDetails();
    }

    public void SaveTokenDetails(string token, string refreshToken, string uid, string expiryTime)
    {
        if (token == null || uid == null || expiryTime == null)
        {
            Debug.LogError("WeiboTokenSaver: Attempting to save bad weibo data, some values were null.");
            DeleteTokenDetails();
            return;
        }

        m_token = token;
        m_refreshToken = refreshToken;
        m_userID = uid;
        m_tokenExpires = expiryTime;

        Dictionary<string, string> saveDict = new Dictionary<string, string>();
        saveDict["WeiboToken"] = m_token;
        saveDict["WeiboUserID"] = m_userID;
        saveDict["WeiboTokenExpires"] = expiryTime;
        if (m_refreshToken != null)
        {
            saveDict["WeiboRefreshToken"] = m_refreshToken;
        }

        string serialised = Json.Serialize(saveDict);
        PlayerPrefs.SetString("WeiboTokenDetails", serialised);
    }

    private void LoadWeiboAccessTokenDetails()
    {
        if (PlayerPrefs.HasKey("WeiboTokenDetails"))
        {
            string weiboTokenDetails = PlayerPrefs.GetString("WeiboTokenDetails");
            Dictionary<string, object> dict = Json.Deserialize(weiboTokenDetails) as Dictionary<string, object>;

            if (dict == null)
            {
                return;
            }

            if (dict.ContainsKey("WeiboToken"))
                m_token = dict["WeiboToken"] as string;

            if (dict.ContainsKey("WeiboUserID"))
                m_userID = dict["WeiboUserID"] as string;
            
            if (dict.ContainsKey("WeiboTokenExpires"))
                m_tokenExpires = dict["WeiboTokenExpires"] as string;
            
            if (dict.ContainsKey("WeiboRefreshToken"))
                m_refreshToken = dict["WeiboRefreshToken"] as string;

            //we shouldn't allow partial data loading. missing the refresh token is fine.
            if (m_token == null || m_userID == null || m_tokenExpires == null)
            {
                Debug.Log("WeiboTokenSaver: Loaded bad data, some valued were loading null.");
                DeleteTokenDetails();
            }
        }
    }

    public void DeleteTokenDetails()
    {
        m_token = null;
        m_refreshToken = null;
        m_userID = null;
        m_tokenExpires = null;
        PlayerPrefs.DeleteKey("WeiboTokenDetails");
    }
}
