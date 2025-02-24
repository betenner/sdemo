using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Facebook.Unity;

public class SdkManager
{
    private static SdkManager _instance;
    public static SdkManager instance
    {
        get 
        { 
            _instance ??= new SdkManager();
            return _instance;
        }
    }

    public void InitFB()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback);
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }
}
