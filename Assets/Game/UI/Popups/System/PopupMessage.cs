/// <summary>
/// This class is responsible for controlling a popup that can be used to show any message. It consists of the following elements that can be configurated:
/// 1)Title.
/// 2)Message to be shown in the body.
/// 3)Buttons. Two layouts are supported:
///    3.1)Two buttons: Confirm, cancel. A delegate can be assigned to each.
///    3.2)A single button: Confirm. A delegate can be assinged to it.
/// 
/// The configuration of the class is done by using the class <c>PopupMessage.Config</c>
/// </summary>

using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PopupController))]
public class PopupMessage : MonoBehaviour
{
    /// <summary>
    /// This class is used to store the configuration to use when a <c>PopupMessage</c> is started. This configuration decides the amount of buttons and the delegates that should be called
    /// when each button is clicked.    
    /// </summary>
    public class Config
    {
        public string TitleTid { get; set; }
        public string MessageTid { get; set; } 
        public string[] MessageParams { get; set; }

        // Use should use these properties instead of the ones defined above if you need to write a text directly. Using tids is the recommended approach, you should use these properties only
        // in exceptional cases when you don't have t
        public string TitleText { get; set; }
        public string MessageText { get; set; }

        public string ConfirmButtonTid { get; set; }
        public Action OnConfirm { get; set; }

        public string CancelButtonTid { get; set; }
        public Action OnCancel { get; set; }

        public string ExtraButtonTid { get; set; }
        public Action OnExtra { get; set; }

        public enum EButtonsMode
        {
            None,
            Confirm,
            ConfirmAndCancel,
            ConfirmAndExtraAndCancel

        };
        public EButtonsMode ButtonMode { get; set; }
        
        public Config()
        {
            Reset();
        }

        public void Reset()
        {
            TitleTid = null;
            MessageTid = null;
            MessageParams = null;
            TitleText = null;
            MessageText = null;
            ConfirmButtonTid = "TID_GEN_OK";
            OnConfirm = null;
            CancelButtonTid = "TID_GEN_CANCEL";
            OnCancel = null;
            ButtonMode = EButtonsMode.None;
        }
    }

    public static Config sm_config;
    public static Config GetConfig()
    {
        if (sm_config == null)
        {
            sm_config = new Config();
        }
        else
        {
            sm_config.Reset();
        }

        return sm_config;
    }

    public const string PATH = "UI/Popups/Message/PF_PopupMessage";

    [SerializeField]
    private Localizer m_titleText;

    [SerializeField]
    private Localizer m_messageText;

    [SerializeField]
    private Button m_buttonCancel;
    [SerializeField]
    private GameObject m_buttonCancelRoot;

    [SerializeField]
    private Button m_buttonConfirmCenter;
    [SerializeField]
    private GameObject m_buttonConfirmCenterRoot;

    [SerializeField]
    private Button m_buttonConfirmRight;
    [SerializeField]
    private GameObject m_buttonConfirmRightRoot;

    [SerializeField]
    private Localizer m_buttonCancelText;

    [SerializeField]
    private Localizer m_buttonConfirmCenterText;

    [SerializeField]
    private Localizer m_buttonConfirmRightText;

    private Config m_config;    

    private bool IsInited { get; set; }
    private void Awake()
    {        
        DebugUtils.Assert(m_titleText != null, "Required field!");
        DebugUtils.Assert(m_messageText != null, "Required field!");        
        DebugUtils.Assert(m_buttonConfirmCenter != null, "Required field!");        
        DebugUtils.Assert(m_buttonConfirmCenterText != null, "Required field!");        

        IsInited = false;

        if (m_buttonCancel != null)
        {
            m_buttonCancel.onClick.AddListener(OnCancel);
        }

        if (m_buttonConfirmRight != null)
        {
            m_buttonConfirmRight.onClick.AddListener(OnConfirm);
        }
    }    

    private void Reset()
    {
        if (m_config != null)
        {
            if (m_config.ButtonMode == Config.EButtonsMode.ConfirmAndExtraAndCancel)
            {
                m_buttonConfirmCenter.onClick.RemoveListener(OnExtra);
            }
            else if (m_config.ButtonMode == Config.EButtonsMode.Confirm)
            {
                m_buttonConfirmCenter.onClick.RemoveListener(OnConfirm);
            }
        }        
    }

    private void Start()
    {
#if UNITY_EDITOR        
        // Only for testing purposes        
        //Test_Open();        
#endif
    }

    public void Configure(Config config)
    {
        if (m_config != null)
        {
            Reset();
        }

        m_config = config;

        // Tid has priority over the plain text
        if (m_config.TitleTid != null)
        {
            m_titleText.Localize(m_config.TitleTid);
        }
        else if (m_config.TitleText != null)
        {
            m_titleText.text.text = m_config.TitleText;
        }

        if (m_config.MessageTid != null)
        {
            if (m_config.MessageParams == null)
            {
                m_messageText.Localize(m_config.MessageTid);
            }
            else
            {
                m_messageText.Localize(m_config.MessageTid, m_config.MessageParams);
            }
        }
        else if (m_config.MessageText != null)
        {
            m_messageText.text.text = m_config.MessageText;
        }

        // All buttons disabled by default since the required ones will be enabled depending on the button mode
        if (m_buttonCancelRoot != null)
        {
            m_buttonCancelRoot.SetActive(false);
        }
        
        m_buttonConfirmCenterRoot.SetActive(false);

        if (m_buttonConfirmRightRoot != null)
        {
            m_buttonConfirmRightRoot.SetActive(false);
        }

        switch (m_config.ButtonMode)
        {            
            case Config.EButtonsMode.Confirm:
            {
                // Center button chosen since there's only one
                m_buttonConfirmCenterRoot.SetActive(true);                
                m_buttonConfirmCenterText.Localize(m_config.ConfirmButtonTid);
                m_buttonConfirmCenter.onClick.AddListener(OnConfirm);
            }
            break;

            case Config.EButtonsMode.ConfirmAndCancel:
            case Config.EButtonsMode.ConfirmAndExtraAndCancel:
            {
                if (m_buttonCancelRoot != null)
                {
                    // Cancel button
                    m_buttonCancelRoot.SetActive(true);
                }

                if (m_buttonCancelText != null)
                {
                    m_buttonCancelText.Localize(m_config.CancelButtonTid);
                }

                // Confirm button: the right button is used because there are two buttons
                if (m_buttonConfirmRightRoot != null)
                {
                    m_buttonConfirmRightRoot.SetActive(true);
                }

                if (m_buttonConfirmRightText != null)
                {
                    m_buttonConfirmRightText.Localize(m_config.ConfirmButtonTid);
                }

                if (m_config.ButtonMode == Config.EButtonsMode.ConfirmAndExtraAndCancel)
                {
                    m_buttonConfirmCenterRoot.SetActive(true);
                    m_buttonConfirmCenterText.Localize(m_config.ExtraButtonTid);
                    m_buttonConfirmCenter.onClick.AddListener(OnExtra);
                }
            }
            break;
        }       
    }

    /// <summary>
    /// Method called when the user clicks on any Confirm buttom. It's called by the editor
    /// </summary>
    public void OnConfirm()
    {
        if (m_config != null && m_config.OnConfirm != null)
        {
            m_config.OnConfirm();
        }

        Close();
    }

    /// <summary>
    /// Method called when the user clicks on the cancel buttom. It's called by the editor
    /// </summary>
    public void OnCancel()
    {
        if (m_config != null && m_config.OnCancel != null)
        {
            m_config.OnCancel();
        }

        Close();
    }

    public void OnExtra()
    {
        if (m_config != null && m_config.OnExtra != null)
        {
            m_config.OnExtra();
        }

        Close();
    }

    /// <summary>
    /// Closes and destroyes this popup
    /// </summary>
    public void Close()
    {        
        GetComponent<PopupController>().Close(true);
    }

    #region test
    private static int sm_testTimesOpened = 0;

    private void Test_Open()
    {
        switch (sm_testTimesOpened)
        {
            case 0:
            {
                Test_ConfigNoButtons();
            }
            break;

            case 1:
            {
                Test_ConfigConfirm();
            }
            break;

            case 2:
            {
                Test_ConfigConfirmAndCancel();
            }
            break;

            case 3:
            {
                Test_ConfigConfirmAndExtraAndCancel();
            }
            break;
        }

        sm_testTimesOpened++;
        sm_testTimesOpened = sm_testTimesOpened % 4;
    }

    private void Test_ConfigNoButtons()
    {
        Config config = GetConfig();
        config.TitleTid = "TID_DRAGON_BALROG_NAME";
        config.MessageTid = "TID_DRAGON_BALROG_DESC";
        Configure(config);

        // The popup is closed automatically after a while
        StartCoroutine(Test_CloseAutomatically(2.0F));        
    }

    System.Collections.IEnumerator Test_CloseAutomatically(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Close();
    }

    private void Test_ConfigConfirm()
    {
        Config config = GetConfig();
        config.TitleTid = "TID_DRAGON_BALROG_NAME";
        config.MessageTid = "TID_DRAGON_BALROG_DESC";        
        config.ButtonMode = Config.EButtonsMode.Confirm;
        config.OnConfirm = Test_OnConfirm;
        Configure(config);
    }

    private void Test_ConfigConfirmAndCancel()
    {
        Config config = GetConfig();
        config.TitleTid = "TID_DRAGON_BALROG_NAME";
        config.MessageTid = "TID_DRAGON_BALROG_DESC";
        config.ButtonMode = Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = Test_OnConfirm;
        config.OnCancel = Test_OnCancel;
        config.ConfirmButtonTid = "TID_GEN_ACCEPT";        
        Configure(config);
    }

    private void Test_ConfigConfirmAndExtraAndCancel()
    {
        Config config = GetConfig();
        config.TitleTid = "TID_DRAGON_BALROG_NAME";
        config.MessageTid = "TID_DRAGON_BALROG_DESC";
        config.ButtonMode = Config.EButtonsMode.ConfirmAndExtraAndCancel;
        config.OnConfirm = Test_OnConfirm;
        config.OnCancel = Test_OnCancel;
        config.OnExtra = Test_OnExtra;
        config.ConfirmButtonTid = "TID_GEN_ACCEPT";
        config.ExtraButtonTid = "EXTRA";
        Configure(config);
    }

    private void Test_OnConfirm()
    {
        Debug.Log("On confirm button clicked");
    }

    private void Test_OnCancel()
    {
        Debug.Log("On cancel button clicked");
    }

    private void Test_OnExtra()
    {
        Debug.Log("On extra button clicked");
    }
    #endregion
}
