using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_IPHONE
using System.Runtime.InteropServices;
namespace Weibo.Unity
{
    public class WeiboIOS : IWeibo
    {
#region DLL_IMPORTS
		[DllImport("__Internal")] private static extern void _InitWeibo(string appkey, string secret, string redirectURL, string unityListenerName);
        [DllImport("__Internal")] private static extern bool _IsWeiboInitialised();
	    [DllImport("__Internal")] private static extern void _WeiboLogin();
#endregion

        public void Init(string key, string secret, string redirectURL)
        {
            Debug.Log("WeiboIOS Init");
			_InitWeibo(key, secret, redirectURL, "ApplicationInitialization");
        }

        public void Login()
        {
            Debug.Log("WeiboIOS Login");
			_WeiboLogin();
        }

        public bool IsInitialised()
        {
			return _IsWeiboInitialised();
        }
    }
}
#endif