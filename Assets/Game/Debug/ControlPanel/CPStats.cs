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
    public TextMeshProUGUI m_DeviceGeneration;    
    public TextMeshProUGUI m_FpsLabel;
	public TextMeshProUGUI m_ScreenSize;
	public TextMeshProUGUI m_LevelName;

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
#if UNITY_IOS
        m_DeviceGeneration.text = "Generation: " + UnityEngine.iOS.Device.generation;
#else
        m_DeviceGeneration.transform.parent.gameObject.SetActive(false);
#endif

        m_FpsLabel.text = "FPS: ";
		m_ScreenSize.text = "Screen Size: " + Screen.width + "x"+ Screen.height;
		m_LevelName.text = "Scene Name: "+ SceneManager.GetActiveScene().name;
		m_ControlPanel = GetComponentInParent<ControlPanel>();

        DeviceToken = null;
    }    

	private void Update()
	{
		m_FpsLabel.text = "FPS: " + m_ControlPanel.GetFPS();

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

            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Debug.Log("Device Token: " + DeviceToken);
            }
        }
    }

    public void DeviceToken_SendToServer()
    {
        /*if (NotificationsManager.SharedInstance != null)
        {
            NotificationsManager.SharedInstance.SendGCMRegistryID();
        }*/
    }
    #endregion
}