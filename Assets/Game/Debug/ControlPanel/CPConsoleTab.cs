﻿// CPConsoleTab
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

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed References
    [SerializeField]
    private ScrollRect m_outputScroll = null;
    [SerializeField]
    private TextMeshProUGUI m_outputText = null;

    private StringBuilder m_outputSb = new StringBuilder();    

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
        ClearConsole();
        PrintLogs();
    }

    /// <summary>
	/// Add a new line into the output console.
	/// </summary>
	/// <param name="_text">The text to be output.</param>
	private void Output(string _text) {
        // Add new line and timestamp
        if (m_outputSb.Length > 0) m_outputSb.AppendLine(); // Don't add new line for the very first line

        /*
        TimeSpan t = DateTime.UtcNow.Subtract(m_startTimestamp);
        m_outputSb.AppendFormat("<color={4}>{0:D2}:{1:D2}:{2:D2}.{3:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds, Colors.WithAlpha(Colors.white, 0.25f).ToHexString("#"));   // [AOC] Unfortunately, this version of Mono still doesn't support TimeSpan formatting (added at .Net 4)
        m_outputSb.Append(": </color>");
        */

        // Add text
        m_outputSb.Append(_text);

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
            Output(sm_logs[i]);
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
    private static List<string> sm_logs = new List<string>();

    private static CPConsoleTab sm_instance;

    public static void Log(string line) {
        sm_logs.Add(line);

        if (sm_instance != null && sm_instance.enabled) {
            sm_instance.Output(line);
        }
    }

    private static void ClearLogs() {
        sm_logs.Clear();
    }
    #endregion
}
