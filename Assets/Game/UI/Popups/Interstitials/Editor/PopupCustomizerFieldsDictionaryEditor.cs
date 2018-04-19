// PopupCustomizerFieldsDictionaryEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/04/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the PopupCustomizer.FieldsDictionary class.
/// </summary>
[CustomPropertyDrawer(typeof(PopupCustomizer.FieldsDictionary), true)]	// True to be used by heir classes as well
public class PopupCustomizerFieldsDictionaryEditor : SerializableDictionaryEditor {
	// Nothing to do, the parent will take care of it
}

/// <summary>
/// Custom editor for the PopupCustomizer.TextfieldsDictionary class.
/// </summary>
[CustomPropertyDrawer(typeof(PopupCustomizer.TextfieldsDictionary), true)]	// True to be used by heir classes as well
public class PopupCustomizerTextfieldsDictionaryEditor : SerializableDictionaryEditor {
	// Nothing to do, the parent will take care of it
}