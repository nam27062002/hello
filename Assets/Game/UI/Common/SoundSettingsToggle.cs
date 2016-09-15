// SoundSettingsToggle.cs
// Hungry Dragon
// 
// Created by David Germade on 15th September 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class can be used to use a slider as a toggle for enabling/disabling the sound on settings popup.
/// </summary>
public class SoundSettingsToggle : MonoBehaviour
{
    [SerializeField]
    private Slider m_slider;

    [SerializeField]
    private Image m_handler;

    void Awake()
    {
        Refresh();
    }

    /// <summary>
    /// It's called by the player when the user changes the value of the slide
    /// </summary>
    public void OnToggleChanged()
    {
        bool viewisEnabled = m_slider.value == m_slider.maxValue;
        bool isEnabled = ApplicationManager.instance.Settings_GetSoundIsEnabled();
        if (isEnabled != viewisEnabled)
        {
            ApplicationManager.instance.Settings_ToggleSoundIsEnabled();
            if (isEnabled)
            {
                AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");
            }

            Refresh();
        }
    }

    private void Refresh()
    {
        if (ApplicationManager.instance.Settings_GetSoundIsEnabled())
        {
            m_slider.value = m_slider.maxValue;
            m_handler.color = Colors.ParseHexString("0x00ff00");
        }
        else
        {
            m_slider.value = m_slider.minValue;
            m_handler.color = Colors.ParseHexString("0xff0000");
        }
    }
}
