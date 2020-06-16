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
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(PopupController))]
public abstract class IPopupMessage : MonoBehaviour
{
    [System.Serializable]
    /// <summary>
    /// This class is used to store the configuration to use when a <c>PopupMessage</c> is started. This configuration decides the amount of buttons and the delegates that should be called
    /// when each button is clicked.    
    /// </summary>
    public class Config
    {
		public enum ETextType {
			DEFAULT,
			SYSTEM
		}
		public ETextType TextType;

        public string TitleTid;
		public bool ShowTitle;
        public string MessageTid; 
        public string[] MessageParams;

        // Use should use these properties instead of the ones defined above if you need to write a text directly. Using tids is the recommended approach, you should use these properties only
        // in exceptional cases when you don't have t
        public string TitleText;
        public string MessageText;

        public string ConfirmButtonTid;
        public Action OnConfirm;
        public bool CloseOnConfirm;

        public string CancelButtonTid;
        public Action OnCancel;

        public string ExtraButtonTid;
        public Action OnExtra;		

        public enum EBackButtonStratety
        {
            None, // Back button is ignored and the popup stays open
            PerformConfirm,
            PerformCancel,
            PerformExtra,
            Close,
            Default // Default configuration: If close button is visible then the popup is just closed. If the Cancel button is visible, then Cancel is performed. In all other cases, Confirm is performed.
        }

        public EBackButtonStratety BackButtonStrategy;

        public enum EButtonsMode
        {
            None,
            Confirm,
            ConfirmAndCancel,
            ConfirmAndExtra,
            ConfirmAndExtraAndCancel

        };
        public EButtonsMode ButtonMode;

        public enum EHighlightButton {
            None,
            Confirm,
            Cancel,
            Extra
		}
        public EHighlightButton HighlightButton;
        
        public bool IsButtonCloseVisible;

        public Config()
        {
            Reset();
        }

        public void Reset()
        {
			TextType = ETextType.DEFAULT;
            TitleTid = null;
			ShowTitle = true;
            MessageTid = null;
            MessageParams = null;
            TitleText = null;
            MessageText = null;
            
            ConfirmButtonTid = "TID_GEN_OK";
            OnConfirm = null;
            
            CancelButtonTid = "TID_GEN_CANCEL";
            OnCancel = null;
            
            ExtraButtonTid = "";
            OnExtra = null;

            ButtonMode = EButtonsMode.None;
            HighlightButton = EHighlightButton.None;
            IsButtonCloseVisible = true;
            CloseOnConfirm = true; 

            // By default the popup stays open when the back button is pressed
            BackButtonStrategy = EBackButtonStratety.Default;
        }
    }
    
    public static Config GetConfig()
    {
        return new Config();        
    }

    [SerializeField] private Button m_buttonCancel;
    [SerializeField] private GameObject m_buttonCancelRoot;
    [SerializeField] private GameObject m_buttonCancelGlow;

    [Space]
    [FormerlySerializedAs("m_buttonConfirmCenter")]
    [SerializeField] private Button m_buttonExtra;
    [FormerlySerializedAs("m_buttonConfirmCenterRoot")]
    [SerializeField] private GameObject m_buttonExtraRoot;
    [SerializeField] private GameObject m_buttonExtraGlow;

    [Space]
    [FormerlySerializedAs("m_buttonConfirmRight")]
    [SerializeField] private Button m_buttonConfirm;
    [FormerlySerializedAs("m_buttonConfirmRightRoot")]
    [SerializeField] private GameObject m_buttonConfirmRoot;
    [SerializeField] private GameObject m_buttonConfirmGlow;

    [Space]
    [SerializeField]
    private GameObject m_buttonCloseRoot;

    private Config m_config;    

    private bool IsConfigured { get; set; }
    private void Awake()
    {        
        DebugUtils.Assert(m_buttonCloseRoot != null, "Required field!");

        // Add all listeners
        if(m_buttonCancel != null) m_buttonCancel.onClick.AddListener(OnCancel);
        if(m_buttonConfirm != null) m_buttonConfirm.onClick.AddListener(OnConfirm);
        if(m_buttonExtra != null) m_buttonExtra.onClick.AddListener(OnExtra);
    }

	private void OnDestroy() {
        // Remove all listeners
        if(m_buttonCancel != null) m_buttonCancel.onClick.RemoveListener(OnCancel);
        if(m_buttonConfirm != null) m_buttonConfirm.onClick.RemoveListener(OnConfirm);
        if(m_buttonExtra != null) m_buttonExtra.onClick.RemoveListener(OnExtra);

        // Reset if needed
        if(IsConfigured) {
            Reset();
        }
	}

	private void Reset()
    {
        // Clear config
        m_config = null;
        
        // Reset flag
        IsConfigured = false;
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
        // Reset previous config
        if (m_config != null)
        {
            Reset();
        }

        // Store new config
        m_config = config;
        
        // Apply new config
        // Back button handler
        PopupBackButtonHandlerWithAction backHandler =  GetComponent<PopupBackButtonHandlerWithAction>();
        if ( backHandler != null )
        {
            Config.EBackButtonStratety backButtonStrategy = m_config.BackButtonStrategy;
            if (backButtonStrategy == Config.EBackButtonStratety.Default)
            {
                if (m_config.IsButtonCloseVisible)
                {
                    backButtonStrategy = Config.EBackButtonStratety.Close;
                }
                else
                {
                    // Default configuration: If close button is visible then the popup is just closed. If the Cancel button is visible, then Cancel is performed. In all other cases, Confirm is performed.
                    switch(config.ButtonMode) {
                        case Config.EButtonsMode.Confirm:
                        case Config.EButtonsMode.ConfirmAndExtra: {
                            backButtonStrategy = Config.EBackButtonStratety.PerformConfirm;
						} break;

                        default: {
                            backButtonStrategy = Config.EBackButtonStratety.PerformCancel;
                        } break;
                    }
                }
            }

            switch (backButtonStrategy)
            {
                case Config.EBackButtonStratety.None:
                    backHandler.OnBackButton = null;
                    break;

                case Config.EBackButtonStratety.PerformConfirm:
                    backHandler.OnBackButton = OnConfirm;
                    break;

                case Config.EBackButtonStratety.PerformCancel:
                    backHandler.OnBackButton = OnCancel;
                    break;

                case Config.EBackButtonStratety.PerformExtra:
                    backHandler.OnBackButton = OnExtra;
                    break;

                case Config.EBackButtonStratety.Close:
                    backHandler.OnBackButton = Close;
                    break;                
            }
        }

        // Close button
		if (m_buttonCloseRoot != null)
        {
            m_buttonCloseRoot.SetActive(m_config.IsButtonCloseVisible);
        }

        // Dynamic buttons - depends on button mode
        if(m_buttonCancelRoot != null) {
            m_buttonCancelRoot.SetActive(
                m_config.ButtonMode == Config.EButtonsMode.ConfirmAndCancel ||
                m_config.ButtonMode == Config.EButtonsMode.ConfirmAndExtraAndCancel
            );
        }

        if(m_buttonConfirmRoot != null) {
            m_buttonConfirmRoot.SetActive(
                m_config.ButtonMode == Config.EButtonsMode.Confirm ||
                m_config.ButtonMode == Config.EButtonsMode.ConfirmAndCancel ||
                m_config.ButtonMode == Config.EButtonsMode.ConfirmAndExtra ||
                m_config.ButtonMode == Config.EButtonsMode.ConfirmAndExtraAndCancel
            );
        }

        if(m_buttonExtraRoot != null) {
            m_buttonExtraRoot.SetActive(
                m_config.ButtonMode == Config.EButtonsMode.ConfirmAndExtra ||
                m_config.ButtonMode == Config.EButtonsMode.ConfirmAndExtraAndCancel
            );
        }

        // Configure glows
        if(m_buttonCancelGlow != null) {
            m_buttonCancelGlow.SetActive(m_config.HighlightButton == Config.EHighlightButton.Cancel);
        }

        if(m_buttonConfirmGlow != null) {
            m_buttonConfirmGlow.SetActive(m_config.HighlightButton == Config.EHighlightButton.Confirm);
		}

        if(m_buttonExtraGlow != null) {
            m_buttonExtraGlow.SetActive(m_config.HighlightButton == Config.EHighlightButton.Extra);
        }

        // Configure texts (abstract)
        ConfigureTexts(m_config);
    }    

	protected abstract void ConfigureTexts(Config _config);

    /// <summary>
    /// Method called when the user clicks on any Confirm buttom. It's called by the editor
    /// </summary>
    public void OnConfirm()
    {
        if (m_config != null)
        {
            if (m_config.OnConfirm != null)
            {
                m_config.OnConfirm();
            }            
        }

        if (m_config == null || m_config.CloseOnConfirm)
        {
            Close();
        }
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
        yield return new WaitForSecondsRealtime(waitTime);
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
