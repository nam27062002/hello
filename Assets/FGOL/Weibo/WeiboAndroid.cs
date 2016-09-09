#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Weibo.Unity
{ 
    public class WeiboAndroid : IWeibo
    {
        public static AndroidJavaObject m_weiboClass = null;
        public void Init(string key, string secret, string redirectURL)
        {
            Debug.Log("WeiboAndroid Init");
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (jc == null)
            {
                Debug.LogError("Could not find class com.unity3d.player.UnityPlayer!");
                return;
            }

            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            if (jo == null)
            {
                Debug.LogError("Could not find currentActivity!");
                return;
            }

            // find the plugin instance
            using (var pluginClass = new AndroidJavaClass("com.fgol.FGOLWeiboAuth"))
            {
                m_weiboClass = pluginClass.CallStatic<AndroidJavaObject>("Initialise", jo, key, secret, redirectURL, "ApplicationInitialization");
            }
        }

        public void Login()
        {
            Debug.Log("WeiboAndroid Login");
            m_weiboClass.Call("StartLogin");
        }
        
        public bool IsInitialised()
        {
            return (m_weiboClass != null);
        }
    }
}
#endif