
//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to enable/disable game objects during specific seasons.
/// </summary>
public class DragonSplashEnable : MonoBehaviour
{
    private Image m_splash = null;
    void Start()
    {
        m_splash = GetComponent<Image>();
        m_splash.enabled = false;
    }
    void Update()
    {
        if (!m_splash.enabled && FeatureSettingsManager.instance.IsFeatureSettingsApplied)
        {
            m_splash.enabled = true;
//            StartCoroutine(post());
        }

    }

    IEnumerator post()
    {
        yield return new WaitForSeconds(0.5f);
        m_splash.enabled = true;
    }
}