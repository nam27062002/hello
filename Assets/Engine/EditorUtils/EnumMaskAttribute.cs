// EnumMaskAttribute.cs
// 
// Created by Alger Ortín Castellví on 06/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom attribute to show an enum field as a bit mask.
/// <example>
/// // First define the Enumeration type and follow .NET-friendly practice of specifying it with System.FlagsAttribute.
/// [System.Flags]
/// enum MyFlagType {
///     One = (1 << 0),
///     Two = (1 << 1),
///     Three = (1 << 2)
/// }
///
/// // Now define the actual field, and tag on the EnumFlagsFieldAttribute.
/// [SerializeField, EnumFlagsField] MyFlagType m_fieldName;
/// </example>
/// 
/// From http://www.codingjargames.com/blog/2014/11/10/bitfields-in-unity/
/// From http://pastebin.com/1xkdHF6w
/// 
/// <remarks>
/// Apparently Unity has issues when the enum has a <c>0</c> entry (probably because 
/// Unity itselfs adds the "Everything" and "Nothing" options already), so avoid 
/// using it in your enum.
/// </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class EnumMaskAttribute : PropertyAttribute {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public EnumMaskAttribute() { 
		// Nothing to do
	}
}

