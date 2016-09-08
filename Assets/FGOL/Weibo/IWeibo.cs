namespace Weibo.Unity
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;


    public interface IWeibo
    {
        void Init(string key, string secret, string redirectURL);
        void Login();
        bool IsInitialised();
    }
}
