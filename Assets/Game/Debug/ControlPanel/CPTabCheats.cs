using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPTabCheats : MonoBehaviour
{
    /// <summary>
	/// Clear all prefs.
	/// </summary>
	public void OnResetPrefs()
    {
        ControlPanel.instance.OnResetPrefs();
    }

}
