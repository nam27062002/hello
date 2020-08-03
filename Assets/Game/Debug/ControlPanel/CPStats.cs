// CPVersion.cs
// Hungry Dragon
// 
// Created by Miguel Angel Linares on 17/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple widget to display number version.
/// </summary>
public class CPStats : MonoBehaviour {
    //------------------------------------------------------------------//
    // MEMBERS															//
    //------------------------------------------------------------------//
    public TextMeshProUGUI m_DeviceModel;
    public TextMeshProUGUI m_ProcessorType;
    public TextMeshProUGUI m_DeviceGeneration;    
    public TextMeshProUGUI m_FpsLabel;
	public TextMeshProUGUI m_ScreenSize;
	public TextMeshProUGUI m_LevelName;
    public TextMeshProUGUI m_freeMemory;
    public TextMeshProUGUI m_maxMemory;
    public TextMeshProUGUI m_totalMemory;

    ControlPanel m_ControlPanel;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() 
	{
		// Just initialize text
		m_DeviceModel.text = "Model: " + SystemInfo.deviceModel;
        m_ProcessorType.text = "Processor: " + SystemInfo.processorType;
#if UNITY_IOS
        m_DeviceGeneration.text = "Generation: " + UnityEngine.iOS.Device.generation;
#else
        m_DeviceGeneration.transform.parent.gameObject.SetActive(false);
#endif

        m_FpsLabel.text = "FPS: ";
		m_LevelName.text = "Scene Name: "+ SceneManager.GetActiveScene().name;

        m_freeMemory.text = "Heap memory: " + SystemInfo.heapMemorySize;
        m_maxMemory.text = "Total memory: " + SystemInfo.deviceMemorySize;
        m_totalMemory.text = "Available memory: " + SystemInfo.availableMemorySize;

        m_ControlPanel = GetComponentInParent<ControlPanel>();

        DeviceToken = null;
    }    

	private void Update()
	{
		m_FpsLabel.text = "FPS: " + FeatureSettingsManager.instance.AverageSystemFPS;
        m_ScreenSize.text = "Screen Size: " + Screen.currentResolution.width + "x" + Screen.currentResolution.height;

        if (NotificationsManager.SharedInstance != null)
        {
            if (DeviceToken != NotificationsManager.SharedInstance.GetDeviceToken())
            {
                DeviceToken = NotificationsManager.SharedInstance.GetDeviceToken();
                DeviceToken_UpdateUI();
            }
        }
    }

    #region DeviceToken

    public TextMeshProUGUI m_DeviceTokenLabel;
    private string DeviceToken { get; set; }    

    private void DeviceToken_UpdateUI()
    {
        if (m_DeviceTokenLabel != null && NotificationsManager.SharedInstance != null)
        {            
            m_DeviceTokenLabel.text = "Device Token: \n" + DeviceToken;            
            Debug.Log("Device Token: " + DeviceToken);            
        }
    }

    public void DeviceToken_SendToServer()
    {
        /*if (NotificationsManager.SharedInstance != null)
        {
            NotificationsManager.SharedInstance.SendGCMRegistryID();
        }*/
    }

    public void Unity_RegisterNotifications()
    {
#if UNITY_IOS
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound, true);
#endif
    }

    public void Unity_ClearRemoteNotifications()
    {
#if UNITY_IOS
        UnityEngine.iOS.NotificationServices.ClearRemoteNotifications();
#endif    
    }
    
    public void Unity_UnregisterRemoteNotifications()
    {
#if UNITY_IOS
        UnityEngine.iOS.NotificationServices.UnregisterForRemoteNotifications();
#endif    
    }
    
    #endregion
}