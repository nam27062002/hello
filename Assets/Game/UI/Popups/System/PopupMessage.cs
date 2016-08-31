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

        public string ConfirmButtonTid { get; set; }
        public Action OnConfirm { get; set; }

        public string CancelButtonTid { get; set; }
        public Action OnCancel { get; set; }

        public enum EButtonsMode
        {
            None,
            Confirm,
            ConfirmAndCancel
        };
        public EButtonsMode ButtonMode { get; set; }

        public Config()
        {
            ConfirmButtonTid = "TID_GEN_OK";
            CancelButtonTid = "TID_GEN_CANCEL";
            ButtonMode = EButtonsMode.None;
        }
    }

    public static readonly string PATH = "UI/Popups/Message/PF_PopupMessage";

    [SerializeField]
    private Localizer m_titleText;

    [SerializeField]
    private Localizer m_messageText;

    [SerializeField]
    private Button m_buttonCancel;

    [SerializeField]
    private Button m_buttonConfirmCenter;

    [SerializeField]
    private Button m_buttonConfirmRight;

    [SerializeField]
    private Localizer m_buttonCancelText;

    [SerializeField]
    private Localizer m_buttonConfirmCenterText;

    [SerializeField]
    private Localizer m_buttonConfirmRightText;

    private Config m_config;    

    private void Awake()
    {        
        DebugUtils.Assert(m_titleText != null, "Required field!");
        DebugUtils.Assert(m_messageText != null, "Required field!");
        DebugUtils.Assert(m_buttonCancel != null, "Required field!");
        DebugUtils.Assert(m_buttonConfirmCenter != null, "Required field!");
        DebugUtils.Assert(m_buttonConfirmRight != null, "Required field!");
        DebugUtils.Assert(m_buttonConfirmCenterText != null, "Required field!");
        DebugUtils.Assert(m_buttonConfirmRightText != null, "Required field!");

        // The listeners are added here to make sure they will be added only once
        m_buttonCancel.onClick.AddListener(OnCancel);
        m_buttonConfirmCenter.onClick.AddListener(OnConfirm);
        m_buttonConfirmRight.onClick.AddListener(OnConfirm);
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
        m_config = config;
        m_titleText.Localize(m_config.TitleTid);
        m_messageText.Localize(m_config.MessageTid);

        // All buttons disabled by default since the required ones will be enabled depending on the button mode
        m_buttonCancel.gameObject.SetActive(false);
        m_buttonConfirmCenter.gameObject.SetActive(false);
        m_buttonConfirmRight.gameObject.SetActive(false);

        switch (m_config.ButtonMode)
        {            
            case Config.EButtonsMode.Confirm:
            {
                // Center button chosen since there's only one
                m_buttonConfirmCenter.gameObject.SetActive(true);                
                m_buttonConfirmCenterText.Localize(m_config.ConfirmButtonTid);                
            }
            break;

            case Config.EButtonsMode.ConfirmAndCancel:
            {
                // Cancel button
                m_buttonCancel.gameObject.SetActive(true);                    
                m_buttonCancelText.Localize(m_config.CancelButtonTid);

                // Confirm button: the right button is used because there are two buttons
                m_buttonConfirmRight.gameObject.SetActive(true);
                m_buttonConfirmRightText.Localize(m_config.ConfirmButtonTid);                
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
        }

        sm_testTimesOpened++;
        sm_testTimesOpened = sm_testTimesOpened % 3;
    }

    private void Test_ConfigNoButtons()
    {
        Config config = new Config();
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
        Config config = new Config();
        config.TitleTid = "TID_DRAGON_BALROG_NAME";
        config.MessageTid = "TID_DRAGON_BALROG_DESC";
        config.ButtonMode = Config.EButtonsMode.Confirm;
        config.OnConfirm = Test_OnConfirm;
        Configure(config);
    }

    private void Test_ConfigConfirmAndCancel()
    {
        Config config = new Config();
        config.TitleTid = "TID_DRAGON_BALROG_NAME";
        config.MessageTid = "TID_DRAGON_BALROG_DESC";
        config.ButtonMode = Config.EButtonsMode.ConfirmAndCancel;
        config.OnConfirm = Test_OnConfirm;
        config.OnCancel = Test_OnCancel;
        config.ConfirmButtonTid = "TID_GEN_ACCEPT";        
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
    #endregion
}
