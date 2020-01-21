// CPConsoleTab
// Hungry Dragon
// 
//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Control panel tab that shows logs
/// </summary>
public class CPConsoleTab : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private int MAX_OUTPUT_LENGTH = 25000;	// Chars

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed References
    [SerializeField]
    private ScrollRect m_outputScroll = null;
    [SerializeField]
    private TextMeshProUGUI m_outputText = null;

    private StringBuilder m_outputSb = new StringBuilder();    

	// This variable is used instead of in order to make it thread independent, otherwise messages printed from other than main thread would cause an exception on device
	private bool m_isEnabled = false;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization
    /// </summary>
    private void Awake() {
        // Check required refs
        Debug.Assert(m_outputScroll != null, "Required field missing!");
        Debug.Assert(m_outputText != null, "Required field missing!");

        sm_instance = this;

        ClearConsole();
    }

    private void OnEnable() {
		m_isEnabled = true;
        ClearConsole();
        PrintLogs();
    }

	private void OnDisable() {
		m_isEnabled = false;
	}

    /// <summary>
	/// Add a new line into the output console.
	/// </summary>
	/// <param name="_text">The text to be output.</param>
	private void Output(string _text, string color=null) {
        // Add new line and timestamp
        if (m_outputSb.Length > 0) m_outputSb.AppendLine(); // Don't add new line for the very first line

		/*
        TimeSpan t = DateTime.UtcNow.Subtract(m_startTimestamp);
        m_outputSb.AppendFormat("<color={4}>{0:D2}:{1:D2}:{2:D2}.{3:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds, Colors.WithAlpha(Colors.white, 0.25f).ToHexString("#"));   // [AOC] Unfortunately, this version of Mono still doesn't support TimeSpan formatting (added at .Net 4)
        m_outputSb.Append(": </color>");
        */

		// Need color?
		bool colorNeeded = !string.IsNullOrEmpty(color);
		if(colorNeeded) m_outputSb.AppendFormat("<color={0}>", color);

        // Add text
        m_outputSb.Append(_text);

		// Close color
		if(colorNeeded) m_outputSb.AppendFormat("</color>");

		// Trim if needed
		if(m_outputSb.Length > MAX_OUTPUT_LENGTH) {
			m_outputSb.Remove(0, m_outputSb.Length - MAX_OUTPUT_LENGTH);
		}

        // Set text
        m_outputText.text = m_outputSb.ToString();

        // Update scroll
        // Don't reset scroll if the scroll position was manually set (different than 0)
        if (m_outputScroll.verticalNormalizedPosition < 0.01f)
        {   // Error margin
            StartCoroutine(ResetScrollPos());
        }

        // Output to console as well
        //Debug.Log(_text);
    }

    private void PrintLogs() {
        Output("Hungry Dragon v" + GameSettings.internalVersion + " console output");

        int count = sm_logs.Count;
        for (int i = 0; i < count; i++) {
            Output(sm_logs[i].message, sm_logs[i].color);
        }
    }

    /// <summary>
	/// Reset scroll position with a small delay.
	/// We need to do it delayed since the layout is not updated until the next frame.
	/// </summary>
	private IEnumerator ResetScrollPos() {
        //yield return new WaitForSeconds(0.1f);
        yield return new WaitForEndOfFrame();
        m_outputScroll.normalizedPosition = Vector2.zero;
    }

    /// <summary>
	/// Clear console button has been pressed.
	/// </summary>
	public void OnClearConsoleButton() {
        ClearConsole();
        ClearLogs();
        PrintLogs();
    }  
    
    private void ClearConsole() {
        m_outputSb.Length = 0;
    } 

    #region log
    private struct LogData
    {
        public string message;
        public string color;
    };

    private static List<LogData> sm_logs = new List<LogData>();    

    private static CPConsoleTab sm_instance;

    public static void Log(string message, string color=null) {
        LogData data = new LogData();
        data.message = message;
        data.color = color;

        sm_logs.Add(data);


        if (sm_instance != null && sm_instance.m_isEnabled) {
            sm_instance.Output(message, color);
        }
    }

    private static void ClearLogs() {
        sm_logs.Clear();
    }
    #endregion
}
