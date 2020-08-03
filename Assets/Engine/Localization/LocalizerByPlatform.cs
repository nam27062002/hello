// TextfieldLocalization.cs
// 
// Created by Miguel Ángel Linares on 13/09/2018
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to automatically localize the text set in the editor
/// on a textfield switching by platform
/// Use this when possible rather than directly setting the text's value.
/// </summary>
//[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizerByPlatform : Localizer
{
    // Exposed members
    [Comment("This parameters will replace tid an awake")]
    [SerializeField] private string m_androidTid = "";
    public string androidTid
    {
        get { return m_androidTid; }
    }

    [SerializeField] private string m_iosTid = "";
    public string iOSTid
    {
        get { return m_iosTid; }
    }

    public override void Awake()
    {
        base.Awake();
#if UNITY_ANDROID
        m_tid = m_androidTid;
#elif UNITY_IOS
        m_tid = m_iosTid;
#endif
    }
}
