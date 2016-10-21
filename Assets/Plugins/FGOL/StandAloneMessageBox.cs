using UnityEngine;
using System.Collections;
using FGOL.Plugins.Native;

// Dialogue which can be used in PC builds
public class StandAloneMessageBox : MonoBehaviour
{
	private static int Layer = 0;

	private string	m_title;
	private string	m_message;
	private string	m_okButton;
	private string	m_cancelButton;

	private int		m_messageID;

	private Rect	m_windowRect = new Rect((Screen.width - 200) / 2, (Screen.height - 300) / 2, 200, 300);

	public static void ShowMessageBox(string title, string message, int msgID = -1)
	{
		GameObject go = new GameObject();
		go.name = "MessageBox";
		StandAloneMessageBox instance = go.AddComponent<StandAloneMessageBox>();
		instance.Init(title, message, msgID);
		Layer++;
	}

	public static void ShowMessageBoxWithButtons(string title, string message, string okButton, string cancelButton, int msgID = -1)
	{
		GameObject go = new GameObject();
		go.name = "MessageBox";
		StandAloneMessageBox instance = go.AddComponent<StandAloneMessageBox>();
		instance.Init(title, message, okButton, cancelButton, msgID);
		Layer++;
	}

	public void Init(string title, string message, int msgID = -1)
	{
		m_title = title;
		m_message = message;
		m_okButton = "Ok";
		m_cancelButton = string.Empty;
		m_messageID = msgID;
	}

	public void Init(string title, string message, string okButton, string cancelButton, int msgID = -1)
	{
		m_title = title;
		m_message = message;
		m_okButton = okButton;
		m_cancelButton = cancelButton;
		m_messageID = msgID;
	}

	private void OnGUI() 
    {
        GUI.Window(0, m_windowRect, DialogWindow, m_title);
    }

    // This is the actual window.
    private void DialogWindow(int windowID)
    {
		GUI.Label(new Rect(m_windowRect.width / 2 - m_title.Length * 2.5f, 20, m_windowRect.width, m_windowRect.height), m_message);

        if(GUI.Button(new Rect(5, m_windowRect.height - m_windowRect.height / 5, m_windowRect.width / 3, m_windowRect.height / 8), m_okButton))
        {
			NotifyAndDestroy("OK");
		}

		if(!string.IsNullOrEmpty(m_cancelButton))
		{
			if(GUI.Button(new Rect(m_windowRect.width - (m_windowRect.width / 3) - 5, m_windowRect.height - m_windowRect.height / 5, m_windowRect.width / 3, m_windowRect.height / 8), m_cancelButton))
			{
				NotifyAndDestroy("CANCEL");
			}
		}

		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}

	private void NotifyAndDestroy(string result)
	{
		if(m_messageID != -1)
		{
			FGOLNativeReceiver.Instance.MessageBoxClick(string.Format("{0}:{1}", m_messageID, result));
		}

		DestroyImmediate(gameObject);
		Layer--;
	}
}
