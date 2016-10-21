using System;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;

namespace Weibo.Unity
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class WeiboDummy : IWeibo
	{
		private string m_appKey;
		private string m_appSecret;
		private string m_redirectURL;
		private string m_token;
		private GameObject listener = null;
		public void Init(string appKey, string appSecret, string redirectURL)
		{
#if UNITY_EDITOR
			m_appKey = appKey;
			m_appSecret = appSecret;
            m_redirectURL = redirectURL;
			listener = GameObject.Find("WeiboGameObject");
#endif
		}

		public void Login()
        {
#if UNITY_EDITOR
			ShowEmptyMockDialog();
#endif
		}

        public bool IsInitialised()
        {
#if UNITY_EDITOR
			return true;
#else
			return false;
#endif
        }


#if UNITY_EDITOR
		public void GetAccessToken(bool success, string message)
		{
			if (success)
			{
				HTTPRequest req = new HTTPRequest(new Uri("https://api.weibo.com/oauth2/access_token"), HTTPMethods.Post, delegate (HTTPRequest request, HTTPResponse response)
				{
					if (response != null)
					{
						Debug.Log("WEIBODUMMY " + response.StatusCode + ": " + response.DataAsText);
						if (response.StatusCode == 200)
						{
							listener.SendMessage("OnLoginCompleteSuccess", response.DataAsText);
						}
						else
						{
							listener.SendMessage("OnLoginCompleteFailed", response.DataAsText);
						}
					}
					else
					{
						Debug.Log("WEIBODUMMY RESPONSE IS NULL " + request.Exception.Message);
					}
				});
				req.AddField("code", message);
				req.AddField("client_id", m_appKey);
				req.AddField("client_secret", m_appSecret);
				req.AddField("redirect_uri", m_redirectURL);
				req.AddField("grant_type", "authorization_code");
				req.Send();
			}
			else
			{
				listener.SendMessage("OnLoginCompleteFailed", message);
			}
		}

		private void ShowEmptyMockDialog()
		{
			string url = String.Format("https://api.weibo.com/oauth2/authorize?client_id={0}&redirect_uri={1}&display=default&language=en&grant_type=authorization_code", m_appKey, m_redirectURL);

			WeiboMockDialog dialog = GameObject.Find("WeiboGameObject").AddComponent<WeiboMockDialog>();
			dialog.SetURL(url);
			dialog.SetAccessTokenFunction(GetAccessToken);
        }
#endif

	}
}