// UIFeedbackMessage.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;
using System;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// To define feedback strings in edible/flamable entities.
/// </summary>
[Serializable]
public class UIFeedbackMessage {
	public string text;
	public EUIFeedbackType type;
}
#endregion